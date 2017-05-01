namespace Ereadian.PowerPointRemoteController.Client
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class Channel : IDisposable
    {
        private readonly string RemoteAddress;
        private readonly int RemotePort;
        private readonly Action<string> ShowMessageFunction;

        private Socket clientSocket;

        public Channel(string address, int port, Action<string> onShowMessage)
        {
            this.RemoteAddress = address;
            this.RemotePort = port;
            this.ShowMessageFunction = onShowMessage;

            this.clientSocket = null;
        }

        private Socket ClientSocket
        {
            get
            {
                return null;
            }
        }

        public void Dispose()
        {
        }

        private Socket GetSocket()
        {
        }
    }
}

