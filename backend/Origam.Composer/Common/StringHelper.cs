namespace Origam.Composer.Common;

using System.Text;

public static class StringHelper
{
    public static string RemoveAllWhitespace(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder();

        foreach (char c in input)
        {
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
