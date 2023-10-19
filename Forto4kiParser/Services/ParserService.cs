using Forto4kiParser.Abstractions;
using Forto4kiParser.Helpers;
using Forto4kiParser.Models;
using HtmlAgilityPack;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Forto4kiParser.Services
{
    public class ParserService : IParserService
    {
        const int MAX_PAGE = 245;

        const string Url = "https://b2b.4tochki.ru";

        private HttpClient _client;

        private readonly ILogger _logger;


        public ParserService(ILogger<ParserService> logger, IConfiguration configuration)
        {
            var cookies = configuration["COOKIES"] ?? throw new InvalidOperationException("cookies not found");
            _logger = logger;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookies);
        }

        public string BuildUrl(Filter filter, int page)
        {
            StringBuilder sb = new StringBuilder(Url);
            sb.Append("/Product/Tire?kpt=1&fc_pst=1&cmpx=0&ft_do=");
            sb.Append("&ft_w=");
            sb.Append(filter.Width);
            sb.Append("&ft_h=");
            sb.Append(filter.Profile);
            sb.Append("&ft_d=");
            sb.Append(filter.Radius);
            sb.Append("&ft_s=");
            sb.Append(Convert.ToInt32(filter.Season));
            sb.Append("&fc_b=");
            sb.Append(Convert.ToInt32(filter.Manufacturer));
            sb.Append("&fc_c=");
            sb.Append(filter.Sae);
            sb.Append("&fc_vc=&ft_p=");
            sb.Append(Convert.ToInt32(filter.Protection));
            if (filter.NearbyWarehouses)
                sb.Append("&fc_wh=1&fc_wh=232&fc_wh=9&fc_wh=3&fc_wh=4");
            if (filter.DistantWarehouses)
                sb.Append("&fc_wh=1046&fc_wh=755&fc_wh=154&fc_wh=949&fc_wh=846&fc_wh=115&fc_wh=766&fc_wh=46&fc_wh=313&fc_wh=57&fc_wh=22&fc_wh=689&fc_wh=853&fc_wh=43&fc_wh=865&fc_wh=42&fc_wh=562&fc_wh=1278&fc_wh=1015");
            sb.Append("&fc_pn=");
            sb.Append(page);
            return sb.ToString();
        }

        public async Task<IEnumerable<Tyre>> GetTyres(Filter filter)
        {
            List<Tyre> tyres = new List<Tyre>();
            for (int page = 0; page < MAX_PAGE; page++)
            {
                var url = BuildUrl(filter, page);
                try
                {
                    var resp = await _client.GetAsync(url);
                    var content = await resp.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);
                    var table = doc.DocumentNode.SelectNodes("//*[@class='table table-1']//tbody//tr");
                    if (table is null)
                        break;
                    var pageTyres = ParseTyres(table);
                    tyres.AddRange(pageTyres);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{ex.Message} - page {page}, url - {url}");
                }
                await Task.Delay(2000);
            }
            return tyres;
        }

        public IEnumerable<Tyre> ParseTyres(HtmlNodeCollection table)
        {
            var tyres = new List<Tyre>();
            foreach (var node in table)
            {
                // Скипаем row
                if (node.ChildNodes.Count == 3)
                    continue;

                if (node.ChildNodes.Count == 11)
                {
                    var warehouseNodes = node.ChildNodes[1].GetNextElementsWithParent(6);
                    var newWarehouse = ParseWarehouse(warehouseNodes);
                    tyres.Last().Warehouses.Add(newWarehouse);
                    continue;
                }

                Tyre tyre = new Tyre();
                var brandText = node.ChildNodes[1].ChildNodes[1].InnerText;
                tyre.Brand = Regex.Replace(brandText, @"\t|\n|\r", string.Empty);

                var saeText = node.ChildNodes[1].ChildNodes[3].InnerText;
                tyre.Sae = Regex.Replace(saeText, @"\t|\n|\r", string.Empty);

                var season = node.ChildNodes[3].SelectSingleNode(".//i[@class='icon icon-sun-b']") is null ? "Зима" : "Лето";
                var hasProtection = node.ChildNodes[3].SelectSingleNode(".//i[@class='icon icon-ship-b']") is null ? "Без шипов" : "Шипы";

                tyre.Description = $"{season}/{hasProtection}";

                tyre.PhotoUrl = node.ChildNodes[3].SelectSingleNode(".//a[@class='product-image-link']")?.GetAttributeValue("imgsrc", "https://i.pinimg.com/originals/54/b7/59/54b7594da47ddcc3ed668f39ebd9be87.png") ?? "https://i.pinimg.com/originals/54/b7/59/54b7594da47ddcc3ed668f39ebd9be87.png";

                var name = node.ChildNodes[5].InnerHtml;
                tyre.Name = Regex.Replace(name, @"\t|\n|\r", string.Empty);

                var nodes = node.ChildNodes[7].GetNextElementsWithParent(6);

                var warehouse = ParseWarehouse(nodes);
                tyre.Warehouses = new List<Warehouse>()
                {
                    warehouse,
                };
                tyres.Add(tyre);
            }

            return tyres;
        }

        public Warehouse ParseWarehouse(HtmlNodeCollection nodes)
        {
            Warehouse warehouse = new Warehouse();
            var warehouseName = nodes[0].SelectSingleNode(".//span[@class='storage ']").InnerText;
            warehouse.Name = Regex.Replace(warehouseName, @"\t|\n|\r", string.Empty).ToUpper();

            var onlinePrice = nodes[2].InnerText;
            var normOnlinePrice = Regex.Replace(onlinePrice, @"\t|\n|\r| ", string.Empty);
            warehouse.InternetPrice = uint.Parse(normOnlinePrice);

            var price = nodes[4].InnerText;
            var normPrice = Regex.Replace(price, @"\t|\n|\r| ", string.Empty);
            warehouse.SupplierPrice = uint.Parse(normPrice);

            var stock = nodes[6].InnerText;
            var encodedStock = Regex.Replace(stock, @"\t|\n|\r| ", string.Empty);
            var decodedStock = WebUtility.HtmlDecode(encodedStock);
            warehouse.Stock = decodedStock;
            return warehouse;
        }
    }
}
