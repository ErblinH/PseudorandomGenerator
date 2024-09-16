namespace PseudorandomGenerator.Application.Detyra3;

public class Analysis
{
    private string[,] original_plaintext = {
            {"A", "2" },
            {"E", "4"}
        };

    private string[,] ciphertext;

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

    #region Analysis

    // Differential Cryptanalysis Attack
    public void DifferentialCryptanalysis()
    {
        Console.WriteLine("Performing Differential Cryptanalysis on the first round");

        // Select a plaintext pair with a small difference
        var plaintext1 = new string[,] { { "A", "2" }, { "E", "4" } };
        var plaintext2 = new string[,] { { "A", "2" }, { "C", "4" } }; // Difference in one byte

        // XOR the plaintexts to find the difference
        var plaintextDiff = XORMatrixOperation(plaintext1, plaintext2);
        PrintMatrix(plaintextDiff, "Difference between plaintexts");

        // Encrypt both plaintexts through the first round
        var encrypted1 = EncryptRound(plaintext1, 1);
        var encrypted2 = EncryptRound(plaintext2, 1);

        // XOR the ciphertexts to find the difference after encryption
        var ciphertextDiff = XORMatrixOperation(encrypted1, encrypted2);
        PrintMatrix(ciphertextDiff, "Difference between ciphertexts after the first round");

        // Analyze the difference to identify potential patterns
        AnalyzeDifferential(ciphertextDiff);
    }

    private void AnalyzeDifferential(string[,] diffMatrix)
    {
        Console.WriteLine("Analyzing differential pattern...");
    }

    public void LinearCryptanalysis()
    {
        Console.WriteLine("Performing Linear Cryptanalysis on the first round");

        // Define a list of random plaintexts to test biases
        var plaintexts = new List<string[,]>
        {
            new string[,] { { "A", "2" }, { "E", "4" } },
            new string[,] { { "A", "3" }, { "F", "5" } },
            // Add more plaintexts with varying bit patterns
        };

        // Track the bias of specific bit positions
        int biasCounter = 0;

        foreach (var plaintext in plaintexts)
        {
            // Encrypt the plaintext through the first round
            var encrypted = EncryptRound(plaintext, 1);

            // Check specific bit patterns between input and output
            var inputBit = ExtractBit(plaintext, 0, 0); // Example: first bit of the first element
            var outputBit = ExtractBit(encrypted, 0, 0); // Example: first bit of the encrypted output

            if (inputBit == outputBit)
            {
                biasCounter++;
            }
        }

        // Calculate the bias and analyze
        double bias = (double)biasCounter / plaintexts.Count;
        Console.WriteLine($"Bias in the first round: {bias}");
    }

    private int ExtractBit(string[,] matrix, int row, int col)
    {
        // Convert the matrix element to binary and return a specific bit
        int value = Convert.ToInt32(matrix[row, col], 16);
        return (value >> 3) & 1; // Example: extract the most significant bit
    }


    #endregion


    #region Encypt

    public void Encrpyt()
    {
        key_0 = key;

        var resultMatrix = XORMatrixOperation(original_plaintext, key);

        PrintMatrix(resultMatrix, "Initial XOR between plaintext and initial key matrix");

        //First round
        Console.WriteLine("Starting first round");
        var firstRound = EncryptRound(resultMatrix, 1);


        //Second round
        Console.WriteLine("Starting second round");
        var secondRound = EncryptRound(firstRound, 2);

        //Third round
        Console.WriteLine("Starting third round");

        //SBOX
        var resultMatrixSBOX = SBOXSubstitution(secondRound);
        PrintMatrix(resultMatrixSBOX, "SBOX Substitution result matrix");

        //ShiftRows
        int lastRow = resultMatrixSBOX.GetLength(0) - 1;
        (resultMatrixSBOX[lastRow, 0], resultMatrixSBOX[lastRow, 1]) = (resultMatrixSBOX[lastRow, 1], resultMatrixSBOX[lastRow, 0]);
        PrintMatrix(resultMatrixSBOX, "Shift Rows result matrix");

        //Generate new key
        var key_final = GenerateKey(key, 4);
        PrintMatrix(key_final, "Key matrix of round:3");

        //AddRoundKey
        var roundKey = XORMatrixOperation(resultMatrixSBOX, key_final);
        PrintMatrix(roundKey, "Round Key result matrix");

        ciphertext = roundKey;

        PrintMatrix(roundKey, "Final encrypted result matrix");
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

}