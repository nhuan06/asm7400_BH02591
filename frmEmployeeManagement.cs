using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.Text.RegularExpressions;

namespace StoreX_SalesManagement
{
    public partial class frmEmployeeManagement : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;
        int selectedEmployeeID = 0;

        public frmEmployeeManagement()
        {
            InitializeComponent();
        }

        private void frmEmployeeManagement_Load(object sender, EventArgs e)
        {
            // Check permission - Only Admin can access
            if (!SessionInfo.IsAdmin())
            {
                MessageBox.Show("You don't have permission to access this feature!", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            LoadEmployees();
            SetInitialState();
        }

        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT EmployeeID, EmployeeCode, EmployeeName, Username, Position, 
                                    PhoneNumber, Email, Address,
                                    CASE WHEN IsFirstLogin = 1 THEN 'Yes' ELSE 'No' END as FirstLogin,
                                    CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Inactive' END as Status
                                    FROM Employees
                                    WHERE IsActive = 1
                                    ORDER BY EmployeeName";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvEmployees.DataSource = dt;
                    dgvEmployees.Columns["EmployeeID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employees: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            btnResetPassword.Enabled = false;
            selectedEmployeeID = 0;

            // Generate employee code
            txtEmployeeCode.Text = GenerateEmployeeCode();
            cboPosition.SelectedIndex = 1; // Default:  Sales
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedEmployeeID == 0)
            {
                MessageBox.Show("Please select an employee to edit!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Cannot edit yourself
            if (selectedEmployeeID == SessionInfo.EmployeeID)
            {
                MessageBox.Show("You cannot edit your own account!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EnableFields(true);
            txtEmployeeCode.Enabled = false;
            txtUsername.Enabled = false; // Cannot change username
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnResetPassword.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd;

                    if (selectedEmployeeID == 0) // Add new
                    {
                        // Check if username exists
                        string checkQuery = "SELECT COUNT(*) FROM Employees WHERE Username = @Username";
                        SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                        checkCmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Username already exists!", "Validation",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtUsername.Focus();
                            return;
                        }

                        // Generate salt and hash password (default:  Password123)
                        string salt = Guid.NewGuid().ToString();
                        string defaultPassword = "Password123";
                        string passwordHash = ComputeHash(defaultPassword + salt);

                        string insertQuery = @"INSERT INTO Employees (EmployeeCode, EmployeeName, Position, Username, 
                                              PasswordHash, Salt, PhoneNumber, Email, Address, IsFirstLogin, IsActive)
                                              VALUES (@EmployeeCode, @EmployeeName, @Position, @Username, 
                                              @PasswordHash, @Salt, @PhoneNumber, @Email, @Address, 1, 1)";

                        cmd = new SqlCommand(insertQuery, conn);
                        cmd.Parameters.AddWithValue("@EmployeeCode", txtEmployeeCode.Text.Trim());
                        cmd.Parameters.AddWithValue("@EmployeeName", txtEmployeeName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Position", cboPosition.Text);
                        cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@Salt", salt);
                        cmd.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text.Trim());

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Employee created successfully!\nDefault password: Password123\nUser must change password on first login.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else // Update
                    {
                        string updateQuery = @"UPDATE Employees SET EmployeeName = @EmployeeName, Position = @Position,
                                              PhoneNumber = @PhoneNumber, Email = @Email, Address = @Address, 
                                              ModifiedDate = GETDATE()
                                              WHERE EmployeeID = @EmployeeID";

                        cmd = new SqlCommand(updateQuery, conn);
                        cmd.Parameters.AddWithValue("@EmployeeName", txtEmployeeName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Position", cboPosition.Text);
                        cmd.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text.Trim());
                        cmd.Parameters.AddWithValue("@EmployeeID", selectedEmployeeID);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Employee updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    LoadEmployees();
                    SetInitialState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving employee: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedEmployeeID == 0)
            {
                MessageBox.Show("Please select an employee to delete!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Cannot delete yourself
            if (selectedEmployeeID == SessionInfo.EmployeeID)
            {
                MessageBox.Show("You cannot delete your own account!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this employee?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        // Soft delete
                        string query = "UPDATE Employees SET IsActive = 0, ModifiedDate = GETDATE() WHERE EmployeeID = @EmployeeID";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@EmployeeID", selectedEmployeeID);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Employee deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadEmployees();
                        SetInitialState();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting employee: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (selectedEmployeeID == 0)
            {
                MessageBox.Show("Please select an employee to reset password!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Reset password to default (Password123)?",
                "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string salt = Guid.NewGuid().ToString();
                        string defaultPassword = "Password123";
                        string passwordHash = ComputeHash(defaultPassword + salt);

                        string query = @"UPDATE Employees SET PasswordHash = @PasswordHash, Salt = @Salt, 
                                        IsFirstLogin = 1, ModifiedDate = GETDATE() 
                                        WHERE EmployeeID = @EmployeeID";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@Salt", salt);
                        cmd.Parameters.AddWithValue("@EmployeeID", selectedEmployeeID);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Password reset successfully!\nNew password: Password123", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error resetting password: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SetInitialState();
        }

        private void dgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvEmployees.Rows[e.RowIndex];
                selectedEmployeeID = Convert.ToInt32(row.Cells["EmployeeID"].Value);

                txtEmployeeCode.Text = row.Cells["EmployeeCode"].Value.ToString();
                txtEmployeeName.Text = row.Cells["EmployeeName"].Value.ToString();
                txtUsername.Text = row.Cells["Username"].Value.ToString();
                cboPosition.Text = row.Cells["Position"].Value.ToString();
                txtPhone.Text = row.Cells["PhoneNumber"].Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";

                btnEdit.Enabled = true;
                btnDelete.Enabled = (selectedEmployeeID != SessionInfo.EmployeeID);
                btnResetPassword.Enabled = true;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (dgvEmployees.DataSource != null)
            {
                (dgvEmployees.DataSource as DataTable).DefaultView.RowFilter =
                    $"EmployeeCode LIKE '%{searchText}%' OR EmployeeName LIKE '%{searchText}%' OR Username LIKE '%{searchText}%'";
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtEmployeeCode.Text))
            {
                MessageBox.Show("Please enter employee code!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmployeeCode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmployeeName.Text))
            {
                MessageBox.Show("Please enter employee name!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmployeeName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter username!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            if (txtUsername.Text.Length < 5)
            {
                MessageBox.Show("Username must be at least 5 characters!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            if (cboPosition.SelectedIndex == -1)
            {
                MessageBox.Show("Please select position!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboPosition.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please enter phone number!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }

            if (!Regex.IsMatch(txtPhone.Text, @"^0\d{9,10}$"))
            {
                MessageBox.Show("Invalid phone number format!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!Regex.IsMatch(txtEmail.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    MessageBox.Show("Invalid email format!", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            return true;
        }

        private string GenerateEmployeeCode()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT TOP 1 EmployeeCode FROM Employees ORDER BY EmployeeID DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string lastCode = result.ToString();
                        int number = int.Parse(lastCode.Substring(3)) + 1;
                        return "EMP" + number.ToString("D3");
                    }
                    else
                    {
                        return "EMP001";
                    }
                }
            }
            catch
            {
                return "EMP001";
            }
        }

        private string ComputeHash(string input)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
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
            btnResetPassword.Enabled = false;
        }

        private void EnableFields(bool enable)
        {
            txtEmployeeCode.Enabled = enable;
            txtEmployeeName.Enabled = enable;
            txtUsername.Enabled = enable;
            cboPosition.Enabled = enable;
            txtPhone.Enabled = enable;
            txtEmail.Enabled = enable;
            txtAddress.Enabled = enable;
        }

        private void ClearFields()
        {
            txtEmployeeCode.Clear();
            txtEmployeeName.Clear();
            txtUsername.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            cboPosition.SelectedIndex = -1;
            selectedEmployeeID = 0;
        }
    }
}