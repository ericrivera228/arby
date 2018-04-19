using System;
using System.Collections.Generic;
using System.Text;

namespace ArbitrationUtilities.OrderObjects
{
    public class OrderBook
    {
        private OrderList _bids;
        private OrderList _asks;

        #region Property Getters and Setters
        public OrderList Bids
        {
            get
            {
                return _bids;
            }
            set
            {
                _bids = value;
            }
        }

        public OrderList Asks
        {
            get
            {
                return _asks;
            }
            set
            {
                _asks = value;
            }
        }
        #endregion

        public OrderBook()
        {
            _bids = new OrderList();
            _asks = new OrderList();
        }

        public void ClearOrderBook()
        {
            if (_bids != null)
            {
                _bids.Clear();
            }

            if (_asks != null)
            {
                _asks.Clear();
            }
        }

        public override string ToString()
        {
            StringBuilder stringbuilder = new StringBuilder("");

            if(_bids != null && _bids.Count > 0)
            {
                stringbuilder.Append("Bids: " + Environment.NewLine);

                foreach (Order order in _bids)
                {
                    stringbuilder.Append("\t" + order.ToString() + Environment.NewLine);
                }
            }

            if(_asks != null && _asks.Count > 0)
            {
                stringbuilder.Append("Asks: " + Environment.NewLine);
    
                foreach (Order order in _asks)
                {
                    stringbuilder.Append("\t" + order.ToString() + Environment.NewLine);
                }
            }

            return stringbuilder.ToString();
        }

        public string AsksToString()
        {
            return OrderTypeToString("asks");
        }

        public string BidsToString()
        {
            return OrderTypeToString("bids");
        }

        private string OrderTypeToString(string type)
        {
            StringBuilder returnString = new StringBuilder();
            List<Order> orderList;

            if(type.Equals("asks", StringComparison.InvariantCultureIgnoreCase))
            {
                orderList = _asks;
            }

            else if(type.Equals("bids", StringComparison.InvariantCultureIgnoreCase))
            {
                orderList = _bids;
            }

            else
            {
                throw new Exception("Invalid type given to OrderTypeToString. Type passed was: " + type + ".");
            }

            if (orderList != null && orderList.Count > 0)
            {
                for (int counter = 0; counter < orderList.Count; counter++)
                {
                    returnString.Append("{" + orderList[counter].Price + ", " + orderList[counter].Amount + "}");

                    if (counter != orderList.Count - 1)
                    {
                        returnString.Append(",");
                    }
                }
            }

            return returnString.ToString();
        }

        public OrderBook Clone()
        {
            OrderBook cloneBook = new OrderBook();

            cloneBook.Asks = Asks.Clone();
            cloneBook.Bids = Bids.Clone();

            return cloneBook;
        }
    }
}
