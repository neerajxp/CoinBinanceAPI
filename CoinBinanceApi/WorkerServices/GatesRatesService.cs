using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
using System.Threading; 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CoinBinanceApi.WorkerServices
{
    public class GatesRatesService : BackgroundService
    {
        private readonly ILogger<GatesRatesService> _logger;

        public GatesRatesService(ILogger<GatesRatesService> logger)
        {
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                //GateRates();
                _logger.LogInformation("Tried connecting");
                TimeSpan ts = new TimeSpan(0, 0, 3);
                await Task.Delay(ts, stoppingToken);
            }
        }

        private void GateRates()
        {
            Console.WriteLine("doe");
        }
    }
}
