namespace SafeExchange.CP.Core.Utilities
{
    public static class DateTimeProvider
    {
        public static bool UseSpecifiedDateTime { get; set; }

        public static DateTime SpecifiedDateTime { get; set; }

        public static DateTime UtcNow => UseSpecifiedDateTime ? SpecifiedDateTime : DateTime.UtcNow;
    }
}
