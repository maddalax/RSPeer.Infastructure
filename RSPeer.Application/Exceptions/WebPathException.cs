using System;

namespace RSPeer.Application.Exceptions
{
    public class WebPathException : Exception
    {
        public WebPathException(string message) : base(message)
        {
        }
    }
}