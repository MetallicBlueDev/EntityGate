﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MetallicBlueDev.EntityGate.Extensions
{
    /// <summary>
    /// Help with string manipulation.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Determines whether the specified string is non-zero and contains characters other than blank spaces
        /// </summary>
        /// <param name="source">The text to check.</param>
        /// <param name="minimumLength">Minimum size of the chain.</param>
        /// <returns></returns>
        internal static bool IsNotNullOrEmpty(this string source, int minimumLength = 0)
        {
            return !string.IsNullOrWhiteSpace(source) && source.Trim().Length > minimumLength;
        }

        /// <summary>
        /// Determine if the strings are equal (insensitive).
        /// </summary>
        /// <param name="value">The text to check.</param>
        /// <param name="other">The text to check.</param>
        /// <returns></returns>
        internal static bool EqualsIgnoreCase(this string value, string other)
        {
            var rslt = false;

            if (value != null)
            {
                if (other != null)
                {
                    rslt = value.Trim().ToUpper(CultureInfo.InvariantCulture).Equals(other.Trim().ToUpper(CultureInfo.InvariantCulture), StringComparison.InvariantCulture);
                }
            }

            return rslt;
        }

        /// <summary>
        /// Determines whether the specified string contains at least one match to the template.
        /// </summary>
        /// <param name="source">The text to check.</param>
        /// <param name="pattern">The correspondence model (regular expression or text with variables).</param>
        /// <returns></returns>
        internal static bool IsMatch(this string source, string pattern)
        {
            var rslt = false;

            if (source.IsNotNullOrEmpty() && pattern.IsNotNullOrEmpty())
            {
                rslt = Regex.IsMatch(source, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            }

            return rslt;
        }
    }
}

