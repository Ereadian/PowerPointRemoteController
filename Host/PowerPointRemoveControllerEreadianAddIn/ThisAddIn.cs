//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="ThisAddIn.cs" company="Ereadian">
//     Copyright (c) Ereadian.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace PowerPointRemoveControllerEreadianAddIn
{
    using System.Text;
    using System.Threading;
    using PowerPoint = Microsoft.Office.Interop.PowerPoint;

    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/cc668192.aspx
    /// </summary>
    public partial class ThisAddIn
    {
        private ManualResetEventSlim stopEvent = null;
        private Thread playThread = null;
        private PowerPoint.SlideShowWindow showWindow;

        /// <summary>
        /// AddIn configurations
        /// </summary>
        private Configurations configurations = new Configurations();

        /// <summary>
        /// AddIn startup
        /// </summary>
        /// <param name="sender">sender instance</param>
        /// <param name="e">event argument</param>
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            if (this.configurations.Enabled)
            {
                this.Application.SlideShowBegin += Application_SlideShowBegin;
                this.Application.SlideShowEnd += Application_SlideShowEnd;
            }
        }

        /// <summary>
        /// AddIn shutdown
        /// </summary>
        /// <param name="sender">sender instance</param>
        /// <param name="e">event argument</param>
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            if (this.configurations.Enabled)
            {
                this.Application.SlideShowBegin -= Application_SlideShowBegin;
                this.Application.SlideShowEnd -= Application_SlideShowEnd;
            }
        }

        /// <summary>
        /// Slide show begin event
        /// </summary>
        /// <param name="showWindow">slide show window</param>
        private void Application_SlideShowBegin(PowerPoint.SlideShowWindow showWindow)
        {
            lock (this)
            {
                this.showWindow = showWindow;
                if (this.stopEvent == null)
                {
                    this.stopEvent = new ManualResetEventSlim(false);
                    this.playThread = new Thread(AutoPlayStub);
                    this.playThread.Start(this);
                }
            }
        }

        /// <summary>
        /// Slid show end event
        /// </summary>
        /// <param name="presentation">PowerPoint presentation</param>
        private void Application_SlideShowEnd(PowerPoint.Presentation presentation)
        {
            lock (this)
            {
                if (this.stopEvent != null)
                {
                    this.stopEvent.Set();
                    if (this.playThread != null)
                    {
                        if (this.playThread.IsAlive && !this.playThread.Join(this.configurations.ThreadStopTimeout))
                        {
                            this.playThread.Abort();
                        }

                        this.playThread = null;
                    }

                    this.stopEvent.Dispose();
                    this.stopEvent = null;
                }

                this.showWindow = null;
            }
        }

        /// <summary>
        /// Auto play stub
        /// </summary>
        /// <param name="state">application instance as state</param>
        private static void AutoPlayStub(object state)
        {
            (state as ThisAddIn).AutoPlayThread();
        }

        /// <summary>
        /// Auto play work thread
        /// </summary>
        private void AutoPlayThread()
        {
            var deviceName = Encoding.ASCII.GetBytes(this.configurations.RemoteDeviceName);
            var nameBuffer = new byte[deviceName.Length];
            var events = new WaitHandle[2] { this.stopEvent.WaitHandle, null};
            var inputBuffer = new byte[1];
            var outputBuffer = new byte[] { 0 };
            while (!this.stopEvent.Wait(0))
            {
                try
                {
                    using (var channel = new Channel(this.configurations.SocketPortNumber))
                    {
                        // wait for connection
                        var asyncResult = channel.BeginAccept();
                        events[1] = asyncResult.AsyncWaitHandle;
                        if (WaitHandle.WaitAny(events) == 0)
                        {
                            channel.Send(outputBuffer);
                            break;
                        }

                        channel.EndAccept(asyncResult);

                        // get device name length
                        asyncResult = channel.BeginReceive(inputBuffer, 1);
                        events[1] = asyncResult.AsyncWaitHandle;
                        if (WaitHandle.WaitAny(events) == 0)
                        {
                            channel.Send(outputBuffer);
                            break;
                        }

                        var size = channel.EndReceive(asyncResult);
                        if ((size != 1) && ((int)inputBuffer[0] != deviceName.Length))
                        {
                            channel.Send(outputBuffer);
                            continue;
                        }

                        // get device name
                        asyncResult = channel.BeginReceive(nameBuffer, nameBuffer.Length);
                        events[1] = asyncResult.AsyncWaitHandle;
                        if (WaitHandle.WaitAny(events) == 0)
                        {
                            channel.Send(outputBuffer);
                            break;
                        }

                        size = channel.EndReceive(asyncResult);
                        if (size != deviceName.Length)
                        {
                            channel.Send(outputBuffer);
                            continue;
                        }

                        bool deviceMatch = true;
                        for (var i = 0; i < size; i++)
                        {
                            if (deviceName[i] != nameBuffer[i])
                            {
                                deviceMatch = false;
                                break;
                            }
                        }

                        if (!deviceMatch)
                        {
                            channel.Send(outputBuffer);
                            break;
                        }

                        while (true)
                        {
                            asyncResult = channel.BeginReceive(inputBuffer, 1);
                            events[1] = asyncResult.AsyncWaitHandle;
                            if (WaitHandle.WaitAny(events) == 0)
                            {
                                break;
                            }

                            size = channel.EndReceive(asyncResult);
                            if (inputBuffer[0] == 0)
                            {
                                this.showWindow.View.Previous();
                            }
                            else
                            {
                                this.showWindow.View.Next();
                            }
                        }

                        channel.Send(outputBuffer);
                    }
                }
                catch
                {
                    // TODO: log error
                    Thread.Sleep(500);
                }
            }
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
