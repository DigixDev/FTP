using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FTP_DLL;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Start_OnClick(object sender, RoutedEventArgs e)
        {
            //FTP_DLL.FTPFactory ftp = new FTPFactory();
            //ftp.setRemoteHost("onedigix.com");
            //ftp.setRemoteUser("onedigix");
            //ftp.setRemotePass("78u$v6nD");
            ////ftp.setRemotePort(990);
            ////ftp.setUseStream(true);
            //ftp.login();
            //ftp.upload("d:\\test300.pdf", false);
            //ftp.upload("d:\\test400.pdf", false);

            //ftp.mkdir("ter9");
            //ftp.chdir("ter9");
            //ftp.mkdir("ter20");

            //var t = ftp.getFileList("");

            //var list=ftp.getDirList("");

            var uploader = new FtpUploader("onedigix", "78u$v6nD", "onedigix.com");
            // var res=await uploader.CheckConnectionAsync();
            // //await uploader.MakeDirectory("testmo");
            // //await uploader.ChangeDirectory("testmo");
            //// await uploader.ChangeDirectoriesAsync("t1/t2/t3");
            var files = Directory.GetFiles("d:\\up\\");
            await uploader.MakeDirectoriesAsync("up\\1\\2\\3");
            await uploader.ChangeDirectoriesAsync("up\\1\\2\\3");
            foreach (var file in files)
            {
                await uploader.UploadFileAsync(file);
            }
            Debugger.Break();


        }
    }
}
