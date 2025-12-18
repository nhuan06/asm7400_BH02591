using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace StoreX_SalesManagement
{
    public partial class frmSalesReport : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["StoreXDB"].ConnectionString;

        public frmSalesReport()
        {
            InitializeComponent();
        }

        private void frmSalesReport_Load(object sender, EventArgs e)
        {
            // Set default date range (current month)
            dtpStartDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpEndDate.Value = DateTime.Now;

            LoadEmployees();
            LoadReport();
        }

        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 0 as EmployeeID, 'All Employees' as EmployeeName
                                    UNION
                                    SELECT EmployeeID, EmployeeCode + ' - ' + EmployeeName as EmployeeName 
                                    FROM Employees 
                                    WHERE Position = 'Sales' AND IsActive = 1
                                    ORDER BY EmployeeID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cboEmployee.DataSource = dt;
                    cboEmployee.DisplayMember = "EmployeeName";
                    cboEmployee.ValueMember = "EmployeeID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employees: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_GetSalesReport", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@StartDate", dtpStartDate.Value.Date);
                    cmd.Parameters.AddWithValue("@EndDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));

                    int employeeID = Convert.ToInt32(cboEmployee.SelectedValue);
                    if (employeeID > 0)
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                    else
                        cmd.Parameters.AddWithValue("@EmployeeID", DBNull.Value);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvSalesReport.DataSource = dt;

                    // Hide some columns
                    if (dgvSalesReport.Columns["SaleID"] != null)
                        dgvSalesReport.Columns["SaleID"].Visible = false;

                    // Format currency columns
                    FormatCurrencyColumns();

                    // Calculate summary
                    CalculateSummary(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatCurrencyColumns()
        {
            string[] currencyColumns = { "TotalAmount", "DiscountAmount", "FinalAmount", "Profit" };

            foreach (string columnName in currencyColumns)
            {
                if (dgvSalesReport.Columns[columnName] != null)
                {
                    dgvSalesReport.Columns[columnName].DefaultCellStyle.Format = "N0";
                    dgvSalesReport.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private void CalculateSummary(DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                lblTotalSales.Text = "0";
                lblTotalRevenue.Text = "0 VND";
                lblTotalProfit.Text = "0 VND";
                return;
            }

            int totalSales = dt.Rows.Count;
            decimal totalRevenue = 0;
            decimal totalProfit = 0;

            foreach (DataRow row in dt.Rows)
            {
                totalRevenue += Convert.ToDecimal(row["FinalAmount"]);
                if (row["Profit"] != DBNull.Value)
                    totalProfit += Convert.ToDecimal(row["Profit"]);
            }

            lblTotalSales.Text = totalSales.ToString();
            lblTotalRevenue.Text = totalRevenue.ToString("N0") + " VND";
            lblTotalProfit.Text = totalProfit.ToString("N0") + " VND";
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            // Export to Excel (basic implementation)
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveDialog.FileName = $"SalesReport_{DateTime.Now:yyyyMMdd}.csv";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToCSV(saveDialog.FileName);
                    MessageBox.Show("Report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(string filePath)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Header
            for (int i = 0; i < dgvSalesReport.Columns.Count; i++)
            {
                if (dgvSalesReport.Columns[i].Visible)
                {
                    sb.Append(dgvSalesReport.Columns[i].HeaderText + ",");
                }
            }
            sb.AppendLine();

            // Data
            foreach (DataGridViewRow row in dgvSalesReport.Rows)
            {
                for (int i = 0; i < dgvSalesReport.Columns.Count; i++)
                {
                    if (dgvSalesReport.Columns[i].Visible)
                    {
                        sb.Append(row.Cells[i].Value?.ToString() + ",");
                    }
                }
                sb.AppendLine();
            }

            System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
        }
    }
}