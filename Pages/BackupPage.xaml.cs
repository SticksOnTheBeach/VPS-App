using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VPSControl.Services;

namespace VPSControl.Pages
{
    public partial class BackupPage : Page
    {
        private readonly DashboardPage _dashboard;
        private readonly SshService _ssh;

        public BackupPage(DashboardPage dashboard)
        {
            InitializeComponent();
            _dashboard = dashboard;
            _ssh = MainWindow.SshService;
            DestCombo.SelectedIndex = 0;
        }

        // ---- Gestion du placeholder (simule PlaceholderText) ----
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb.Foreground == Brushes.Gray)
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                // Remet le texte selon le champ
                if (tb == SourceBox)
                    tb.Text = "/var/www or /etc";
                else if (tb == BackupDirBox)
                    tb.Text = "/var/backups";

                tb.Foreground = Brushes.Gray;
            }
        }

        // ---- Création d'une sauvegarde ----
        private async void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            var source = SourceBox.Text.Trim();
            var backupDir = BackupDirBox.Text.Trim();

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(backupDir))
            {
                MessageBox.Show("Source and backup directory are required.");
                return;
            }

            // Nom du fichier
            var baseName = Path.GetFileName(source.TrimEnd('/')).Replace(".", "_");
            var filename = $"backup_{baseName}_{DateTime.Now:yyyyMMdd-HHmmss}.tar.gz";
            var remotePath = $"{backupDir.TrimEnd('/')}/{filename}";

            // Commande distante
            var cmd = $"tar -czvf '{remotePath}' -C '{(source == "/" ? "/" : Path.GetDirectoryName(source) ?? "/")}' '{(source == "/" ? "." : Path.GetFileName(source))}'";

            var (outp, err) = await _ssh.ExecuteCommandAsync(cmd);

            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show($"Backup error: {err}");
            }
            else
            {
                MessageBox.Show("Backup created: " + remotePath);
                BackupsList.Items.Add(remotePath);

                // Téléchargement si choix "Local"
                if (DestCombo.SelectedIndex == 1)
                {
                    var dlg = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = filename,
                        DefaultExt = ".tar.gz"
                    };

                    if (dlg.ShowDialog() == true)
                    {
                        var (ok, msg) = await _ssh.DownloadFileAsync(remotePath, dlg.FileName);
                        MessageBox.Show(msg);
                    }
                }
            }
        }
    }
}
