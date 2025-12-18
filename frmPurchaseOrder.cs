using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace StoreX_SalesManagement
{
    public partial class frmPurchaseOrder : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;
        DataTable dtPOItems;
        int currentPOID = 0;

        public frmPurchaseOrder()
        {
            InitializeComponent();
        }

        private void frmPurchaseOrder_Load(object sender, EventArgs e)
        {
            // Check permission - Only Warehouse and Admin
            if (!SessionInfo.IsWarehouse() && !SessionInfo.IsAdmin())
            {
                MessageBox.Show("You don't have permission to access this feature!", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            LoadSuppliers();
            LoadProducts();
            InitializePOItemsTable();
            cboStatus.SelectedIndex = 0; // Default:  Pending
            SetInitialState();
            cboFilterStatus.SelectedIndex = 0; // Default:  All
            LoadPOList();
        }
        private void LoadPOList()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                            po.PurchaseOrderID,
                            po.POCode,
                            po.OrderDate,
                            s. SupplierName,
                            e. EmployeeName,
                            po.TotalAmount,
                            po. Status,
                            po.ReceivedDate
                            FROM PurchaseOrders po
                            INNER JOIN Suppliers s ON po. SupplierID = s. SupplierID
                            INNER JOIN Employees e ON po.EmployeeID = e. EmployeeID";

                    // Filter by status if not "All"
                    if (cboFilterStatus.SelectedIndex > 0)
                    {
                        query += " WHERE po.Status = @Status";
                    }

                    query += " ORDER BY po.OrderDate DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    if (cboFilterStatus.SelectedIndex > 0)
                    {
                        cmd.Parameters.AddWithValue("@Status", cboFilterStatus.Text);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvPOList.DataSource = dt;

                    // Hide PurchaseOrderID
                    if (dgvPOList.Columns["PurchaseOrderID"] != null)
                        dgvPOList.Columns["PurchaseOrderID"].Visible = false;

                    // Format currency
                    if (dgvPOList.Columns["TotalAmount"] != null)
                        dgvPOList.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";

                    // Format dates
                    if (dgvPOList.Columns["OrderDate"] != null)
                        dgvPOList.Columns["OrderDate"].DefaultCellStyle.Format = "dd/MM/yyyy";

                    if (dgvPOList.Columns["ReceivedDate"] != null)
                        dgvPOList.Columns["ReceivedDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading PO list: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== SEARCH PURCHASE ORDER ====================
        private void txtSearchPO_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearchPO.Text.Trim();
            if (dgvPOList.DataSource != null)
            {
                try
                {
                    (dgvPOList.DataSource as DataTable).DefaultView.RowFilter =
                        $"POCode LIKE '%{searchText}%' OR SupplierName LIKE '%{searchText}%' OR EmployeeName LIKE '%{searchText}%'";
                }
                catch
                {
                    // Ignore filter errors
                }
            }
        }

        // ==================== FILTER BY STATUS ====================
        private void cboFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPOList();
        }

        // ==================== REFRESH LIST ====================
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadPOList();
            MessageBox.Show("Purchase Orders list refreshed!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ==================== DOUBLE CLICK TO VIEW DETAILS ====================
        private void dgvPOList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPOList.Rows[e.RowIndex];
                int poID = Convert.ToInt32(row.Cells["PurchaseOrderID"].Value);
                string poCode = row.Cells["POCode"].Value.ToString();

                ViewPODetails(poID, poCode);
            }
        }

        // ==================== VIEW PO DETAILS ====================
        private void ViewPODetails(int poID, string poCode)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                            p.ProductCode,
                            p.ProductName,
                            pod. Quantity,
                            pod.UnitCost,
                            pod.LineTotal
                            FROM PurchaseOrderDetails pod
                            INNER JOIN Products p ON pod.ProductID = p.ProductID
                            WHERE pod.PurchaseOrderID = @POID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@POID", poID);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    string details = $"PO Code: {poCode}\n\n";
                    details += "Products:\n";
                    details += "=================================\n";

                    foreach (DataRow row in dt.Rows)
                    {
                        details += $"{row["ProductCode"]} - {row["ProductName"]}\n";
                        details += $"Qty: {row["Quantity"]} x {Convert.ToDecimal(row["UnitCost"]):N0} = {Convert.ToDecimal(row["LineTotal"]):N0} VND\n\n";
                    }

                    MessageBox.Show(details, "Purchase Order Details",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading PO details: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadSuppliers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT SupplierID, SupplierCode + ' - ' + SupplierName as SupplierDisplay FROM Suppliers WHERE IsActive = 1 ORDER BY SupplierName";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cboSupplier.DataSource = dt;
                    cboSupplier.DisplayMember = "SupplierDisplay";
                    cboSupplier.ValueMember = "SupplierID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading suppliers: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT ProductID, ProductCode + ' - ' + ProductName as ProductDisplay,
                                    ProductCode, ProductName, CostPrice
                                    FROM Products
                                    WHERE IsActive = 1
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
                MessageBox.Show("Error loading products: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePOItemsTable()
        {
            dtPOItems = new DataTable();
            dtPOItems.Columns.Add("ProductID", typeof(int));
            dtPOItems.Columns.Add("ProductCode", typeof(string));
            dtPOItems.Columns.Add("ProductName", typeof(string));
            dtPOItems.Columns.Add("Quantity", typeof(int));
            dtPOItems.Columns.Add("UnitCost", typeof(decimal));
            dtPOItems.Columns.Add("LineTotal", typeof(decimal));

            dgvPOItems.DataSource = dtPOItems;
            dgvPOItems.Columns["ProductID"].Visible = false;

            // Format currency columns
            dgvPOItems.Columns["UnitCost"].DefaultCellStyle.Format = "N0";
            dgvPOItems.Columns["LineTotal"].DefaultCellStyle.Format = "N0";
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
            EnableFields(true);
            txtPOCode.Text = GeneratePOCode();
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnNew.Enabled = false;
            btnMarkReceived.Enabled = false;
            cboStatus.Enabled = false; // Status auto set to Pending
        }

        private void cboProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProduct.SelectedValue != null && cboProduct.DataSource != null)
            {
                try
                {
                    DataRowView row = (DataRowView)cboProduct.SelectedItem;
                    txtCost.Text = row["CostPrice"].ToString();
                }
                catch { }
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (cboProduct.SelectedValue == null)
            {
                MessageBox.Show("Please select a product!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtCost.Text, out decimal cost) || cost <= 0)
            {
                MessageBox.Show("Please enter valid unit cost!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCost.Focus();
                return;
            }

            try
            {
                DataRowView selectedProduct = (DataRowView)cboProduct.SelectedItem;
                int productID = Convert.ToInt32(selectedProduct["ProductID"]);
                string productCode = selectedProduct["ProductCode"].ToString();
                string productName = selectedProduct["ProductName"].ToString();
                int quantity = (int)nudQuantity.Value;
                decimal unitCost = decimal.Parse(txtCost.Text);
                decimal lineTotal = quantity * unitCost;

                // Check if product already exists
                bool found = false;
                foreach (DataRow row in dtPOItems.Rows)
                {
                    if (Convert.ToInt32(row["ProductID"]) == productID)
                    {
                        // Update quantity
                        int newQty = Convert.ToInt32(row["Quantity"]) + quantity;
                        row["Quantity"] = newQty;
                        row["UnitCost"] = unitCost;
                        row["LineTotal"] = newQty * unitCost;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // Add new row
                    DataRow newRow = dtPOItems.NewRow();
                    newRow["ProductID"] = productID;
                    newRow["ProductCode"] = productCode;
                    newRow["ProductName"] = productName;
                    newRow["Quantity"] = quantity;
                    newRow["UnitCost"] = unitCost;
                    newRow["LineTotal"] = lineTotal;
                    dtPOItems.Rows.Add(newRow);
                }

                CalculateTotal();
                nudQuantity.Value = 1;
                txtCost.Text = "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvPOItems.CurrentRow != null)
            {
                dgvPOItems.Rows.Remove(dgvPOItems.CurrentRow);
                CalculateTotal();
            }
        }

        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (DataRow row in dtPOItems.Rows)
            {
                total += Convert.ToDecimal(row["LineTotal"]);
            }
            txtTotalAmount.Text = total.ToString("N0");
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
                        // 1. Insert Purchase Order Header
                        string poQuery = @"INSERT INTO PurchaseOrders (POCode, SupplierID, EmployeeID, OrderDate, 
                                          TotalAmount, Status, Notes)
                                          VALUES (@POCode, @SupplierID, @EmployeeID, @OrderDate, 
                                          @TotalAmount, @Status, @Notes);
                                          SELECT SCOPE_IDENTITY();";

                        SqlCommand poCmd = new SqlCommand(poQuery, conn, transaction);
                        poCmd.Parameters.AddWithValue("@POCode", txtPOCode.Text);
                        poCmd.Parameters.AddWithValue("@SupplierID", cboSupplier.SelectedValue);
                        poCmd.Parameters.AddWithValue("@EmployeeID", SessionInfo.EmployeeID);
                        poCmd.Parameters.AddWithValue("@OrderDate", dtpOrderDate.Value);
                        poCmd.Parameters.AddWithValue("@TotalAmount", decimal.Parse(txtTotalAmount.Text.Replace(",", "")));
                        poCmd.Parameters.AddWithValue("@Status", cboStatus.Text);
                        poCmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(txtNotes.Text) ? DBNull.Value : (object)txtNotes.Text);

                        int poID = Convert.ToInt32(poCmd.ExecuteScalar());
                        currentPOID = poID;

                        // 2. Insert PO Details
                        foreach (DataRow row in dtPOItems.Rows)
                        {
                            string detailQuery = @"INSERT INTO PurchaseOrderDetails (PurchaseOrderID, ProductID, Quantity, UnitCost, LineTotal)
                                                  VALUES (@PurchaseOrderID, @ProductID, @Quantity, @UnitCost, @LineTotal)";

                            SqlCommand detailCmd = new SqlCommand(detailQuery, conn, transaction);
                            detailCmd.Parameters.AddWithValue("@PurchaseOrderID", poID);
                            detailCmd.Parameters.AddWithValue("@ProductID", row["ProductID"]);
                            detailCmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                            detailCmd.Parameters.AddWithValue("@UnitCost", row["UnitCost"]);
                            detailCmd.Parameters.AddWithValue("@LineTotal", row["LineTotal"]);
                            detailCmd.ExecuteNonQuery();
                        }

                        // 3. If status is "Received", update inventory
                        if (cboStatus.Text == "Received")
                        {
                            foreach (DataRow row in dtPOItems.Rows)
                            {
                                string updateInventory = @"UPDATE Products 
                                                          SET InventoryQuantity = InventoryQuantity + @Quantity,
                                                              ModifiedDate = GETDATE()
                                                          WHERE ProductID = @ProductID";

                                SqlCommand inventoryCmd = new SqlCommand(updateInventory, conn, transaction);
                                inventoryCmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                                inventoryCmd.Parameters.AddWithValue("@ProductID", row["ProductID"]);
                                inventoryCmd.ExecuteNonQuery();
                            }

                            // Update ReceivedDate
                            string updatePO = "UPDATE PurchaseOrders SET ReceivedDate = GETDATE() WHERE PurchaseOrderID = @POID";
                            SqlCommand updateCmd = new SqlCommand(updatePO, conn, transaction);
                            updateCmd.Parameters.AddWithValue("@POID", poID);
                            updateCmd.ExecuteNonQuery();
                        }

                        // 4. Audit log
                        string auditQuery = @"INSERT INTO AuditLog (EmployeeID, Action, TableName, RecordID, NewValue, LogDate)
                                            VALUES (@EmployeeID, 'Create', 'PurchaseOrders', @RecordID, @NewValue, GETDATE())";
                        SqlCommand auditCmd = new SqlCommand(auditQuery, conn, transaction);
                        auditCmd.Parameters.AddWithValue("@EmployeeID", SessionInfo.EmployeeID);
                        auditCmd.Parameters.AddWithValue("@RecordID", poID);
                        auditCmd.Parameters.AddWithValue("@NewValue", $"PO {txtPOCode.Text} created");
                        auditCmd.ExecuteNonQuery();

                        transaction.Commit();

                        MessageBox.Show($"Purchase Order saved successfully!\nPO Code: {txtPOCode.Text}\nTotal:  {txtTotalAmount.Text} VND",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                MessageBox.Show("Error saving purchase order: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadPOList(); // Refresh list

            SetInitialState();
        }

        private void btnMarkReceived_Click(object sender, EventArgs e)
        {
            if (currentPOID == 0)
            {
                MessageBox.Show("No purchase order selected!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Mark this Purchase Order as Received?\nThis will update inventory! ",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlTransaction transaction = conn.BeginTransaction();

                        try
                        {
                            // Update PO status
                            string updatePO = @"UPDATE PurchaseOrders 
                                               SET Status = 'Received', ReceivedDate = GETDATE() 
                                               WHERE PurchaseOrderID = @POID";
                            SqlCommand updateCmd = new SqlCommand(updatePO, conn, transaction);
                            updateCmd.Parameters.AddWithValue("@POID", currentPOID);
                            updateCmd.ExecuteNonQuery();

                            // Update inventory for each item
                            foreach (DataRow row in dtPOItems.Rows)
                            {
                                string updateInventory = @"UPDATE Products 
                                                          SET InventoryQuantity = InventoryQuantity + @Quantity,
                                                              ModifiedDate = GETDATE()
                                                          WHERE ProductID = @ProductID";

                                SqlCommand inventoryCmd = new SqlCommand(updateInventory, conn, transaction);
                                inventoryCmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                                inventoryCmd.Parameters.AddWithValue("@ProductID", row["ProductID"]);
                                inventoryCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            MessageBox.Show("Purchase Order marked as Received!\nInventory updated successfully! ",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                    MessageBox.Show("Error updating purchase order: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            LoadPOList(); // Refresh list

            SetInitialState();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to cancel? ", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SetInitialState();
            }
        }

        private void txtCost_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow digits and backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private bool ValidateInput()
        {
            if (cboSupplier.SelectedValue == null)
            {
                MessageBox.Show("Please select a supplier!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboSupplier.Focus();
                return false;
            }

            if (dtPOItems.Rows.Count == 0)
            {
                MessageBox.Show("Please add at least one item to the purchase order!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private string GeneratePOCode()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT TOP 1 POCode FROM PurchaseOrders ORDER BY PurchaseOrderID DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string lastCode = result.ToString();
                        int number = int.Parse(lastCode.Substring(3)) + 1;
                        return "PO-" + number.ToString("D6");
                    }
                    else
                    {
                        return "PO-000001";
                    }
                }
            }
            catch
            {
                return "PO-000001";
            }
        }

        private void SetInitialState()
        {
            ClearForm();
            EnableFields(false);
            btnNew.Enabled = true;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            btnMarkReceived.Enabled = false;
            currentPOID = 0;
        }

        private void EnableFields(bool enable)
        {
            cboSupplier.Enabled = enable;
            dtpOrderDate.Enabled = enable;
            txtNotes.Enabled = enable;
            cboProduct.Enabled = enable;
            txtCost.Enabled = enable;
            nudQuantity.Enabled = enable;
            btnAddItem.Enabled = enable;
            btnRemoveItem.Enabled = enable;
        }

        private void ClearForm()
        {
            txtPOCode.Clear();
            if (cboSupplier.Items.Count > 0) cboSupplier.SelectedIndex = 0;
            dtpOrderDate.Value = DateTime.Now;
            cboStatus.SelectedIndex = 0;
            txtNotes.Clear();
            if (cboProduct.Items.Count > 0) cboProduct.SelectedIndex = 0;
            txtCost.Text = "0";
            nudQuantity.Value = 1;
            txtTotalAmount.Text = "0";
            dtPOItems.Clear();
        }
    }
}