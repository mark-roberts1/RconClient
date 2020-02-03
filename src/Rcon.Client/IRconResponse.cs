using System;
using System.Collections.Generic;
using System.Text;

namespace Rcon.Client
{
    public interface IRconResponse
    {
        string Text { get; }
    }

    public class RconResponse : IRconResponse
    {
        internal RconResponse(byte[] data)
        {
            Text = ASCIIEncoding.UTF8.GetString(data);
        }

        public string Text { get; }
    }
}
