using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MELHARFI.Gfx;

namespace MELHARFI
{
    public static class Cryptography
    {
        /// <summary>
        /// Decrypt a file already encrypted with the CryptDecryt tool you can found in https://github.com/melharfi/CryptDecrypt
        /// </summary>
        /// <param name="path">path is a string representing a path of a file</param>
        /// <param name="crypt">crypt is a byte value between 0 until 255 to decrypt the file</param>
        /// <returns></returns>
        public static byte[] DecryptFile(string path, byte crypt)
        {
            // pour decrypter les assets
            try
            {
                byte[] b1 = File.ReadAllBytes(path);
                for (int cnt = 0; cnt < b1.Length; cnt++)
                    b1[cnt]--;
                byte[] b2 = new byte[b1.Length - 4];
                for (int cnt = 0; cnt < b2.Length; cnt++)
                    b2[cnt] = b1[cnt];
                return b2;
            }
            catch (Exception ex)
            {
                if (Manager.OutputErrorCallBack != null)
                    Manager.OutputErrorCallBack("Error \n" + ex);
                else if (Manager.ShowErrorsInMessageBox)
                    MessageBox.Show("Error \n" + ex);
                return new byte[0];
            }
        }

        /// <summary>
        /// Encrypting method with the Base64 algorthm
        /// </summary>
        /// <param name="str">str is a string value to encrypt</param>
        /// <returns>return an encrypted string</returns>
        public static string Encode64(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Decrypt a string with the Base64 algorithm
        /// </summary>
        /// <param name="str">str is a string value to be decrypted</param>
        /// <returns>return a decrypted string</returns>
        public static string Decode64(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }
    }
}
