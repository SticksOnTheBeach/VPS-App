using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VPSControl.Services
{
    public class SshService
    {
        private SshClient? _client;
        private SftpClient? _sftp;
        public string CurrentDirectory { get; set; } = "~";

        public bool IsConnected => _client != null && _client.IsConnected;

        public Task<(bool success, string message)> ConnectAsync(string host, string username, string password, int port = 22)
        {
            return Task.Run(() =>
            {
                try
                {
                    Disconnect();
                    _client = new SshClient(host, port, username, password);
                    _client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                    _client.Connect();
                    _sftp = new SftpClient(host, port, username, password);
                    _sftp.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                    _sftp.Connect();
                    // get pwd
                    var cmd = _client.RunCommand("pwd");
                    CurrentDirectory = string.IsNullOrWhiteSpace(cmd.Result) ? "~" : cmd.Result.Trim();
                    return (true, "Connected");
                }
                catch (Exception ex)
                {
                    Disconnect();
                    return (false, ex.Message);
                }
            });
        }

        public void Disconnect()
        {
            try { _sftp?.Dispose(); } catch { }
            try { _client?.Disconnect(); _client?.Dispose(); } catch { }
            _client = null;
            _sftp = null;
            CurrentDirectory = "~";
        }

        public Task<(string output, string error)> ExecuteCommandAsync(string command)
        {
            return Task.Run(() =>
            {
                if (_client == null || !_client.IsConnected) return ("", "Not connected");
                try
                {
                    var cmd = _client.RunCommand($"cd '{CurrentDirectory}' && {command}");
                    // update current dir if needed
                    if (command.StartsWith("cd "))
                    {
                        var pwd = _client.RunCommand($"cd '{CurrentDirectory}' && pwd");
                        CurrentDirectory = pwd.Result.Trim();
                    }
                    return (cmd.Result, cmd.Error);
                }
                catch (Exception ex)
                {
                    return ("", ex.Message);
                }
            });
        }

        public IEnumerable<Renci.SshNet.Sftp.SftpFile>? ListDirectory(string remotePath)
        {
            if (_sftp == null || !_sftp.IsConnected) return null;
            try
            {
                return _sftp.ListDirectory(remotePath).OfType<Renci.SshNet.Sftp.SftpFile>();
            }
            catch { return null; }
        }


        public Task<(bool ok, string msg)> DownloadFileAsync(string remotePath, string localPath, IProgress<double>? progress = null)
        {
            return Task.Run(() =>
            {
                if (_sftp == null || !_sftp.IsConnected) return (false, "Not connected");
                try
                {
                    using var fs = File.OpenWrite(localPath);
                    _sftp.DownloadFile(remotePath, fs, (ul) =>
                    {
                        if (progress != null && _sftp.GetAttributes(remotePath).Size > 0)
                        {
                            progress.Report((double)ul / _sftp.GetAttributes(remotePath).Size);
                        }
                    });
                    return (true, "Downloaded");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            });
        }

        public Task<(bool ok, string msg)> UploadFileAsync(string localPath, string remotePath, IProgress<double>? progress = null)
        {
            return Task.Run(() =>
            {
                if (_sftp == null || !_sftp.IsConnected) return (false, "Not connected");
                try
                {
                    using var fs = File.OpenRead(localPath);
                    _sftp.UploadFile(fs, remotePath, (ul) =>
                    {
                        if (progress != null && fs.Length > 0)
                        {
                            progress.Report((double)ul / fs.Length);
                        }
                    });
                    return (true, "Uploaded");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            });
        }

        public bool EnsureConnectedForSftp() => _sftp != null && _sftp.IsConnected;
    }
}
