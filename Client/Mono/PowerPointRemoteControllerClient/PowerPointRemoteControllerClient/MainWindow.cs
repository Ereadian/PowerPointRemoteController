namespace Ereadian.PowerPointRemoteController.Client
{
	using System;
	using System.Configuration;
    using System.Diagnostics;
    using System.Threading;
    using Gtk;

	public partial class MainWindow: Gtk.Window
	{
        private const int ThreadJoinTimeout = 5000;
        private const int DefaultRemotePort = 5555;
        private const int DefaultHeartbet = 100;

        private readonly int Heartbet;
		private ManualResetEventSlim StopEvent;
		private Thread monitorThread;
        private Channel Channel;

		public MainWindow () : base (Gtk.WindowType.Toplevel)
		{
            Build ();
            //this.PrevPageButton.Sensitive = false;
            //this.NextPageButton.Sensitive = false;

            this.Channel = new Channel(
                ConfigurationManager.AppSettings["RemoteAddress"],
                GetInteger("RemotePort", DefaultRemotePort),
                ConfigurationManager.AppSettings["ClientName"],
                message => this.ShowMessage(message));
            this.Heartbet = GetInteger("Heartbet", DefaultHeartbet);
            this.StopEvent = new ManualResetEventSlim (false);
            this.monitorThread = new Thread(RunMonitorThread);
            this.monitorThread.Start(this);
        }

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
            this.StopMonitorThread();
            Application.Quit();
			a.RetVal = true;
		}

		protected void OnNextPage (object sender, EventArgs e)
		{
            this.ShowMessage("Next");
		}

		protected void OnPreviousPage (object sender, EventArgs e)
		{
            this.ShowMessage("Previous");
		}

		private static void RunMonitorThread(object state)
        {
            var instance = state as MainWindow;
            try
            {
                instance.MonitorWorkerThread();
            }
            catch(Exception ex)
            {
                instance.ShowMessage(ex.Message);
            }
        }

		private void MonitorWorkerThread()
		{
            var waitTime = 0;
            var watch = new Stopwatch();
            while (!this.StopEvent.Wait(waitTime))
            {
                watch.Start();
                //check network and action
                watch.Stop();
                waitTime = Math.Max(0, this.Heartbet - (int)watch.ElapsedMilliseconds);
                watch.Reset();
            }
		}

        private void StopMonitorThread()
        {
            lock (this) 
            {
                if (this.StopEvent != null) 
                {
                    this.StopEvent.Set();
                    if (this.monitorThread != null)
                    {
                        if (this.monitorThread.IsAlive)
                        {
                            try
                            {
                                if (!this.monitorThread.Join(ThreadJoinTimeout))
                                {
                                    this.monitorThread.Abort();
                                }
                            }
                            catch
                            {
                            }
                        }

                        this.monitorThread = null;
                        if (this.Channel != null)
                        {
                            this.Channel.Dispose();
                            this.Channel = null;
                        }
                    }

                    this.StopEvent.Dispose();
                    this.StopEvent = null;
                }
            }
        }

        private void ShowMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                lock (this)
                {
                    this.MessageLine.Text = message;
                }
            }
        }

        private static int GetInteger(string configurationKey, int defaultValue)
        {
            var rawValue = ConfigurationManager.AppSettings[configurationKey];
            if (!string.IsNullOrEmpty(rawValue))
            {
                int value;
                if (int.TryParse(rawValue, out value))
                {
                    return value;
                }
            }

            return defaultValue;
        }
	}
}