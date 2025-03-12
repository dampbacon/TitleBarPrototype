using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TitleBarPrototype
{

    public partial class Form1 : Form
    {

        #region Fields

        private Panel titleBar;
        private Button btnClose, btnMinimize, btnRestore;

        #endregion Fields

        #region Constructor

        public Form1()
        {
            InitializeComponent();
            var designSize = this.ClientSize;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Size = designSize;

            HandleResize();
            this.Resize += (s, e) => HandleResize();

            InitializeCustomTitleBar();
        }

        #endregion Constructor

        #region titleBarTest

        private void InitializeCustomTitleBar()
        {
            titleBar = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.DarkSlateGray
            };
            titleBar.MouseDown += TitleBar_MouseDown;
            this.Controls.Add(titleBar);

            btnMinimize = CreateTitleBarButton("➖", Color.Gray);
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnRestore = CreateTitleBarButton("❐", Color.Gray);
            btnRestore.Click += (s, e) =>
            {
                this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            };

            btnClose = CreateTitleBarButton("✖", Color.Firebrick);
            btnClose.Click += (s, e) => this.Close();

            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnRestore);
            titleBar.Controls.Add(btnClose);

        }

        private Button CreateTitleBarButton(string text, Color backColor)
        {
            var button = new Button
            {
                Text = text,
                ForeColor = Color.White,
                BackColor = backColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(45, 30),
                Dock = DockStyle.Right
            };
            button.FlatAppearance.BorderSize = 0;
            button.MouseEnter += (s, e) => button.BackColor = Color.Yellow;
            button.MouseLeave += (s, e) => button.BackColor = backColor;
            return button;
        }

        #endregion titleBarTest

        #region Window Dragging

        [DllImport("user32.dll")]
        private static extern void ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern void SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        #endregion Window Dragging

        #region Resize Handling

        private void HandleResize()
        {
            if (WindowState == FormWindowState.Maximized)
                Padding = new Padding(8, 8, 8, 8);
            else
                Padding = new Padding(0);
        }

        #endregion Resize Handling

        private const int WM_NCCALCSIZE = 0x83;
        private const int WM_NCHITTEST = 0x0084;

        #region The Almighty WndProc

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == WM_NCHITTEST && message.Result == (IntPtr)0)
            {
                int GrabSize = 10; // THIS IS THE THING THAT DETERMINES THE AREA YOU CAN GRAB ON THE BORDER
                int x = unchecked((short)(long)message.LParam);
                int y = unchecked((short)((long)message.LParam >> 16));
                var rect2 = new Rectangle(DesktopLocation.X + GrabSize, DesktopLocation.Y + GrabSize, ClientSize.Width - (GrabSize * 2), ClientSize.Height - (GrabSize * 2));
                if (!rect2.Contains(x, y))
                {
                    //CUSTOM DEFINITION OF THE FLAGS FOR WM_NCHITTEST
                    if (y > rect2.Bottom && x < rect2.Left) message.Result = (IntPtr)16; // HTBOTTOMLEFT
                    else if (y < rect2.Top && x > rect2.Right) message.Result = (IntPtr)14; // HTTOPRIGHT
                    else if (y > rect2.Bottom && x > rect2.Right) message.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    else if (y < rect2.Top && x < rect2.Left) message.Result = (IntPtr)13; // HTTOPLEFT
                    else if (y > rect2.Bottom) message.Result = (IntPtr)15; // HTBOTTOM
                    else if (y < rect2.Top) message.Result = (IntPtr)2; // HTCAPTION for moving the window
                    else if (x < rect2.Left) message.Result = (IntPtr)10; // HTLEFT
                    else if (x > rect2.Right) message.Result = (IntPtr)11; // HTRIGHT
                }
                else
                {
                    message.Result = (IntPtr)1; // HTCLIENT in a client area
                }
                return;
            }

            // Prevents Windows from drawing the default window border and title bar,
            // allowing full control over the non-client area.
            if (message.Msg == WM_NCCALCSIZE && message.WParam.ToInt32() == 1)
            {
                message.Result = (IntPtr)0; // THE SECRET SAUCE
                return;
            }
            base.WndProc(ref message);
        }

        #endregion The Almighty WndProc

    }

}
