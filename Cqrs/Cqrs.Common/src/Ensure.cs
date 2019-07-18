using System;

namespace Cqrs
{
    public static class Ensure
    {
        public static void NotNull<T>(T @object, string variableName) where T : class
        {
            if (@object == null)
                throw new ArgumentNullException(variableName);
        }
        public static void NotEmpty(string @string, string variableName)
        {
            if (@string != null && string.IsNullOrEmpty(@string))
                throw new ArgumentException(variableName);
        }
        public static void NotNullOrEmpty(string @string, string variableName)
        {
            if (string.IsNullOrEmpty(@string))
                throw new ArgumentNullException(variableName);
        }
        public static void NotWhiteSpace(string @string, string variableName)
        {
            if (@string != null && string.IsNullOrWhiteSpace(@string))
                throw new ArgumentException(variableName);
        }
        public static void NotNullOrWhiteSpace(string @string, string variableName)
        {
            if (string.IsNullOrWhiteSpace(@string))
                throw new ArgumentNullException(variableName);
        }
        public static void Positive(int number, string variableName)
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be positive."));
        }
        public static void Positive(long number, string variableName)
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be positive."));
        }
        public static void Nonnegative(long number, string variableName)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(variableName,string.Concat(variableName, " should be non negative."));
        }
        public static void Nonnegative(int number, string variableName)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(variableName, string.Concat(variableName, " should be non negative."));
        }
        public static void NotEmptyGuid(Guid guid, string variableName)
        {
            if (Guid.Empty == guid)
                throw new ArgumentException(variableName, variableName + " shoud be non-empty GUID.");
        }
        public static void Equal(int expected, int actual, string variableName)
        {
            if (expected != actual)
                throw new ArgumentException(string.Format("{0} expected value: {1}, actual value: {2}", variableName, expected, actual));
        }
        public static void Equal(long expected, long actual, string variableName)
        {
            if (expected != actual)
                throw new ArgumentException(string.Format("{0} expected value: {1}, actual value: {2}", variableName, expected, actual));
        }
        public static void Equal(bool expected, bool actual, string variableName)
        {
            if (expected != actual)
                throw new ArgumentException(string.Format("{0} expected value: {1}, actual value: {2}", variableName, expected, actual));
        }        
    }
}
