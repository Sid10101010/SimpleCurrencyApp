using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleCurrencyApp.classes
{
    public class CurrencyLoader
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CurrencyLoader(string baseUrl, string apiKey)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl;
            _apiKey = apiKey;
        }


        private async Task<RatesDto> LoadRatesDataAsync()
        {
            string url = $"{_baseUrl}/latest.json?app_id={_apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            RatesDto result = JsonSerializer.Deserialize<RatesDto>(responseBody, _options) ?? new();
            return result;
        }

        private async Task<Dictionary<string, string>> LoadCurrencyInfoAsync()
        {
            string url = $"{_baseUrl}/currencies.json?app_id={_apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Dictionary<string, string> result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody, _options) ?? new();
            return result;
        }

        public async Task<CurrencyConverter> LoadCurrencyDataAsync()
        {
            RatesDto rates = await LoadRatesDataAsync();
            Dictionary<string, string> currencyData = await LoadCurrencyInfoAsync();
            Dictionary<string, Currency> currencies = new();
            foreach (var rate in rates.Rates) {
                string currencyName = currencyData[rate.Key];
                Currency currency = new(rate.Key, currencyName, rate.Value);
                currencies.Add(rate.Key, currency);
            }
            CurrencyConverter converter = new(currencies);
            return converter;
        }

    }
}
