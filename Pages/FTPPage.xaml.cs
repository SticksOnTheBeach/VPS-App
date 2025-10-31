using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VPSControl.Services;

namespace VPSControl.Pages
{
    public partial class FTPPage : Page
    {
        private readonly DashboardPage _dashboard;
        private readonly SshService _ssh;
        private string _localPath;

        public FTPPage(DashboardPage dashboard)
        {
            InitializeComponent();
            _dashboard = dashboard;
            _ssh = MainWindow.SshService;
            _localPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            RefreshLocal();
            RefreshRemote();
        }

        private void RefreshLocal()
        {
            LocalList.Items.Clear();
            try
            {
                var dirs = Directory.GetDirectories(_localPath).Select(d => new FileInfo(d).Name);
                var files = Directory.GetFiles(_localPath).Select(f => Path.GetFileName(f));
                foreach (var d in dirs) LocalList.Items.Add("[DIR] " + d);
                foreach (var f in files) LocalList.Items.Add(f);
            }
            catch (Exception ex) { LocalList.Items.Add("Error: " + ex.Message); }
        }

        private void RefreshRemote()
        {
            RemoteList.Items.Clear();
            if (!_ssh.IsConnected) { RemoteList.Items.Add("Not connected"); return; }
            var list = _ssh.ListDirectory(_ssh.CurrentDirectory);
            if (list == null) { RemoteList.Items.Add("Error listing"); return; }
            foreach (var item in list)
            {
                if (item.Name == "." || item.Name == "..") continue;
                RemoteList.Items.Add((item.IsDirectory ? "[DIR] " : "") + item.Name);
            }
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                var local = dlg.FileName;
                var remote = $"{_ssh.CurrentDirectory}/{Path.GetFileName(local)}".Replace("\\", "/");
                var progress = new Progress<double>(p => { /* could update UI */ });
                var (ok, msg) = await _ssh.UploadFileAsync(local, remote, progress);
                MessageBox.Show(msg);
                RefreshRemote();
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (RemoteList.SelectedItem == null) return;
            var name = RemoteList.SelectedItem.ToString() ?? "";
            if (name.StartsWith("[DIR]")) { MessageBox.Show("Directory download not implemented"); return; }
            var remote = $"{_ssh.CurrentDirectory}/{name}".Replace("\\", "/");
            var dlg = new SaveFileDialog { FileName = name };
            if (dlg.ShowDialog() == true)
            {
                var (ok, msg) = await _ssh.DownloadFileAsync(remote, dlg.FileName);
                MessageBox.Show(msg);
                RefreshLocal();
            }
        }
    }
}
