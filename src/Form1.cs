using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyboardHook1;

namespace Program_Hide
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region DWM Thumbnail Components
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, int nIndex);

        [DllImport("dwmapi.dll")]
        static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [StructLayout(LayoutKind.Sequential)]
        internal struct PSIZE
        {
            public int x;
            public int y;
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);

        [DllImport("user32.dll")]
        static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);

        delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        static readonly int GWL_STYLE = -16;

        static readonly ulong WS_VISIBLE = 0x10000000L;
        static readonly ulong WS_BORDER = 0x00800000L;
        static readonly ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        internal class Window
        {
            public string Title;
            public IntPtr Handle;

            public override string ToString()
            {
                return Title;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
        {
            internal Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private List<Window> windows;

        private void GetWindows()
        {
            windows = new List<Window>();

            EnumWindows(Callback, 0);

            lstWindows.Items.Clear();
            foreach (Window w in windows)
            {
                lstWindows.Items.Add(w);
            }
        }

        private bool Callback(IntPtr hwnd, int lParam)
        {
            if (this.Handle != hwnd && (GetWindowLongA(hwnd, GWL_STYLE) & TARGETWINDOW) == TARGETWINDOW)
            {
                StringBuilder sb = new StringBuilder(200);
                GetWindowText(hwnd, sb, sb.Capacity);
                Window t = new Window();
                t.Handle = hwnd;
                t.Title = sb.ToString();
                windows.Add(t);
            }

            return true; //continue enumeration
        }

        private IntPtr thumb;

        bool LoadedForm;
        private void lstWindows_SelectedIndexChanged(object sender, EventArgs e)
        {
            StopRecording();
            if (!LoadedForm)
            {
                savedSize = image.Size;
                LoadedForm = true;
            }

            if (!image.Visible)
            {
                image.Visible = true;
            }

            if (lstWindows.SelectedIndex > -1)
            {
                button1.Enabled = true;

                Window w = (Window)lstWindows.SelectedItem;
                if (thumb != IntPtr.Zero)
                    DwmUnregisterThumbnail(thumb);

                int i = DwmRegisterThumbnail(this.Handle, w.Handle, out thumb);
                if (i == 0)
                    UpdateThumb();


                tempHandle = w.Handle;
                tempTitle = w.Title;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        static readonly int DWM_TNP_VISIBLE = 0x8;
        static readonly int DWM_TNP_OPACITY = 0x4;
        static readonly int DWM_TNP_RECTDESTINATION = 0x1;



        const short HWND_BOTTOM = 1;
        const short HWND_NOTOPMOST = -2;
        const short HWND_TOP = 0;
        const short HWND_TOPMOST = -1;

        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_SHOWWINDOW = 0x0040;
        const int SWP_NOACTIVATE = 0x0010;
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); //for all nCmdShow flags: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        const int SW_MAXIMIZE = 3;
        const int SW_SHOW = 5;
        const int SW_SHOWNORMAL = 1;
        const int SW_RESTORE = 9; //these also works with showwindowasync

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        //[DllImport("user32.dll", SetLastError = true)]
        //static extern bool BringWindowToTop(HandleRef hWnd);

        #endregion





        #region Form1_Load | Intializes


        KeyboardHook listener = new KeyboardHook();

        IntPtr tempHandle;
        string tempTitle;

        Color DarkGray = Color.FromArgb(41, 41, 41);

        public static List<IntPtr> storedHandles = new List<IntPtr>();
        public static List<string> storedTitles = new List<string>();

        ContextMenuStrip contextMenu1 = new ContextMenuStrip();
        ToolStripMenuItem OpenItem = new ToolStripMenuItem("Open");
        ToolStripMenuItem HelpItem = new ToolStripMenuItem("Help");
        ToolStripMenuItem CloseItem = new ToolStripMenuItem("Close");

        public static Form3 f3;
        #endregion

        [STAThread]
        private void Form1_Load(object sender, EventArgs e)
        {
            button6.Enabled = false;
            f3 = new Form3();
            UpdateKeybindLabel(true);

            button4.Visible = false;

            notifyIcon1.Text = "Program Hide"; 
            notifyIcon1.ContextMenuStrip = contextMenu1;
            notifyIcon1.Icon = SystemIcons.Information;

            notifyIcon1.Visible = true;

            contextMenu1.Items.Add(OpenItem);
            contextMenu1.Items.Add(HelpItem);
            contextMenu1.Items.Add(CloseItem);

            contextMenu1.BackColor = Color.Black;

            OpenItem.BackColor = DarkGray;
            HelpItem.BackColor = DarkGray;
            CloseItem.BackColor = DarkGray;
            OpenItem.ForeColor = Color.White;
            HelpItem.ForeColor = Color.White;
            CloseItem.ForeColor = Color.White;

            OpenItem.Image = SystemIcons.Application.ToBitmap();
            HelpItem.Image = SystemIcons.Information.ToBitmap();
            CloseItem.Image = SystemIcons.Hand.ToBitmap();

            OpenItem.Click += OpenItem_Click;
            CloseItem.Click += CloseItem_Click;
            HelpItem.Click += HelpItem_Click;

            notifyIcon1.ShowBalloonTip(3000, "Program Hide - Startup Notification", "Right Click the Tool Tip or Click Here for Help Using this Product.", ToolTipIcon.Info);
            notifyIcon1.BalloonTipClicked += (send, args) =>
            {
                ShowHelp();
            };

            listener.KeyDown += listener_KeyDown;
            listener.KeyUp += listener_KeyUp;

            button1.Enabled = false;

            GetWindows();

            tm1.Enabled = true;
            timer1.Enabled = true;
            tm1.Interval = 1000;
        }

        private void OpenItem_Click(object sender, EventArgs e)
        {
            this.Show();
            BringWindowToTop(this.Handle);
        }

        private void HelpItem_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private void CloseItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void ShowHelp()
        {
            MessageBox.Show("Not Implemented Yet!", "Program Hide", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        #region KeyDown Detecion System

        bool alreadyShownMB;
        bool keyIsNew;

        List<KeyboardHook.VKeys> currentKeys = new List<KeyboardHook.VKeys>();
        private void listener_KeyDown(KeyboardHook.VKeys key)
        {
            if (!currentKeys.Contains(key))
            {
                currentKeys.Add(key);
                keyIsNew = true;
            }

            if (keyIsNew)
            {
                if (ShowHideRecording)
                {
                    if (!TempKeysList.Contains(key))
                    {
                        TempKeysList.Add(key);

                        if (!alreadyShownMB)
                        {
                            if (TempKeysList.Count > 4)
                            {
                                alreadyShownMB = true;
                                MessageBox.Show("Reminder: The current key combination max is 4.", "Program Hide - Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                        textBox1.Text = "";

                        for (int i = 0; i < TempKeysList.Count; i++)
                        {
                            if (i < 1)
                            {
                                textBox1.Text += TempKeysList[i].ToString();
                            }
                            else
                            {
                                textBox1.Text += " + " + TempKeysList[i].ToString();
                            }

                            textBox1.SelectionStart = textBox1.TextLength;
                        }
                }
            }

            keyIsNew = false;
        }
        #endregion

        #region KeyUp Detection System
        
        private void listener_KeyUp(KeyboardHook.VKeys key)
        {
            if (currentKeys.Contains(key))
            {
                currentKeys.Remove(key);
            }

            if (ShowHideRecording)
            {
                if (TempKeysList.Contains(key))
                {
                    TempKeysList.Remove(key);

                    textBox1.Text = "";

                    for (int i = 0; i < TempKeysList.Count; i++)
                    {
                        if (i < 1)
                        {
                            textBox1.Text += TempKeysList[i].ToString();
                        }
                        else
                        {
                            textBox1.Text += " + " + TempKeysList[i].ToString();
                        }

                        textBox1.SelectionStart = textBox1.TextLength;
                    }
                }
            }
        }
        #endregion


        bool keyListenerInstalled;
        public void UpdateKeyRecording(bool boolean)
        {
            if (boolean)
            {
                keyListenerInstalled = true;
                listener.Install();
            }
            else
            {
                keyListenerInstalled = false;
                listener.Uninstall();
            }
        }


        public static bool ShowHideRecording;
        public static bool Form1ForceRecord;

        public static List<KeyboardHook.VKeys> ShowHideKeysList = new List<KeyboardHook.VKeys>();
        List<KeyboardHook.VKeys> TempKeysList = new List<KeyboardHook.VKeys>();
        private void button2_Click(object sender, EventArgs e)
        {
            Form1ForceRecord = true;
            Form2.Form2ForceRecord = false;

            this.ActiveControl = null;
            if (!ShowHideRecording)
            {
                timer1.Enabled = true;
                UpdateKeyRecording(true);
                TempKeysList.Clear();

                ShowHideRecording = true;
                button2.Text = "Stop Recording";
                button4.Visible = true;
            }
            else
            {
                timer1.Enabled = false;
                UpdateKeyRecording(false);
                alreadyShownMB = false;
                ShowHideRecording = false;
                button2.Text = "Record Keys";
                button4.Visible = false;
                textBox1.Text = "";

                if (TempKeysList.Count > 4 || TempKeysList.Count < 2)
                {
                    if (TempKeysList.Count > 4)
                    {
                        MessageBox.Show("Your new key combination was not saved.\nReason: The key combination contains more than 4 keys.", "Program Hide - Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (TempKeysList.Count < 2 && TempKeysList.Count > 0)
                    {
                        MessageBox.Show("Your new key combination was not saved.\nReason: The key combination contains less than 2 keys.", "Program Hide - Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (TempKeysList.Count == 0)
                    {
                        MessageBox.Show("Your new key combination was not saved.\nReason: The key combination is empty.", "Program Hide - Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    SetNewKeys();
                    UpdateKeybindLabel(false);

                    if (addedYet)
                    {
                        Form3.AllKeyBinds[0] = ShowHideKeysList;
                    }
                }

                currentKeys.Clear();
            }
        }

        public void SetNewKeys()
        {
            ShowHideKeysList = TempKeysList.ToList();
        }

        public void UpdateKeybindLabel(bool resetDefault)
        {
            if (resetDefault)
            {
                ShowHideKeysList.Clear();
                ShowHideKeysList.Add(KeyboardHook.VKeys.LMENU);
                ShowHideKeysList.Add(KeyboardHook.VKeys.LSHIFT);
            }

            label2.Text = "";
            label2.Text += "Current:\n";
            for (int i = 0; i < ShowHideKeysList.Count; i++)
            {
                if (i < 1)
                {
                    label2.Text += ShowHideKeysList[i].ToString();
                }
                else
                {
                    label2.Text += " + " + ShowHideKeysList[i].ToString();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StopRecording();
            if (ShowHideKeysList.Count == 2)
            {
                if (ShowHideKeysList.Contains(KeyboardHook.VKeys.LMENU) && ShowHideKeysList.Contains(KeyboardHook.VKeys.LSHIFT))
                {
                    return;
                }
            }
            UpdateKeybindLabel(true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        public void StopRecording()
        {
            Form2.Form2ForceRecord = false;
            if (button4.Visible)
            {
                button4.Visible = false;
            }

            ShowHideRecording = false;
            button2.Text = "Record Keys";
            listener.Uninstall();
            button4.Visible = false;
            textBox1.Text = "";
        }


        private void tm1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (!IsWindow(windows[i].Handle))
                {
                    lstWindows.Items.RemoveAt(i);
                    windows.RemoveAt(i);
                    lstWindows.SelectedIndex = -1;
                    button1.Enabled = false;
                }
            }
            UpdateThumb();
        }

        public static Form2 f2;
        private void button1_Click(object sender, EventArgs e)
        {
            StopRecording();
            if (ShowHideKeysList.Count == 2 && ShowHideKeysList.Contains(KeyboardHook.VKeys.LSHIFT) && ShowHideKeysList.Contains(KeyboardHook.VKeys.LMENU))
            {
                DialogResult returnValue = MessageBox.Show("Are you sure you want to keep the Default Keybind?\nIt can be changed later in the Keybind List Menu", "Program Hide - Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            
                if (!(returnValue == DialogResult.Yes))
                {
                    return;
                }
            }

            AddToFullList();

            EnableButtonSix();

            if (!IsWindow(tempHandle))
            {
                MessageBox.Show("The window: \n\"" + tempTitle + "\"\n no longer exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (lstWindows.SelectedIndex > -1)
                {
                    lstWindows.Items.RemoveAt(lstWindows.SelectedIndex);
                }
                lstWindows.SelectedIndex = -1;
                return;
            }

            OpenOfType("WIN");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StopRecording();
            if (ShowHideKeysList.Count == 2 && ShowHideKeysList.Contains(KeyboardHook.VKeys.LSHIFT) && ShowHideKeysList.Contains(KeyboardHook.VKeys.LMENU))
            {
                DialogResult returnValue = MessageBox.Show("Are you sure you want to keep the Default Keybind?\nIt can be changed later in the Keybind List Menu", "Program Hide - Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (!(returnValue == DialogResult.Yes))
                {
                    return;
                }
            }

            AddToFullList();
            EnableButtonSix();


            OpenOfType("APP");
        }

        public void OpenOfType(string type)
        {
            fc = Application.OpenForms;
            for (int i = 0; i < fc.Count; i++)
            {
                if (fc[i].Name == "Form2")
                {
                    if (f2.Tag.ToString() == type)
                    {
                        f2.Show();
                        f2.BringToFront();
                        break;
                    }
                }
                if (i == fc.Count - 1)
                {
                    if (type == "WIN")
                    {
                        f2 = new Form2(tempHandle, tempTitle, type);
                    }
                    else
                    {
                        f2 = new Form2((IntPtr)0, null, type);
                    }
                    f2.Show();
                    f2.BringToFront();
                }
            }
        }

        public void EnableButtonSix()
        {
            if (Form3.AllKeyBinds.Count > 0)
            {
                button6.Enabled = true;
                button6_Click(new object(), new EventArgs());
                f3.Show();
                f3.Hide();
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            StopRecording();
            OpenFormThree();
        }

        FormCollection fc;
        public void OpenFormThree()
        {
            fc = Application.OpenForms;
            for (int i = 0; i < fc.Count; i++)
            {
                if (fc[i].Name == "Form3")
                {
                    break;
                }

                if (i == fc.Count - 1)
                {
                    f3 = new Form3();
                }
            }
            f3.Show();
            f3.BringToFront();
        }

        bool addedYet;
        public void AddToFullList()
        {
            if (!addedYet)
            {
                Form3.AllKeyBinds.Add(ShowHideKeysList.ToList());
                Form3.AllHandles.Add(this.Handle);
                Form3.AllTitles.Add(this.Text);
                Form3.ShowingWindow.Add(true);
                Form3.Maximize.Add(false);
                addedYet = true;
            }
        }

        private void UpdateThumb()
        {
            if (thumb != IntPtr.Zero)
            {
                PSIZE size;
                DwmQueryThumbnailSourceSize(thumb, out size);

                DWM_THUMBNAIL_PROPERTIES props = new DWM_THUMBNAIL_PROPERTIES();
                props.dwFlags = DWM_TNP_VISIBLE | DWM_TNP_RECTDESTINATION | DWM_TNP_OPACITY;

                props.fVisible = true;
                props.opacity = 255;


                if (size.x < image.Width && size.y < image.Height)
                {
                    props.rcDestination = new Rect(image.Left + ((image.Width - (props.rcDestination.Left + size.x)) / 2), image.Top + ((image.Height - (props.rcDestination.Top + size.y)) / 2), image.Right, image.Bottom);
                }
                else if (size.x < image.Width && !(size.y < image.Height))
                {
                    props.rcDestination = new Rect(image.Left + ((image.Width - (props.rcDestination.Left + size.x)) / 2), image.Top, image.Right, image.Bottom);
                }
                else if (!(size.x < image.Width) && size.y < image.Height)
                {
                    props.rcDestination = new Rect(image.Left, image.Top + ((image.Height - (props.rcDestination.Top + size.y)) / 2), image.Right, image.Bottom);
                }
                else if (!(size.x < image.Width && size.y < image.Height))
                {
                    props.rcDestination = new Rect(image.Left, image.Top, image.Right, image.Bottom);
                }

                if (size.x < image.Width)
                    props.rcDestination.Right = props.rcDestination.Left + size.x;
                if (size.y < image.Height)
                    props.rcDestination.Bottom = props.rcDestination.Top + size.y;

                DwmUpdateThumbnailProperties(thumb, ref props);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            UpdateThumb();
        }


        List<IntPtr> PreviousHandles = new List<IntPtr>();
        List<string> PreviousTitles = new List<string>();
        int previousIndex;
        Size savedSize;
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            StopRecording();
            PreviousHandles.Clear();
            PreviousTitles.Clear();

            foreach (Window w in windows)
            {
                PreviousHandles.Add(w.Handle);
                PreviousTitles.Add(w.Title);
            }
            previousIndex = lstWindows.SelectedIndex;

            GetWindows();

            if (previousIndex > -1)
            {
                if (windows[previousIndex].Handle == PreviousHandles[previousIndex])
                {
                    lstWindows.SelectedIndex = previousIndex;
                }
                else
                {
                    savedSize = image.Size;
                    image.Size = new Size(0, 0);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you would like to close the program?\nNote: Your keybinds will no longer be functional.", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                notifyIcon1.BalloonTipClosed += (s, args) =>
                {
                    var thisIcon = (NotifyIcon)sender;
                    thisIcon.Visible = false;
                    thisIcon.Dispose();
                };

                if (keyListenerInstalled)
                {
                    listener.Uninstall();
                }

                listener.KeyDown -= listener_KeyDown;
                listener.KeyUp -= listener_KeyUp;
            }
            else
            {
                e.Cancel = true;
            }
        }

    private void textBox1_Click(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ShowHideRecording)
            {
                if (Form2.Form2ForceRecord)
                {
                    StopRecording();
                }
            }
        }
    }
}