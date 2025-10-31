using System;
using System.Windows;
using System.Windows.Controls;

namespace VPSControl.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly Action<string> _navigateParent;

        public DashboardPage(Action<string> navigateParent)
        {
            InitializeComponent();
            _navigateParent = navigateParent;
            ContentFrame.Content = new HomePage(this);
        }

        private void ShowHome(object sender, RoutedEventArgs e) => ContentFrame.Content = new HomePage(this);
        private void ShowBackup(object sender, RoutedEventArgs e) => ContentFrame.Content = new BackupPage(this);
        private void ShowFTP(object sender, RoutedEventArgs e) => ContentFrame.Content = new FTPPage(this);
        private void ShowSettings(object sender, RoutedEventArgs e) => ContentFrame.Content = new SettingsPage(this);

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // disconnect if connected
            MainWindow.SshService.Disconnect();
            _navigateParent("Login");
        }
    }
}