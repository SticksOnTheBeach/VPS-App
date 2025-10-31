using System.Windows;
using System.Windows.Controls;

namespace VPSControl.Pages
{
    public partial class SettingsPage : Page
    {
        private readonly DashboardPage _dashboard;
        public SettingsPage(DashboardPage dashboard)
        {
            InitializeComponent();
            _dashboard = dashboard;

            // default values
            HostBox.Text = "82.165.195.234";
            UserBox.Text = "root";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // In this simplified version we just update the SshService defaults
            // The Connect dialog reads these if empty
            MessageBox.Show("Saved locally (in UI). For persistent settings, implement file storage.");
        }
    }
}