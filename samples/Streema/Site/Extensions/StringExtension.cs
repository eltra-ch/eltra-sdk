using System;
using System.Text;
using System.Text.RegularExpressions;

namespace StreemaMaster.Helpers
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string stringToCompare, StringComparison comparison)
        {
            return source?.IndexOf(stringToCompare, comparison) >= 0;
        }
        
        public static int ExtractInt(this string source)
        {
            int result = 0;
            String textWithNumbers = Regex.Replace(source, "[^0-9]", "");
            
            if (int.TryParse(textWithNumbers, out var numberValue))
            {
                result = numberValue;
            }

            return result; 
        }

        public static string ConvertToUtf8(this string source, Encoding sourceEncoding)
        {
            string result = source;
            
            if (!string.IsNullOrEmpty(source))
            {
                Encoding targetEncoding = Encoding.UTF8;

                if (targetEncoding.EncodingName != sourceEncoding.EncodingName)
                {
                    byte[] sourceEncodingBytes = sourceEncoding.GetBytes(source);
                    byte[] targetEncodingBytes = Encoding.Convert(sourceEncoding, targetEncoding, sourceEncodingBytes);

                    result = targetEncoding.GetString(targetEncodingBytes);
                }
            }

            return result;
        }

        public static string CharsetFix(this string source)
        {
            string result = string.Empty;

            source = source.Trim();

            foreach (var c in source)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                {
                    result += c;
                }
            }

            return result.ToLower();
        }
    }
}
