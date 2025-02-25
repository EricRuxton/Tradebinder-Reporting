namespace TradeBinder_CRON.Services
{
    public class SchedulingService : IHostedService, IDisposable
    {
        private Timer? _timer;
        private readonly HttpClient _httpClient;
        private readonly DailyPricingService _dailyPricingService;

        public SchedulingService(DailyPriceData priceData)
        {

            //Initialize http client
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");

            _dailyPricingService = new DailyPricingService(priceData);

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CRONScheduler, null, TimeSpan.Zero, TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        private void CRONScheduler(object? state)
        {
            _dailyPricingService.UpdateGlobalPricingInfo();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
