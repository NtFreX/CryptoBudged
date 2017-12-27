using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using CryptoBudged.Models;

namespace CryptoBudged.Views.Widgets
{
    /// <summary>
    /// Interaction logic for TradingViewWidget.xaml
    /// </summary>
    public partial class TradingViewWidget : UserControl
    {
        public bool TradingPairNotFound
        {
            get => (bool)GetValue(TradingPairNotFoundProperty);
            set => SetValue(TradingPairNotFoundProperty, value);
        }

        public string CurrencyShortName
        {
            get => (string)GetValue(CurrencyShortNameProperty);
            set => SetValue(CurrencyShortNameProperty, value);
        }

        public static readonly DependencyProperty TradingPairNotFoundProperty =
            DependencyProperty.Register(nameof(TradingPairNotFound), typeof(bool), typeof(TradingViewWidget));


        public static readonly DependencyProperty CurrencyShortNameProperty =
            DependencyProperty.Register(nameof(CurrencyShortName), typeof(string), typeof(TradingViewWidget), new FrameworkPropertyMetadata(CurrencyShortNamePropertyChanged));

        private static void CurrencyShortNamePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is TradingViewWidget tradingViewWidget))
                return;

            var shortName = dependencyPropertyChangedEventArgs.NewValue.ToString().ToLower() + "usd";
            var platform = tradingViewWidget.tradingPairs.FirstOrDefault(x => x.Value.Contains(shortName.ToUpper()));
            if (string.IsNullOrEmpty(platform.Key))
            {
                tradingViewWidget.TradingPairNotFound = true;
                return;
            }
            tradingViewWidget.TradingPairNotFound = false;

            tradingViewWidget.WebBrowser.Address = $"https://cryptowat.ch/{platform.Key.ToLower()}/{shortName}";
        }

        public TradingViewWidget()
        {
            TradingPairNotFound = false;

            InitializeComponent();
        }


        private readonly Dictionary<string /* exchange */, List<string> /* trading pairs*/> tradingPairs =
            new Dictionary<string, List<string>>
            {
                { "Poloniex", new []
                    {
                        "AMPBTC",
                        "ARDRBTC",
                        "BCHBTC",
                        "BCHETH",
                        "BCHUSDT",
                        "BCNBTC",
                        "BCNXMR",
                        "BCYBTC",
                        "BELABTC",
                        "BLKBTC",
                        "BLKXMR",
                        "BTCDBTC",
                        "BTCDXMR",
                        "BTCUSDT",
                        "BTMBTC",
                        "BTSBTC",
                        "BURSTBTC",
                        "CLAMBTC",
                        "CVCBTC",
                        "CVCETH",
                        "DAOBTC",
                        "DAOETH",
                        "DASHBTC",
                        "DASHUSDT",
                        "DASHXMR",
                        "DCRBTC",
                        "DGBBTC",
                        "DOGEBTC",
                        "EMC2BTC",
                        "ETCBTC",
                        "ETCETH",
                        "ETCUSDT",
                        "ETHBTC",
                        "ETHUSDT",
                        "EXPBTC",
                        "FCTBTC",
                        "FLDCBTC",
                        "FLOBTC",
                        "GAMEBTC",
                        "GASBTC",
                        "GASETH",
                        "GNOBTC",
                        "GNOETH",
                        "GNTBTC",
                        "GNTETH",
                        "GRCBTC",
                        "HUCBTC",
                        "LBCBTC",
                        "LSKBTC",
                        "LSKETH",
                        "LTCBTC",
                        "LTCUSDT",
                        "LTCXMR",
                        "MAIDBTC",
                        "MAIDXMR",
                        "NAUTBTC",
                        "NAVBTC",
                        "NEOSBTC",
                        "NMCBTC",
                        "NOTEBTC",
                        "NXCBTC",
                        "NXTBTC",
                        "NXTUSDT",
                        "NXTXMR",
                        "OMGBTC",
                        "OMGETH",
                        "OMNIBTC",
                        "PASCBTC",
                        "PINKBTC",
                        "POTBTC",
                        "PPCBTC",
                        "RADSBTC",
                        "REPBTC",
                        "REPETH",
                        "REPUSDT",
                        "RICBTC",
                        "SBDBTC",
                        "SCBTC",
                        "SDCBTC",
                        "SJCXBTC",
                        "STEEMBTC",
                        "STEEMETH",
                        "STORJBTC",
                        "STRATBTC",
                        "STRBTC",
                        "STRUSDT",
                        "SYSBTC",
                        "VIABTC",
                        "VRCBTC",
                        "VTCBTC",
                        "XBCBTC",
                        "XCPBTC",
                        "XEMBTC",
                        "XMRBTC",
                        "XMRUSDT",
                        "XPMBTC",
                        "XRPBTC",
                        "XRPUSDT",
                        "XVCBTC",
                        "ZECBTC",
                        "ZECETH",
                        "ZECUSDT",
                        "ZECXMR",
                        "ZRXBTC",
                        "ZRXETH"
                    }.ToList()
                },
                { "Bitfinex", new []
                    {
                        "AVTBTC",
                        "AVTETH",
                        "AVTUSD",
                        "BCCBTC",
                        "BCCUSD",
                        "BCHBTC",
                        "BCHETH",
                        "BCHUSD",
                        "BCUBTC",
                        "BCUUSD",
                        "BT1BTC",
                        "BT1USD",
                        "BT2BTC",
                        "BT2USD",
                        "BTCEUR",
                        "BTCUSD",
                        "BTGBTC",
                        "BTGUSD",
                        "DASHBTC",
                        "DASHUSD",
                        "DATABTC",
                        "DATAETH",
                        "DATAUSD",
                        "DATBTC",
                        "DATETH",
                        "DATUSD",
                        "EDOBTC",
                        "EDOETH",
                        "EDOUSD",
                        "EOSBTC",
                        "EOSETH",
                        "EOSUSD",
                        "ETCBTC",
                        "ETCUSD",
                        "ETHBTC",
                        "ETHUSD",
                        "ETPBTC",
                        "ETPETH",
                        "ETPUSD",
                        "IOTBTC",
                        "IOTETH",
                        "IOTUSD",
                        "LTCBTC",
                        "LTCUSD",
                        "NEOBTC",
                        "NEOETH",
                        "NEOUSD",
                        "OMGBTC",
                        "OMGETH",
                        "OMGUSD",
                        "QTUMBTC",
                        "QTUMETH",
                        "QTUMUSD",
                        "SANBTC",
                        "SANETH",
                        "SANUSD",
                        "XMRBTC",
                        "XMRUSD",
                        "XRPBTC",
                        "XRPUSD",
                        "ZECBTC",
                        "ZECUSD"
                    }.ToList()
                },
                {
                    "Kraken", new []
                    {
                        "BCHBTC",
                        "BCHEUR",
                        "BCHUSD",
                        "BTCCAD",
                        "BTCEUR",
                        "BTCGBP",
                        "BTCJPY",
                        "BTCUSD",
                        "DASHBTC",
                        "DASHEUR",
                        "DASHUSD",
                        "DOGEBTC",
                        "EOSBTC",
                        "EOSETH",
                        "ETCBTC",
                        "ETCETH",
                        "ETCEUR",
                        "ETCUSD",
                        "ETHBTC",
                        "ETHCAD",
                        "ETHEUR",
                        "ETHGBP",
                        "ETHJPY",
                        "ETHUSD",
                        "GNOBTC",
                        "GNOETH",
                        "GNOEUR",
                        "GNOUSD",
                        "ICNBTC",
                        "ICNETH",
                        "LTCBTC",
                        "LTCCAD",
                        "LTCEUR",
                        "LTCUSD",
                        "MLNBTC",
                        "MLNETH",
                        "REPBTC",
                        "REPCAD",
                        "REPETH",
                        "REPEUR",
                        "REPJPY",
                        "REPUSD",
                        "STRBTC",
                        "STREUR",
                        "STRUSD",
                        "USDTUSD",
                        "XMRBTC",
                        "XMREUR",
                        "XMRUSD",
                        "XRPBTC",
                        "XRPCAD",
                        "XRPEUR",
                        "XRPJPY",
                        "XRPUSD",
                        "ZECBTC",
                        "ZECCAD",
                        "ZECEUR",
                        "ZECGBP",
                        "ZECJPY",
                        "ZECUSD"
                    }.ToList()
                },
                {
                    "Bittrex", new []
                    {
                        "BATBTC",
                        "BCCBTC",
                        "BCCUSDT",
                        "BLKBTC",
                        "BTCUSDT",
                        "DCRBTC",
                        "EDGBTC",
                        "ETCBTC",
                        "ETCETH",
                        "ETCUSDT",
                        "ETHBTC",
                        "ETHUSDT",
                        "NEOBTC",
                        "NEOETH",
                        "NEOUSDT",
                        "OMGBTC",
                        "OMGETH",
                        "QTUMBTC",
                        "STRATBTC",
                        "VRCBTC",
                        "VRMBTC",
                        "VTCBTC",
                        "XRPUSDT",
                        "ZECUSDT",
                        "ZENBTC"
                    }.ToList()
                },
                {
                    "Quoine", new []
                    {
                        "BCHJPY",
                        "BCHSGD",
                        "BCHUSD",
                        "BTCAUD",
                        "BTCCNY",
                        "BTCEUR",
                        "BTCHKD",
                        "BTCIDR",
                        "BTCINR",
                        "BTCJPY",
                        "BTCPHP",
                        "BTCSGD",
                        "BTCUSD",
                        "ETHAUD",
                        "ETHBTC",
                        "ETHCNY",
                        "ETHEUR",
                        "ETHHKD",
                        "ETHIDR",
                        "ETHINR",
                        "ETHJPY",
                        "ETHPHP",
                        "ETHSGD",
                        "ETHUSD"
                    }.ToList()
                },
                {
                    "BitMEX", new []
                    {
                        "BCHBTC",
                        "BCHBTC",
                        "BTCJPY",
                        "BTCJPY",
                        "BTCUSD",
                        "BTCUSD",
                        "BTCUSD",
                        "DASHBTC",
                        "DASHBTC",
                        "ETCBTC",
                        "ETHBTC",
                        "ETHBTC",
                        "LTCBTC",
                        "LTCBTC",
                        "XMRBTC",
                        "XMRBTC",
                        "XRPBTC",
                        "XRPBTC",
                        "XTZBTC",
                        "XTZBTC",
                        "XTZBTC",
                        "ZECBTC",
                        "ZECBTC"
                    }.ToList()
                },
                {
                    "BTC-e", new []
                    {
                        "BCHBTC",
                        "BCHUSD",
                        "BTCEUR",
                        "BTCRUR",
                        "BTCUSD",
                        "ETHBTC",
                        "ETHEUR",
                        "ETHLTC",
                        "ETHRUR",
                        "ETHUSD",
                        "LTCBTC",
                        "LTCUSD",
                        "NMCUSD",
                        "PPCUSD",
                        "ZECBTC",
                        "ZECUSD"
                    }.ToList()
                },
                {
                    "Bitstamp", new []
                    {
                        "BCHBTC",
                        "BCHEUR",
                        "BCHUSD",
                        "BTCEUR",
                        "BTCUSD",
                        "ETHBTC",
                        "ETHEUR",
                        "ETHUSD",
                        "EURUSD",
                        "LTCBTC",
                        "LTCEUR",
                        "LTCUSD",
                        "XRPBTC",
                        "XRPEUR",
                        "XRPUSD"
                    }.ToList()
                },
                {
                    "Bithumb", new []
                    {
                        "BCHKRW",
                        "BTCKRW",
                        "BTGKRW",
                        "DASHKRW",
                        "ETCKRW",
                        "ETHKRW",
                        "LTCKRW",
                        "QTUMBTC",
                        "QTUMETH",
                        "QTUMKRW",
                        "XMRKRW",
                        "XRPKRW",
                        "ZECKRW"
                    }.ToList()
                },
                {
                    "GDAX", new []
                    {
                        "BCHBTC",
                        "BCHEUR",
                        "BCHUSD",
                        "BTCEUR",
                        "BTCGBP",
                        "BTCUSD",
                        "ETHBTC",
                        "ETHEUR",
                        "ETHUSD",
                        "LTCBTC",
                        "LTCEUR",
                        "LTCUSD"
                    }.ToList()
                },
                {
                    "OKCoin", new []
                    {
                        "BTCCNY",
                        "BTCUSD",
                        "BTCUSD",
                        "BTCUSD",
                        "BTCUSD",
                        "LTCCNY",
                        "LTCUSD",
                        "LTCUSD",
                        "LTCUSD",
                        "LTCUSD"
                    }.ToList()
                },
                {
                    "CEX.IO", new []
                    {
                        "BCHUSD",
                        "BTCEUR",
                        "BTCUSD",
                        "ETHBTC",
                        "ETHUSD",
                        "LTCBTC",
                        "LTCUSD"
                    }.ToList()
                },
                {
                    "bitFlyer", new []
                    {
                        "BCHBTC",
                        "BTCJPY",
                        "BTCJPY",
                        "BTCJPY",
                        "BTCJPY",
                        "BTCUSD",
                        "ETHBTC"
                    }.ToList()
                },
                {
                    "Qryptos", new []
                    {
                        "ETCBTC",
                        "ETHBTC",
                        "LTCBTC",
                        "REPBTC",
                        "XMRBTC",
                        "XRPBTC",
                        "ZECBTC"
                    }.ToList()
                },
                {
                    "QuadrigaCX", new[]
                    {
                        "BCHCAD",
                        "BTCCAD",
                        "BTCUSD",
                        "BTGCAD",
                        "ETHCAD",
                        "ETHUSD",
                        "LTCCAD"
                    }.ToList()
                },
                {
                    "Bitsquare", new []
                    {
                        "BTCAUD",
                        "BTCEUR",
                        "BTCUSD",
                        "ETCBTC",
                        "ETHBTC",
                        "LTCBTC"
                    }.ToList()
                },
                {
                    "Gemini", new []
                    {
                        "BTCUSD",
                        "ETHBTC",
                        "ETHUSD"
                    }.ToList()
                },
                {
                    "Luno", new []
                    {
                        "BTCZAR",
                        "ETHBTC"
                    }.ToList()
                }
            };
    }
}
