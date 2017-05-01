using System;
using Gtk;

public partial class MainWindow: Gtk.Window
{
	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnNextPage (object sender, EventArgs e)
	{
		this.TimeLabel.Text = "Next";
	}

	protected void OnPreviousPage (object sender, EventArgs e)
	{
		this.TimeLabel.Text = "Previous";
	}
}
