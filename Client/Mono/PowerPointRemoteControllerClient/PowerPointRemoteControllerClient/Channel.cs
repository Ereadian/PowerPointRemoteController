namespace Ereadian.PowerPointRemoteController.Client
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public class Channel : IDisposable
    {
        private readonly string RemoteAddress;
        private readonly int RemotePort;
        private readonly string Name;
        private readonly Action<string> ShowMessageFunction;

        private Socket clientSocket;

        public Channel(string address, int port, string clientName, Action<string> onShowMessage)
        {
            this.RemoteAddress = address;
            this.RemotePort = port;
            this.Name = clientName;
            this.ShowMessageFunction = onShowMessage;

            this.clientSocket = null;
        }

        private Socket ClientSocket
        {
            get
            {
                if (this.clientSocket == null)
                {
                    this.clientSocket = this.CreateSocket();
                }

                return this.clientSocket;
            }
        }

        public bool Send(byte[] data)
        {
            var socket = this.ClientSocket;
            if (socket != null)
            {
                try
                {
                    socket.Send(data);
                    return true;
                }
                catch(Exception exception)
                {
                    this.ShowMessage(exception.Message);
                    this.Close();
                }
            }

            return false;
        }

        public bool Receive(byte[] data)
        {
            var socket = this.ClientSocket;
            if (socket != null)
            {
                try
                {
                    socket.Receive(data);
                    return true;
                }
                catch(Exception exception)
                {
                    this.ShowMessage(exception.Message);
                    this.Close();
                }
            }

            return false;
        }

        public void Dispose()
        {
            this.Close();
        }

        private void Close()
        {
            if (this.clientSocket != null)
            {
                try
                {
                    this.clientSocket.Dispose();
                    this.clientSocket = null;
                }
                catch
                {
                }
            }
        }

        private Socket CreateSocket()
        {
            if (string.IsNullOrEmpty(this.RemoteAddress))
            {
                this.ShowMessage("Remote address is null or empty");
                return null;
            }

            var addressList = Dns.GetHostAddresses(this.RemoteAddress);
            if ((addressList == null) || (addressList.Length < 1))
            {
                this.ShowMessage("Failed to parse remote address: {0}", this.RemoteAddress);
                return null;
            }

            var endpoint = new IPEndPoint(addressList[0], this.RemotePort);
            Socket socket = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(endpoint);
                var content = Encoding.ASCII.GetBytes(this.Name);
                var header = new byte[]{ (byte)content.Length};
                socket.Send(header);
                socket.Send(header);
            }
            catch (Exception exception)
            {
                this.ShowMessage(exception.Message);
                if (socket != null)
                {
                    socket.Dispose();
                    socket = null;
                }
            }

            return socket;
        }

        private void ShowMessage(string message, params string[] parameters)
        {
            if (this.ShowMessageFunction != null)
            {
                var builder = new StringBuilder(DateTime.Now.ToShortTimeString());
                builder.Append(':');
                if (!string.IsNullOrEmpty(message))
                {
                    if ((parameters == null) || (parameters.Length < 1))
                    {
                        builder.Append(message);
                    }
                    else
                    {
                        builder.AppendFormat(CultureInfo.CurrentCulture, message, parameters);
                    }
                }

                this.ShowMessageFunction(builder.ToString());
            }
        }
    }
}

