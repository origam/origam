using System;
using System.Drawing;

namespace Origam.Extensions
{
    public static class StringExtensions
    {
        public static FontStyle ToFont(this string str)
        {
            return (FontStyle)Enum.Parse(typeof(FontStyle), str);
        }
    }
}