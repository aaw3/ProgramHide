using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyboardHook1;

namespace Program_Hide
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        const int SW_MAXIMIZE = 3;
        const int SW_SHOW = 5;
        const int SW_HIDE = 0;
        const int SW_SHOWNORMAL = 1;
        const int SW_RESTORE = 9;

        public static List<List<KeyboardHook.VKeys>> AllKeyBinds = new List<List<KeyboardHook.VKeys>>(); //list of keyvalues inside of a list of keybinds
        public static List<IntPtr> AllHandles = new List<IntPtr>();
        public static List<string> AllTitles = new List<string>();

        public static List<bool> Maximize = new List<bool>();
        public static List<bool> CIOnce = new List<bool>();
        public static List<bool> CIAlways = new List<bool>();

        public static List<string> ProgramType = new List<string>();

        public static List<bool> ShowingWindow = new List<bool>();

        ScrollBar vScrollBar1;

        public KeyboardHook listener = new KeyboardHook();
        private void Form3_Load(object sender, EventArgs e)
        {
            label1.Visible = false;
            panel1.Location = new Point(0, 0);
            panel1.MouseWheel += Panel1_MouseWheel;

            vScrollBar1 = new VScrollBar();
            vScrollBar1.Dock = DockStyle.Right;

            vScrollBar1.Scroll += (osender, eargs) => {
                UpdateScroll(true);
            };

            vScrollBar1.Maximum = 0;


            Debug.WriteLine(vScrollBar1.Maximum);

            this.Controls.Add(vScrollBar1);
            vScrollBar1.BringToFront();

            timer1.Enabled = true;

            listener.KeyDown += Listener_KeyDown;
            listener.KeyUp += Listener_KeyUp;

            listener.Install();
        }


        public void ShowForm(int i) 
        {
            if (i == 0)
            {
                if (Form1.ShowHideRecording)
                {
                    return;
                }
            }
            if (ShowingWindow[i])
            {
                ShowWindowAsync(AllHandles[i], SW_HIDE);
                ShowingWindow[i] = false;
            }
            else
            {
                ShowWindowAsync(AllHandles[i], SW_SHOW);
                SetForegroundWindow(AllHandles[i]);

                if (Maximize[i])
                {
                    ShowWindowAsync(AllHandles[i], SW_MAXIMIZE);
                }
                ShowingWindow[i] = true;
            }
        }


        List<KeyboardHook.VKeys> CurrentKeys = new List<KeyboardHook.VKeys>();
        private void Listener_KeyDown(KeyboardHook.VKeys key)
        {
            if (!CurrentKeys.Contains(key))
            {
                CurrentKeys.Add(key);
            
            
                if (CurrentKeys.Count >= 2)
                {
                    for (int i = 0; i < AllKeyBinds.Count; i++)
                    {
                        for (int a = 0; a < AllKeyBinds[i].Count; a++)
                        {
                            if (!CurrentKeys.Contains(AllKeyBinds[i][a]))
                            {
                                break;
                            }
                            else if (a == AllKeyBinds[i].Count - 1)
                            {
                                ShowForm(i);
                                UpdatePanels();
                            }
                        }
                    }
                }
            
            
            
            
            
            }
        }
        private void Listener_KeyUp(KeyboardHook.VKeys key)
        {
            CurrentKeys.Remove(key);
        }

        int changeInScroll = 5;
        int scrollFreeSpace;
        float VScrollBar1Translation;
        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            SendScrollInput(e);
        }

        bool resetPanelLocation;
        public void SendScrollInput(MouseEventArgs e)
        {
            Debug.WriteLine(vScrollBar1.Value + " / " + vScrollBar1.Maximum);

            if (panel1.Height - this.ClientSize.Height > 0)
            {
                scrollFreeSpace = panel1.Height - this.ClientSize.Height;
                Debug.WriteLine(scrollFreeSpace);

                VScrollBar1Translation = (float)Math.Floor((double)(scrollFreeSpace / changeInScroll));
                vScrollBar1.Maximum = (int)VScrollBar1Translation;
                Debug.WriteLine("OUT OF: " + scrollFreeSpace / changeInScroll);
                Debug.WriteLine(vScrollBar1.Maximum);

                if (vScrollBar1.Maximum > 0)
                {
                    if (e.Delta > 0)
                    {
                        if (vScrollBar1.Value - changeInScroll > vScrollBar1.Minimum)
                        {
                            vScrollBar1.Value -= 5;
                        }
                        else
                        {
                            vScrollBar1.Value = 0;
                        }
                    }
                    else
                    {
                        if (vScrollBar1.Value + changeInScroll < vScrollBar1.Maximum)
                        {
                            vScrollBar1.Value += 5;
                        }
                        else
                        {
                            Debug.WriteLine(vScrollBar1.Maximum);
                            vScrollBar1.Value = vScrollBar1.Maximum;
                        }
                    }

                    UpdateScroll(false);
                }
            }
        }


        int tempInt1;
        public void UpdateScroll(bool usedScrollBar)
        {
            if (resetPanelLocation)
            {
                resetPanelLocation = false;
                panel1.Location = new Point(0, 0);
                vScrollBar1.Value = 0;
            }
            else
            {
                tempInt1 = (0 - (vScrollBar1.Value * (panel1.Height / vScrollBar1.Maximum)) + (vScrollBar1.Value * ((this.ClientSize.Height + 40) / vScrollBar1.Maximum)));
                if (usedScrollBar)
                {
                    Debug.WriteLine(vScrollBar1.Value + " OUT OF " + vScrollBar1.Maximum);

                    if (vScrollBar1.Value > ((vScrollBar1.Maximum / 10) * 9))
                    {
                        Debug.WriteLine("true");
                        panel1.Location = new Point(0, tempInt1);
                    }
                    else 
                    {
                        Debug.WriteLine("TRUE");
                        panel1.Location = new Point(0, tempInt1 - (int)(tempInt1 / -8.9f));
                    }
                }
                else
                {
                    if (vScrollBar1.Maximum > 0)
                    {
                        Debug.WriteLine(tempInt1);
                        if (!(tempInt1 > 0))
                        {
                            Debug.WriteLine("WORKING");
                            panel1.Location = new Point(0, tempInt1);
                            Debug.WriteLine(tempInt1);
                        }
                    }
                }
            }
        }

        int prevListValue;
        Panel p1;
        Label l1;
        Label l2;
        Label l3;
        List<Panel> PanelList = new List<Panel>();
        List<Label> LabelList = new List<Label>();
        List<Label> NumLabelList = new List<Label>();
        private void timer1_Tick(object sender, EventArgs e) 
        {
            if (AllKeyBinds.Count != prevListValue)
            {
                UpdatePanels();
            }
        }



        private void Form3_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                forcePanelUpdate = true;
                UpdatePanels();
            }
        }

        public static bool forcePanelUpdate;
        public void UpdatePanels()
        {
            if (!this.Visible)
            {
                if (!forcePanelUpdate)
                {
                    return;
                }
                else
                {
                    forcePanelUpdate = false;
                }
            }

            DisposeOldPanels();
            for (int i = 0; i < AllKeyBinds.Count; i++)
            {
                p1 = new Panel();
                PanelList.Add(p1);
                panel1.Controls.Add(p1);
                p1.BackColor = Color.FromKnownColor(KnownColor.ControlLight);
                p1.Size = new Size(540, 380);
                p1.Location = new Point((this.Width / 2) - (p1.Width / 2), 40 + i * (p1.Size.Height + 50));

                panel1.Size = new Size(panel1.Width, 40 + i * (p1.Size.Height + 50) + (p1.Size.Height + 50));

                if (panel1.Size.Height < this.ClientSize.Height)
                {
                    panel1.Height = this.ClientSize.Height;
                }

                l1 = new Label();
                LabelList.Add(l1);
                l1.AutoSize = true;
                p1.Controls.Add(l1);
                l1.Text = AllTitles[i];
                l1.Location = new Point((p1.Width / 2) - (l1.Width / 2), 20);
                l1.BorderStyle = BorderStyle.FixedSingle;

                l2 = new Label();
                NumLabelList.Add(l2);
                p1.Controls.Add(l2);
                l2.Text = "#" + (i + 1);
                l2.Location = new Point(10, 10);
                l2.AutoSize = true;
                l2.Font = new Font(DefaultFont.Name, 14.25f, FontStyle.Bold);
                l2.BorderStyle = BorderStyle.FixedSingle;

                l3 = new Label();
                p1.Controls.Add(l3);
                l3.Text = "Visible: " + ShowingWindow[i];
                l3.Location = new Point(10, 45);
                l3.AutoSize = true;
                l3.Font = new Font(DefaultFont.Name, 14.25f, FontStyle.Bold);

                p1.BorderStyle = BorderStyle.FixedSingle;
                Debug.WriteLine(AllKeyBinds.Count);
                Debug.WriteLine(PanelList[i].Location + "   " + PanelList[i].Size);

                panel1.Controls.Add(label1);
                label1.Text = "Total: " + AllKeyBinds.Count;
                label1.Visible = true;

                resetPanelLocation = true;
                SendScrollInput(new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
                UpdateScroll(false);
            }

            prevListValue = AllKeyBinds.Count;

        }

        public void DisposeOldPanels()
        {
            label1.Visible = false;
            panel1.Controls.Clear();

            for (int i = 0; i < PanelList.Count; i++)
            {
                PanelList[i].Dispose();
                LabelList[i].Dispose();
                NumLabelList[i].Dispose();
            }

            PanelList.Clear();
            LabelList.Clear();
            NumLabelList.Clear();

        }

        private void Form3_Resize(object sender, EventArgs e)
        {
            panel1.Size = new Size(this.ClientSize.Width, panel1.Size.Height);
            Debug.WriteLine("Resized");
            UpdatePanels();
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            listener.KeyDown -= Listener_KeyDown;
            listener.KeyUp -= Listener_KeyUp;
            listener.Uninstall();
        }
    }
}
