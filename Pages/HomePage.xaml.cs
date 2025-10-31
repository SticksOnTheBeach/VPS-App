using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VPSControl.Services;

namespace VPSControl.Pages
{
    public partial class HomePage : Page
    {
        private readonly DashboardPage _dashboard;
        private readonly SshService _ssh;

        public HomePage(DashboardPage dashboard)
        {
            InitializeComponent();
            _dashboard = dashboard;
            _ssh = MainWindow.SshService;
            UpdateStatus();
        }

        // ----------- Connexion SSH -----------
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ConnectDialog();
            if (dlg.ShowDialog() == true)
            {
                var host = dlg.Host;
                var user = dlg.User;
                var pass = dlg.Password;

                AppendTerminal($"Connecting to {user}@{host}...");
                var (success, message) = await _ssh.ConnectAsync(host, user, pass);
                AppendTerminal(message);
                UpdateStatus();
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            _ssh.Disconnect();
            AppendTerminal("Disconnected from server.");
            UpdateStatus();
        }

        // ----------- Envoi de commande -----------
        private async void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            var cmd = CommandBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(cmd) || CommandBox.Foreground == Brushes.Gray)
                return;

            AppendTerminal($"> {cmd}");
            var (output, error) = await _ssh.ExecuteCommandAsync(cmd);
            if (!string.IsNullOrEmpty(output))
                AppendTerminal(output);
            if (!string.IsNullOrEmpty(error))
                AppendTerminal($"ERROR: {error}");
        }

        private async void Reboot_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Type YES to confirm reboot.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                AppendTerminal("Rebooting...");
                var (output, error) = await _ssh.ExecuteCommandAsync("reboot");
                AppendTerminal(output + error);
            }
        }

        private async void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Type YES to confirm shutdown.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                AppendTerminal("Shutting down...");
                var (output, error) = await _ssh.ExecuteCommandAsync("shutdown -h now");
                AppendTerminal(output + error);
            }
        }

        // ----------- Terminal / UI -----------
        private void AppendTerminal(string text)
        {
            TerminalOutput.AppendText(text + Environment.NewLine + Environment.NewLine);
            TerminalOutput.ScrollToEnd();
        }

        private void UpdateStatus()
        {
            if (_ssh.IsConnected)
            {
                StatusText.Text = "ONLINE";
                StatusText.Foreground = Brushes.Green;
            }
            else
            {
                StatusText.Text = "OFFLINE";
                StatusText.Foreground = Brushes.Red;
            }
        }

        // ----------- Placeholder CommandBox -----------
        private void CommandBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CommandBox.Foreground == Brushes.Gray)
            {
                CommandBox.Text = "";
                CommandBox.Foreground = Brushes.Black;
            }
        }

        private void CommandBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommandBox.Text))
            {
                CommandBox.Text = "Enter command...";
                CommandBox.Foreground = Brushes.Gray;
            }
        }
    }
}
