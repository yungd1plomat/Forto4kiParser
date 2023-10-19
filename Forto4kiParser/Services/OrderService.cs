using Forto4kiParser.Abstractions;
using System.Text;

namespace Forto4kiParser.Services
{
    public class OrderService : BackgroundService, IDisposable
    {
        const string template = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                  <SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""Wcf.ClientService.Client.WebAPI.TS3"" xmlns:ns2=""http://schemas.datacontract.org/2004/07/TS3.Domain.Models.Client.ClientSoapService.CreateOrder"">
                                      <SOAP-ENV:Body>
                                          <ns1:CreateOrder>
                                              <ns1:login>LOGIN</ns1:login>
                                              <ns1:password>PASSWORD</ns1:password>
                                              <ns1:order>
                                                  <ns2:is_test>IS_TEST</ns2:is_test>
                                                  <ns2:product_list>
                                                      <ns2:OrderProduct>
                                                          <ns2:code>CODE</ns2:code>
                                                          <ns2:quantity>QUANTITY</ns2:quantity>
                                                      </ns2:OrderProduct>
                                                  </ns2:product_list>
                                              </ns1:order>
                                          </ns1:CreateOrder>
                                      </SOAP-ENV:Body>
                                  </SOAP-ENV:Envelope>";

        const string BaseUrl = "http://api-b2b.4tochki.ru/WCF/ClientService.svc?wsdl";

        const string Action = "Wcf.ClientService.Client.WebAPI.TS3/ClientService/CreateOrder";

        const int Delay = 3000;


        private readonly ITelegramProvider _telegramProvider;

        private readonly ILogger _logger;

        private readonly IOrderProvider _orderProvider;

        private readonly string _login;

        private readonly string _password;

        private HttpClient _client { get; set; }


        public OrderService(IOrderProvider orderProvider,
            ITelegramProvider telegramProvider,
            ILogger<OrderService> logger,
            IConfiguration configuration)
        {
            _orderProvider = orderProvider;
            _telegramProvider = telegramProvider;
            _logger = logger;
            _login = configuration["LOGIN"] ?? throw new InvalidOperationException("Login not found");
            _password = configuration["PASSWORD"] ?? throw new InvalidOperationException("Password not found");
            _client = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var order = _orderProvider.GetQueueOrder();
                if (order is not null)
                {
                    var tyre = order.Value.Key;
                    var quantity = order.Value.Value;
                    using (var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl))
                    {
                        var body = template.Replace("LOGIN", _login)
                            .Replace("PASSWORD", _password)
                            .Replace("IS_TEST", "false")
                            .Replace("CODE", tyre.Sae)
                            .Replace("QUANTITY", quantity.ToString());
                        req.Content = new StringContent(body, Encoding.UTF8, "text/xml");
                        req.Headers.Add("SOAPAction", Action);
                        var resp = await _client.SendAsync(req);
                        var response = await resp.Content.ReadAsStringAsync();
                        _telegramProvider.EnqueueOrder(tyre, resp.IsSuccessStatusCode, quantity);
                        _logger.LogInformation($"New order {tyre.Sae} with {resp.StatusCode}");
                    }
                }
                await Task.Delay(Delay);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _client.Dispose();
        }
    }
}
