using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    internal static class InternalExtensionMethods
    {
        internal static T ThrowIfNull<T>(this T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(T));

            return obj;
        }

        internal static string ThrowIfNullOrWhitespace(this string str, string paramName)
        {
            if (string.IsNullOrWhiteSpace(str)) throw new ArgumentNullException(paramName);

            return str;
        }
    }
}
