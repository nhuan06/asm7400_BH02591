using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace StoreX_SalesManagement
{
    public partial class frmChangePassword : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;

        public frmChangePassword()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtOldPassword.Text))
            {
                MessageBox.Show("Please enter current password!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Please enter new password!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNewPassword.Text.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gọi SP để đổi mật khẩu
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpdatePassword", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@EmployeeID", SessionInfo.EmployeeID);
                        cmd.Parameters.AddWithValue("@OldPassword", txtOldPassword.Text);
                        cmd.Parameters.AddWithValue("@NewPassword", txtNewPassword.Text);

                        SqlParameter pResult = new SqlParameter("@Result", SqlDbType.Int);
                        pResult.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pResult);

                        SqlParameter pMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 200);
                        pMessage.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pMessage);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        int result = Convert.ToInt32(pResult.Value);
                        string message = pMessage.Value.ToString();

                        if (result == 1)
                        {
                            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            SessionInfo.IsFirstLogin = false;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (SessionInfo.IsFirstLogin)
            {
                MessageBox.Show("You must change your password before continuing!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.Close();
        }

        private void txtNewPassword_TextChanged(object sender, EventArgs e)
        {
            // Hiển thị độ mạnh mật khẩu
            string password = txtNewPassword.Text;

            if (password.Length < 8)
                lblStrength.Text = "Weak";
            else if (HasUpperLower(password) && HasDigit(password))
                lblStrength.Text = "Strong";
            else
                lblStrength.Text = "Medium";
        }

        private bool HasUpperLower(string text)
        {
            bool hasUpper = false, hasLower = false;
            foreach (char c in text)
            {
                if (char.IsUpper(c)) hasUpper = true;
                if (char.IsLower(c)) hasLower = true;
            }
            return hasUpper && hasLower;
        }

        private bool HasDigit(string text)
        {
            foreach (char c in text)
            {
                if (char.IsDigit(c)) return true;
            }
            return false;
        }
    }
}