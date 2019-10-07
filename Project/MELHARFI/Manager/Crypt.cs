using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MELHARFI.Manager
{
    public class Crypt
    {
        /// <summary>
        /// Decrypt a file already encrypted with the CryptDecryt tool you can found in https://github.com/melharfi/CryptDecrypt
        /// </summary>
        /// <param name="path">path is a string representing a path of a file</param>
        /// <param name="key">it's a 8 character key using Rijndael AES encryptiong algo</param>
        /// <param name="iv">it's a 8 character initializing IV using Rijndael AES encryptiong algo</param>
        /// <returns></returns>
        public Stream DecryptFile(string inputFile, string Key, string IV)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(Key);
            byte[] iv = UE.GetBytes(IV);

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);

            RijndaelManaged RMCrypto = new RijndaelManaged();

            CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateDecryptor(key, iv), CryptoStreamMode.Read);

            int data;
            Stream stream = new MemoryStream();
            while ((data = cs.ReadByte()) != -1)
                stream.WriteByte((byte)data);

            cs.Close();
            fsCrypt.Close();

            return stream;
        }

        /// <summary>
        /// Encrypting method with the Base64 algorthm
        /// </summary>
        /// <param name="str">str is a string value to encrypt</param>
        /// <returns>return an encrypted string</returns>
        public string Encode64(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Decrypt a string with the Base64 algorithm
        /// </summary>
        /// <param name="str">str is a string value to be decrypted</param>
        /// <returns>return a decrypted string</returns>
        public string Decode64(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }
    }
}
