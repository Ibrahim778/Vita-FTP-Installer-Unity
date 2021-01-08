/*
 * This file is made by jllopisol@gmail.com all credits and I mean ALL credits of this file go to him
 * You can donate to him via paypal:
 * https://www.paypal.com/donate/?cmd=_s-xclick&hosted_button_id=RMFDRTBU49E8E
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

namespace PeXploit
{
    internal static class Functions
    {
        public static string[] StaticKeys => new string[9]
        {
      "syscon_manager_key=D413B89663E1FE9F75143D3BB4565274",
      "keygen_key=6B1ACEA246B745FD8F93763B920594CD53483B82",
      "savegame_param_sfo_key=0C08000E090504040D010F000406020209060D03",
      "trophy_param_sfo_key=5D5B647917024E9BB8D330486B996E795D7F4392",
      "tropsys_dat_key=B080C40FF358643689281736A6BF15892CFEA436",
      "tropusr_dat_key=8711EFF406913F0937F115FAB23DE1A9897A789A",
      "troptrns_dat_key=91EE81555ACC1C4FB5AAE5462CFE1C62A4AF36A5",
      "tropconf_sfm_key=E2ED33C71C444EEBC1E23D635AD8E82F4ECA4E94",
      "fallback_disc_hash_key=D1C1E10B9C547E689B805DCD9710CE8D"
        };

        public static ushort SwapByteOrder(this ushort value) => (ushort)((uint)(((int)value & (int)byte.MaxValue) << 8) | ((uint)value & 65280U) >> 8);

        public static uint SwapByteOrder(this uint value) => (uint)(((int)value & (int)byte.MaxValue) << 24 | ((int)value & 65280) << 8) | (value & 16711680U) >> 8 | (value & 4278190080U) >> 24;

        public static ulong SwapByteOrder(this ulong value) => (ulong)((long)((value & 18374686479671623680UL) >> 56) | (long)((value & 71776119061217280UL) >> 40) | (long)((value & 280375465082880UL) >> 24) | (long)((value & 1095216660480UL) >> 8) | ((long)value & 4278190080L) << 8 | ((long)value & 16711680L) << 24 | ((long)value & 65280L) << 40 | ((long)value & (long)byte.MaxValue) << 56);

        public static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int index = 0; index < a.Length; ++index)
            {
                if ((int)a[index] != (int)b[index])
                    return false;
            }
            return true;
        }

        public static byte[] DecryptWithPortability(byte[] iv, byte[] data, int data_size)
        {
            AesCryptoServiceProvider cryptoServiceProvider = new AesCryptoServiceProvider();
            cryptoServiceProvider.Mode = CipherMode.CBC;
            cryptoServiceProvider.Padding = PaddingMode.Zeros;
            byte[] staticKey = Functions.GetStaticKey("syscon_manager_key");
            if (iv.Length != 16)
                Array.Resize<byte>(ref iv, 16);
            return cryptoServiceProvider.CreateDecryptor(staticKey, iv).TransformFinalBlock(data, 0, data_size);
        }

        public static byte[] EncryptWithPortability(byte[] iv, byte[] data, int data_size)
        {
            AesCryptoServiceProvider cryptoServiceProvider = new AesCryptoServiceProvider();
            cryptoServiceProvider.Mode = CipherMode.CBC;
            cryptoServiceProvider.Padding = PaddingMode.Zeros;
            byte[] staticKey = Functions.GetStaticKey("syscon_manager_key");
            if (iv.Length != 16)
                Array.Resize<byte>(ref iv, 16);
            return cryptoServiceProvider.CreateEncryptor(staticKey, iv).TransformFinalBlock(data, 0, data_size);
        }

        public static byte[] StringToByteArray(this string hex)
        {
            if (hex.Length % 2 != 0)
                hex = hex.PadLeft(hex.Length + 1, '0');
            return Enumerable.Range(0, hex.Length).Where<int>((Func<int, bool>)(x => x % 2 == 0)).Select<int, byte>((Func<int, byte>)(x => Convert.ToByte(hex.Substring(x, 2), 16))).ToArray<byte>();
        }

        public static byte[] GetStaticKey(string name)
        {
            foreach (string staticKey in Functions.StaticKeys)
            {
                if (staticKey.Split('=')[0].ToLower() == name.ToLower())
                    return staticKey.Split('=')[1].StringToByteArray();
            }
            return (byte[])null;
        }

        private static SecureFileInfo[] xDownloadAldosGameConfig()
        {
            try
            {
                string inputtext = new WebClient().DownloadString("http://ps3tools.aldostools.org/games.conf");
                return inputtext == null || inputtext.Length < 100 ? new SecureFileInfo[0] : Functions.ReadConfigFromtext(inputtext);
            }
            catch
            {
                return new SecureFileInfo[0];
            }
        }

        public static SecureFileInfo[] ReadConfigFromtext(string inputtext)
        {
            List<SecureFileInfo> secureFileInfoList = new List<SecureFileInfo>();
            using (StringReader stringReader = new StringReader(inputtext))
            {
                string str1 = stringReader.ReadLine();
                while (str1 != null && stringReader.Peek() > -1 && !str1.Equals("; -- UNPROTECTED GAMES --"))
                    Application.DoEvents();
                string str2;
                string str3 = str2 = stringReader.ReadLine();
                while (str3 != null && stringReader.Peek() > -1 && str3.StartsWith(";"))
                    secureFileInfoList.Add(new SecureFileInfo(str2.Replace(";", ""), "", "", "", false));
                label_13:
                while (stringReader.Peek() > -1)
                {
                    string str4;
                    string str5 = str4 = stringReader.ReadLine();
                    while (true)
                    {
                        string name;
                        string id;
                        string str6;
                        do
                        {
                            string str7;
                            do
                            {
                                do
                                {
                                    if (str5 == null || stringReader.Peek() <= -1 || !str5.StartsWith(";"))
                                        goto label_13;
                                }
                                while (str4 == null);
                                name = str4.Replace(";", "");
                                str7 = str4 = stringReader.ReadLine();
                            }
                            while (str7 == null || !str7.StartsWith("["));
                            id = str4;
                            str6 = stringReader.ReadLine();
                        }
                        while (str6 == null);
                        string dischashkey = str6.Split('=')[1];
                        string securefileid = str6.Split('=')[1];
                        secureFileInfoList.Add(new SecureFileInfo(name, id, securefileid, dischashkey, !string.IsNullOrEmpty(securefileid) && securefileid.Length == 32));
                    }
                }
                stringReader.Close();
            }
            return secureFileInfoList.ToArray();
        }

        public static SecureFileInfo[] DownloadAldosGameConfig()
        {
            SecureFileInfo[] x = new SecureFileInfo[0];
            Thread thread = new Thread((ThreadStart)(() => x = Functions.xDownloadAldosGameConfig()));
            thread.Start();
            while (thread.ThreadState != ThreadState.Stopped)
                Application.DoEvents();
            return x;
        }

        public static byte[] GetHMACSHA1(byte[] key, byte[] data, int start, int length) => new HMACSHA1(key).ComputeHash(data, start, length);

        public static byte[] CalculateFileHMACSha1(string file, byte[] key)
        {
            byte[] hash;
            using (FileStream fileStream = new FileStream(file, FileMode.Open))
            {
                hash = new HMACSHA1(key).ComputeHash((Stream)fileStream);
                fileStream.Close();
            }
            return hash;
        }

        public static byte[] CalculateFileHMACSha1(Stream input, byte[] key) => new HMACSHA1(key).ComputeHash(input);

        public static byte[] Decrypt(byte[] key, byte[] input, int length)
        {
            Array.Resize<byte>(ref key, 16);
            Aes aes1 = Aes.Create();
            aes1.Key = key;
            aes1.BlockSize = 128;
            aes1.Mode = CipherMode.ECB;
            aes1.Padding = PaddingMode.Zeros;
            Aes aes2 = Aes.Create();
            aes2.Key = key;
            aes1.BlockSize = 128;
            aes2.Mode = CipherMode.ECB;
            aes2.Padding = PaddingMode.Zeros;
            int num = length / 16;
            byte[] numArray1 = new byte[length];
            for (int index1 = 0; index1 < num; ++index1)
            {
                byte[] inputBuffer1 = new byte[16];
                Array.Copy((Array)input, index1 * 16, (Array)inputBuffer1, 0, 16);
                byte[] inputBuffer2 = new byte[16];
                Array.Copy((Array)BitConverter.GetBytes(((ulong)index1).SwapByteOrder()), 0, (Array)inputBuffer2, 0, 8);
                byte[] numArray2 = aes1.CreateEncryptor().TransformFinalBlock(inputBuffer2, 0, inputBuffer2.Length);
                byte[] numArray3 = aes2.CreateDecryptor().TransformFinalBlock(inputBuffer1, 0, inputBuffer1.Length);
                for (int index2 = 0; index2 < 16; ++index2)
                    numArray3[index2] ^= numArray2[index2];
                Array.Copy((Array)numArray3, 0, (Array)numArray1, index1 * 16, 16);
            }
            return numArray1;
        }

        public static byte[] Encypt(byte[] key, byte[] input, int length)
        {
            Array.Resize<byte>(ref key, 16);
            Aes aes1 = Aes.Create();
            aes1.Key = key;
            aes1.BlockSize = 128;
            aes1.Mode = CipherMode.ECB;
            aes1.Padding = PaddingMode.Zeros;
            Aes aes2 = Aes.Create();
            aes2.Key = key;
            aes1.BlockSize = 128;
            aes2.Mode = CipherMode.ECB;
            aes2.Padding = PaddingMode.Zeros;
            int num = length / 16;
            byte[] numArray1 = new byte[length];
            for (int index1 = 0; index1 < num; ++index1)
            {
                byte[] inputBuffer1 = new byte[16];
                Array.Copy((Array)input, index1 * 16, (Array)inputBuffer1, 0, 16);
                byte[] inputBuffer2 = new byte[16];
                Array.Copy((Array)BitConverter.GetBytes(((ulong)index1).SwapByteOrder()), 0, (Array)inputBuffer2, 0, 8);
                byte[] numArray2 = aes1.CreateEncryptor().TransformFinalBlock(inputBuffer2, 0, inputBuffer2.Length);
                for (int index2 = 0; index2 < 16; ++index2)
                    inputBuffer1[index2] ^= numArray2[index2];
                Array.Copy((Array)aes2.CreateEncryptor().TransformFinalBlock(inputBuffer1, 0, inputBuffer1.Length), 0, (Array)numArray1, index1 * 16, 16);
            }
            return numArray1;
        }
    }
}
