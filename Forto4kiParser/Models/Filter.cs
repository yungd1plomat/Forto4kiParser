using Forto4kiParser.Models.Enums;

namespace Forto4kiParser.Models
{
    public class Filter
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double? Width { get; set; }

        public double? Profile { get; set; }

        public double? Radius { get; set; }

        public string? Sae { get;set; }

        public Season? Season { get; set; }

        public bool NearbyWarehouses { get; set; }

        public bool DistantWarehouses { get; set; }

        public Manufacturer? Manufacturer { get; set; }

        public Protection? Protection { get; set; }
    }
}
