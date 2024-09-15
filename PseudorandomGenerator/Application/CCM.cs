namespace PseudorandomGenerator.Application;

public class CCM
{
    private string[,] original_plaintext = {
        {"A", "2" },
        {"E", "4"}
    };

    private string[,] key = {
        { "C", "F" },
        { "A", "1"}
    };

    private string[,] SBOX = {
        { "6", "B", "0", "4" },
        { "7", "E", "2", "F" },
        { "9", "8", "A", "C" },
        { "3", "1", "5", "D" }
    };

    private string[,] mds = {
        { "1", "1" },
        { "1", "2"}
    };

    private string[,] AAD = {
        { "A", "B" },
        { "C", "D" }
    };

    public void EncryptWithCCM()
    {
        var nonce = GenerateNonce();

        var authenticationTag = CBCMAC(original_plaintext, nonce, AAD);

        var encryptedMessage = EncryptCTR(original_plaintext, nonce);

        var finalResult = CombineMessageAndTag(encryptedMessage, authenticationTag);

        PrintMatrix(finalResult, "Final result with CCM");
    }

    public void EncryptWithGCM()
    {
        // Step 0: Generate a nonce (or receive it from external input)
        var nonce = GenerateNonce(); // Randomly generate the nonce

        // Step 1: Encrypt the message using CTR mode
        var encryptedMessage = EncryptCTR(original_plaintext, nonce);

        // Step 2: Define a precomputed H value (in a real implementation, you'd derive it from AES(0^128))
        string[,] H = {
            { "2", "3" },
            { "4", "5" }
        };

        // Step 3: Compute GHASH for the ciphertext and AAD
        var ghashResult = GHASH(H, encryptedMessage, AAD);

        // Step 4: Combine the encrypted message with the GHASH result (authentication tag)
        var finalResult = CombineMessageAndTag(encryptedMessage, ghashResult);

        PrintMatrix(finalResult, "Final result with GCM");
    }

    private string[,] EncryptXTS(string[,] plaintext, int blockIndex)
    {
        // Step 1: Generate a tweak for the given block index
        var tweak = GenerateTweak(blockIndex);

        // Step 2: XOR the plaintext with the tweak (pre-encryption)
        var tweakedPlaintext = XORMatrixOperation(plaintext, tweak);

        // Step 3: Encrypt the tweaked plaintext using AES (reuse your EncryptCTR logic here or similar)
        var encryptedMatrix = EncryptCTR(tweakedPlaintext, tweak);

        // Step 4: XOR the result with the tweak again (post-encryption)
        var finalCiphertext = XORMatrixOperation(encryptedMatrix, tweak);

        return finalCiphertext;
    }

    private string[,] GenerateTweak(int blockIndex)
    {
        // Generate a tweak based on the block index (for example, random tweak for simplicity)
        string[,] tweak = new string[2, 2];
        Random random = new Random();

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                // Use block index as part of the tweak generation for each element
                tweak[i, j] = (random.Next(0, 16) ^ blockIndex).ToString("X");
            }
        }

        PrintMatrix(tweak, "Generated Tweak for block " + blockIndex);
        return tweak;
    }


    private string[,] GHASH(string[,] H, string[,] ciphertext, string[,] AAD)
    {
        string[,] ghashResult = new string[2, 2];

        // XOR the AAD and ciphertext (simplified GHASH logic for your case)
        var intermediate = XORMatrixOperation(AAD, ciphertext);

        // Multiply the result by H in the Galois field
        ghashResult = MultiplyMatricesGF(H, intermediate);

        return ghashResult;
    }



    private string[,] GenerateNonce()
    {
        // Simulating a 2x2 matrix as a nonce with random values (for 16 bits).
        Random random = new Random();
        string[,] nonce = new string[2, 2];

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                nonce[i, j] = random.Next(0, 16).ToString("X");  // Generates a random hex digit
            }
        }

        PrintMatrix(nonce, "Generated Nonce");
        return nonce;
    }


    private string[,] CBCMAC(string[,] plaintext, string[,] nonce, string[,] AAD)
    {
        // Combine the nonce, AAD, and plaintext
        // Perform SBOX substitution, XOR operations, and ShiftRows as needed
        // Use AES block encryption for CBC-MAC tag generation

        // Your custom CBC-MAC logic goes here
        // This would operate similarly to your EncryptRound function
        var macResult = EncryptRound(plaintext, 1); // Example for one round

        return macResult;
    }

    private string[,] EncryptCTR(string[,] plaintext, string[,] nonce)
    {
        int blockNumber = 0;

        // Generate the counter based on the nonce and block number
        var counter = GenerateCounter(nonce, blockNumber);

        var encryptedMatrix = XORMatrixOperation(plaintext, counter);

        return encryptedMatrix;
    }

    private string[,] GenerateCounter(string[,] nonce, int blockNumber)
    {
        // Initialize the counter using the nonce.
        // Assuming the nonce is a 2x2 matrix, we'll use it directly as the starting counter.

        string[,] counter = new string[2, 2];

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                // Convert the nonce element to an integer, and add the block number as the increment.
                int nonceValue = Convert.ToInt32(nonce[i, j], 16);
                int counterValue = nonceValue + blockNumber; // Increment based on block number

                // Store the result as a hexadecimal string back into the counter.
                counter[i, j] = counterValue.ToString("X");
            }
        }

        PrintMatrix(counter, "Generated Counter for block " + blockNumber);
        return counter;
    }


    private string[,] CombineMessageAndTag(string[,] ciphertext, string[,] authTag)
    {
        // Combine the encrypted message with the MAC tag
        // In your case, it might involve concatenating matrices
        string[,] combinedResult = new string[2, 2];

        // Assuming a simple combination for this example:
        combinedResult[0, 0] = ciphertext[0, 0];
        combinedResult[0, 1] = ciphertext[0, 1];
        combinedResult[1, 0] = authTag[0, 0];
        combinedResult[1, 1] = authTag[0, 1];

        return combinedResult;
    }

    private string[,] EncryptRound(string[,] matrix, int count)
    {
        //SBOX
        var resultMatrixSBOX = SBOXSubstitution(matrix);
        PrintMatrix(resultMatrixSBOX, "SBOX Substitution result matrix");

        //ShiftRows
        int lastRow = resultMatrixSBOX.GetLength(0) - 1;
        (resultMatrixSBOX[lastRow, 0], resultMatrixSBOX[lastRow, 1]) = (resultMatrixSBOX[lastRow, 1], resultMatrixSBOX[lastRow, 0]);
        PrintMatrix(resultMatrixSBOX, "Shift Rows result matrix");

        //MixColumns
        string[,] resultMatrixMixColumns = MultiplyMatricesGF(mds, resultMatrixSBOX);
        PrintMatrix(resultMatrixMixColumns, "Mix Columns result matrix");

        //Generate new key
        key = GenerateKey(key, count);
        PrintMatrix(key, $"Key matrix of round:{count}");

        //AddRoundKey
        var roundKey = XORMatrixOperation(resultMatrixMixColumns, key);
        PrintMatrix(roundKey, "Round Key result matrix");

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

}