//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="ThisAddIn.cs" company="Ereadian">
//     Copyright (c) Ereadian.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace PowerPointRemoveControllerEreadianAddIn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;
    using System.Globalization;
    using System.Diagnostics;

    public class Channel : IDisposable
    {
        private Socket hostSocket;
        private Socket clientSocket;

        public Channel(int port)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
            this.hostSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.hostSocket.Bind(endpoint);
            this.hostSocket.Listen(1);
        }

        public IAsyncResult BeginAccept()
        {
            return this.hostSocket.BeginAccept(null, null);
        }

        public void EndAccept(IAsyncResult asyncResult)
        {
            this.clientSocket = this.hostSocket.EndAccept(asyncResult);
        }

        public IAsyncResult BeginReceive(byte[] buffer, int size)
        {
            return this.clientSocket.BeginReceive(buffer, 0, size, SocketFlags.None, null, null);
        }

        public int EndReceive(IAsyncResult asyncResult)
        {
            return this.clientSocket.EndReceive(asyncResult);
        }

        public int Send(byte[] buffer)
        {
            return this.clientSocket.Send(buffer, buffer.Length, SocketFlags.None);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.clientSocket != null)
            {
                this.clientSocket.Close();
                this.clientSocket.Dispose();
                this.clientSocket = null;
            }

            if (this.hostSocket != null)
            {
                this.hostSocket.Dispose();
                this.hostSocket = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
