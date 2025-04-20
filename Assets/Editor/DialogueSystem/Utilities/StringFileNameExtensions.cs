namespace DialogueSystem.Utilities
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    
    public static class StringExtensions
    {
        /// <summary>
        /// Removes all whitespace characters (spaces, tabs, newlines, etc.) from the string.
        /// </summary>
        public static string RemoveWhiteSpaces(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            
            return new string(str.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        /// <summary>
        /// Removes everything except letters (A–Z, a–z, Unicode letters), digits (0–9), 
        /// hyphens (-) and underscores (_).  This strips out # % & { } \ &lt;&gt; * ? / $ ! ' " : @ + ` | = emojis, alt‐codes, etc.
        /// </summary>
        public static string RemoveSpecialCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var sb = new StringBuilder(str.Length);
            foreach (var c in str)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
    
}