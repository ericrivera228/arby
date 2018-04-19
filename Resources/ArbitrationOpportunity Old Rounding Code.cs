        //private void RoundAmounts()
        //{
        //    //In the case of fee being applied to fiat, then the buy and sell amounts are the same. Thus, round them
        //    //to the same number of decimal places.
        //    if (BuyExchange.CurrencyTypeBuyFeeIsAppliedTo == CurrencyType.Fiat)
        //    {
        //        //Round the trade amount to the lowest of the two two exchanges
        //        int decimalPlaces = Math.Min(BuyExchange.AmountDecimalPlace, SellExchange.AmountDecimalPlace);

        //        //Round the buy amount.
        //        BuyAmount = RoundAmount(BuyAmount, decimalPlaces, _buyOrderList, "floor");
        //        SellAmount = RoundAmount(SellAmount, decimalPlaces, _sellOrderList, "floor");
        //    }

        //    //In the case of fee being applied to btc
        //    else
        //    {
        //        //Sell exchange has less decimal places
        //        if (SellExchange.AmountDecimalPlace < BuyExchange.AmountDecimalPlace)
        //        {
        //            //Round the sell amount
        //            SellAmount = RoundAmount(SellAmount, SellExchange.AmountDecimalPlace, _sellOrderList, "floor");

        //            //Recalculate the buy amount
        //            decimal newBuyAmount = Decimal.Multiply(SellAmount, (Decimal.Add(1, BuyExchange.TradeFeeAsDecimal)));

        //            BuyAmount = RoundAmount(SellAmount, newBuyAmount, BuyExchange.AmountDecimalPlace, _buyOrderList, "regular");
        //        }

        //        else
        //        {
        //            //Round the buy amount
        //            BuyAmount = RoundAmount(BuyAmount, BuyExchange.AmountDecimalPlace, _buyOrderList, "floor");

        //            //Recaluate the sell amount based upon the new buy amount
        //            decimal newSellAmount = Decimal.Multiply(BuyAmount, (Decimal.Subtract(1, BuyExchange.TradeFeeAsDecimal)));

        //            SellAmount = RoundAmount(SellAmount, newSellAmount, SellExchange.AmountDecimalPlace, _sellOrderList, "regular");
        //        }
        //    }
        //}

        //private decimal RoundAmount(decimal originalAmount, int decimalPlaces, OrderList orderList, string roundMethod)
        //{
        //    return RoundAmount(originalAmount, originalAmount, decimalPlaces, orderList, roundMethod);
        //}

        //private decimal RoundAmount(decimal originalAmount, decimal newAmountToRound, int decimalPlaces, OrderList orderList, string roundMethod)
        //{
        //    decimal newAmount;

        //    if (roundMethod == "floor")
        //    {
        //        newAmount = MathHelpers.FloorRound(newAmountToRound, decimalPlaces);    
        //    }
        //    else
        //    {
        //        newAmount = Math.Round(newAmountToRound, decimalPlaces);    
        //    }
            
        //    //Determine how much btc amount needs to be left off this order
        //    decimal choppedAmount = originalAmount - newAmount;

        //    //If there is an amount of btc to be lost from this trade because of the new calculation, remove it 
        //    if (choppedAmount > 0m)
        //    {
        //        RemoveAmountFromOrderList(choppedAmount, orderList);
        //    }
        //    else if (choppedAmount < 0m)
        //    {
        //        //Add the chopped amount to the last part of the order
        //        orderList[orderList.Count - 1].Amount = orderList[orderList.Count - 1].Amount + (-1*choppedAmount);
        //    }

        //    return newAmount;
        //}

        ///// <summary>
        ///// Removes the specified amount of btc from the components orders of the given order list, starting from the bottom of the lists.
        ///// </summary>
        ///// <param name="amountToRemove">Amount of btc to remove from each side of this arbitration trade.</param>
        ///// <param name="orderList">List of orders to remove the specified amount from.</param>
        //private void RemoveAmountFromOrderList(decimal amountToRemove, OrderList orderList)
        //{
        //    //Loop through the list of order components and remove the amount that is being chopped
        //    //from the round.
        //    while (amountToRemove > 0)
        //    {
        //        //Get the last order from the list
        //        Order lastOrder = orderList.Last();

        //        //Remove choppedAmount from the buy and sell sides

        //        //If the amount to be lost is greater than the order at the bottom of the list, 
        //        //remove that order.
        //        //Comparison is >= so that when the amount to be chopped is the same as the amount of the order, that order is removed.
        //        if (amountToRemove >= lastOrder.Amount)
        //        {
        //            amountToRemove -= lastOrder.Amount;
        //            orderList.RemoveAt(orderList.Count - 1);
        //        }

        //        //Order at the bottom of the tier is larger than the amount that is lost due to rounding;
        //        //remove chopped amount from only order.
        //        else
        //        {
        //            lastOrder.Amount -= amountToRemove;

        //            //Recalculate the worth for the order with the trade fee multiplier
        //            //lastOrder.Worth = lastOrder.Amount*lastOrder.Price*tradeFeeMultiplier;
        //            lastOrder.Worth = lastOrder.Amount * lastOrder.Price;

        //            //Amount has been subtracted, break from the loop
        //            break;
        //        }
        //    }
        //}