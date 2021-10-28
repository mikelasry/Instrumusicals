using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class SecurityManager
    {
        public static string GenerateSalt()
        {
            var saltBytes = new byte[64];

            using (var provider = new RNGCryptoServiceProvider())
            {
                provider.GetNonZeroBytes(saltBytes);
            }

            return Convert.ToBase64String(saltBytes);
        }


        public static string HashPassword(string _Password, string _Salt)
        {
            var saltBytes = Convert.FromBase64String(_Salt);
            using ( var rfc2898DeriveBytes = new Rfc2898DeriveBytes(_Password, saltBytes, 100000))
            {
                return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(64));
            }
        }

        public static bool Validate(User user, string givenPassword)
        {
            return user.Hash == HashPassword(givenPassword, user.Salt);
        }
    }
}
