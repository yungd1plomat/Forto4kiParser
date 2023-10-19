
using System.Text;
using System.Xml.Linq;

namespace Forto4kiParser.Services
{
    public class CancelOrderService : BackgroundService
    {
		const string getOrderTemplate = @"<?xml version=""1.0"" encoding=""UTF-8""?>
										  <SOAP-ENV:Envelope
											  xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/""
											  xmlns:ns1=""Wcf.ClientService.Client.WebAPI.TS3""
											  xmlns:ns2=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""
											  xmlns:ns3=""http://schemas.datacontract.org/2004/07/TS3.Domain.Models.Client.ClientSoapService.GetOrderStatus"">
											  <SOAP-ENV:Body>
												  <ns1:GetOrderStatus>
													  <ns1:login>LOGIN</ns1:login>
													  <ns1:password>PASSWORD</ns1:password>
													  <ns1:filter>
														  <ns3:statusList>
															  <ns2:int>4</ns2:int>
														  </ns3:statusList>
													  </ns1:filter>
												  </ns1:GetOrderStatus>
											  </SOAP-ENV:Body>
										  </SOAP-ENV:Envelope>";

		const string cancelOrderTemplate = @"<?xml version=""1.0"" encoding=""UTF-8""?>
											 <SOAP-ENV:Envelope
												 xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/""
												 xmlns:ns1=""Wcf.ClientService.Client.WebAPI.TS3"">
												 <SOAP-ENV:Body>
													 <ns1:SetOrderStatus>
														 <ns1:login>LOGIN</ns1:login>
														 <ns1:password>PASSWORD</ns1:password>
														 <ns1:orderNumber>ORDERID</ns1:orderNumber>
														 <ns1:statusID>14</ns1:statusID>
													 </ns1:SetOrderStatus>
												 </SOAP-ENV:Body>
											 </SOAP-ENV:Envelope>";

		const string BaseUrl = "http://api-b2b.4tochki.ru/WCF/ClientService.svc";

		const string GetOrderAction = "Wcf.ClientService.Client.WebAPI.TS3/ClientService/GetOrderStatus";

		const string CancelOrderAction = "Wcf.ClientService.Client.WebAPI.TS3/ClientService/SetOrderStatus";

        const int Delay = 1500;

        private readonly ILogger _logger;

		private readonly HttpClient _httpClient;

		private readonly string _login;

		private readonly string _password;

        public CancelOrderService(ILogger<CancelOrderService> logger, IConfiguration configuration) 
        {
            _logger = logger;
			_httpClient = new HttpClient();
            _login = configuration["LOGIN"]?? throw new InvalidOperationException("Login not found");
            _password = configuration["PASSWORD"] ?? throw new InvalidOperationException("Password not found");
        }

		private async Task<IEnumerable<string>> GetOrders()
		{
            using (var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl))
            {
                req.Headers.Add("SOAPAction", GetOrderAction);

                var body = getOrderTemplate.Replace("LOGIN", _login)
                    .Replace("PASSWORD", _password);
                req.Content = new StringContent(body, Encoding.UTF8, "text/xml");
                
                var resp = await _httpClient.SendAsync(req);
				if (resp.IsSuccessStatusCode)
				{
					var response = await resp.Content.ReadAsStringAsync();
                    XDocument doc = XDocument.Parse(response);
                    XNamespace ns = "http://schemas.datacontract.org/2004/07/TS3.Domain.Models.Client.ClientSoapService.GetOrderStatus";
                    var orders = doc.Descendants(ns + "GetOrderStatusResult");
					return orders.Select(x => x.Element(ns + "orderNumber").Value);
                }
				return Array.Empty<string>();
            }
        }

		private async Task CancelOrder(string orderId)
		{
            using (var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl))
            {
                req.Headers.Add("SOAPAction", CancelOrderAction);

                var body = cancelOrderTemplate.Replace("LOGIN", _login)
                    .Replace("PASSWORD", _password)
					.Replace("ORDERID", orderId);
                req.Content = new StringContent(body, Encoding.UTF8, "text/xml");

                var resp = await _httpClient.SendAsync(req);
                _logger.LogInformation($"Отменили заказ {orderId}");
            }
        }

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var cancelOrders = await GetOrders();
				if (cancelOrders.Any())
				{
					foreach (var orderId in cancelOrders)
					{
						if (string.IsNullOrEmpty(orderId)) continue;
						await CancelOrder(orderId);
                        await Task.Delay(Delay);
                    }
				}
                await Task.Delay(Delay * 3);
            }
		}
    }
}
