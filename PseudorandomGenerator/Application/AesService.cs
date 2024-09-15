using System.Security.Cryptography;
using System.Text;

namespace PseudorandomGenerator.Application;

public class AesService
{
    #region AES ready
    public void Test()
    {
        string filePath = "test.txt";
        string secretKey = "ThisIsASecretKey";

        // Read the content of the text document
        string plainText = File.ReadAllText(filePath);
        Console.WriteLine("Original Text: " + plainText);

        // Encrypt the content
        string encryptedText = Encrypt(plainText, secretKey);
        Console.WriteLine("Encrypted Text: " + encryptedText);

        // Decrypt the content
        string decryptedText = Decrypt(encryptedText, secretKey);
        Console.WriteLine("Decrypted Text: " + decryptedText);
    }

    static string Encrypt(string plainText, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = new byte[16]; // Initialization vector with 0s

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }

    static string Decrypt(string cipherText, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = new byte[16]; // Initialization vector with 0s

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
    #endregion

    #region AES simplified

    private static readonly int[] SBox = new int[] { 0x9, 0x4, 0xA, 0xB, 0xD, 0x1, 0x8, 0x5, 0x6, 0x2, 0x0, 0x3, 0xC, 0xE, 0xF, 0x7 };
    private static readonly int[] InvSBox = new int[] { 0xA, 0x5, 0x9, 0xB, 0x1, 0x7, 0x8, 0xF, 0x6, 0x0, 0x2, 0x3, 0xC, 0x4, 0xD, 0xE };
    private static readonly int[] Rcon = new int[] { 0x0, 0x80, 0x30 };

    public void AES_simplified()
    {
        byte[] key = GenerateRandomBytes(4); // 32-bit key for simplified AES
        string plaintext = ReadPlaintextFromFile("test.txt");

        // Convert plaintext to a 16-bit integer for this simplified example
        int plaintextInt = ConvertStringTo16BitInt(plaintext);

        int ciphertext = Encrypt(plaintextInt, BitConverter.ToInt32(key, 0));
        int decryptedText = Decrypt(ciphertext, BitConverter.ToInt32(key, 0));

        Console.WriteLine($"Plaintext: {plaintext}");
        Console.WriteLine($"Ciphertext: 0x{ciphertext:X}");
        Console.WriteLine($"Decrypted Text: {Convert16BitIntToString(decryptedText)}");
    }

    public static string ReadPlaintextFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file was not found.", filePath);
        }

        return File.ReadAllText(filePath);
    }

    public static byte[] GenerateRandomBytes(int length)
    {
        byte[] bytes = new byte[length];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(bytes);
        }
        return bytes;
    }

    public static int ConvertStringTo16BitInt(string input)
    {
        return ToHex16BitInt(Encoding.UTF8.GetBytes(input.PadRight(2)));
    }

    public static string Convert16BitIntToString(int input)
    {
        byte[] bytes = BitConverter.GetBytes((ushort)input);
        return Encoding.UTF8.GetString(bytes).TrimEnd('\0');
    }

    private static int Encrypt(int plaintext, int key)
    {
        int[] roundKeys = KeyExpansion(key);
        int state = AddRoundKey(plaintext, roundKeys[0]);

        for (int i = 1; i < 3; i++)
        {
            state = SubBytes(state);
            state = ShiftRows(state);
            if (i != 2)
            {
                state = MixColumns(state);
            }
            state = AddRoundKey(state, roundKeys[i]);
        }

        return state;
    }

    private static int Decrypt(int ciphertext, int key)
    {
        int[] roundKeys = KeyExpansion(key);
        int state = AddRoundKey(ciphertext, roundKeys[2]);

        for (int i = 2; i > 0; i--)
        {
            state = InvShiftRows(state);
            state = InvSubBytes(state);
            state = AddRoundKey(state, roundKeys[i]);
            if (i != 1)
            {
                state = InvMixColumns(state);
            }
        }

        state = AddRoundKey(state, roundKeys[0]);

        return state;
    }

    private static int[] KeyExpansion(int key)
    {
        int[] roundKeys = new int[3];
        roundKeys[0] = key;

        for (int i = 1; i < 3; i++)
        {
            int temp = roundKeys[i - 1] >> 8;
            temp = SubWord(temp) ^ Rcon[i];
            roundKeys[i] = roundKeys[i - 1] ^ (temp << 8);
        }

        return roundKeys;
    }

    private static int SubWord(int word)
    {
        return (SBox[(word >> 4) & 0xF] << 4) | SBox[word & 0xF];
    }

    private static int AddRoundKey(int state, int roundKey)
    {
        return state ^ roundKey;
    }

    private static int SubBytes(int state)
    {
        return (SBox[(state >> 12) & 0xF] << 12) | (SBox[(state >> 8) & 0xF] << 8) | (SBox[(state >> 4) & 0xF] << 4) | SBox[state & 0xF];
    }

    private static int InvSubBytes(int state)
    {
        return (InvSBox[(state >> 12) & 0xF] << 12) | (InvSBox[(state >> 8) & 0xF] << 8) | (InvSBox[(state >> 4) & 0xF] << 4) | InvSBox[state & 0xF];
    }

    private static int ShiftRows(int state)
    {
        return (state & 0xF00F) | ((state & 0x0F00) << 4) | ((state & 0x00F0) >> 4);
    }

    private static int InvShiftRows(int state)
    {
        return (state & 0xF00F) | ((state & 0x0F00) >> 4) | ((state & 0x00F0) << 4);
    }

    private static int MixColumns(int state)
    {
        int[] s = new int[] { (state >> 12) & 0xF, (state >> 8) & 0xF, (state >> 4) & 0xF, state & 0xF };
        return ((s[0] ^ s[1] ^ s[2] ^ s[3]) << 12) | ((s[0] ^ s[2]) << 8) | ((s[1] ^ s[3]) << 4) | (s[0] ^ s[1]);
    }

    private static int InvMixColumns(int state)
    {
        return MixColumns(state); // Simplified inverse for demonstration purposes
    }

    public static int ToHex16BitInt(byte[] bytes)
    {
        if (bytes.Length != 2) throw new ArgumentException("Array must be exactly 2 bytes long.");
        return (bytes[0] << 8) | bytes[1];
    }

    #endregion

}
