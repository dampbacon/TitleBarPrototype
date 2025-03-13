using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TitleBarPrototype
{
    public partial class Form1 : Form
    {
        #region Fields

        private TransparentPanel titleBar;
        private ContentPanel contentPanel;
        private TransparentButton btnClose, btnMinimize, btnRestore;
        private Panel bottomPanel;
        private Button testButton1;
        private Button testButton2;
        private TextBox testTextBox;

        #endregion Fields

        #region Constructor

        public Form1()
        {
            InitializeComponent();
            var designSize = ClientSize;
            FormBorderStyle = FormBorderStyle.Sizable;
            Size = designSize;

            HandleResize();
            Resize += (s, e) => HandleResize();

            InitializeCustomTitleBar();
            InitializeContentPanel();
            AddTestControls();
        }

        #endregion Constructor

        #region titleBarTest

        private void InitializeCustomTitleBar()
        {
            // Create title bar that passes resize messages through
            titleBar = new TransparentPanel
            {
                Height = 30,
                Width = ClientSize.Width,
                BackColor = Color.DarkSlateGray,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            titleBar.MouseDown += TitleBar_MouseDown;
            Controls.Add(titleBar);

            // Create buttons that also pass resize messages through
            btnMinimize = CreateTitleBarButton("➖", Color.Gray);
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;

            btnRestore = CreateTitleBarButton("❐", Color.Gray);
            btnRestore.Click += (s, e) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            };

            btnClose = CreateTitleBarButton("✖", Color.Firebrick);
            btnClose.Click += (s, e) => Close();

            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnRestore);
            titleBar.Controls.Add(btnClose);
        }

        // Custom panel that passes through mouse events for resize areas
        private class TransparentPanel : Panel
        {
            protected override void WndProc(ref Message m)
            {
                const int WM_NCHITTEST = 0x0084;
                const int HTTRANSPARENT = -1;

                if (m.Msg == WM_NCHITTEST)
                {
                    // Get the point in screen coordinates
                    Point screenPoint = new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16);

                    // Convert to client coordinates
                    Point clientPoint = PointToClient(screenPoint);

                    // Check if point is in the resize area (edge of the form)
                    int edgeSize = 10;

                    // Check if point is within edge distance of form edges
                    Form parentForm = FindForm();
                    if (parentForm != null)
                    {
                        Point formPoint = parentForm.PointToClient(screenPoint);
                        if (formPoint.X < edgeSize || formPoint.X > parentForm.ClientSize.Width - edgeSize ||
                            formPoint.Y < edgeSize || formPoint.Y > parentForm.ClientSize.Height - edgeSize)
                        {
                            // Make these areas "transparent" to mouse events
                            m.Result = (IntPtr)HTTRANSPARENT;
                            return;
                        }
                    }
                }

                base.WndProc(ref m);
            }
        }

        // Custom panel for content that adds padding and passes through resize events
        private class ContentPanel : Panel
        {
            protected override void WndProc(ref Message m)
            {
                const int WM_NCHITTEST = 0x0084;
                const int HTTRANSPARENT = -1;

                if (m.Msg == WM_NCHITTEST)
                {
                    // Get the point in screen coordinates
                    Point screenPoint = new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16);

                    // Check if point is in the resize area (edge of the form)
                    int edgeSize = 10;

                    // Check if point is within edge distance of form edges
                    Form parentForm = FindForm();
                    if (parentForm != null)
                    {
                        Point formPoint = parentForm.PointToClient(screenPoint);
                        if (formPoint.X < edgeSize || formPoint.X > parentForm.ClientSize.Width - edgeSize ||
                            formPoint.Y < edgeSize || formPoint.Y > parentForm.ClientSize.Height - edgeSize)
                        {
                            // Make these areas "transparent" to mouse events
                            m.Result = (IntPtr)HTTRANSPARENT;
                            return;
                        }
                    }
                }

                base.WndProc(ref m);
            }
        }

        // Custom button that passes through mouse events for resize areas
        private class TransparentButton : Button
        {
            protected override void WndProc(ref Message m)
            {
                const int WM_NCHITTEST = 0x0084;
                const int HTTRANSPARENT = -1;

                if (m.Msg == WM_NCHITTEST)
                {
                    // Get the point in screen coordinates
                    Point screenPoint = new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16);

                    // Check if point is in the resize area (edge of the form)
                    int edgeSize = 10;

                    // Check if point is within edge distance of form edges
                    Form parentForm = FindForm();
                    if (parentForm != null)
                    {
                        Point formPoint = parentForm.PointToClient(screenPoint);

                        // Make corners and edges transparent to allow resizing
                        bool isTopEdge = formPoint.Y < edgeSize;
                        bool isBottomEdge = formPoint.Y > parentForm.ClientSize.Height - edgeSize;
                        bool isLeftEdge = formPoint.X < edgeSize;
                        bool isRightEdge = formPoint.X > parentForm.ClientSize.Width - edgeSize;

                        // Specifically ensure top-right corner passes through
                        if ((isTopEdge && isRightEdge) ||
                            (isBottomEdge && isLeftEdge) ||
                            (isBottomEdge && isRightEdge) ||
                            (isTopEdge && isLeftEdge) ||
                            isBottomEdge || isLeftEdge || isRightEdge)
                        {
                            // Pass through mouse events for all resize edges/corners
                            m.Result = (IntPtr)HTTRANSPARENT;
                            return;
                        }
                    }
                }

                base.WndProc(ref m);
            }
        }

        private TransparentButton CreateTitleBarButton(string text, Color backColor)
        {
            var button = new TransparentButton
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

        #region Content Panel

        private void InitializeContentPanel()
        {
            // Create content panel with padding
            contentPanel = new ContentPanel
            {
                BackColor = Color.WhiteSmoke,
                Dock = DockStyle.Fill,
                Padding = new Padding(15), // Add padding around content
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Position it below the title bar
            contentPanel.Location = new Point(0, titleBar.Height);
            contentPanel.Size = new Size(ClientSize.Width, ClientSize.Height - titleBar.Height);

            // Add the panel to the form
            Controls.Add(contentPanel);

            // Make sure the content panel is behind the title bar in z-order
            titleBar.BringToFront();
        }

        #endregion Content Panel

        #region Test Controls

        private void AddTestControls()
        {
            // Create a panel docked to the bottom of the content panel
            bottomPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = Color.LightGray,
                Padding = new Padding(5)
            };

            // Add test buttons to the bottom panel
            testButton1 = new Button
            {
                Text = "Test Button 1",
                Dock = DockStyle.Left,
                Width = 100,
                Height = 40
            };
            testButton1.Click += (s, e) => MessageBox.Show("Test Button 1 clicked!");

            testButton2 = new Button
            {
                Text = "Test Button 2",
                Dock = DockStyle.Right,
                Width = 100,
                Height = 40
            };
            testButton2.Click += (s, e) => MessageBox.Show("Test Button 2 clicked!");

            // Add a test text box to the content area
            testTextBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Text = "Try resizing the form by dragging any edge or corner. " +
                      "This textbox should resize with the form, and the buttons " +
                      "should stay docked to the bottom."
            };

            // Add test label in the center
            Label testLabel = new Label
            {
                Text = "Resize from any edge or corner to test functionality",
                AutoSize = true,
                Font = new Font("Arial", 14),
                ForeColor = Color.DarkBlue,
                Location = new Point(50, 50)
            };

            // Add controls to their respective containers
            bottomPanel.Controls.Add(testButton1);
            bottomPanel.Controls.Add(testButton2);
            contentPanel.Controls.Add(bottomPanel);
            contentPanel.Controls.Add(testTextBox);
            contentPanel.Controls.Add(testLabel);
        }

        #endregion Test Controls

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

            // Update title bar width when form is resized
            if (titleBar != null)
                titleBar.Width = ClientSize.Width;

            // Update content panel size and position
            if (contentPanel != null)
            {
                contentPanel.Size = new Size(ClientSize.Width, ClientSize.Height - titleBar.Height);
                contentPanel.Location = new Point(0, titleBar.Height);
            }
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

                // LParam stores X (low 16 bits) and Y (high 16 bits) in a 32-bit integer, even on 64-bit systems.
                // Casting to long ensures compatibility with both 32-bit and 64-bit architectures.
                // The (short) cast extracts signed 16-bit values.
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