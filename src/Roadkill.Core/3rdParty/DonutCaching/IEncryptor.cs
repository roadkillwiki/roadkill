using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevTrends.MvcDonutCaching
{
    public interface IEncryptor
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
    }
}
