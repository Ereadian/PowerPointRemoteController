namespace TestClient
{
    using System;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        protected override void OnClosed(EventArgs e)
        {
            Console.Beep();
            base.OnClosed(e);
        }
    }
}
