using System;
using System.Linq;

namespace Slippi.NET.Utils;

public static class FullWidthConverter
{
    /// <summary>
    /// Converts a fullwidth string to a halfwidth string.
    /// </summary>
    /// <param name="str">The input string containing fullwidth characters.</param>
    /// <returns>A string with fullwidth characters converted to halfwidth.</returns>
    public static string ToHalfwidth(string str)
    {
        return new string([.. str.Select(c =>
        {
            int charCode = (int)c;

            // Standard fullwidth encodings
            // https://en.wikipedia.org/wiki/Halfwidth_and_Fullwidth_Forms_(Unicode_block)
            if (charCode > 0xff00 && charCode < 0xff5f)
            {
                return (char)(0x0020 + (charCode - 0xff00));
            }

            // Space
            if (charCode == 0x3000)
            {
                return (char)0x0020;
            }

            // Exceptions found in Melee/Japanese keyboards
            // Single quote: '
            if (charCode == 0x2019)
            {
                return (char)0x0027;
            }

            // Double quote: "
            if (charCode == 0x201d)
            {
                return (char)0x0022;
            }

            return (char)charCode;
        })]);
    }
}