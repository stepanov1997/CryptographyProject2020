using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CryptographyProject2019.Model;

namespace CryptographyProject2019.Controller
{
    public class AccountsController
    {
        private static AccountsController ac;

        private AccountsController()
        {
            Accounts = new Dictionary<string, Account>();
        }

        public Dictionary<string, Account> Accounts { get; set; }

        public Account CurrentAccount { get; set; }

        public static AccountsController GetInstance()
        {
            if (ac != null)
                return ac;
            ac = new AccountsController();
            ac.DeSerializeNow();
            return ac;
        }

        public bool AddAccount(Account account)
        {
            if (Accounts.ContainsKey(account.Username))
                return false;
            Accounts.Add(account.Username, account);
            SerializeNow();
            return true;
        }

        public bool RemoveAccount(string username)
        {
            if (!Accounts.ContainsKey(username))
                return false;
            Accounts.Remove(username);
            return true;
        }

        public Account GetAccount(string username, string password, string pathToCertificate)
        {
            var acc = Accounts.TryGetValue(username, out var tmp) ? tmp : null;
            if (acc == null) return null;
            var data = Encoding.Unicode.GetBytes(password);
            HashAlgorithm ha = new MD5CryptoServiceProvider();
            var hashpass = Encoding.Unicode.GetString(ha.ComputeHash(data));
            var cert = new X509Certificate2();
            cert.Import(pathToCertificate);
            if (!ValidateController.ValidateCertificates(cert)) return null;
            return File.Exists(pathToCertificate)
                   && username == Program.ReadName(cert)
                   && acc.PasswordHash == hashpass
                ? acc
                : null;
        }

        public void SerializeNow()
        {
            using (var f = File.OpenWrite("accounts.dat"))
            {
                var b = new BinaryFormatter();
                b.Serialize(f, Accounts);
            }
        }

        public void DeSerializeNow()
        {
            if (!File.Exists("accounts.dat"))
            {
                Accounts = new Dictionary<string, Account>();
                return;
            }
            using (var f = File.OpenRead("accounts.dat"))
            {
                var bin = new BinaryFormatter();
                if (!(bin.Deserialize(f) is Dictionary<string, Account> accounts)) return;
                Accounts = accounts;
            }
        }

        public void ChangeCurrentAccount(Account account)
        {
            CurrentAccount = account;
            var source =
                $"{Directory.GetCurrentDirectory()}/../../CryptoFiles/private/{Path.GetFileNameWithoutExtension(account.PathToCertificate)}.key";
            var dest = $"{Directory.GetCurrentDirectory()}/../../CurrentUsers/{account.Username}.key";
            if (File.Exists(dest))
            {
                if (File.Exists(source))
                {
                    File.WriteAllText(dest, File.ReadAllText(source));
                }
                else
                {
                    var CA_Source =
                        $"{Directory.GetCurrentDirectory()}/../../CryptoFiles/private/ca.key";
                    File.WriteAllText(dest, File.ReadAllText(CA_Source));
                }
            }
            else
            {
                File.Copy(source, dest);
            }
        }

        public List<Account> ReadOnlineAccounts()
        {
            return Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/../../CurrentUsers/", "*.key")
                .AsEnumerable()
                .Select(Path.GetFileNameWithoutExtension)
                .Where(e => Accounts.ContainsKey(e))
                .Where(e => e != CurrentAccount.Username)
                .Select(e => Accounts[e])
                .ToList();
        }
    }
}