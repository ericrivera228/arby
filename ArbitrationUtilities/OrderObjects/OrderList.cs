using System.Collections.Generic;

namespace ArbitrationUtilities.OrderObjects
{
    /// <summary>
    /// Class to represent a list of orders. It is a List of Type Order that has a clone method.
    /// </summary>
    public class OrderList : List<Order>
    {
        /// <summary>
        /// Returns a deep copy of the this order list.
        /// </summary>
        /// <returns></returns>
        public OrderList Clone()
        {
            OrderList cloneList = new OrderList();

            foreach (Order order in this)
            {
                //Create new orders for deep cloning of this OrderList.
                cloneList.Add(new Order(order.Amount, order.Price));
            }

            return cloneList;
        }
    }
}
