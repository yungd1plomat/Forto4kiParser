namespace Forto4kiParser.Helpers
{
    public static class StringExtension
    {
        private static readonly string[] escapeChars = new string[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };

        public static string Escape(this string str)
        {
            foreach (var ch in escapeChars)
            {
                str = str.Replace(ch, "\\" + ch);
            }
            return str;
        }
    }
}
