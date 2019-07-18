using System;

namespace Cqrs.Infrastructure.Utilities
{
    public static class RandomUtils
    {
        private static readonly Random rnd = new Random(DateTime.UtcNow.Millisecond);
        private static readonly char[] allowableChars = "ABCDEFGHJKMNPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        private static readonly char[] allowableNumChars = "0123456789".ToCharArray();

        public static string GenerateCode(int length)
        {
            var result = new char[length];
            lock (rnd) {
                for (int i = 0; i < length; i++) {
                    result[i] = allowableChars[rnd.Next(0, allowableChars.Length)];
                }
            }

            return new string(result);
        }

        public static string GenerateNum(int length)
        {
            var result = new char[length];
            lock (rnd) {
                for (int i = 0; i < length; i++) {
                    result[i] = allowableNumChars[rnd.Next(0, allowableNumChars.Length)];
                }
            }

            return new string(result);
        }
    }
}
