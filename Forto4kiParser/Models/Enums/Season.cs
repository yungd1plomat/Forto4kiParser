using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Forto4kiParser.Models.Enums
{
    public enum Season
    {
        [Description("Лето")]
        Summer = 1,

        [Description("Зима")]
        Winter = 2,

        [Description("Зима (шип.)")]
        WinterWithSpikes = 101,

        [Description("Зима (не шип.)")]
        WinterNoSpikes = 102,
    }
}
