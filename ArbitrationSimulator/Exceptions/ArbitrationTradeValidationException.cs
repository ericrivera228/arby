using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArbitrationSimulator.Exceptions
{
    public class ArbitrationTradeValidationException : Exception
    {
        public string ExchangeBalanceDetailMessage;

        public ArbitrationTradeValidationException(string errorMessage, string exchangeBalanceDetailMessage)
            : base(errorMessage)
        {
            ExchangeBalanceDetailMessage = exchangeBalanceDetailMessage;
        }
    }
}
