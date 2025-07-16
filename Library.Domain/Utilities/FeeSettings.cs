namespace Library.Domain.Utilities;

public static class FeeSettings
{
    public const int Days = 14; // Default loan period
    public const decimal FeePerDayPerBook = 1.50m; // Fee per day per book
    public const decimal MaxFee = 50.00m; // Maximum fee cap
}
