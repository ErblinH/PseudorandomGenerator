namespace PseudorandomGenerator.Application.Detyra2;
public class CCMEncryption
{
    private string[,] original_plaintext = {
            {"A", "2" },
            {"E", "4"}
        };

    private string[,] ciphertext;

    private string[,] CCMciphertext;

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

    private string[,] nonce = {
        { "0", "1" },
        { "0", "0" }
    };

    private int counter = 0; // Initialize the counter

    #region CCM

    public void EncryptCCM()
    {
        key_0 = key;
        counter = 0;

        // Compute CBC-MAC over plaintext
        var mac = ComputeCBCMAC(original_plaintext);
        PrintMatrix(mac, "ComputeCBCMAC");

        // Encrypt plaintext using CTR mode
        CCMciphertext = new string[original_plaintext.GetLength(0), original_plaintext.GetLength(1)];

        var okk = new List<string>();

        for (int i = 0; i < original_plaintext.GetLength(0); i++)
        {
            for (int j = 0; j < original_plaintext.GetLength(1); j++)
            {
                // Generate counter block
                var counterBlock = GenerateCounterBlock(counter++);

                // Encrypt counter block
                var encryptedCounter = EncryptBlock(counterBlock);
                PrintMatrix(encryptedCounter, $"encryptedCounter counter={counter}");

                var ok = XOR(original_plaintext[i, j], encryptedCounter[i, j]);

                okk.Add(ok);

                // XOR with plaintext
                //ciphertext[i, j] = ok;
                //ciphertext[i, j] = XOR(original_plaintext[i, j], encryptedCounter[i, j]);
            }
        }

        string[,] myArray = {
            {okk[0], okk[1]},
            {okk[2], okk[3]}
        };

        CCMciphertext = myArray;

        // Encrypt MAC using CTR mode
        var encryptedMac = EncryptMAC(mac);
        PrintMatrix(encryptedMac, "encryptedMac");

        // Append encrypted MAC to ciphertext (you may need to adjust data structures)
        AppendMacToCiphertext(encryptedMac);

        // Output final ciphertext
        PrintMatrix(CCMciphertext, "Final encrypted ciphertext with MAC");
    }

    private string[,] EncryptBlock(string[,] block)
    {
        key = new string[,]{
            { "C", "F" },
            { "A", "1"}
        };

        return Encrpyt(block);
    }
    private string[,] ComputeCBCMAC(string[,] plaintext)
    {
        string[,] mac = { { "0", "0" }, { "0", "0" } }; // Initialize MAC to zero

        for (int i = 0; i < plaintext.GetLength(0); i++)
        {
            for (int j = 0; j < plaintext.GetLength(1); j++)
            {
                // XOR plaintext block with MAC
                mac[i, j] = XOR(plaintext[i, j], mac[i, j]);
            }
        }

        // Encrypt MAC value
        mac = EncryptBlock(mac);

        return mac;
    }
    private string[,] EncryptMAC(string[,] mac)
    {
        // Use the next counter value for MAC encryption
        var counterBlock = GenerateCounterBlock(counter++);

        // Encrypt counter block
        var encryptedCounter = EncryptBlock(counterBlock);

        // XOR with MAC
        var encryptedMac = new string[mac.GetLength(0), mac.GetLength(1)];
        for (int i = 0; i < mac.GetLength(0); i++)
        {
            for (int j = 0; j < mac.GetLength(1); j++)
            {
                encryptedMac[i, j] = XOR(mac[i, j], encryptedCounter[i, j]);
            }
        }
        return encryptedMac;
    }
    private void AppendMacToCiphertext(string[,] encryptedMac)
    {
        int ciphertextRows = CCMciphertext.GetLength(0);
        int ciphertextCols = CCMciphertext.GetLength(1);

        int macRows = encryptedMac.GetLength(0);
        int macCols = encryptedMac.GetLength(1);

        // Create a new matrix to hold the ciphertext + MAC
        string[,] finalCiphertext = new string[ciphertextRows + macRows, ciphertextCols];

        // Copy the original ciphertext to the new matrix
        for (int i = 0; i < ciphertextRows; i++)
        {
            for (int j = 0; j < ciphertextCols; j++)
            {
                finalCiphertext[i, j] = CCMciphertext[i, j];
            }
        }

        // Copy the encrypted MAC to the new matrix
        for (int i = 0; i < macRows; i++)
        {
            for (int j = 0; j < macCols; j++)
            {
                finalCiphertext[ciphertextRows + i, j] = encryptedMac[i, j];
            }
        }

        // Replace the original ciphertext with the new ciphertext + MAC
        CCMciphertext = finalCiphertext;

        PrintMatrix(CCMciphertext, "Ciphertext with appended encrypted MAC");
    }

    public void DecryptCCM()
    {
        Console.WriteLine("Starting CCM decrypt");

        counter = 1;

        // Split ciphertext into actual ciphertext and encrypted MAC
        var encryptedMac = ExtractMacFromCiphertext(); // ciphertext is updated to exclude MAC
        PrintMatrix(encryptedMac, "encryptedMac");

        // Decrypt MAC using CTR mode
        var decryptedMac = DecryptMAC(encryptedMac);
        PrintMatrix(decryptedMac, "decryptedMac");

        var okk = new List<string>();

        // Decrypt ciphertext using CTR mode (only the actual ciphertext part, excluding MAC)
        var plaintext = new string[CCMciphertext.GetLength(0), CCMciphertext.GetLength(1)];
        counter = 0;  // Reset the counter for CTR mode
        for (int i = 0; i < CCMciphertext.GetLength(0); i++)
        {
            for (int j = 0; j < CCMciphertext.GetLength(1); j++)
            {
                // Generate counter block
                var counterBlock = GenerateCounterBlock(counter++); // Counter should increment for each block

                // Encrypt the counter block to generate keystream (same as encryption process)
                var keystreamBlock = EncryptBlock(counterBlock);

                var ok = XOR(CCMciphertext[i, j], keystreamBlock[i, j]);

                okk.Add(ok);
                // XOR keystream with ciphertext to get plaintext
                // plaintext[i, j] = XOR(ciphertext[i, j], keystreamBlock[i, j]);
            }
        }

        string[,] myArray = {
            {okk[0], okk[1]},
            {okk[2], okk[3]}
        };

        plaintext = myArray;

        // Compute CBC-MAC over the decrypted plaintext to compare with decrypted MAC
        var computedMac = ComputeCBCMAC(plaintext);
        var computedMac2 = ComputeCBCMAC(original_plaintext);

        // Verify MAC
        if (VerifyMac(decryptedMac, computedMac))
        {
            Console.WriteLine("Decryption successful and MAC verified.");
            PrintMatrix(plaintext, "Decrypted plaintext");
        }
        else
        {
            Console.WriteLine("MAC verification failed!");
        }
    }


    private string[,] ExtractMacFromCiphertext()
    {
        int totalRows = CCMciphertext.GetLength(0);
        int cols = CCMciphertext.GetLength(1);

        // Assume that the MAC is the same size as the plaintext block (adjust as needed)
        int macRows = 2;  // Assuming the MAC size is 2 rows
        int ciphertextRows = totalRows - macRows;

        // Create a matrix for the encrypted MAC
        string[,] extractedMac = new string[macRows, cols];

        // Copy the MAC from the end of the ciphertext
        for (int i = 0; i < macRows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                extractedMac[i, j] = CCMciphertext[ciphertextRows + i, j];
            }
        }

        // Remove MAC from ciphertext by creating a new matrix for the plaintext-only part
        string[,] actualCiphertext = new string[ciphertextRows, cols];

        for (int i = 0; i < ciphertextRows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                actualCiphertext[i, j] = CCMciphertext[i, j];
            }
        }

        // Update the original ciphertext to only hold the encrypted message (without MAC)
        CCMciphertext = actualCiphertext;

        PrintMatrix(extractedMac, "Extracted encrypted MAC");
        return extractedMac;
    }
    private string[,] DecryptMAC(string[,] encryptedMac)
    {
        // Use the same counter value used during encryption
        var counterBlock = GenerateCounterBlock(4);

        // Encrypt counter block
        var encryptedCounter = EncryptBlock(counterBlock);

        // XOR with encrypted MAC to get original MAC
        var mac = new string[encryptedMac.GetLength(0), encryptedMac.GetLength(1)];
        for (int i = 0; i < encryptedMac.GetLength(0); i++)
        {
            for (int j = 0; j < encryptedMac.GetLength(1); j++)
            {
                mac[i, j] = XOR(encryptedMac[i, j], encryptedCounter[i, j]);
            }
        }
        return mac;
    }
    private bool VerifyMac(string[,] mac1, string[,] mac2)
    {
        for (int i = 0; i < mac1.GetLength(0); i++)
        {
            for (int j = 0; j < mac1.GetLength(1); j++)
            {
                if (mac1[i, j] != mac2[i, j])
                    return false;
            }
        }
        return true;
    }
    private string[,] GenerateCounterBlock(int counter)
    {
        // Convert counter to a 2x2 matrix (adjust based on your block size)
        string[,] counterMatrix = {
            { nonce[0, 0], nonce[0, 1] },
            { counter.ToString("X"), "0" }
        };

        PrintMatrix(counterMatrix, $"counterBlock counter={counter}");

        return counterMatrix;
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
