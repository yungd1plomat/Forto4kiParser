﻿using Forto4kiParser.Abstractions;
using Forto4kiParser.Helpers;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Forto4kiParser.Services
{
    public class OrderService : BackgroundService, IDisposable
    {
        // ЧЛ Казанцев Ярослав Романович (Договор Договор 2252 М/Аф-др от 13.06.2023), Частное лицо
        const string customerId = "26730";

        const string template = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                  <SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""Wcf.ClientService.Client.WebAPI.TS3"" xmlns:ns2=""http://schemas.datacontract.org/2004/07/TS3.Domain.Models.Client.ClientSoapService.CreateOrder"">
                                      <SOAP-ENV:Body>
                                          <ns1:CreateOrder>
                                              <ns1:login>LOGIN</ns1:login>
                                              <ns1:password>PASSWORD</ns1:password>
                                              <ns1:order>
                                                  <ns2:is_test>IS_TEST</ns2:is_test>
                                                  <ns2:customerId>CUSTOMERID</ns2:customerId>
                                                  <ns2:product_list>
                                                      <ns2:OrderProduct>
                                                          <ns2:code>CODE</ns2:code>
                                                          <ns2:quantity>QUANTITY</ns2:quantity>
                                                          <ns2:wrh>WAREHOUSE</ns2:wrh>
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
                    using (var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl))
                    {
                        var wrhId = WarehouseHelper.GetWrhIdByName(order.Warehouse.Name);
                        var body = template.Replace("LOGIN", _login)
                            .Replace("PASSWORD", _password)
                            .Replace("IS_TEST", "false")
                            .Replace("CODE", order.Tyre.Sae)
                            .Replace("CUSTOMERID", customerId)
                            .Replace("QUANTITY", order.Quantity.ToString())
                            .Replace("WAREHOUSE", wrhId);
                        req.Content = new StringContent(body, Encoding.UTF8, "text/xml");
                        req.Headers.Add("SOAPAction", Action);
                        var resp = await _client.SendAsync(req);
                        var response = await resp.Content.ReadAsStringAsync();

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(response);
                        XmlNamespaceManager ns = new XmlNamespaceManager(xmlDoc.NameTable);
                        ns.AddNamespace("a", "http://schemas.datacontract.org/2004/07/TS3.Domain.Models.Client.ClientSoapService.CreateOrder");

                        var orderUrl = xmlDoc?.SelectSingleNode("//a:URL", ns)?.InnerText;
                        var orderSuccess = xmlDoc?.SelectSingleNode("//a:success", ns)?.InnerText;
                        bool.TryParse(orderSuccess, out bool isSuccess);
                        _telegramProvider.EnqueueOrder(order.Tyre, isSuccess, orderUrl, order.Quantity);
                        _logger.LogInformation($"New order {order.Tyre.Sae} with {resp.StatusCode} on {order.Warehouse.Name}");
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
