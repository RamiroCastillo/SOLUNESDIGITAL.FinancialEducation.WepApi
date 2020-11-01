using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SOLUNESDIGITAL.Framework.Common
{
    public class Tools
    {
        public static string randomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[20];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
    }
}
