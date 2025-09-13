namespace AdventOfCode2021.Tools;

public static class ChineseRemainderTheorem
{
    /// <summary>
    /// Solves a system of simultaneous congruences using the Chinese Remainder Theorem.
    /// </summary>
    /// <param name="moduli">Array of moduli.</param>
    /// <param name="remainders">Array of remainders.</param>
    /// <returns>The smallest non-negative solution to the system of congruences.</returns>
    public static long GetSmallestNumber(long[] moduli, long[] remainders)
    {
        long productOfModuli = 1;
        long result = 0;

        // Compute the product of all moduli
        foreach (long modulus in moduli)
        {
            productOfModuli *= modulus;
        }

        // Compute the result using CRT formula
        for (int i = 0; i < moduli.Length; i++)
        {
            long partialProduct = productOfModuli / moduli[i];  // Partial product
            long modularInverse = ComputeModularInverse(partialProduct, moduli[i]); // Modular inverse
            result += remainders[i] * modularInverse * partialProduct;
        }

        return result % productOfModuli;
    }

    /// <summary>
    /// Computes the modular inverse of a number using the Extended Euclidean Algorithm.
    /// </summary>
    /// <param name="number">The number to find the inverse of.</param>
    /// <param name="modulus">The modulus.</param>
    /// <returns>The modular inverse of the number.</returns>
    private static long ComputeModularInverse(long number, long modulus)
    {
        long originalModulus = modulus, temp, quotient;
        long previousCoefficient = 0;
        long currentCoefficient = 1;

        if (modulus == 1)
            return 0;

        while (number > 1)
        {
            quotient = number / modulus;
            temp = modulus;
            modulus = number % modulus;
            number = temp;
            temp = previousCoefficient;
            previousCoefficient = currentCoefficient - quotient * previousCoefficient;
            currentCoefficient = temp;
        }

        return (currentCoefficient < 0) ? currentCoefficient + originalModulus : currentCoefficient;
    }
}