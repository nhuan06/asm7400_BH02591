using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.Text.RegularExpressions;

namespace StoreX_SalesManagement
{
    public partial class frmCustomerManagement : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;
        int selectedCustomerID = 0;

        public frmCustomerManagement()
        {
            InitializeComponent();
        }

        private void frmCustomerManagement_Load(object sender, EventArgs e)
        {
            LoadCustomers();
            SetInitialState();
        }

        private void LoadCustomers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT CustomerID, CustomerCode, CustomerName, PhoneNumber, 
                                    Email, Address, DateOfBirth,
                                    (SELECT COUNT(*) FROM Sales WHERE CustomerID = c.CustomerID AND Status = 'Completed') as TotalPurchases,
                                    (SELECT ISNULL(SUM(FinalAmount), 0) FROM Sales WHERE CustomerID = c.CustomerID AND Status = 'Completed') as TotalSpent
                                    FROM Customers c
                                    WHERE IsActive = 1
                                    ORDER BY CustomerName";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvCustomers.DataSource = dt;
                    dgvCustomers.Columns["CustomerID"].Visible = false;

                    // Format currency column
                    if (dgvCustomers.Columns["TotalSpent"] != null)
                    {
                        dgvCustomers.Columns["TotalSpent"].DefaultCellStyle.Format = "N0";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ClearFields();
            EnableFields(true);
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            selectedCustomerID = 0;

            // Generate customer code
            txtCustomerCode.Text = GenerateCustomerCode();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedCustomerID == 0)
            {
                MessageBox.Show("Please select a customer to edit!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EnableFields(true);
            txtCustomerCode.Enabled = false; // Don't allow edit code
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query;
                    if (selectedCustomerID == 0) // Add new
                    {
                        query = @"INSERT INTO Customers (CustomerCode, CustomerName, PhoneNumber, Email, Address, DateOfBirth, IsActive)
                                 VALUES (@CustomerCode, @CustomerName, @PhoneNumber, @Email, @Address, @DateOfBirth, 1)";
                    }
                    else // Update
                    {
                        query = @"UPDATE Customers SET CustomerName = @CustomerName, PhoneNumber = @PhoneNumber,
                                 Email = @Email, Address = @Address, DateOfBirth = @DateOfBirth, ModifiedDate = GETDATE()
                                 WHERE CustomerID = @CustomerID";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CustomerCode", txtCustomerCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
                    cmd.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@DateOfBirth", dtpDateOfBirth.Value);

                    if (selectedCustomerID > 0)
                        cmd.Parameters.AddWithValue("@CustomerID", selectedCustomerID);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Customer saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadCustomers();
                    SetInitialState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving customer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCustomerID == 0)
            {
                MessageBox.Show("Please select a customer to delete!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        // Soft delete
                        string query = "UPDATE Customers SET IsActive = 0, ModifiedDate = GETDATE() WHERE CustomerID = @CustomerID";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@CustomerID", selectedCustomerID);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCustomers();
                        SetInitialState();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting customer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SetInitialState();
        }

        private void btnViewHistory_Click(object sender, EventArgs e)
        {
            if (selectedCustomerID == 0)
            {
                MessageBox.Show("Please select a customer to view history!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Open purchase history form
            frmCustomerPurchaseHistory historyForm = new frmCustomerPurchaseHistory(selectedCustomerID, txtCustomerName.Text);
            historyForm.ShowDialog();
        }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];
                selectedCustomerID = Convert.ToInt32(row.Cells["CustomerID"].Value);

                txtCustomerCode.Text = row.Cells["CustomerCode"].Value.ToString();
                txtCustomerName.Text = row.Cells["CustomerName"].Value.ToString();
                txtPhone.Text = row.Cells["PhoneNumber"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";

                if (row.Cells["DateOfBirth"].Value != DBNull.Value)
                    dtpDateOfBirth.Value = Convert.ToDateTime(row.Cells["DateOfBirth"].Value);

                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
                btnViewHistory.Enabled = true;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (dgvCustomers.DataSource != null)
            {
                (dgvCustomers.DataSource as DataTable).DefaultView.RowFilter =
                    $"CustomerCode LIKE '%{searchText}%' OR CustomerName LIKE '%{searchText}%' OR PhoneNumber LIKE '%{searchText}%'";
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtCustomerCode.Text))
            {
                MessageBox.Show("Please enter customer code!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerCode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Please enter customer name!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please enter phone number!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }

            // Validate phone format (Vietnamese:  10-11 digits, starts with 0)
            if (!Regex.IsMatch(txtPhone.Text, @"^0\d{9,10}$"))
            {
                MessageBox.Show("Invalid phone number format!  Must be 10-11 digits starting with 0.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }

            // Validate email if provided
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!Regex.IsMatch(txtEmail.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    MessageBox.Show("Invalid email format!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }

        private string GenerateCustomerCode()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT TOP 1 CustomerCode FROM Customers ORDER BY CustomerID DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string lastCode = result.ToString();
                        int number = int.Parse(lastCode.Substring(3)) + 1;
                        return "CUS" + number.ToString("D3");
                    }
                    else
                    {
                        return "CUS001";
                    }
                }
            }
            catch
            {
                return "CUS001";
            }
        }

        private void SetInitialState()
        {
            EnableFields(false);
            ClearFields();
            btnAdd.Enabled = true;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            btnViewHistory.Enabled = false;
        }

        private void EnableFields(bool enable)
        {
            txtCustomerCode.Enabled = enable;
            txtCustomerName.Enabled = enable;
            txtPhone.Enabled = enable;
            txtEmail.Enabled = enable;
            txtAddress.Enabled = enable;
            dtpDateOfBirth.Enabled = enable;
        }

        private void ClearFields()
        {
            txtCustomerCode.Clear();
            txtCustomerName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            dtpDateOfBirth.Value = DateTime.Now;
            selectedCustomerID = 0;
        }
    }
}