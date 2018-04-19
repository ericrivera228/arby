using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BtcE;

namespace ApiTest
{
	class Program
	{
		static void Main(string[] args) {
            //var depth3 = BtceApiV3.GetDepth(new BtcePair[] { BtcePair.btc_usd });
            //var ticker3 = BtceApiV3.GetTicker(new BtcePair[] { BtcePair.btc_usd });
            //var trades3 = BtceApiV3.GetTrades(new BtcePair[] { BtcePair.btc_usd });
            //var ticker = BtceApi.GetTicker(BtcePair.btc_usd);
            //var trades = BtceApi.GetTrades(BtcePair.btc_usd);
            //var btcusdDepth = BtceApi.GetDepth(BtcePair.usd_rur);
            //var fee = BtceApi.GetFee(BtcePair.usd_rur);

            var btceApi = new BtceApi("O5T8M85C-GS1WK7ZM-2DVFSGOJ-EYNIM4YX-VIHYXUWN", "c2085aacb53bbcc06e789001385722c690c8a9528d37fc78e062c1f251682a57");
            var info = btceApi.GetInfo();
            //var transHistory = btceApi.GetTransHistory();
            //var tradeHistory = btceApi.GetTradeHistory(count: 20);
            //var orderList = btceApi.GetOrderList();
            //var tradeAnswer = btceApi.Trade(BtcePair.btc_usd, TradeType.Sell, 20, 0.1m);
            //var cancelAnswer = btceApi.CancelOrder(tradeAnswer.OrderId);
		}
	}
}
