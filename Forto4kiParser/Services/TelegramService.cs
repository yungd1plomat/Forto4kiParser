using Forto4kiParser.Abstractions;

namespace Forto4kiParser.Services
{
    public class TelegramService : BackgroundService, IDisposable
    {
        const string BaseUrl = "https://api.telegram.org";

        const int Delay = 3000;


        private readonly string _token;

        private readonly ITelegramProvider _telegramProvider;

        private HttpClient _client { get; set; }


        public TelegramService(string token, ITelegramProvider provider)
        {
            _token = token;
            _client = new HttpClient();
            _telegramProvider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _telegramProvider.GetQueueMsg();
                if (message is not null)
                {
                    await _client.PostAsJsonAsync($"{BaseUrl}/bot{_token}/sendPhoto?parse_mode=MarkdownV2", message);
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
