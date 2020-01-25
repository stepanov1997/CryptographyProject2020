using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptographyProject2019.Controller
{
    static class ClosingController
    {
        public static void CloseApplication()
        {
            string path = Directory.GetCurrentDirectory() +
                          $@"\..\..\CurrentUsers\{AccountsController.GetInstance().CurrentAccount.Username}.key";
            while (true)
            {
                try
                {
                    FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Delete);
                    File.Delete(path);
                    fileStream.Close();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(200);
                }
            }
            string path2 = Directory.GetCurrentDirectory() +
                          $@"\..\..\ReceivedMessages\{AccountsController.GetInstance().CurrentAccount.Username}";
            Directory.Delete(path2, true);
            string path3 = Directory.GetCurrentDirectory() +
                          $@"\..\..\ChatRequests\{AccountsController.GetInstance().CurrentAccount.Username}.sesreq";
            File.Delete(path3);
            Process.GetCurrentProcess().Kill();
        }
    }
}
