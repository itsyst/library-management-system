namespace Library.Domain.Utilities;

public static class IsbnGenerator
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Generates a random valid ISBN-13
    /// </summary>
    public static string GenerateRandomIsbn13()
    {
        // ISBN-13 format: 978-group-publisher-title-check
        // Total first 12 digits: 978 (3) + group (1) + publisher (3) + title (5) = 12

        string prefix = "978"; // 3 digits

        // Generate random group identifier (1 digit: 0-9)
        string group = _random.Next(0, 10).ToString(); // 1 digit

        // Generate random publisher code (3 digits: 100-999)
        string publisher = _random.Next(100, 1000).ToString(); // 3 digits

        // Generate random title number (5 digits: 10000-99999)
        string title = _random.Next(10000, 100000).ToString(); // 5 digits

        // Combine first 12 digits: 3 + 1 + 3 + 5 = 12
        string isbn12 = prefix + group + publisher + title;

        // Verify we have exactly 12 digits
        if (isbn12.Length != 12)
        {
            throw new InvalidOperationException($"Generated ISBN12 has invalid length: {isbn12.Length}. Expected 12. Generated: {isbn12}");
        }

        // Calculate check digit
        string checkDigit = CalculateIsbn13CheckDigit(isbn12);

        return isbn12 + checkDigit;
    }

    /// <summary>
    /// Alternative method using padding to ensure exact digit counts
    /// </summary>
    public static string GenerateRandomIsbn13WithPadding()
    {
        string prefix = "978"; // 3 digits

        // Generate and pad to ensure exact lengths
        string group = _random.Next(0, 10).ToString().PadLeft(1, '0'); // 1 digit
        string publisher = _random.Next(1, 1000).ToString().PadLeft(3, '0'); // 3 digits
        string title = _random.Next(1, 100000).ToString().PadLeft(5, '0'); // 5 digits

        // Combine: 3 + 1 + 3 + 5 = 12 digits
        string isbn12 = prefix + group + publisher + title;

        // Calculate check digit
        string checkDigit = CalculateIsbn13CheckDigit(isbn12);

        return isbn12 + checkDigit;
    }

    /// <summary>
    /// Calculates the check digit for ISBN-13
    /// </summary>
    private static string CalculateIsbn13CheckDigit(string isbn12)
    {
        if (string.IsNullOrEmpty(isbn12) || isbn12.Length != 12)
        {
            throw new ArgumentException($"ISBN12 must be exactly 12 digits. Received: '{isbn12}' with length {isbn12?.Length ?? 0}");
        }

        // Verify all characters are digits
        foreach (char c in isbn12)
        {
            if (!char.IsDigit(c))
            {
                throw new ArgumentException($"ISBN12 contains non-digit character: '{c}' in '{isbn12}'");
            }
        }

        int sum = 0;

        for (int i = 0; i < 12; i++)
        {
            int digit = int.Parse(isbn12[i].ToString());
            // Multiply by 1 if position is even (0-based), by 3 if position is odd
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int remainder = sum % 10;
        int checkDigit = (remainder == 0) ? 0 : 10 - remainder;

        return checkDigit.ToString();
    }

    /// <summary>
    /// Validates if an ISBN-13 is correctly formatted
    /// </summary>
    public static bool IsValidIsbn13(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return false;

        // Remove hyphens and spaces
        isbn = isbn.Replace("-", "").Replace(" ", "");

        if (isbn.Length != 13)
            return false;

        if (!isbn.StartsWith("978") && !isbn.StartsWith("979"))
            return false;

        // Verify all characters are digits
        foreach (char c in isbn)
        {
            if (!char.IsDigit(c))
                return false;
        }

        try
        {
            // Verify check digit
            string isbn12 = isbn.Substring(0, 12);
            string expectedCheckDigit = CalculateIsbn13CheckDigit(isbn12);

            return isbn[12].ToString() == expectedCheckDigit;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates ISBN with real-world format constraints
    /// </summary>
    public static string GenerateRealisticIsbn13()
    {
        // More realistic ISBN generation based on actual ISBN ranges
        string prefix = "978"; // Most common prefix

        // Common English-speaking group identifiers
        string[] commonGroups = { "0", "1" }; // English language
        string group = commonGroups[_random.Next(commonGroups.Length)];

        // Generate publisher and title to total 8 digits for group 0/1
        int publisherLength = _random.Next(2, 6); // 2-5 digits for publisher
        int titleLength = 8 - publisherLength; // Remaining digits for title

        string publisher = _random.Next((int)Math.Pow(10, publisherLength - 1), (int)Math.Pow(10, publisherLength))
                                 .ToString()
                                 .PadLeft(publisherLength, '0');

        string title = _random.Next((int)Math.Pow(10, titleLength - 1), (int)Math.Pow(10, titleLength))
                             .ToString()
                             .PadLeft(titleLength, '0');

        string isbn12 = prefix + group + publisher + title;
        string checkDigit = CalculateIsbn13CheckDigit(isbn12);

        return isbn12 + checkDigit;
    }
}
