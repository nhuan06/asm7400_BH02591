using System;
using System.Windows.Forms;

namespace StoreX_SalesManagement
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Hiển thị thông tin user
            this.Text = $"Store X Sales Management - Welcome {SessionInfo.EmployeeName} ({SessionInfo.Position})";

            // Phân quyền menu theo role
            SetMenuPermissions();
        }

        private void SetMenuPermissions()
        {
            if (SessionInfo.IsAdmin())
            {
                // Admin: Full access
                mnuEmployeeManagement.Enabled = true;
                mnuPurchaseOrder.Enabled = true;
                mnuReports.Enabled = true;
            }
            else if (SessionInfo.IsSales())
            {
                // Sales:  Không được vào Employee Management, Purchase Order
                mnuEmployeeManagement.Enabled = false;
                mnuPurchaseOrder.Enabled = false;
                mnuReports.Enabled = false; // Hoặc giới hạn một số report
            }
            else if (SessionInfo.IsWarehouse())
            {
                // Warehouse:  Chỉ được Product và Purchase Order
                mnuEmployeeManagement.Enabled = false;
                mnuSales.Enabled = false;
                mnuCustomerManagement.Enabled = false;
                mnuReports.Enabled = false;
            }
        }

        // Menu Click Events
        private void mnuEmployeeManagement_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmEmployeeManagement());
        }

        private void mnuProductManagement_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmProductManagement());
        }

        private void mnuCustomerManagement_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmCustomerManagement());
        }

        private void mnuSales_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmSales());
        }

        private void mnuPurchaseOrder_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmPurchaseOrder());
        }

        private void mnuSalesReport_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmSalesReport());
        }

        private void mnuChangePassword_Click(object sender, EventArgs e)
        {
            frmChangePassword frm = new frmChangePassword();
            frm.ShowDialog();
        }

        private void mnuLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Clear session
                SessionInfo.EmployeeID = 0;
                SessionInfo.EmployeeName = string.Empty;

                this.Close();
                frmLogin loginForm = new frmLogin();
                loginForm.Show();
            }
        }

        // Helper method để mở child form trong MDI
        private void OpenChildForm(Form childForm)
        {
            // Đóng tất cả child forms đang mở
            foreach (Form frm in this.MdiChildren)
            {
                frm.Close();
            }

            // Mở form mới
            childForm.MdiParent = this;
            childForm.WindowState = FormWindowState.Maximized;
            childForm.Show();
        }
    }
}