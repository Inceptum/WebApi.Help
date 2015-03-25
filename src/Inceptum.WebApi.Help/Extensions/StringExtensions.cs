using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Inceptum.WebApi.Help.Extensions
{
    internal static class StringExtensions
    {
        public static string GetContentTypeByResourceName(string resourcePath)
        {
            if (resourcePath == null) throw new ArgumentNullException("resourcePath");

            var ext = (Path.GetExtension(resourcePath) ?? string.Empty).ToLowerInvariant();
            switch (ext)
            {
                case ".js":
                    return "application/javascript";
                case ".css":
                    return "text/css";
                case ".html":
                    return "text/html";
                default:
                    return "application/octet-stream";
            }
        }

        public static string ToSlug(this string str, int maxLen = 50)
        {
            if (string.IsNullOrWhiteSpace(str)) throw new ArgumentNullException("str");
            if (maxLen <= 0) throw new ArgumentException("maxLen is expected to be a positive value", "maxLen");

            // Remove invalid chars
            str = Regex.Replace(str, @"[^a-z0-9\s-_]", "", RegexOptions.IgnoreCase);

            // Multiple spaces => into one
            str = Regex.Replace(str, @"\s+", " ").Trim();
            
            // Cut and trim 
            if (str.Length > maxLen)
                str = str.Substring(0, maxLen).Trim();

            // Convert spaces to hyphens
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }
    }
}