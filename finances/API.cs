using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;
using System;

namespace finances
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "2d078771a0033dc8207c2f1861e80ba0";
        private readonly Dispatcher _dispatcher;
        public event Action<double> OnRateUpdated;

        public CurrencyService(string apiKey, Dispatcher dispatcher)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
            _dispatcher = dispatcher;
        }

        public async Task<double> GetExchangeRateAsync(string baseCurrency, string targetCurrency)
        {
            var url = $"http://data.fixer.io/api/latest?access_key={_apiKey}&symbols={targetCurrency}";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var exchangeRates = JsonConvert.DeserializeObject<ExchangeRateResponse>(response);

                if (exchangeRates.Success && exchangeRates.Rates.ContainsKey(targetCurrency))
                {
                    var rate = exchangeRates.Rates[targetCurrency];
                    _dispatcher.Invoke(() => OnRateUpdated?.Invoke(rate));
                    return rate;
                }
                else
                {
                    throw new Exception("Currency rate not found.");
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request-specific exceptions
                throw new Exception("Error while fetching currency rate.", ex);
            }
        }

        public class ExchangeRateResponse
        {
            public bool Success { get; set; }
            public Dictionary<string, double> Rates { get; set; }
        }
    }
}
