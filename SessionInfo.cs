using System;

namespace StoreX_SalesManagement
{
    /// <summary>
    /// Lưu thông tin session của user đang đăng nhập
    /// </summary>
    public static class SessionInfo
    {
        public static int EmployeeID { get; set; }
        public static string EmployeeName { get; set; }
        public static string Username { get; set; }
        public static string Position { get; set; }
        public static bool IsFirstLogin { get; set; }
        public static DateTime LoginTime { get; set; }

        public static bool IsAdmin()
        {
            return Position == "Admin";
        }

        public static bool IsSales()
        {
            return Position == "Sales";
        }

        public static bool IsWarehouse()
        {
            return Position == "Warehouse";
        }
    }
}