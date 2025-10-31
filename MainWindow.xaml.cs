using System.Windows;
using VPSControl.Pages;
using VPSControl.Services;

namespace VPSControl
{
    public partial class MainWindow : Window
    {
        public static Database Database { get; private set; }
        public static SshService SshService { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            // Init services
            Database = new Database();
            SshService = new SshService();

            // Ensure DB
            Database.Init();

            // Navigate to login
            MainFrame.Content = new LoginPage(GoToPage);
        }

        // simple navigation callback
        private void GoToPage(string pageName)
        {
            switch (pageName)
            {
                case "Signup":
                    MainFrame.Content = new SignupPage(GoToPage);
                    break;
                case "Dashboard":
                    MainFrame.Content = new DashboardPage(GoToPage);
                    break;
                case "Login":
                default:
                    MainFrame.Content = new LoginPage(GoToPage);
                    break;
            }
        }
    }
}