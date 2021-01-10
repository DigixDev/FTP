using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;

using FTP_DLL.Properties;

namespace FTP_DLL
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {


                FTPFactory ftp = new FTPFactory();

                ftp.setDebug(true);
                ftp.setRemoteHost(Settings.Default.TargetFtpSource);

                //Connect to SSL Port (990)
                ftp.setRemotePort(990);
                ftp.loginWithoutUser();

                string cmd = "AUTH SSL";
                ftp.sendCommand(cmd);

                //Create SSL Stream
                ftp.getSslStream();
                ftp.setUseStream(true);


                //Login  FTP Secure
                ftp.setRemoteUser(Settings.Default.TargetFtpSecureUser);
                ftp.setRemotePass(Settings.Default.TargetFtpSecurePass);
                ftp.login();

                //Set ASCII Mode
                ftp.setBinaryMode(false);


                //Upload file

                // Send Argument if you want
                //cmd = "site arg1 arg2";
                //ftp.sendCommand(cmd);

                ftp.upload("", false);
                ftp.uploadSecure(@"Filepath", false);


                ftp.close();





            }
            catch (Exception e)
            {
                Console.WriteLine("Caught Error :" + e.Message);
            }

        }
    }
}
