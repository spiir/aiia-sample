namespace Aiia.Sample.Extensions
{
    public static class StringExtensions
    {
        public static bool IsSet(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}