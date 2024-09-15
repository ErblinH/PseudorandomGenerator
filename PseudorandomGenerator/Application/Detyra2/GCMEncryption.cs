namespace PseudorandomGenerator.Application.Detyra2;

public class GCMEncryption
{
    private string[,] original_plaintext = {
            {"A", "2" },
            {"E", "4"}
        };

    private string[,] ciphertext;

    private string[,] GCMciphertext;

    private string[,] key = {
            { "C", "F" },
            { "A", "1"}
        };

    private string[,] key_0;
    private string[,] key_1;
    private string[,] key_2;
    private string[,] key_3;

    private string[,] SBOX = {
            { "6", "B", "0", "4" },
            { "7", "E", "2", "F" },
            { "9", "8", "A", "C" },
            { "3", "1", "5", "D" }
        };

    private string[,] inverse_SBOX = {
        { "2", "D", "6", "C" },
        { "3", "E", "0", "4" },
        { "9", "8", "A", "1" },
        { "B", "F", "5", "7" }
    };

    private string[,] mds = {
            { "1", "1" },
            { "1", "2"}
        };

    private string[,] inverse_mds = {
        { "F", "E" },
        { "E", "E" }
    };

    private string[,] aad = {
        {"1", "2"},
        {"3", "4"}
    };

    #region GCM
    public (string[,] ciphertext, string tag) EncryptWithGCMComplete(string initialCounter)
    {
        // Step 1: Encrypt the plaintext using CTR mode
        var ciphertext = EncryptWithGCM(original_plaintext, key, initialCounter);

        // Step 2: Compute the Galois Hash for authentication
        var tag = ComputeGaloisHash(ciphertext, aad);

        // Return both the ciphertext and the authentication tag
        return (ciphertext, tag);
    }

    public string[,] DecryptWithGCMComplete(string[,] ciphertext, string initialCounter, string tag)
    {
        key = new string[,] {
            { "C", "F" },
            { "A", "1"}
        };
        // Step 1: Recompute the Galois Hash for authentication
        var computedTag = ComputeGaloisHash(ciphertext, aad);

        // Step 2: Verify the tag
        if (computedTag != tag)
        {
            throw new Exception("Authentication failed: invalid tag");
        }

        // Step 3: Decrypt the ciphertext using CTR mode
        return EncryptWithGCM(ciphertext, key, initialCounter); // CTR decryption is the same as encryption
    }

    public string[,] EncryptWithGCM(string[,] plaintext, string[,] key, string initialCounter)
    {
        string[,] encryptedText = new string[2, 2];
        string[,] counter = ConvertHexStringToMatrix(initialCounter); // Convert counter to matrix form
        PrintMatrix(counter, "Counter");

        // XOR each plaintext block with AES-encrypted counter block
        for (int i = 0; i < 2; i++)
        {
            // Encrypt the counter
            var encryptedCounter = EncryptBlock(counter, key);
            PrintMatrix(encryptedCounter, "encryptedCounter");

            // XOR with plaintext
            encryptedText[i, 0] = XOR(plaintext[i, 0], encryptedCounter[i, 0]);
            encryptedText[i, 1] = XOR(plaintext[i, 1], encryptedCounter[i, 1]);

            // Increment the counter (simplified for this case)
            IncrementCounter(counter);
            PrintMatrix(counter, "Incremented counter");
        }

        return encryptedText;
    }

    private string[,] EncryptBlock(string[,] block, string[,] key)
    {
        key = new string[,]{
            { "C", "F" },
            { "A", "1"}
        };

        return Encrpyt(block);
    }


    private string[,] ConvertHexStringToMatrix(string hexString)
    {
        string[,] matrix = new string[2, 2];

        // Ensure the hex string has at least 4 characters (2 hex digits per position)
        if (hexString.Length != 4)
        {
            throw new ArgumentException("Hex string must be 4 characters long.");
        }

        // Fill the matrix row by row
        matrix[0, 0] = hexString.Substring(0, 1); // First character
        matrix[0, 1] = hexString.Substring(1, 1); // Second character
        matrix[1, 0] = hexString.Substring(2, 1); // Third character
        matrix[1, 1] = hexString.Substring(3, 1); // Fourth character

        return matrix;
    }


    private void IncrementCounter(string[,] counter)
    {
        // Simple increment of the counter (treat as hex values)
        int value = Convert.ToInt32(counter[1, 1], 16);
        value = (value + 1) % 0x10000; // Wrap around after 65536
        counter[1, 1] = value.ToString("X");
    }

    public string ComputeGaloisHash(string[,] ciphertext, string[,] aad)
    {
        string ghash = "0"; // Initialize the hash as 0

        // Process additional authenticated data (AAD) if any
        if (aad != null)
        {
            ghash = GaloisFieldMultiply(ghash, ConvertMatrixToString(aad));
        }

        // Process ciphertext
        ghash = GaloisFieldMultiply(ghash, ConvertMatrixToString(ciphertext));

        return ghash;
    }

    private string GaloisFieldMultiply(string h, string x)
    {
        int hInt = Convert.ToInt32(h, 16);
        int xInt = Convert.ToInt32(x, 16);

        int result = GFMul16(hInt, xInt);

        return result.ToString("X");
    }

    private string ConvertMatrixToString(string[,] matrix)
    {
        return matrix[0, 0] + matrix[0, 1] + matrix[1, 0] + matrix[1, 1];
    }



    private static int GFMul16(int a, int b)
    {
        int p = 0;
        int irreducible = 0x840B; // P(x) irreducible polynomial from octal 210013

        for (int i = 0; i < 16; i++)
        {
            if ((b & 1) != 0)
            {
                p ^= a; // XOR for addition in GF(2)
            }
            bool carry = (a & 0x8000) != 0; // Check if the high bit is set
            a <<= 1;
            if (carry)
            {
                a ^= irreducible; // Reduce with irreducible polynomial if there's a carry
            }
            b >>= 1;
        }

        return p & 0xFFFF; // Return only the lower 16 bits
    }


    #endregion

    #region Encypt

    public string[,] Encrpyt(string[,] block)
    {
        key_0 = key;

        var resultMatrix = XORMatrixOperation(block, key);

        //First round
        var firstRound = EncryptRound(resultMatrix, 1);


        //Second round
        var secondRound = EncryptRound(firstRound, 2);

        //Third round
        //SBOX
        var resultMatrixSBOX = SBOXSubstitution(secondRound);

        //ShiftRows
        int lastRow = resultMatrixSBOX.GetLength(0) - 1;
        (resultMatrixSBOX[lastRow, 0], resultMatrixSBOX[lastRow, 1]) = (resultMatrixSBOX[lastRow, 1], resultMatrixSBOX[lastRow, 0]);

        //Generate new key
        var key_final = GenerateKey(key, 4);

        //AddRoundKey
        var roundKey = XORMatrixOperation(resultMatrixSBOX, key_final);

        ciphertext = roundKey;

        return roundKey;
    }

    private string[,] EncryptRound(string[,] matrix, int count)
    {
        //SBOX
        var resultMatrixSBOX = SBOXSubstitution(matrix);

        //ShiftRows
        int lastRow = resultMatrixSBOX.GetLength(0) - 1;
        (resultMatrixSBOX[lastRow, 0], resultMatrixSBOX[lastRow, 1]) = (resultMatrixSBOX[lastRow, 1], resultMatrixSBOX[lastRow, 0]);

        //MixColumns
        string[,] resultMatrixMixColumns = MultiplyMatricesGF(mds, resultMatrixSBOX);

        //Generate new key
        key = GenerateKey(key, count);

        switch (count)
        {
            case 1:
                key_1 = key;
                break;

            case 2:
                key_2 = key;
                break;

            case 4:
                key_3 = key;
                break;
        }

        //AddRoundKey
        var roundKey = XORMatrixOperation(resultMatrixMixColumns, key);

        return roundKey;
    }

    private static string[,] MultiplyMatricesGF(string[,] matrixA, string[,] matrixB)
    {
        string[,] resultMatrix = new string[2, 2];

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int result = 0;
                for (int k = 0; k < 2; k++)
                {
                    int valueA = Convert.ToInt32(matrixA[i, k], 16);
                    int valueB = Convert.ToInt32(matrixB[k, j], 16);
                    result ^= GFMul(valueA, valueB);
                }
                resultMatrix[i, j] = result.ToString("X");
            }
        }

        return resultMatrix;
    }

    private static int GFMul(int a, int b)
    {
        int p = 0;
        int carry;
        int irreducible = 0x13; // x^4 + x + 1

        for (int i = 0; i < 4; i++)
        {
            if ((b & 1) != 0)
            {
                p ^= a;
            }

            carry = a & 0x8;
            a <<= 1;

            if (carry != 0)
            {
                a ^= irreducible;
            }

            b >>= 1;
        }

        return p & 0xF; // Mask to get only the lower 4 bits
    }

    private string GFunction(string hexElement, int count)
    {
        List<int> binaryList = new List<int>();

        int value = Convert.ToInt32(hexElement, 16);

        string binaryString = Convert.ToString(value, 2).PadLeft(4, '0');

        foreach (char bit in binaryString)
        {
            binaryList.Add(bit - '0');
        }

        List<int> rotatedList = new List<int>();

        for (int i = 1; i < binaryList.Count; i++)
        {
            rotatedList.Add(binaryList[i]);
        }

        rotatedList.Add(binaryList[0]);

        int row = (rotatedList[0] << 1) | rotatedList[1];

        int col = (rotatedList[2] << 1) | rotatedList[3];

        var sboxMpped = SBOX[row, col];

        int value1 = Convert.ToInt32(sboxMpped, 16);
        int value2 = Convert.ToInt32(count.ToString(), 16);

        int result = value1 ^ value2;

        return result.ToString("X");
    }

    private string[,] GenerateKey(string[,] key, int count)
    {
        var k0 = key[0, 0];
        var k1 = key[1, 0];

        var k2 = key[0, 1];
        var k3 = key[1, 1];

        var gvalue = GFunction(k3, count);

        var k0_1 = XOR(k0, gvalue);

        var k1_1 = XOR(k0_1, k1);

        var k2_1 = XOR(k1_1, k2);

        var k3_1 = XOR(k2_1, k3);

        string[,] key_updated = {
            { k0_1, k2_1 },
            { k1_1, k3_1}
        };

        key_3 = key_updated;

        return key_updated;
    }

    private string XOR(string first, string second)
    {
        int value1 = Convert.ToInt32(first, 16);
        int value2 = Convert.ToInt32(second, 16);

        return (value1 ^ value2).ToString("X");
    }

    private void PrintMatrix(string[,] matrix, string message)
    {
        Console.WriteLine(message);

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    private string[,] SBOXSubstitution(string[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        string[,] resultMatrixSBOX = new string[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int value = Convert.ToInt32(matrix[i, j], 16);

                int row = (value & 0xC) >> 2; // First two bits
                int col = value & 0x3;        // Last two bits

                resultMatrixSBOX[i, j] = SBOX[row, col];
            }
        }

        return resultMatrixSBOX;
    }

    private string[,] XORMatrixOperation(string[,] first, string[,] second)
    {
        int rows = first.GetLength(0);
        int cols = first.GetLength(1);

        string[,] resultMatrix = new string[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int value1 = Convert.ToInt32(first[i, j], 16);
                int value2 = Convert.ToInt32(second[i, j], 16);

                int result = value1 ^ value2;

                resultMatrix[i, j] = result.ToString("X");
            }
        }

        return resultMatrix;
    }

    #endregion Encypt

    #region Decrypt

    public void Decrypt()
    {
        //First round decryption
        // Inverse AddRoundKey
        var roundKey = XORMatrixOperation(ciphertext, key_3);

        // Inverse ShiftRows
        int lastRow = roundKey.GetLength(0) - 1;
        (roundKey[lastRow, 0], roundKey[lastRow, 1]) = (roundKey[lastRow, 1], roundKey[lastRow, 0]);

        // Inverse SBOX Substitution
        var resultMatrixSBOX = InverseSBOXSubstitution(roundKey);

        //Second round
        var secondRound = DecryptRound(resultMatrixSBOX, key_2);

        //Third round
        var thirdRound = DecryptRound(secondRound, key_1);

        var resultMatrix = XORMatrixOperation(thirdRound, key_0);
    }

    private string[,] DecryptRound(string[,] matrix, string[,] key)
    {
        //Generate new key
        //AddRoundKey
        var roundKey = XORMatrixOperation(matrix, key);

        //MixColumns
        string[,] resultMatrixMixColumns = MultiplyMatricesGFInverse(inverse_mds, roundKey);

        //ShiftRows
        int lastRow = resultMatrixMixColumns.GetLength(0) - 1;
        (resultMatrixMixColumns[lastRow, 0], resultMatrixMixColumns[lastRow, 1]) = (resultMatrixMixColumns[lastRow, 1], resultMatrixMixColumns[lastRow, 0]);

        //SBOX
        var resultMatrixSBOX = InverseSBOXSubstitution(resultMatrixMixColumns);

        return resultMatrixSBOX;
    }

    private string[,] InverseSBOXSubstitution(string[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        string[,] resultMatrixSBOX = new string[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int value = Convert.ToInt32(matrix[i, j], 16);

                int row = (value & 0xC) >> 2;
                int col = value & 0x3;

                resultMatrixSBOX[i, j] = inverse_SBOX[row, col];
            }
        }

        return resultMatrixSBOX;
    }

    private string[,] MultiplyMatricesGFInverse(string[,] matrixA, string[,] matrixB)
    {
        // This will use the inverse of MixColumns matrix (inverse_mds)
        string[,] resultMatrix = new string[2, 2];

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int result = 0;
                for (int k = 0; k < 2; k++)
                {
                    int valueA = Convert.ToInt32(matrixA[i, k], 16);
                    int valueB = Convert.ToInt32(matrixB[k, j], 16);
                    result ^= GFMul(valueA, valueB);
                }
                resultMatrix[i, j] = result.ToString("X");
            }
        }

        return resultMatrix;
    }


    #endregion
}