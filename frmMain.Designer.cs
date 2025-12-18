namespace StoreX_SalesManagement
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuEmployeeManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProductManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCustomerManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSales = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPurchaseOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReports = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSalesReport = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProfitReport = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuInventoryReport = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuChangePassword = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblUserInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDateTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.menuStrip1.Font = new System.Drawing.Font("Arial", 11F);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEmployeeManagement,
            this.mnuProductManagement,
            this.mnuCustomerManagement,
            this.mnuSales,
            this.mnuPurchaseOrder,
            this.mnuReports,
            this.mnuSettings,
            this.mnuLogout});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1200, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuEmployeeManagement
            // 
            this.mnuEmployeeManagement.ForeColor = System.Drawing.Color.White;
            this.mnuEmployeeManagement.Name = "mnuEmployeeManagement";
            this.mnuEmployeeManagement.Size = new System.Drawing.Size(180, 21);
            this.mnuEmployeeManagement.Text = "Employee Management";
            this.mnuEmployeeManagement.Click += new System.EventHandler(this.mnuEmployeeManagement_Click);
            // 
            // mnuProductManagement
            // 
            this.mnuProductManagement.ForeColor = System.Drawing.Color.White;
            this.mnuProductManagement.Name = "mnuProductManagement";
            this.mnuProductManagement.Size = new System.Drawing.Size(168, 21);
            this.mnuProductManagement.Text = "Product Management";
            this.mnuProductManagement.Click += new System.EventHandler(this.mnuProductManagement_Click);
            // 
            // mnuCustomerManagement
            // 
            this.mnuCustomerManagement.ForeColor = System.Drawing.Color.White;
            this.mnuCustomerManagement.Name = "mnuCustomerManagement";
            this.mnuCustomerManagement.Size = new System.Drawing.Size(181, 21);
            this.mnuCustomerManagement.Text = "Customer Management";
            this.mnuCustomerManagement.Click += new System.EventHandler(this.mnuCustomerManagement_Click);
            // 
            // mnuSales
            // 
            this.mnuSales.ForeColor = System.Drawing.Color.White;
            this.mnuSales.Name = "mnuSales";
            this.mnuSales.Size = new System.Drawing.Size(56, 21);
            this.mnuSales.Text = "Sales";
            this.mnuSales.Click += new System.EventHandler(this.mnuSales_Click);
            // 
            // mnuPurchaseOrder
            // 
            this.mnuPurchaseOrder.ForeColor = System.Drawing.Color.White;
            this.mnuPurchaseOrder.Name = "mnuPurchaseOrder";
            this.mnuPurchaseOrder.Size = new System.Drawing.Size(126, 21);
            this.mnuPurchaseOrder.Text = "Purchase Order";
            this.mnuPurchaseOrder.Click += new System.EventHandler(this.mnuPurchaseOrder_Click);
            // 
            // mnuReports
            // 
            this.mnuReports.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSalesReport,
            this.mnuProfitReport,
            this.mnuInventoryReport});
            this.mnuReports.ForeColor = System.Drawing.Color.White;
            this.mnuReports.Name = "mnuReports";
            this.mnuReports.Size = new System.Drawing.Size(72, 21);
            this.mnuReports.Text = "Reports";
            // 
            // mnuSalesReport
            // 
            this.mnuSalesReport.Name = "mnuSalesReport";
            this.mnuSalesReport.Size = new System.Drawing.Size(180, 22);
            this.mnuSalesReport.Text = "Sales Report";
            this.mnuSalesReport.Click += new System.EventHandler(this.mnuSalesReport_Click);
            // 
            // mnuProfitReport
            // 
            this.mnuProfitReport.Name = "mnuProfitReport";
            this.mnuProfitReport.Size = new System.Drawing.Size(180, 22);
            this.mnuProfitReport.Text = "Profit Report";
            // 
            // mnuInventoryReport
            // 
            this.mnuInventoryReport.Name = "mnuInventoryReport";
            this.mnuInventoryReport.Size = new System.Drawing.Size(180, 22);
            this.mnuInventoryReport.Text = "Inventory Report";
            // 
            // mnuSettings
            // 
            this.mnuSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuChangePassword});
            this.mnuSettings.ForeColor = System.Drawing.Color.White;
            this.mnuSettings.Name = "mnuSettings";
            this.mnuSettings.Size = new System.Drawing.Size(75, 21);
            this.mnuSettings.Text = "Settings";
            // 
            // mnuChangePassword
            // 
            this.mnuChangePassword.Name = "mnuChangePassword";
            this.mnuChangePassword.Size = new System.Drawing.Size(195, 22);
            this.mnuChangePassword.Text = "Change Password";
            this.mnuChangePassword.Click += new System.EventHandler(this.mnuChangePassword_Click);
            // 
            // mnuLogout
            // 
            this.mnuLogout.ForeColor = System.Drawing.Color.White;
            this.mnuLogout.Name = "mnuLogout";
            this.mnuLogout.Size = new System.Drawing.Size(67, 21);
            this.mnuLogout.Text = "Logout";
            this.mnuLogout.Click += new System.EventHandler(this.mnuLogout_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lblUserInfo,
            this.toolStripStatusLabel2,
            this.lblDateTime});
            this.statusStrip1.Location = new System.Drawing.Point(0, 639);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(36, 17);
            this.toolStripStatusLabel1.Text = "User:";
            // 
            // lblUserInfo
            // 
            this.lblUserInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUserInfo.Name = "lblUserInfo";
            this.lblUserInfo.Size = new System.Drawing.Size(59, 17);
            this.lblUserInfo.Text = "Unknown";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(1015, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // lblDateTime
            // 
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(75, 17);
            this.lblDateTime.Text = "2025-12-10";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 661);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Store X - Sales Management System";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuEmployeeManagement;
        private System.Windows.Forms.ToolStripMenuItem mnuProductManagement;
        private System.Windows.Forms.ToolStripMenuItem mnuCustomerManagement;
        private System.Windows.Forms.ToolStripMenuItem mnuSales;
        private System.Windows.Forms.ToolStripMenuItem mnuPurchaseOrder;
        private System.Windows.Forms.ToolStripMenuItem mnuReports;
        private System.Windows.Forms.ToolStripMenuItem mnuSalesReport;
        private System.Windows.Forms.ToolStripMenuItem mnuProfitReport;
        private System.Windows.Forms.ToolStripMenuItem mnuInventoryReport;
        private System.Windows.Forms.ToolStripMenuItem mnuSettings;
        private System.Windows.Forms.ToolStripMenuItem mnuChangePassword;
        private System.Windows.Forms.ToolStripMenuItem mnuLogout;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel lblUserInfo;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel lblDateTime;
    }
}