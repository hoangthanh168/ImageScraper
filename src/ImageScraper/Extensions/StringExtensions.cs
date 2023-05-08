using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ImageScraper.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> GetLines(this string str, bool removeEmptyLines = false)
        {
            return str.Split(new[] { "\r\n", "\r", "\n" },
                removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        }

        public static string SanitizeTitleInCSharp(this string title)
        {
            string sanitizedTitle = title.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in sanitizedTitle)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            sanitizedTitle = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            // Chuyển đổi ký tự đặc biệt thành dấu gạch ngang
            sanitizedTitle = Regex.Replace(sanitizedTitle, @"[^\w\s-]", "-", RegexOptions.Compiled);
            // Xóa các khoảng trắng thừa và chuyển thành ký tự thấp hơn
            sanitizedTitle = Regex.Replace(sanitizedTitle.Trim(), @"\s+", "-", RegexOptions.Compiled).ToLowerInvariant();

            return sanitizedTitle;
        }
        public static string CreateHash(this string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}