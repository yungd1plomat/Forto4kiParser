namespace Forto4kiParser.Models
{
    public class Tyre
    {
        public string Brand { get; set; }

        public string Sae { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string PhotoUrl { get; set; }

        public IList<Warehouse> Warehouses { get; set; }
    }
}
