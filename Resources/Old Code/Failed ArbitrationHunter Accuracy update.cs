private struct  AmountInfo
{
	public decimal buyAmount;
	public decimal sellAmount;
}

/// <summary>
/// Determines how much btc should be traded taking into account how much btc is availabe, how much btc the given order is for,
/// and the amount of fiat available. Returns a decimal representing how much btc should be traded.
/// </summary>
/// <param name="availableBtc">Amount of btc that is available for sale in the sell exchange.</param>
/// <param name="order">Order that is to be filled.</param>
/// <param name="availableFiat">Amount of fiat available for use in the buy exchange.</param>
/// <param name="buyExchangeTradeFeeAsDecimal">Exchange fee of the buy exchange as a decimal (needed to calculate if ther is enough money
///     to for a btc purchase).</param>
/// <param name="currencyTypeBuyFeeIsAppliedTo">The currency type the buy fee is applied to at the buy exchange.</param>
/// <returns></returns>
private static AmountInfo DetermineTradeAmount(Decimal availableBtc, Order order, Decimal availableFiat, Decimal buyExchangeTradeFeeAsDecimal, CurrencyType currencyTypeBuyFeeIsAppliedTo)
{
	decimal costOfBtc;
	AmountInfo returnAmountInfo = new AmountInfo();

	returnAmountInfo.sellAmount = Math.Min(availableBtc, order.Amount);
	
	if (currencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat)
	{
		//Fee applied to fiat; buy and sell amounts are the same
		returnAmountInfo.buyAmount = returnAmountInfo.sellAmount;

		//Cost calculated with fee
		costOfBtc = Decimal.Multiply(Decimal.Multiply(returnAmountInfo.buyAmount, order.Price), Decimal.Add(buyExchangeTradeFeeAsDecimal, 1));    
	}
	else
	{
		//Fee apploied to bitcoin; need to determine how much extra to buy to take into acount fee
		returnAmountInfo.buyAmount = Decimal.Multiply(returnAmountInfo.sellAmount, Decimal.Add(1, buyExchangeTradeFeeAsDecimal));

		//Cost calcualted without fee
		costOfBtc = Decimal.Multiply(returnAmountInfo.buyAmount, order.Price);    
	}

	//Not enough fiat available to just fill the order or use all the available btc; calculate how much btc can be bought
	if (costOfBtc > availableFiat)
	{
		//Calculate the amount of btc that can be bought based upon how the buy fee is applied
		if (currencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat)
		{
			//Buy and sell amounts are the same
			decimal tradeAmount;
			
			tradeAmount = Decimal.Divide(availableFiat, Decimal.Multiply(order.Price, Decimal.Add(buyExchangeTradeFeeAsDecimal, 1)));

			returnAmountInfo.buyAmount = tradeAmount;
			returnAmountInfo.sellAmount = tradeAmount;
		}
		else
		{
			//First calculate buy amount, then need to recalculate sell amount
			returnAmountInfo.buyAmount = Decimal.Divide(availableFiat, order.Price);
			returnAmountInfo.sellAmount = Decimal.Multiply(returnAmountInfo.buyAmount, Decimal.Subtract(1, buyExchangeTradeFeeAsDecimal));
		}
	}

	return returnAmountInfo;
}

private static bool CalculateArbitration(AmountInfo tradeAmount, BaseExchange buyExchange, BaseExchange sellExchange, ref ArbitrationOpportunity returnOpportunity, Order ask, Order bid, ref decimal availableBtc, ref decimal availableFiat)
{
	decimal buyCost;

	if (buyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Bitcoin)
	{
		//Since buy fee is applied to btc, buy cost is just amount * price
		buyCost = Decimal.Multiply(tradeAmount.buyAmount, ask.Price);
	}
	else
	{
		//Since buy fee is applied to fiat, take fee into account when calculating buyCost
		buyCost = Decimal.Multiply(Decimal.Add(1.0m, buyExchange.TradeFeeAsDecimal), Decimal.Multiply(tradeAmount.buyAmount, ask.Price));      
	}

	decimal sellCost = Decimal.Multiply(Decimal.Subtract(1.0m, sellExchange.TradeFeeAsDecimal), Decimal.Multiply(tradeAmount.sellAmount, bid.Price));
	decimal profit = Decimal.Subtract(sellCost, buyCost);

	//Just to weed out arbitration opportunities whose profit is so low that they aren't practical. Note, this isn't the Minimum profit requirement,
	//to return an arbitration opportunity, this limit just keeps the algorithm from adding to an arbitration opportunity trade amounts that don't add
	//at least 0.01 profit to the overall opportunity. 
	if (profit > 0.01m)
	{
		if (returnOpportunity == null)
		{
			returnOpportunity = new ArbitrationOpportunity(buyExchange, sellExchange);
		}

		//Increase the amount of the arbitration opportunity; add the new buy and sell orders to this opportunity
		returnOpportunity.BuyAmount = Decimal.Add(returnOpportunity.BuyAmount, tradeAmount.buyAmount);
		returnOpportunity.SellAmount = Decimal.Add(returnOpportunity.SellAmount, tradeAmount.sellAmount);
		returnOpportunity.AddBuyOrder(new Order(tradeAmount.buyAmount, ask.Price, buyCost));
		returnOpportunity.AddSellOrder(new Order(tradeAmount.buyAmount, bid.Price, sellCost));

		//Decrement MaxBTC and MaxFiat; since some was used on this iteration, less will be available for use on subsequent iterations
		availableBtc = Decimal.Subtract(availableBtc, tradeAmount.sellAmount);
		availableFiat = Decimal.Subtract(availableFiat, buyCost);

		return true;
	}

	return false;
}

public ArbitrationOpportunity CalculateArbitration(BaseExchange buyExchange, BaseExchange sellExchange, decimal availableBtc, decimal availableFiat, decimal minimumProfit, bool accountForBtcTransferFee)
{
	ArbitrationOpportunity returnOpportunity = null;

	//These two lists keep track of the asks and bids lists as they are used
	List<Order> runningAskList;
	List<Order> runningBidList;

	//Only need to clone the Asks of the buy exchange (since we aren't selling there don't need the bids), and only need to clone the Bids of the sell
	//exchange (since we aren't buying there don't need the asks)
	if (buyExchange.OrderBook != null && buyExchange.OrderBook.Asks != null && sellExchange.OrderBook != null && sellExchange.OrderBook.Bids != null)
	{
		runningAskList = buyExchange.OrderBook.Asks.Clone();
		runningBidList = sellExchange.OrderBook.Bids.Clone();
	}
	else
	{
		//Defensive code: at this point, both orderbooks and their order lists shouldn't be null. But just in case, return null.
		return null;
	}

	//Continue to look for aribtration while:
	//  - The next ask has a higher price than the next bid (meaning there is a potential for arbitration)
	//  - There is btc left to use up
	//  - There is fiat left to use. Note, this is limited at 1.00 to prevent any fringe situation where this method finds arbitration with a very small
	//    amount of btc. If there is less than 1.00, just don't bother looking for arbitration as that isn't practical. This would probably be assuaged
	//    by the min profit limit anyways, but just to be safe. 
	while(runningBidList.Count > 0 && runningAskList.Count > 0 && runningBidList[0].Price > runningAskList[0].Price && availableBtc > 0 && availableFiat > 1.00m)
	{
		AmountInfo tradeAmountInfo;
		Order bid = runningBidList[0];
		Order ask = runningAskList[0];

		bool calculateProfitResult;

		//Part of the ask (buy) order has been fulfilled
		if ((buyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat && ask.Amount > bid.Amount) || (buyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Bitcoin && ask.Amount > Decimal.Multiply(bid.Amount, Decimal.Add(1, buyExchange.TradeFeeAsDecimal))))
		{
			tradeAmountInfo = DetermineTradeAmount(availableBtc, bid, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo);
			calculateProfitResult = CalculateArbitration(tradeAmountInfo, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

			if (calculateProfitResult)
			{
				ask.Amount = decimal.Subtract(ask.Amount, tradeAmountInfo.buyAmount);

				//If the entire bid order was filled, remove it
				if (tradeAmountInfo.sellAmount >= bid.Amount)
				{
					runningBidList.RemoveAt(0);
				}
			}
		}

		//All of the ask (buy) order has been filled
		{
			tradeAmountInfo = DetermineTradeAmount(availableBtc, ask, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo);
			calculateProfitResult = CalculateArbitration(tradeAmountInfo, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

			if (calculateProfitResult)
			{
				bid.Amount = Decimal.Subtract(bid.Amount, tradeAmountInfo.sellAmount);

				//If the entire ask order was filled, remove it
				if (tradeAmountInfo.buyAmount >= ask.Amount)
				{
					runningAskList.RemoveAt(0);
				}
			}
		}

		//The ask and buy orders are the same amount so the both get filled
		else
		{
			tradeAmountInfo = DetermineTradeAmount(availableBtc, ask, availableFiat, buyExchange.TradeFeeAsDecimal, buyExchange.CurrencyTypeBuyFeeIsAppliedTo);
			calculateProfitResult = CalculateArbitration(tradeAmountInfo, buyExchange, sellExchange, ref returnOpportunity, ask, bid, ref availableBtc, ref availableFiat);

			if (calculateProfitResult)
			{
				//If both orders were wholly filled, delete them.
				if (tradeAmountInfo.buyAmount >= ask.Amount)
				{
					runningAskList.RemoveAt(0);
				}

				if(tradeAmountInfo.sellAmount >= bid.Amount)
				{
					runningBidList.RemoveAt(0);
				}
			}
		}

		//If no profit could be found, exit the while loop
		if (!calculateProfitResult)
		{
			break;
		}

	}

	//An oppportunity was found, now see if it meets the minimum profit requirements
	if (returnOpportunity != null)
	{
		if (returnOpportunity.BuyExchangeOrderBook == null || returnOpportunity.SellExchangeOrderBook == null)
		{
			//If arbitration can occur, add the order books to returnOpportunity for recordkeeping and trouble shooting
			returnOpportunity.BuyExchangeOrderBook = buyExchange.OrderBook;
			returnOpportunity.SellExchangeOrderBook = sellExchange.OrderBook; 
		}

		returnOpportunity.CalculateArbitrationOpportunityCosts();

		//Arbitration calculation finished, now take into account the transfer trade fee if specified:
		if (accountForBtcTransferFee)
		{
			//Calculate the average btc cost in fiat
			decimal averageCost = (returnOpportunity.BuyPrice + returnOpportunity.SellPrice) / 2;

			//Using the average buy/sell cost, determine if the opportunity is stil profitable after losing that amount of btc in dollars.
			returnOpportunity.Profit -= returnOpportunity.BuyExchange.BtcTransferFee * averageCost;
		}

		//Now that arbitration has been fully calculated between the two exchange, and the opportunity has the required minimum profit, see if the buy and sell orders are valid:
		if (returnOpportunity.Profit >= minimumProfit)
		{
			Order buyOrder = new Order(returnOpportunity.BuyAmount, returnOpportunity.BuyPrice);
			Order sellOrder = new Order(returnOpportunity.BuyAmount, returnOpportunity.SellPrice);

			if (returnOpportunity.BuyExchange.IsOrderValidForExchange(buyOrder) && returnOpportunity.SellExchange.IsOrderValidForExchange(sellOrder))
			{
				return returnOpportunity;   
			}
		}
	}
			   
	return null;    
}