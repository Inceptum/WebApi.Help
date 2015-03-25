using System;
using System.IO;

namespace Inceptum.WebApi.Help.Extensions
{
    internal static class StreamExtensions
    {
        public static byte[] ReadAll(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}