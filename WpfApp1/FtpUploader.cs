using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class FtpUploader
    {
        #region const

        private const string 
            FTP_CMD_USER = "USER ",
            FTP_CMD_PASS = "PASS ",
            FTP_CMD_QUIT = "QUIT",
            FTP_CMD_STOR = "STOR ",
            FTP_CMD_RETR = "RETR",
            FTP_CMD_PORT = "PORT",
            FTP_CMD_ABORT = "ABOR",
            FTP_CMD_PWD = "PWD ",
            FTP_CMD_CWD = "CWD ",
            FTP_CMD_TYPE = "TYPE",
            FTP_CMD_RNFR = "RNFR",
            FTP_CMD_RNTO = "RNTO",
            FTP_CMD_DELE = "DELE",
            FTP_CMD_PASV = "PASV ",
            FTP_CMD_LIST = "LIST",
            FTP_CMD_NLST = "NLST",
            FTP_CMD_HELP = "HELP",
            FTP_CMD_STAT = "STAT",
            FTP_CMD_MKD = "MKD ",
            FTP_CMD_NOOP = "NOOP";

        private const int
            FTP_RESP_BANNER = 220,
            FTP_RESP_USER_OK = 331,
            FTP_RESP_PASS_OK = 230,
            FTP_RESP_DIR_CREATED = 257,
            FTP_RESP_DIR_EXISTS = 550,
            FTP_RESP_PASV_OK = 227;

        #endregion

        #region private

        private string _host, _userName, _password;
        private int _port;
        private TcpClient _client;

        #endregion

        #region properties

        public string LastError { get; private set; }

        #endregion

        #region private methods

        private void CleanUp()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        private async Task<bool> LoginAsync()
        {
            try
            {
                if (IsNotLogged(_client))
                    CleanUp();
                else
                    return true;

                if (_client == null)
                    _client = new TcpClient(AddressFamily.InterNetwork);

                var hostEntry = await Dns.GetHostEntryAsync(_host);

                await _client.ConnectAsync(hostEntry.AddressList[0], _port);

                var code = await ReceiveCodeAsync();
                if (code != FTP_RESP_BANNER)
                    return false;

                await SendCommandAsync(FTP_CMD_USER + _userName);
                code = await ReceiveCodeAsync();
                if (code != FTP_RESP_USER_OK)
                    return false;

                await SendCommandAsync(FTP_CMD_PASS + _password);
                code = await ReceiveCodeAsync();
                if (code != FTP_RESP_PASS_OK)
                    return false;

                return true;

            }
            catch (Exception e)
            {
                LastError = e.Message;
                return false;
            }
        }

        private async Task<TcpClient> CreateLoginAsync()
        {
            try
            {
                var client = new TcpClient(AddressFamily.InterNetwork);

                var hostEntry = await Dns.GetHostEntryAsync(_host);

                await client.ConnectAsync(hostEntry.AddressList[0], _port);

                var code = await ReceiveCodeAsync(client);
                if (code != FTP_RESP_BANNER)
                    return null;

                await SendCommandAsync(client, FTP_CMD_USER + _userName);
                code = await ReceiveCodeAsync(client);
                if (code != FTP_RESP_USER_OK)
                    return null;

                await SendCommandAsync(client, FTP_CMD_PASS + _password);
                code = await ReceiveCodeAsync(client);
                if (code != FTP_RESP_PASS_OK)
                    return null;

                return client;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return null;
            }
        }

        private bool IsNotLogged(TcpClient client)
        {
            return _client == null || _client.Connected == false;
        }

        private async Task<TcpClient> MakeDataClient()
        {
            try
            {
                var client = new TcpClient(AddressFamily.InterNetwork);

                var hostEntry = await Dns.GetHostEntryAsync(_host);

                await _client.ConnectAsync(hostEntry.AddressList[0], _port);

                var code = await ReceiveCodeAsync();
                if (code != FTP_RESP_BANNER)
                    return null;

                await SendCommandAsync(FTP_CMD_USER + _userName);
                code = await ReceiveCodeAsync();
                if (code != FTP_RESP_USER_OK)
                    return null;

                await SendCommandAsync(FTP_CMD_PASS + _password);
                code = await ReceiveCodeAsync();
                if (code != FTP_RESP_PASS_OK)
                    return null;

                return client;

            }
            catch (Exception e)
            {
                LastError = e.Message;
                return null;
            }
        }

        private async Task<bool> SendCommandAsync(TcpClient client, string cmd)
        {
            try
            {
                var buf = Encoding.ASCII.GetBytes(cmd + "\r\n");
                await client.GetStream().WriteAsync(buf, 0, buf.Length);
                return true;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return false;
            }
        }

        private async Task<bool> SendCommandAsync(string cmd)
        {
            return await SendCommandAsync(_client, cmd);
        }

        private async Task<string> ReceiveAsync(TcpClient client)
        {
            try
            {
                var buf = new byte[1024];
                var count =await client.GetStream().ReadAsync(buf, 0, 1024);
                return Encoding.ASCII.GetString(buf, 0, count);
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return String.Empty;
            }
        }

        private async Task<string> ReceiveAsync()
        {
            return await ReceiveAsync(_client);
        }

        private async Task<int> ReceiveCodeAsync(TcpClient client)
        {
            var res = await ReceiveAsync(client);
            var mc = Regex.Match(res, "\\d+");
            if (mc.Success == false)
                return 0;

            return Convert.ToInt32(mc.Value);
        }

        private async Task<int> ReceiveCodeAsync()
        {
            return await ReceiveCodeAsync(_client);
        }

        #endregion

        #region public methods

        public async Task<bool> MakeDirectoryAsync(string name)
        {
            try
            {
                await LoginAsync();

                await SendCommandAsync(FTP_CMD_MKD + name);
                var code = await ReceiveCodeAsync();
                if (code == 257 || code == 550)
                    return true;

                return false;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return false;
            }
        }

        public async Task<bool> MakeDirectoriesAsync(string names)
        {
            try
            {
                var parts = names.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (await MakeDirectoryAsync(part) == false)
                        return false;

                    if (await ChangeDirectoryAsync(part) == false)
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                LastError = e.Message;
            }

            return false;
        }

        public async Task<bool> ChangeDirectoryAsync(string name)
        {
            try
            {
                await LoginAsync();

                await SendCommandAsync(FTP_CMD_CWD + name);
                var code = await ReceiveCodeAsync();
                if (code == 250)
                    return true;

                return false;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return false;
            }
        }

        public async Task<bool> ChangeDirectoriesAsync(string names)
        {
            try
            {
                var parts = names.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (await ChangeDirectoryAsync(part) == false)
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                LastError = e.Message;
            }

            return false;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            return await LoginAsync();
        }

        public async Task<bool> UploadFileAsync(string filePath)
        {
            try
            {
                int code;
                await LoginAsync();
                var fileInfo=new FileInfo(filePath);

                using (var client = await CreateDataClientAsync())
                using (var reader=fileInfo.OpenRead())
                {
                    var res = await SendCommandAsync(FTP_CMD_STOR + fileInfo.Name);
                    code = await ReceiveCodeAsync();
                    if (!(code == 125 || code == 150))
                        return false;
                    await reader.CopyToAsync(client.GetStream());
                }

                code = await ReceiveCodeAsync();
                if (!(code == 226 || code == 250))
                    return false;


                return true;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return false;
            }
        }

        private async Task<TcpClient> CreateDataClientAsync()
        {
            try
            {
                await SendCommandAsync(FTP_CMD_PASV);
                var res = await ReceiveAsync();
                if (res.StartsWith("227") == false)
                    return null;

                var mcs = Regex.Matches(res, @"(?<number>\d+)[,)]");
                if (mcs.Count != 6)
                    return null;

                var parts = new int[6];
                for (var i = 0; i < 6; i++)
                {
                    parts[i] = Convert.ToInt32(mcs[i].Groups["number"].Value);
                }

                var ip = $"{parts[0]}.{parts[1]}.{parts[2]}.{parts[3]}";
                var port = (parts[4] << 8) + parts[5];

                var hostEntry = await Dns.GetHostEntryAsync(ip);

                var client = new TcpClient();
                await client.ConnectAsync(hostEntry.AddressList[0], port);
               
                return client;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return null;
            }
        }

        public FtpUploader(string userName, string password, string host, int port = 21)
        {
            _userName = userName;
            _password = password;
            _host = host;
            _port = port;
        }

        #endregion
    }
}
