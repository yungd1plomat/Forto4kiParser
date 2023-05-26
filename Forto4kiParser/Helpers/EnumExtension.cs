using System.ComponentModel;

namespace Forto4kiParser.Helpers
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}
