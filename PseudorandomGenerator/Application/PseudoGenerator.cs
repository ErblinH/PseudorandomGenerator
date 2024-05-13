using System.Security.Cryptography;
using System.Text;

namespace PseudorandomGenerator.Application
{
    public class PseudoGenerator
    {
        #region Hash Function
        public string HashEncrypt(int m, string data, int n, int seedlen)
        {
            // Deklarojmë algoritmin për hashing
            SHA256 sha256 = SHA256.Create();
            string W = string.Empty;
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            for (int i = 1; i <= m; i++)
            {
                // Llogarisim hash funksionin
                byte[] hashBytes = sha256.ComputeHash(dataBytes);
                string wi = BitConverter.ToString(hashBytes).Replace("-", "");
                W += wi;

                // Perditesojme te dhenat dhe kalkulojme rangun mod 2^seedlen
                long dataValue = BitConverter.ToInt64(dataBytes, 0);
                dataValue = (dataValue + 1) % (1L << seedlen);
                dataBytes = BitConverter.GetBytes(dataValue);
            }

            // Kthjme bitet n-te nga ana e majte te vleres W
            // Konvertojme ne byte
            int nBytes = n / 8;

            //Rregullojme nese nuk eshte shumefish i 8
            if (n % 8 != 0) nBytes++;
            byte[] resultBytes = Encoding.UTF8.GetBytes(W);
            if (resultBytes.Length > nBytes)
                Array.Resize(ref resultBytes, nBytes);

            // Konvertojme ne string qe me e pa si duket
            return BitConverter.ToString(resultBytes).Replace("-", "");
        }
        #endregion

        #region NIST SP
        public byte[] Generate(byte[] key, byte[] initialValue, int n, int outlen)
        {
            // Caktojme numrin e iterimeve
            int m = (int)Math.Ceiling((double)n / outlen);
            // Inicializojme rezultatin si vlere e zbrazet
            byte[] W = new byte[0];
            byte[] data = initialValue;

            // Deklarojme bllokun e perkohshem
            using (HMAC hmac = new HMACSHA256(key))
            {
                for (int i = 1; i <= m; i++)
                {
                    // Iterojme ku si hyrje marrim te dhenat paraprake
                    data = hmac.ComputeHash(data);
                    W = Concatenate(W, data);
                }
            }

            // Kthejme n bitet e majte te vleres W
            // Konvertojme ne byte
            int nBytes = n / 8;

            //Rregullojme nese nuk eshte shumefish i 8
            if (n % 8 != 0) nBytes++;
            if (W.Length > nBytes)
            {
                W = W.Take(nBytes).ToArray();
            }

            return W;
        }
        #endregion

        #region IEEE
        public byte[] GeneratePseudoRandomBytes(byte[] key, byte[] V, int n, int outlen)
        {
            // LLogarisim numrin e iterimeve
            int m = (int)Math.Ceiling((double)n / outlen);
            // Inicializojme variablen qe do te mbaje rezultatin
            byte[] W = new byte[0];
            byte[] concatenatedBytes = new byte[0];

            using (HMAC hmac = new HMACSHA256(key))
            {
                for (int i = 1; i <= m; i++)
                {
                    // Fillojme procesin e konkatimit se vleres hyreve me counter
                    byte[] counterBytes = BitConverter.GetBytes(i);
                    byte[] data = new byte[V.Length + counterBytes.Length];
                    Buffer.BlockCopy(V, 0, data, 0, V.Length);
                    Buffer.BlockCopy(counterBytes, 0, data, V.Length, counterBytes.Length);

                    // Llogarisim vleren e hashit
                    byte[] wi = hmac.ComputeHash(data);
                    concatenatedBytes = Concatenate(concatenatedBytes, wi);
                }
            }

            // Kthejme n bitet nga ana e majte te W
            int byteCount = n / 8 + (n % 8 == 0 ? 0 : 1);
            byte[] result = new byte[byteCount];
            Buffer.BlockCopy(concatenatedBytes, 0, result, 0, result.Length);

            // Nese nuk eshte shumefish i 8 atehere i largojme bitet e tepert
            if (n % 8 != 0)
            {
                int bitsToClear = 8 - (n % 8);
                result[result.Length - 1] &= (byte)(255 << bitsToClear);
            }

            return result;
        }
        #endregion

        #region TLS
        public byte[] GenerateTLS(byte[] key, byte[] V, int n, int outlen)
        {
            // Llogarisim numrin e iterimeve
            int m = (int)Math.Ceiling((double)n / outlen);
            byte[][] A = new byte[m + 1][];
            byte[] W = new byte[0];

            // Inicializojme vleren fillestare me V
            A[0] = V;

            using (HMAC hmac = new HMACSHA256(key))
            {
                for (int i = 1; i <= m; i++)
                {
                    // Gjenerojme A(i) si HMAC te A(i-1)
                    A[i] = hmac.ComputeHash(A[i - 1]);

                    // Konkatinojme A(i) dhe V
                    byte[] data = Concatenate(A[i], V);

                    // Gjenerojme wi si HMAC te te dhenave te konkatinuara
                    byte[] wi = hmac.ComputeHash(data);

                    // Bashkojme wi tek W
                    W = Concatenate(W, wi);
                }
            }

            // Kthejme n bitet e majte te W
            return TruncateToNBits(W, n);
        }
        #endregion

        #region Private methods
        private static byte[] Concatenate(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
            return result;
        }

        private static byte[] TruncateToNBits(byte[] data, int n)
        {
            int nBytes = n / 8;
            byte[] result = new byte[nBytes];
            Buffer.BlockCopy(data, 0, result, 0, nBytes);

            if (n % 8 != 0)
            {
                int bitsToKeep = n % 8;
                byte mask = (byte)(0xFF << (8 - bitsToKeep));
                result[nBytes - 1] &= mask;
            }

            return result;
        }

        #endregion
    }
}