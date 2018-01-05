using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CryptoBudged.Extensions;
using CryptoBudged.Services.CurrencyExchange;
using CryptoBudged.ThirdPartyApi;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;

namespace CryptoBudged.Views
{
    public class MainWindowViewModel : BindableBase
    {
        private bool _isLoading;
        private bool _hasError;
        private string _loadingText;
        private string _errorText;

        public string ErrorText
        {
            get => _errorText;
            set => SetProperty(ref _errorText, value);
        }
        public string LoadingText
        {
            get => _loadingText;
            set => SetProperty(ref _loadingText, value);
        }
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public DelegateCommand SettingsCommdand { get; }
        public DelegateCommand InformationCommand { get; }

        public MainWindowViewModel()
        {
            HasError = false;

            InformationCommand = new DelegateCommand(ExecuteInformationCommandAsync);
            SettingsCommdand = new DelegateCommand(ExecuteSettingsCommdand);

            //Task.Run(async () =>
            //{
            //    try
            //    {
            //        var encryptor = new HMACSHA256(Encoding.UTF8.GetBytes("?"));
            //        using (var client = new HttpClient())
            //        {
            //            client.DefaultRequestHeaders.Add("X-MBX-APIKEY", "?");

            //            var symbols = JObject.Parse(await BinanceApi.Instance.GetExchangeInfoAsync()).Value<JArray>("symbols").Select(x => x.Value<string>("symbol"));
            //            var trades = new List<string>();
            //            foreach (var symbol in symbols)
            //            {

            //                var now = DateTime.UtcNow.ToUnixTimeMilliseconds();
            //                var uri = new Uri($"https://www.binance.com/api/v3/myTrades?symbol={symbol}&timestamp={now}&recvWindow=60000");
            //                var signature = ByteToString(encryptor.ComputeHash(Encoding.UTF8.GetBytes(uri.Query.Replace("?", ""))));
            //                var url = uri + "&signature=" + signature;

            //                var request = new HttpRequestMessage(HttpMethod.Get, url);
            //                var response = await client.SendAsync(request);
            //                trades.Add(await response.Content.ReadAsStringAsync());
            //            }
            //        }
            //    }
            //    catch
            //    {
                    
            //    }
            //});

            if (!CurrencyExchangeFactory.Instance.IsInitialized)
            {
                IsLoading = true;

                var dispatcher = Dispatcher.CurrentDispatcher;
                var lastProgressReport = DateTime.MinValue;
                void ProgressCallback(double progress)
                {
                    if (lastProgressReport <= DateTime.Now - TimeSpan.FromSeconds(1))
                    {
                        dispatcher.Invoke(() =>
                        {
                            LoadingText = $"Loading historical exchange rates {progress:0.00}%...";
                        });

                        lastProgressReport = DateTime.Now;
                    }
                }

                Task.Run(async () =>
                {
                    try
                    {
                        ProgressCallback(0.00);
                        await CurrencyExchangeFactory.Instance.InitializeAsync(ProgressCallback);
                    }
                    catch (Exception exce)
                    {
                        dispatcher.Invoke(() =>
                        {
                            HasError = false;
                            ErrorText = $"Error during loading of historical exchange rates ({exce.Message})";
                        });
                    }
                    finally
                    {
                        dispatcher.Invoke(() => IsLoading = false);
                    }
                });
            }
        }

        private string ByteToString(byte[] buff)
        {
            var sbinary = "";
            foreach (byte t in buff)
                sbinary += t.ToString("X2"); /* hex format */
            return sbinary;
        }

        private void ExecuteSettingsCommdand()
        { }

        private async void ExecuteInformationCommandAsync()
        {
            await DialogHost.Show(new InfoDialog());
        }
    }
}
