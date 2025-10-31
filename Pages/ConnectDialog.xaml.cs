using System.Windows;

namespace VPSControl.Pages
{
    public partial class ConnectDialog : Window
    {
        public string Host => HostBox.Text;
        public string User => UserBox.Text;
        public string Password => PasswordBox.Password;

        public ConnectDialog()
        {
            InitializeComponent();
            // defaults (from settings)
            HostBox.Text = "82.165.195.234";
            UserBox.Text = "root";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
        private void Connect_Click(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }
    }
}