using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace StoreX_SalesManagement
{
    public partial class frmSales : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;
        DataTable dtSaleItems;

        public frmSales()
        {
            InitializeComponent();
        }

        private void frmSales_Load(object sender, EventArgs e)
        {
            LoadCustomers();
            LoadProducts();
            InitializeSaleItemsTable();
            cboPaymentMethod.SelectedIndex = 0; // Default:  Cash
            SetInitialState();
            LoadSalesList();
        }

        private void LoadCustomers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT CustomerID, CustomerCode + ' - ' + CustomerName as CustomerDisplay FROM Customers WHERE IsActive = 1 ORDER BY CustomerName";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cboCustomer.DataSource = dt;
                    cboCustomer.DisplayMember = "CustomerDisplay";
                    cboCustomer.ValueMember = "CustomerID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT ProductID, ProductCode + ' - ' + ProductName as ProductDisplay, 
                                    ProductCode, ProductName, SellingPrice, InventoryQuantity
                                    FROM Products 
                                    WHERE IsActive = 1 AND InventoryQuantity > 0
                                    ORDER BY ProductName";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cboProduct.DataSource = dt;
                    cboProduct.DisplayMember = "ProductDisplay";
                    cboProduct.ValueMember = "ProductID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeSaleItemsTable()
        {
            dtSaleItems = new DataTable();
            dtSaleItems.Columns.Add("ProductID", typeof(int));
            dtSaleItems.Columns.Add("ProductCode", typeof(string));
            dtSaleItems.Columns.Add("ProductName", typeof(string));
            dtSaleItems.Columns.Add("Quantity", typeof(int));
            dtSaleItems.Columns.Add("UnitPrice", typeof(decimal));
            dtSaleItems.Columns.Add("LineTotal", typeof(decimal));

            dgvSaleItems.DataSource = dtSaleItems;
            dgvSaleItems.Columns["ProductID"].Visible = false;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
            EnableFields(true);
            txtSaleCode.Text = GenerateSaleCode();
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnNew.Enabled = false;
        }

        private void cboProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProduct.SelectedValue != null && cboProduct.DataSource != null)
            {
                try
                {
                    DataRowView row = (DataRowView)cboProduct.SelectedItem;
                    txtPrice.Text = row["SellingPrice"].ToString();

                    // Check available quantity
                    int availableQty = Convert.ToInt32(row["InventoryQuantity"]);
                    nudQuantity.Maximum = availableQty;

                    if (availableQty == 0)
                    {
                        MessageBox.Show("This product is out of stock!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch { }
            }
        }
        private void txtDiscountAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Chỉ cho phép nhập số và backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void nudQuantity_ValueChanged(object sender, EventArgs e)
        {
            // Can add real-time calculation here if needed
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (cboProduct.SelectedValue == null)
            {
                MessageBox.Show("Please select a product!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataRowView selectedProduct = (DataRowView)cboProduct.SelectedItem;
                int productID = Convert.ToInt32(selectedProduct["ProductID"]);
                string productCode = selectedProduct["ProductCode"].ToString();
                string productName = selectedProduct["ProductName"].ToString();
                int quantity = (int)nudQuantity.Value;
                decimal unitPrice = decimal.Parse(txtPrice.Text);
                decimal lineTotal = quantity * unitPrice;

                // Check if product already exists in grid
                bool found = false;
                foreach (DataRow row in dtSaleItems.Rows)
                {
                    if (Convert.ToInt32(row["ProductID"]) == productID)
                    {
                        // Update quantity
                        int newQty = Convert.ToInt32(row["Quantity"]) + quantity;

                        // Check available inventory
                        int availableQty = Convert.ToInt32(selectedProduct["InventoryQuantity"]);
                        if (newQty > availableQty)
                        {
                            MessageBox.Show($"Insufficient stock!  Available: {availableQty}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        row["Quantity"] = newQty;
                        row["LineTotal"] = newQty * unitPrice;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // Check inventory
                    int availableQty = Convert.ToInt32(selectedProduct["InventoryQuantity"]);
                    if (quantity > availableQty)
                    {
                        MessageBox.Show($"Insufficient stock! Available:  {availableQty}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Add new row
                    DataRow newRow = dtSaleItems.NewRow();
                    newRow["ProductID"] = productID;
                    newRow["ProductCode"] = productCode;
                    newRow["ProductName"] = productName;
                    newRow["Quantity"] = quantity;
                    newRow["UnitPrice"] = unitPrice;
                    newRow["LineTotal"] = lineTotal;
                    dtSaleItems.Rows.Add(newRow);
                }

                CalculateTotals();
                nudQuantity.Value = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvSaleItems.CurrentRow != null)
            {
                dgvSaleItems.Rows.Remove(dgvSaleItems.CurrentRow);
                CalculateTotals();
            }
        }

        private void txtDiscountAmount_TextChanged(object sender, EventArgs e)
        {
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            decimal total = 0;
            foreach (DataRow row in dtSaleItems.Rows)
            {
                total += Convert.ToDecimal(row["LineTotal"]);
            }

            txtTotalAmount.Text = total.ToString("N0");

            decimal discount = 0;
            if (decimal.TryParse(txtDiscountAmount.Text, out discount))
            {
                if (discount < 0) discount = 0;
                if (discount > total) discount = total;
            }

            decimal finalAmount = total - discount;
            txtFinalAmount.Text = finalAmount.ToString("N0");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // 1. Insert Sale Header
                        string saleQuery = @"INSERT INTO Sales (SaleCode, CustomerID, EmployeeID, SaleDate, 
                                            TotalAmount, DiscountAmount, FinalAmount, PaymentMethod, Status, Notes)
                                            VALUES (@SaleCode, @CustomerID, @EmployeeID, @SaleDate, 
                                            @TotalAmount, @DiscountAmount, @FinalAmount, @PaymentMethod, 'Completed', @Notes);
                                            SELECT SCOPE_IDENTITY();";

                        SqlCommand saleCmd = new SqlCommand(saleQuery, conn, transaction);
                        saleCmd.Parameters.AddWithValue("@SaleCode", txtSaleCode.Text);
                        saleCmd.Parameters.AddWithValue("@CustomerID", cboCustomer.SelectedValue);
                        saleCmd.Parameters.AddWithValue("@EmployeeID", SessionInfo.EmployeeID);
                        saleCmd.Parameters.AddWithValue("@SaleDate", dtpSaleDate.Value);
                        saleCmd.Parameters.AddWithValue("@TotalAmount", decimal.Parse(txtTotalAmount.Text.Replace(",", "")));
                        saleCmd.Parameters.AddWithValue("@DiscountAmount", decimal.Parse(txtDiscountAmount.Text));
                        saleCmd.Parameters.AddWithValue("@FinalAmount", decimal.Parse(txtFinalAmount.Text.Replace(",", "")));
                        saleCmd.Parameters.AddWithValue("@PaymentMethod", cboPaymentMethod.Text);
                        saleCmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(txtNotes.Text) ? DBNull.Value : (object)txtNotes.Text);

                        int saleID = Convert.ToInt32(saleCmd.ExecuteScalar());

                        // 2. Insert Sale Details & Update Inventory
                        foreach (DataRow row in dtSaleItems.Rows)
                        {
                            // Insert sale detail
                            string detailQuery = @"INSERT INTO SaleDetails (SaleID, ProductID, Quantity, UnitPrice, DiscountPercent, LineTotal)
                                                  VALUES (@SaleID, @ProductID, @Quantity, @UnitPrice, 0, @LineTotal)";

                            SqlCommand detailCmd = new SqlCommand(detailQuery, conn, transaction);
                            detailCmd.Parameters.AddWithValue("@SaleID", saleID);
                            detailCmd.Parameters.AddWithValue("@ProductID", row["ProductID"]);
                            detailCmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                            detailCmd.Parameters.AddWithValue("@UnitPrice", row["UnitPrice"]);
                            detailCmd.Parameters.AddWithValue("@LineTotal", row["LineTotal"]);
                            detailCmd.ExecuteNonQuery();

                            // Update inventory
                            string updateInventory = @"UPDATE Products 
                                                      SET InventoryQuantity = InventoryQuantity - @Quantity,
                                                          ModifiedDate = GETDATE()
                                                      WHERE ProductID = @ProductID";

                            SqlCommand inventoryCmd = new SqlCommand(updateInventory, conn, transaction);
                            inventoryCmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                            inventoryCmd.Parameters.AddWithValue("@ProductID", row["ProductID"]);
                            inventoryCmd.ExecuteNonQuery();
                        }

                        // 3. Log audit
                        string auditQuery = @"INSERT INTO AuditLog (EmployeeID, Action, TableName, RecordID, NewValue, LogDate)
                                            VALUES (@EmployeeID, 'Create', 'Sales', @RecordID, @NewValue, GETDATE())";
                        SqlCommand auditCmd = new SqlCommand(auditQuery, conn, transaction);
                        auditCmd.Parameters.AddWithValue("@EmployeeID", SessionInfo.EmployeeID);
                        auditCmd.Parameters.AddWithValue("@RecordID", saleID);
                        auditCmd.Parameters.AddWithValue("@NewValue", $"Sale {txtSaleCode.Text} created");
                        auditCmd.ExecuteNonQuery();

                        transaction.Commit();

                        MessageBox.Show($"Sale saved successfully!\nSale Code: {txtSaleCode.Text}\nTotal:  {txtFinalAmount.Text} VND",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Ask to print invoice
                        DialogResult printResult = MessageBox.Show("Do you want to print invoice?", "Print",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (printResult == DialogResult.Yes)
                        {
                            PrintInvoice(saleID);
                        }

                        SetInitialState();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving sale: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadSalesList();
        }

        private void LoadSalesList()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Lấy hóa đơn hôm nay
                    string query = @"SELECT 
                            s.SaleID,
                            s.SaleCode,
                            s.SaleDate,
                            c.CustomerName,
                            e. EmployeeName,
                            s.TotalAmount,
                            s. DiscountAmount,
                            s.FinalAmount,
                            s.PaymentMethod,
                            s. Status
                            FROM Sales s
                            INNER JOIN Customers c ON s. CustomerID = c.CustomerID
                            INNER JOIN Employees e ON s.EmployeeID = e.EmployeeID
                            WHERE CAST(s.SaleDate AS DATE) = CAST(GETDATE() AS DATE)
                            ORDER BY s. SaleDate DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvSalesList.DataSource = dt;

                    // Hide SaleID column
                    if (dgvSalesList.Columns["SaleID"] != null)
                        dgvSalesList.Columns["SaleID"].Visible = false;

                    // Format currency columns
                    if (dgvSalesList.Columns["TotalAmount"] != null)
                        dgvSalesList.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";

                    if (dgvSalesList.Columns["DiscountAmount"] != null)
                        dgvSalesList.Columns["DiscountAmount"].DefaultCellStyle.Format = "N0";

                    if (dgvSalesList.Columns["FinalAmount"] != null)
                        dgvSalesList.Columns["FinalAmount"].DefaultCellStyle.Format = "N0";

                    // Format date column
                    if (dgvSalesList.Columns["SaleDate"] != null)
                        dgvSalesList.Columns["SaleDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sales list: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== SEARCH HÓA ĐƠN ====================
        private void txtSearchSale_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearchSale.Text.Trim();
            if (dgvSalesList.DataSource != null)
            {
                try
                {
                    (dgvSalesList.DataSource as DataTable).DefaultView.RowFilter =
                        $"SaleCode LIKE '%{searchText}%' OR CustomerName LIKE '%{searchText}%' OR EmployeeName LIKE '%{searchText}%'";
                }
                catch
                {
                    // Ignore filter errors
                }
            }
        }

        // ==================== REFRESH DANH SÁCH ====================
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadSalesList();
            MessageBox.Show("Sales list refreshed!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ==================== DOUBLE CLICK ĐỂ XEM CHI TIẾT ====================
        private void dgvSalesList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSalesList.Rows[e.RowIndex];
                int saleID = Convert.ToInt32(row.Cells["SaleID"].Value);
                string saleCode = row.Cells["SaleCode"].Value.ToString();

                // Hiển thị chi tiết hóa đơn
                ViewSaleDetails(saleID, saleCode);
            }
        }

        // ==================== XEM CHI TIẾT HÓA ĐƠN ====================
        private void ViewSaleDetails(int saleID, string saleCode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                            p.ProductCode,
                            p.ProductName,
                            sd. Quantity,
                            sd.UnitPrice,
                            sd. LineTotal
                            FROM SaleDetails sd
                            INNER JOIN Products p ON sd.ProductID = p.ProductID
                            WHERE sd.SaleID = @SaleID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@SaleID", saleID);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Hiển thị trong message box hoặc form riêng
                    string details = $"Sale Code: {saleCode}\n\n";
                    details += "Products:\n";
                    details += "=================================\n";

                    foreach (DataRow row in dt.Rows)
                    {
                        details += $"{row["ProductCode"]} - {row["ProductName"]}\n";
                        details += $"Qty: {row["Quantity"]} x {Convert.ToDecimal(row["UnitPrice"]):N0} = {Convert.ToDecimal(row["LineTotal"]):N0} VND\n\n";
                    }

                    MessageBox.Show(details, "Sale Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sale details: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to cancel this sale?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SetInitialState();
            }
        }

        private bool ValidateInput()
        {
            if (cboCustomer.SelectedValue == null)
            {
                MessageBox.Show("Please select a customer!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCustomer.Focus();
                return false;
            }

            if (dtSaleItems.Rows.Count == 0)
            {
                MessageBox.Show("Please add at least one item to the sale!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private string GenerateSaleCode()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Lấy mã sale code lớn nhất
                    string query = "SELECT TOP 1 SaleCode FROM Sales ORDER BY SaleID DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string lastCode = result.ToString();

                        // SALE-2025001 → Lấy "2025001"
                        string yearAndNumber = lastCode.Substring(5); // Bỏ "SALE-"

                        // Tăng số lên 1
                        int number = int.Parse(yearAndNumber) + 1;

                        // Tạo mã mới với năm hiện tại
                        return "SALE-" + DateTime.Now.Year + number.ToString("D3");
                    }
                    else
                    {
                        // Nếu chưa có sale nào → Tạo mã đầu tiên
                        return "SALE-" + DateTime.Now.Year + "001";
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu lỗi → Dùng timestamp để đảm bảo unique
                return "SALE-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        private void PrintInvoice(int saleID)
        {
            // Simple print functionality - you can enhance this with Crystal Reports or other tools
            MessageBox.Show($"Print Invoice for Sale ID: {saleID}\n(Print functionality to be implemented)",
                "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetInitialState()
        {
            ClearForm();
            EnableFields(false);
            btnNew.Enabled = true;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
        }

        private void EnableFields(bool enable)
        {
            cboCustomer.Enabled = enable;
            dtpSaleDate.Enabled = enable;
            cboPaymentMethod.Enabled = enable;
            txtNotes.Enabled = enable;
            cboProduct.Enabled = enable;
            nudQuantity.Enabled = enable;
            txtDiscountAmount.Enabled = enable;
            btnAddItem.Enabled = enable;
            btnRemoveItem.Enabled = enable;
        }

        private void ClearForm()
        {
            txtSaleCode.Clear();
            if (cboCustomer.Items.Count > 0) cboCustomer.SelectedIndex = 0;
            dtpSaleDate.Value = DateTime.Now;
            cboPaymentMethod.SelectedIndex = 0;
            txtNotes.Clear();
            if (cboProduct.Items.Count > 0) cboProduct.SelectedIndex = 0;
            nudQuantity.Value = 1;
            txtDiscountAmount.Text = "0";
            txtTotalAmount.Text = "0";
            txtFinalAmount.Text = "0";
            dtSaleItems.Clear();
        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {

        }
    }
}