//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="Channel.cs" company="Ereadian">
//     Copyright (c) Ereadian.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace TestClient
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public class Channel
    {
        private readonly string HostAddress;
        private readonly int HostPort;
        private readonly string DeviceName;

        private Socket clientSocket;

        public Channel(string hostAddress, int hostPort, string deviceName)
        {
            this.HostAddress = hostAddress;
            this.HostPort = hostPort;
            this.DeviceName = deviceName;
            this.clientSocket = null;
        }

        public bool EnsureConnected()
        {
            lock (this)
            {
                if ((this.clientSocket != null) && (this.clientSocket.Connected))
                {
                    return true;
                }

                if (this.clientSocket != null)
                {
                    this.clientSocket.Dispose();
                    this.clientSocket = null;
                }

                this.clientSocket = ConnectHost();
            }

            return true;
        }

        public bool SendCommand(byte[] command)
        {
            if (!this.EnsureConnected())
            {
                Console.Beep();
                return false;
            }

            try
            {
                var count = this.clientSocket.Send(command);
                if (count != command.Length)
                {
                    return false;
                }
            }
            catch
            {
                this.clientSocket.Dispose();
                this.clientSocket = null;
                return false;
            }

            return true;
        }

        private Socket ConnectHost()
        {
            Socket socket = null;
            var addressList = Dns.GetHostAddresses(this.HostAddress);
            if ((addressList != null) && (addressList.Length > 0))
            {
                try
                {
                    var endpoint = new IPEndPoint(addressList[0], this.HostPort);
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.clientSocket = socket;
                    socket.Connect(endpoint);

                    var command = new byte[1];
                    var deviceNameBytes = Encoding.ASCII.GetBytes(this.DeviceName);
                    command[0] = (byte)(deviceNameBytes.Length);
                    socket.Send(command);
                    socket.Send(deviceNameBytes);
                }
                catch
                {
                    if (socket != null)
                    {
                        socket.Dispose();
                        socket = null;
                    }
                }
            }

            return socket;
        }
    }
}
