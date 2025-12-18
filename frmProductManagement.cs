using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.IO;

namespace StoreX_SalesManagement
{
    public partial class frmProductManagement : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;
        int selectedProductID = 0;
        string selectedImagePath = "";

        public frmProductManagement()
        {
            InitializeComponent();
        }

        private void frmProductManagement_Load(object sender, EventArgs e)
        {
            LoadProducts();
            LoadCategories();
            SetInitialState();
        }

        private void LoadProducts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT p.ProductID, p.ProductCode, p.ProductName, c.CategoryName,
                                    p.CostPrice, p. SellingPrice, p. InventoryQuantity, p.MinimumStock,
                                    CASE 
                                        WHEN p. InventoryQuantity = 0 THEN 'Out of Stock'
                                        WHEN p.InventoryQuantity <= p.MinimumStock THEN 'Low Stock'
                                        ELSE 'In Stock'
                                    END as StockStatus
                                    FROM Products p
                                    INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                                    WHERE p.IsActive = 1
                                    ORDER BY p.ProductName";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvProducts.DataSource = dt;
                    dgvProducts.Columns["ProductID"].Visible = false;

                    // Format currency columns
                    if (dgvProducts.Columns["CostPrice"] != null)
                        dgvProducts.Columns["CostPrice"].DefaultCellStyle.Format = "N0";

                    if (dgvProducts.Columns["SellingPrice"] != null)
                        dgvProducts.Columns["SellingPrice"].DefaultCellStyle.Format = "N0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT CategoryID, CategoryName FROM Categories WHERE IsActive = 1 ORDER BY CategoryName";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cboCategory.DataSource = dt;
                    cboCategory.DisplayMember = "CategoryName";
                    cboCategory.ValueMember = "CategoryID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== BUTTON ADD ====================
        private void btnAdd_Click(object sender, EventArgs e)
        {
            ClearFields();
            EnableFields(true);

            selectedProductID = 0; // Reset ID
            txtProductCode.Text = GenerateProductCode();

            // Enable/Disable buttons
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;

            txtProductName.Focus();
        }

        // ==================== BUTTON EDIT ====================
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product from the list first!",
                    "No Product Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            EnableFields(true);
            txtProductCode.Enabled = false; // Cannot change product code

            // Enable/Disable buttons
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;

            txtProductName.Focus();
        }

        // ==================== BUTTON SAVE ====================
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query;
                    SqlCommand cmd;

                    if (selectedProductID == 0) // ADD NEW
                    {
                        // Check if product code already exists
                        string checkQuery = "SELECT COUNT(*) FROM Products WHERE ProductCode = @ProductCode";
                        SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                        checkCmd.Parameters.AddWithValue("@ProductCode", txtProductCode.Text.Trim());
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Product code already exists!", "Duplicate",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtProductCode.Focus();
                            return;
                        }

                        query = @"INSERT INTO Products (ProductCode, ProductName, CategoryID, CostPrice, SellingPrice, 
                                 InventoryQuantity, MinimumStock, ImagePath, IsActive, CreatedDate, ModifiedDate)
                                 VALUES (@ProductCode, @ProductName, @CategoryID, @CostPrice, @SellingPrice, 
                                 @Quantity, @MinStock, @ImagePath, 1, GETDATE(), GETDATE())";
                    }
                    else // UPDATE EXISTING
                    {
                        query = @"UPDATE Products 
                                 SET ProductName = @ProductName, 
                                     CategoryID = @CategoryID,
                                     CostPrice = @CostPrice, 
                                     SellingPrice = @SellingPrice,
                                     InventoryQuantity = @Quantity, 
                                     MinimumStock = @MinStock, 
                                     ImagePath = @ImagePath,
                                     ModifiedDate = GETDATE()
                                 WHERE ProductID = @ProductID";
                    }

                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ProductCode", txtProductCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@ProductName", txtProductName.Text.Trim());
                    cmd.Parameters.AddWithValue("@CategoryID", cboCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@CostPrice", decimal.Parse(txtCostPrice.Text));
                    cmd.Parameters.AddWithValue("@SellingPrice", decimal.Parse(txtSellingPrice.Text));
                    cmd.Parameters.AddWithValue("@Quantity", int.Parse(txtQuantity.Text));
                    cmd.Parameters.AddWithValue("@MinStock", 10);
                    cmd.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(selectedImagePath) ? "" : selectedImagePath);

                    if (selectedProductID > 0)
                    {
                        cmd.Parameters.AddWithValue("@ProductID", selectedProductID);
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        string message = selectedProductID == 0 ? "Product added successfully!" : "Product updated successfully!";
                        MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadProducts();
                        SetInitialState();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving product: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== BUTTON DELETE ====================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product from the list first!",
                    "No Product Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Check permission
            if (SessionInfo.IsSales())
            {
                MessageBox.Show("Sales staff cannot delete products!",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete:\n\n{txtProductCode.Text} - {txtProductName.Text}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        // Soft delete
                        string query = @"UPDATE Products 
                                        SET IsActive = 0, ModifiedDate = GETDATE() 
                                        WHERE ProductID = @ProductID";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@ProductID", selectedProductID);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Product deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadProducts();
                            SetInitialState();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting product: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ==================== BUTTON CANCEL ====================
        private void btnCancel_Click(object sender, EventArgs e)
        {
            SetInitialState();
        }

        // ==================== BUTTON BROWSE IMAGE ====================
        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Product Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = openFileDialog.FileName;
                    picProduct.Image = System.Drawing.Image.FromFile(selectedImagePath);
                    picProduct.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading image: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== DATAGRIDVIEW CELL CLICK ====================
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                LoadSelectedProduct(e.RowIndex);
            }
        }

        // ==================== DATAGRIDVIEW SELECTION CHANGED ====================
        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null && dgvProducts.CurrentRow.Index >= 0)
            {
                LoadSelectedProduct(dgvProducts.CurrentRow.Index);
            }
        }

        // ==================== LOAD SELECTED PRODUCT ====================
        private void LoadSelectedProduct(int rowIndex)
        {
            try
            {
                DataGridViewRow row = dgvProducts.Rows[rowIndex];

                if (row.Cells["ProductID"].Value == null) return;

                selectedProductID = Convert.ToInt32(row.Cells["ProductID"].Value);

                txtProductCode.Text = row.Cells["ProductCode"].Value?.ToString() ?? "";
                txtProductName.Text = row.Cells["ProductName"].Value?.ToString() ?? "";

                // Set category
                string categoryName = row.Cells["CategoryName"].Value?.ToString() ?? "";
                for (int i = 0; i < cboCategory.Items.Count; i++)
                {
                    cboCategory.SelectedIndex = i;
                    if (cboCategory.Text == categoryName)
                        break;
                }

                txtCostPrice.Text = row.Cells["CostPrice"].Value?.ToString() ?? "0";
                txtSellingPrice.Text = row.Cells["SellingPrice"].Value?.ToString() ?? "0";
                txtQuantity.Text = row.Cells["InventoryQuantity"].Value?.ToString() ?? "0";

                // Load image if path exists
                // TODO: Implement image loading

                // Enable Edit/Delete buttons ONLY if not in edit mode
                if (!btnSave.Enabled)
                {
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = !SessionInfo.IsSales();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading product details: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== SEARCH TEXT CHANGED ====================
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (dgvProducts.DataSource != null)
            {
                try
                {
                    (dgvProducts.DataSource as DataTable).DefaultView.RowFilter =
                        $"ProductCode LIKE '%{searchText}%' OR ProductName LIKE '%{searchText}%'";
                }
                catch
                {
                    // Ignore filter errors
                }
            }
        }

        // ==================== VALIDATE INPUT ====================
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtProductCode.Text))
            {
                MessageBox.Show("Please enter product code!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductCode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please enter product name!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductName.Focus();
                return false;
            }

            if (cboCategory.SelectedValue == null)
            {
                MessageBox.Show("Please select category!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCategory.Focus();
                return false;
            }

            decimal costPrice;
            if (!decimal.TryParse(txtCostPrice.Text, out costPrice) || costPrice < 0)
            {
                MessageBox.Show("Please enter valid cost price (must be >= 0)!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCostPrice.Focus();
                return false;
            }

            decimal sellingPrice;
            if (!decimal.TryParse(txtSellingPrice.Text, out sellingPrice) || sellingPrice < 0)
            {
                MessageBox.Show("Please enter valid selling price (must be >= 0)!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSellingPrice.Focus();
                return false;
            }

            if (sellingPrice <= costPrice)
            {
                MessageBox.Show("Selling price must be greater than cost price!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSellingPrice.Focus();
                return false;
            }

            int quantity;
            if (!int.TryParse(txtQuantity.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Please enter valid quantity (must be >= 0)!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantity.Focus();
                return false;
            }

            return true;
        }

        // ==================== GENERATE PRODUCT CODE ====================
        private string GenerateProductCode()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT TOP 1 ProductCode FROM Products ORDER BY ProductID DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string lastCode = result.ToString();
                        // Extract number from PROD001 format
                        string numberPart = lastCode.Substring(4);
                        int number = int.Parse(numberPart) + 1;
                        return "PROD" + number.ToString("D3");
                    }
                    else
                    {
                        return "PROD001";
                    }
                }
            }
            catch
            {
                return "PROD001";
            }
        }

        // ==================== SET INITIAL STATE ====================
        private void SetInitialState()
        {
            EnableFields(false);
            ClearFields();

            btnAdd.Enabled = true;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
        }

        // ==================== ENABLE/DISABLE FIELDS ====================
        private void EnableFields(bool enable)
        {
            txtProductCode.Enabled = enable;
            txtProductName.Enabled = enable;
            cboCategory.Enabled = enable;
            txtCostPrice.Enabled = enable;
            txtSellingPrice.Enabled = enable;
            txtQuantity.Enabled = enable;
            btnBrowseImage.Enabled = enable;
        }

        // ==================== CLEAR FIELDS ====================
        private void ClearFields()
        {
            txtProductCode.Clear();
            txtProductName.Clear();
            txtCostPrice.Text = "0";
            txtSellingPrice.Text = "0";
            txtQuantity.Text = "0";

            if (cboCategory.Items.Count > 0)
                cboCategory.SelectedIndex = 0;

            picProduct.Image = null;
            selectedProductID = 0;
            selectedImagePath = "";
        }

        private void picProduct_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}