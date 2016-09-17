using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Squad.Admin.Rcon
{
    /// <summary>
    /// References a method to be called when an exception occurs.
    /// </summary>
    /// <param name="ex">Thrown exception.</param>
    public delegate void ErrorCallback(Exception ex);
    /// <summary>
    /// References a method to be called when an attempt to perform some action is made.
    /// </summary>
    /// <param name="attempt"></param>
    public delegate void AttemptCallback(int attempt);

    internal static class Util
    {

        internal static int[] ShipIds = new int[] { 2400, 2401, 2402, 2412, 2430, 2406, 2405 };

        internal static string BytesToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        internal static string BytesToString(byte[] bytes, int index, int count)
        {
            // Determine if the byte array represents a UTF8 encoded response or a Unicode response

            // None of the first 10 elements in the array of bytes should be a 0, especially the odd elements
            // Use lamda expression and the modulus function to return only odd elements from the array of the return
            byte[] oddElements = bytes.Where((value, i) => i % 2 == 1 && i < 10).ToArray();
            // Now convert to int
            int[] bytesAsInts = oddElements.Select(x => (int)x).ToArray();
            int val = bytesAsInts.Sum();

            // A value of 0 will be unicode, otherwise it is UTF8
            if (val == 0)
            {
                return Encoding.Unicode.GetString(bytes, index, count);
            }
            else
            {
                return Encoding.UTF8.GetString(bytes, index, count);
            }
        }

        internal static byte[] StringToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        internal static byte[] MergeByteArrays(byte[] array1, byte[] array2)
        {
            byte[] newArray = new byte[array1.Length + array2.Length];
            Buffer.BlockCopy(array1, 0, newArray, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, newArray, array1.Length, array2.Length);
            return newArray;
        }

        internal static byte[] MergeByteArrays(byte[] array1, byte[] array2, byte[] array3)
        {
            byte[] newArray = new byte[array1.Length + array2.Length + array3.Length];
            Buffer.BlockCopy(array1, 0, newArray, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, newArray, array1.Length, array2.Length);
            Buffer.BlockCopy(array3, 0, newArray, array1.Length + array2.Length, array3.Length);
            return newArray;
        }

        internal static IPEndPoint ToIPEndPoint(string endPointStr)
        {
            IPEndPoint iPEndPoint = null;
            IPAddress address;
            int port;
            string[] endpoints = endPointStr.Split(':');
            if (endpoints.Length == 2 && IPAddress.TryParse(endpoints[0], out address) && int.TryParse(endpoints[1], out port))
                iPEndPoint = new IPEndPoint(address, port);
            return iPEndPoint;
        }
    }
}
