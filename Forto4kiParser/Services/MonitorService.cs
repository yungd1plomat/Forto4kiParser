using Forto4kiParser.Abstractions;
using Forto4kiParser.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Forto4kiParser.Services
{
    public class MonitorService : BackgroundService
    {
        private readonly IMemoryCache _cache;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger _logger;

        private readonly ITelegramProvider _telegramProvider;

        private readonly IParserService _parserService;

        public MonitorService(IMemoryCache memoryCache, 
                              IServiceScopeFactory scopeFactory, 
                              ILogger<MonitorService> logger, 
                              ITelegramProvider telegramProvider, 
                              IParserService parserService)
        {
            _cache = memoryCache;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _telegramProvider = telegramProvider;
            _parserService = parserService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Не рекомендуется инъекция scoped сервисов в singleton, поэтому будем сами управлять жизненным циклом бд
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDb>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    var filters = await db.Filters.ToListAsync();
                    int parsed = 0;
                    foreach (var filter in filters) 
                    {
                        var tyres = await _parserService.GetTyres(filter);
                        foreach (var tyre in tyres)
                        {
                            if (!_cache.TryGetValue(tyre.Sae, out _))
                            {
                                _cache.Set<object>(tyre.Sae, null, new MemoryCacheEntryOptions
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                                });
                                _telegramProvider.Enqueue(tyre);
                                parsed++;
                            }
                        }
                        await Task.Delay(10000);
                    }
                    if (parsed > 0)
                        _logger.LogInformation($"Parsed {parsed} new tyres");
                    await Task.Delay(10000);
                }
            }
        }
    }
}
