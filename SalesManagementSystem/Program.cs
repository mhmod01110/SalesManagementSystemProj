namespace SalesManagementSystem.UI
{
    using System;
    using System.Windows.Forms;
    using System.Drawing;
    using SalesManagementSystem.BLL;
    using SalesManagementSystem.DAL;

    // Main Application Entry Point
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}