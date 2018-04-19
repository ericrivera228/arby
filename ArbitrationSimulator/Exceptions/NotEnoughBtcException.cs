using System;

namespace ArbitrationSimulator.Exceptions
{
    /// <summary>
    /// Nothing special about this exception type. It was just created to for proper error detecting.
    /// </summary>
    public class NotEnoughBtcException : Exception
    {
        public NotEnoughBtcException(string message) : base(message){}
    }
}
