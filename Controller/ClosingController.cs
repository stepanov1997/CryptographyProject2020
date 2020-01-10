using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptographyProject2019.Controller
{
    static class ClosingController
    {
        public static void CloseApplication()
        {
            while (true)
            {
                try
                {
                    string path = Directory.GetCurrentDirectory() +
                                  $@"\..\..\CurrentUsers\{AccountsController.GetInstance().CurrentAccount.Username}.key";
                    File.Delete(path);
                    break;
                }
                catch (IOException)
                {
                    Task.Delay(1000);
                }
            }
            Process.GetCurrentProcess().Kill();
        }
    }
}
