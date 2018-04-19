using System;

namespace ArbitrationUtilities.OrderObjects
{
    public class Order
    {
        private decimal _amount;
        private decimal _price;
        private decimal _worth;

        public decimal Amount
        {
            get
            {
                return _amount;
            }

            set
            {
                _amount = value;

                _worth = _amount*_price;
            }
        }

        public decimal Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
            }
        }

        public decimal Worth
        {
            get
            {
                return _worth;
            }
            set
            {
                _worth = value;
            }
        }

        /// <summary>
        /// Worth is calculated from the given amount and price.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        public Order(decimal amount, decimal price)
        {
            _amount = amount;
            _price = price;
            _worth = amount*price;
        }

        /// <summary>
        /// Overloaded constructor that includes worth. This allows the worth to be calculated with an exchange fee.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        /// <param name="worth"></param>
        public Order(decimal amount, decimal price, decimal worth)
        {
            _amount = amount;
            _price = price;
            _worth = worth;
        }

        public void ConvertPrice(decimal converstionRate)
        {
            _price = Decimal.Multiply(Price, converstionRate);
        }

        public override string ToString()
        {
            return "Price : " + Price + ", Amount : " + Amount;
        }

        public Order Clone()
        {
            return new Order(_amount, _price, _worth);
        }
    }
}
