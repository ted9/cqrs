using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cqrs.Infrastructure.Utilities
{
    public static class CryptoUtils
    {
        public static string EncryptByBase64(string source, string charset = "utf-8")
        {
            return EncryptByBase64(source, Encoding.GetEncoding(charset));
        }
        public static string EncryptByBase64(string source, Encoding encoding)
        {
            return Convert.ToBase64String(encoding.GetBytes(source));
        }

        public static string DecryptByBase64(string source, string charset = "utf-8")
        {
            return DecryptByBase64(source, Encoding.GetEncoding(charset));
        }
        public static string DecryptByBase64(string source, Encoding encoding)
        {
            return encoding.GetString(Convert.FromBase64String(source));
        }



        private readonly static byte[] aesKeys = { 0x41, 0x72, 0x65, 0x79, 0x6F, 0x75, 0x6D, 0x79, 0x53, 0x6E, 0x6F, 0x77, 0x6D, 0x61, 0x6E, 0x3F };

        public static string EncryptByAES(string source, string key, string charset = "utf-8")
        {
            try {
                var enoding = Encoding.GetEncoding(charset);

                RijndaelManaged rijndaelProvider = new RijndaelManaged();
                rijndaelProvider.Key = enoding.GetBytes(key.Substring(0, 32).PadRight(32, ' '));
                rijndaelProvider.IV = aesKeys;

                ICryptoTransform rijndaelEncrypt = rijndaelProvider.CreateEncryptor();

                byte[] inputData = enoding.GetBytes(source);
                byte[] encryptedData = rijndaelEncrypt.TransformFinalBlock(inputData, 0, inputData.Length);

                return Convert.ToBase64String(encryptedData);
            }
            catch {
                return string.Empty;
            }
            
        }

        public static string DecryptByAES(string source, string key, string charset = "utf-8")
        {
            try {
                var enoding = Encoding.GetEncoding(charset);

                RijndaelManaged rijndaelProvider = new RijndaelManaged();
                rijndaelProvider.Key = enoding.GetBytes(key.Substring(0, 32).PadRight(32, ' '));
                rijndaelProvider.IV = aesKeys;
                ICryptoTransform rijndaelDecrypt = rijndaelProvider.CreateDecryptor();

                byte[] inputData = Convert.FromBase64String(source);
                byte[] decryptedData = rijndaelDecrypt.TransformFinalBlock(inputData, 0, inputData.Length);

                return enoding.GetString(decryptedData);
            }
            catch {
                return string.Empty;
            }
        }

        public static string MD5(string source, string charset = "utf-8")
        {
            Ensure.NotNullOrWhiteSpace(source, "source");

            StringBuilder sb = new StringBuilder(32);

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(charset).GetBytes(source));
            for (int i = 0; i < t.Length; i++) {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();

        }


        public static string EncryptByRSA(string content, string privateKey, string charset = "utf-8")
        {
            try {
                var encoding = Encoding.GetEncoding(charset);
                byte[] data = encoding.GetBytes(content);
                RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
                SHA1 sh = new SHA1CryptoServiceProvider();
                byte[] signData = rsa.SignData(data, sh);
                return Convert.ToBase64String(signData);
            }
            catch {
                return string.Empty;
            }
        }

        public static bool VerifyByRSA(string content, string signedString, string publicKey, string charset = "utf-8")
        {
            bool result = false;

            var encoding = Encoding.GetEncoding(charset);
            byte[] Data = encoding.GetBytes(content);
            byte[] data = Convert.FromBase64String(signedString);
            RSAParameters paraPub = ConvertFromPublicKey(publicKey);
            RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
            rsaPub.ImportParameters(paraPub);

            SHA1 sh = new SHA1CryptoServiceProvider();
            result = rsaPub.VerifyData(Data, sh, data);
            return result;
        }

        public static string DecryptByRSA(string content, string privateKey, string charset = "utf-8")
        {
            try {
                var encoding = Encoding.GetEncoding(charset);

                byte[] DataToDecrypt = Convert.FromBase64String(content);
                List<byte> result = new List<byte>();

                for (int j = 0; j < DataToDecrypt.Length / 128; j++) {
                    byte[] buf = new byte[128];
                    for (int i = 0; i < 128; i++) {
                        buf[i] = DataToDecrypt[i + 128 * j];
                    }
                    result.AddRange(decrypt(buf, privateKey, charset));
                }
                byte[] source = result.ToArray();
                char[] asciiChars = new char[encoding.GetCharCount(source, 0, source.Length)];
                encoding.GetChars(source, 0, source.Length, asciiChars, 0);
                return new string(asciiChars);
            }
            catch {
                return string.Empty;
            }
        }

        private static RSAParameters ConvertFromPublicKey(string pemFileConent)
        {
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            if (keyData.Length < 162) {
                throw new ArgumentException("pem file content is incorrect.");
            }
            byte[] pemModulus = new byte[128];
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, 29, pemModulus, 0, 128);
            Array.Copy(keyData, 159, pemPublicExponent, 0, 3);
            RSAParameters para = new RSAParameters();
            para.Modulus = pemModulus;
            para.Exponent = pemPublicExponent;
            return para;
        }

        private static RSAParameters ConvertFromPrivateKey(string pemFileConent)
        {
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            if (keyData.Length < 609) {
                throw new ArgumentException("pem file content is incorrect.");
            }

            int index = 11;
            byte[] pemModulus = new byte[128];
            Array.Copy(keyData, index, pemModulus, 0, 128);

            index += 128;
            index += 2;
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, index, pemPublicExponent, 0, 3);

            index += 3;
            index += 4;
            byte[] pemPrivateExponent = new byte[128];
            Array.Copy(keyData, index, pemPrivateExponent, 0, 128);

            index += 128;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);
            byte[] pemPrime1 = new byte[64];
            Array.Copy(keyData, index, pemPrime1, 0, 64);

            index += 64;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);
            byte[] pemPrime2 = new byte[64];
            Array.Copy(keyData, index, pemPrime2, 0, 64);

            index += 64;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);
            byte[] pemExponent1 = new byte[64];
            Array.Copy(keyData, index, pemExponent1, 0, 64);

            index += 64;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);
            byte[] pemExponent2 = new byte[64];
            Array.Copy(keyData, index, pemExponent2, 0, 64);

            index += 64;
            index += ((int)keyData[index + 1] == 64 ? 2 : 3);
            byte[] pemCoefficient = new byte[64];
            Array.Copy(keyData, index, pemCoefficient, 0, 64);

            RSAParameters para = new RSAParameters();
            para.Modulus = pemModulus;
            para.Exponent = pemPublicExponent;
            para.D = pemPrivateExponent;
            para.P = pemPrime1;
            para.Q = pemPrime2;
            para.DP = pemExponent1;
            para.DQ = pemExponent2;
            para.InverseQ = pemCoefficient;
            return para;
        }

        private static byte[] decrypt(byte[] data, string privateKey, string input_charset)
        {
            RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
            SHA1 sh = new SHA1CryptoServiceProvider();
            return rsa.Decrypt(data, false);
        }

        private static RSACryptoServiceProvider DecodePemPrivateKey(string pemstr)
        {
            byte[] pkcs8privatekey = Convert.FromBase64String(pemstr);
            if (pkcs8privatekey != null)
                return DecodePrivateKeyInfo(pkcs8privatekey);
            else
                return null;
        }

        private static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
        {

            byte[] seqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);           
            byte bt = 0;
            ushort twobytes = 0;

            try {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	             
                    binr.ReadByte();	  
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	  
                else
                    return null;


                bt = binr.ReadByte();
                if (bt != 0x02)
                    return null;

                twobytes = binr.ReadUInt16();

                if (twobytes != 0x0001)
                    return null;

                seq = binr.ReadBytes(15);		   
                if (!CompareBytearrays(seq, seqOID))	      
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x04)	    
                    return null;

                bt = binr.ReadByte();		                 
                if (bt == 0x81)
                    binr.ReadByte();
                else
                    if (bt == 0x82)
                        binr.ReadUInt16();
                byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                return rsacsp;
            }

            catch (Exception) {
                return null;
            }

            finally { binr.Close(); }

        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a) {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);           
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	             
                    binr.ReadByte();	  
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	  
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)	 
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;


                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception) {
                return null;
            }
            finally { binr.Close(); }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)		 
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();	     
            else
                if (bt == 0x82) {
                    highbyte = binr.ReadByte();	      
                    lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else {
                    count = bt;		      
                }



            while (binr.ReadByte() == 0x00) {	     
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);		          
            return count;
        }
    }
}
