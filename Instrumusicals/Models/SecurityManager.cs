using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class SecurityManager
    {
        public static string GenrateSalt(int nSalt)
        {
            var saltBytes = new byte[nSalt];

            using (var provider = new RNGCryptoServiceProvider())
            {
                provider.GetNonZeroBytes(saltBytes);
            }

            return Convert.ToBase64String(saltBytes);
        }


        public static string HashPassword(string _Password, string _Salt, int nIterations, int nHash)
        {
            var saltBytes = Convert.FromBase64String(_Salt);
            using ( var rfc2898DeriveBytes = new Rfc2898DeriveBytes(_Password, saltBytes, nIterations))
            {
                return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(nHash));
            }
        }
    }
}
