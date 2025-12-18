using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace StoreX_SalesManagement
{
    public partial class frmCustomerPurchaseHistory : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;
        int customerID;
        string customerName;

        public frmCustomerPurchaseHistory(int customerId, string custName)
        {
            InitializeComponent();
            this.customerID = customerId;
            this.customerName = custName;
        }

        private void frmCustomerPurchaseHistory_Load(object sender, EventArgs e)
        {
            lblCustomerInfo.Text = $"Customer: {customerName}";
            LoadPurchaseHistory();
            LoadSummary();
        }

        private void LoadPurchaseHistory()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_GetCustomerPurchaseHistory", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvPurchaseHistory.DataSource = dt;

                    // Hide SaleID
                    if (dgvPurchaseHistory.Columns["SaleID"] != null)
                        dgvPurchaseHistory.Columns["SaleID"].Visible = false;

                    // Format currency columns
                    if (dgvPurchaseHistory.Columns["FinalAmount"] != null)
                    {
                        dgvPurchaseHistory.Columns["FinalAmount"].DefaultCellStyle.Format = "N0";
                        dgvPurchaseHistory.Columns["FinalAmount"].HeaderText = "Amount (VND)";
                    }

                    if (dgvPurchaseHistory.Columns["SaleDate"] != null)
                    {
                        dgvPurchaseHistory.Columns["SaleDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                        dgvPurchaseHistory.Columns["SaleDate"].HeaderText = "Purchase Date";
                    }

                    if (dgvPurchaseHistory.Columns["SoldBy"] != null)
                    {
                        dgvPurchaseHistory.Columns["SoldBy"].HeaderText = "Sold By";
                    }

                    if (dgvPurchaseHistory.Columns["TotalItems"] != null)
                    {
                        dgvPurchaseHistory.Columns["TotalItems"].HeaderText = "Items";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase history: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSummary()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                                    COUNT(*) as TotalPurchases,
                                    ISNULL(SUM(FinalAmount), 0) as TotalSpent,
                                    ISNULL(AVG(FinalAmount), 0) as AvgPurchaseValue,
                                    MAX(SaleDate) as LastPurchaseDate
                                    FROM Sales
                                    WHERE CustomerID = @CustomerID
                                    AND Status = 'Completed'";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        lblTotalPurchases.Text = reader["TotalPurchases"].ToString();

                        decimal totalSpent = Convert.ToDecimal(reader["TotalSpent"]);
                        lblTotalSpent.Text = totalSpent.ToString("N0") + " VND";

                        decimal avgValue = Convert.ToDecimal(reader["AvgPurchaseValue"]);
                        lblAvgPurchase.Text = avgValue.ToString("N0") + " VND";

                        if (reader["LastPurchaseDate"] != DBNull.Value)
                        {
                            DateTime lastDate = Convert.ToDateTime(reader["LastPurchaseDate"]);
                            lblLastPurchase.Text = lastDate.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            lblLastPurchase.Text = "No purchases yet";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading summary: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}