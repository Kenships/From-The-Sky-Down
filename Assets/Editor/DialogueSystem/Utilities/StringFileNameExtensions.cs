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
        /// <summary>
        /// Trims off everything before and including the disk-root, returning an "Assets/…" path.
        /// E.g. "C:\Projects\MyGame\Assets\Foo\bar.asset" → "Assets/Foo/bar.asset"
        /// </summary>
        public static string TrimFilePathToAssetPath(this string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(fullPath));

            // Find the "Assets" folder marker
            int idx = fullPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                throw new ArgumentException($"The path does not contain an 'Assets' segment: {fullPath}");

            // Grab from "Assets" onward, replace backslashes with slashes
            string relative = fullPath.Substring(idx);
            return relative.Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
        }
    }
    
}