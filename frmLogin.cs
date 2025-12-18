using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace StoreX_SalesManagement
{
    public partial class frmLogin : Form
    {
        // Biến lưu connection string
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;

        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter username!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter password!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // Gọi Stored Procedure để login
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_AuthenticateUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Input parameters
                        cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text);

                        // Output parameters
                        SqlParameter pResult = new SqlParameter("@Result", SqlDbType.Int);
                        pResult.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pResult);

                        SqlParameter pEmployeeID = new SqlParameter("@EmployeeID", SqlDbType.Int);
                        pEmployeeID.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pEmployeeID);

                        SqlParameter pEmployeeName = new SqlParameter("@EmployeeName", SqlDbType.NVarChar, 100);
                        pEmployeeName.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pEmployeeName);

                        SqlParameter pPosition = new SqlParameter("@Position", SqlDbType.NVarChar, 50);
                        pPosition.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pPosition);

                        SqlParameter pIsFirstLogin = new SqlParameter("@IsFirstLogin", SqlDbType.Bit);
                        pIsFirstLogin.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pIsFirstLogin);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        int result = Convert.ToInt32(pResult.Value);

                        if (result == 1)
                        {
                            // Lưu thông tin session vào static class
                            SessionInfo.EmployeeID = Convert.ToInt32(pEmployeeID.Value);
                            SessionInfo.EmployeeName = pEmployeeName.Value.ToString();
                            SessionInfo.Position = pPosition.Value.ToString();
                            SessionInfo.Username = txtUsername.Text.Trim();
                            SessionInfo.IsFirstLogin = Convert.ToBoolean(pIsFirstLogin.Value);

                            MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Kiểm tra nếu lần đầu login → bắt đổi mật khẩu
                            if (SessionInfo.IsFirstLogin)
                            {
                                frmChangePassword frmChange = new frmChangePassword();
                                frmChange.ShowDialog();
                            }

                            // Mở Main form
                            this.Hide();
                            frmMain mainForm = new frmMain();
                            mainForm.ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password!", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtPassword.Clear();
                            txtUsername.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            // Test connection
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot connect to database!\n" + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}