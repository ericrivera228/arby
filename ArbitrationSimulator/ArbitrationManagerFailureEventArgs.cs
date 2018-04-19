namespace ArbitrationSimulator
{
    public class ArbitrationManagerFailureEventArgs
    {
            private readonly string message;

            public ArbitrationManagerFailureEventArgs(string test)
            {
               message = test;
            }

            public string Message
            {
                get { return message; }
            }
    }
}
