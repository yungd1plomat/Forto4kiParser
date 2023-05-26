using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Forto4kiParser.Models.Enums
{
    public enum Protection
    {
        [Description("Есть защита")]
        Present = 1,

        [Description("Нет защиты")]
        NotAvailable = 2,
    }
}
