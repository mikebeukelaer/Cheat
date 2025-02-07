using System;
using System.Collections.Generic;

namespace Cheat
{
    public static class CUtils
    {
        public struct FileInfo
        {
            public string Name;
            public bool AutoCopy;
            public List<string> Tags;
        }

        public static string FirstLetterToUpperCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("There is no first letter");

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

    }
}
