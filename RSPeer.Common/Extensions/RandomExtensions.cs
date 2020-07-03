using System;
using System.Security.Cryptography;

namespace RSPeer.Common.Extensions
{
    public static class RandomExtensions
    {
        public static int RandomNumber(int length = 16)
        {
            var provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[length];
            provider.GetBytes(byteArray);
            return BitConverter.ToInt32(byteArray, 0);
        }
    }
}