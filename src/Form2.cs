using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using KeyboardHook1;
using System.Diagnostics;
using System.Threading;

namespace Program_Hide
{
    public partial class Form2 : Form
    {
        public IntPtr inHandle;
        public string inTitle;
        public string keybindMethod;

        public Form2(IntPtr inputHandle, string inputTitle, string theKeybindMethod) //prevent making duplicate of the same window
        {
            InitializeComponent();

            this.Tag = theKeybindMethod;

            if (theKeybindMethod == "WIN")
            {
                inHandle = inputHandle;
                inTitle = inputTitle;
                checkBox1.Visible = true;
                checkBox2.Visible = false;
                checkBox3.Visible = false;
                button1.Location = new Point(342, 91);
            }

            if (theKeybindMethod == "APP")
            {
                checkBox1.Visible = true;
                checkBox2.Visible = true;
                checkBox3.Visible = true;
                button1.Location = new Point(342, 136);
            }

            keybindMethod = theKeybindMethod;
        }

        #region Form2_Load | Intializes

        KeyboardHook listener = new KeyboardHook();

        OpenFileDialog ofd1 = new OpenFileDialog();

        ToolTip tt1 = new ToolTip();
        ToolTip tt2 = new ToolTip();
        ToolTip tt3 = new ToolTip();
        #endregion



        private void Form2_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            button4.Visible = false;
            button1.Enabled = false;
            listener.KeyDown += listener_KeyDown;
            listener.KeyUp += listener_KeyUp;

            #region CheckBox | Tooltips

            tt1.AutoPopDelay = 32767;
            tt1.InitialDelay = 500;
            tt1.ReshowDelay = 500;
            tt1.SetToolTip(checkBox1, "On Keybind Press: Maximize when the Window is Shown");

            tt2.AutoPopDelay = 32767;
            tt2.InitialDelay = 500;
            tt2.ReshowDelay = 500;
            tt2.SetToolTip(checkBox2, "On Keybind Press: Create Instance of App if not Running");

            tt3.AutoPopDelay = 32767;
            tt3.InitialDelay = 500;
            tt3.ReshowDelay = 500;
            tt3.SetToolTip(checkBox3, "On Keybind Press: Always create new Instance of App");
            #endregion

            if (keybindMethod == "WIN")
            {
                if (inTitle.Length <= 28)
                {
                    label1.Text = "\"" + inTitle + "\"";
                }
                else
                {
                    label1.Text = "\"" + inTitle.Substring(0, 28) + "...\"";
                }

                label1.Location = new Point(((this.Width / 2) - label1.Width / 2), label1.Location.Y);
            }

            if (keybindMethod == "APP") 
            {
                label1.Visible = false;
                checkBox2.Visible = true;
                checkBox3.Visible = true;
            }
        }

        #region KeyUp Detection System
        bool alreadyShownMB;
        bool keyIsNew;
        public static bool NewWindowRecording;
        public static bool Form2ForceRecord;

        public static List<KeyboardHook.VKeys> NewWindowKeysList = new List<KeyboardHook.VKeys>();

        List<KeyboardHook.VKeys> currentKeys = new List<KeyboardHook.VKeys>();
        List<KeyboardHook.VKeys> TempKeysList = new List<KeyboardHook.VKeys>();
        private void listener_KeyDown(KeyboardHook.VKeys key)
        {
            if (!currentKeys.Contains(key))
            {
                currentKeys.Add(key);
                keyIsNew = true;
            }

            if (keyIsNew)
            {
                if (NewWindowRecording)
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

        public void listener_KeyUp(KeyboardHook.VKeys key)
        {
            if (currentKeys.Contains(key))
            {
                currentKeys.Remove(key);
            }

            if (NewWindowRecording)
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

        string tempFileLocation;
        string applicationLocation;
        string fileName;
        private void button3_Click(object sender, EventArgs e)
        {
            StopRecording();
            tempFileLocation = Interaction.InputBox("Please Enter your Application's File Path", "Open File");

            if (tempFileLocation.Contains(".exe") && File.Exists(tempFileLocation))
            {
                applicationLocation = ofd1.FileName;
                fileName = Path.GetFileName(tempFileLocation);
                ReceivedFile();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StopRecording();
            ofd1.Filter = "Executable Files (*.exe)|*.exe";
            ofd1.InitialDirectory = Environment.ExpandEnvironmentVariables("%PROGRAMFILES(x86)%");
            ofd1.Title = "Select your Application File";
            ofd1.CheckFileExists = true;
            ofd1.CheckPathExists = true;
            ofd1.Multiselect = false;

            if (ofd1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            else
            {
                applicationLocation = ofd1.FileName;
                fileName = Path.GetFileName(ofd1.FileName);
                ReceivedFile();
            }
        }

        public void ReceivedFile()
        {
            if (fileName.Length <= 28)
            {
                label1.Text = "\"" + fileName + "\"";
            }
            else
            {
                label1.Text = "\"" + fileName.Substring(0, 28) + "....exe\"";
            }

            label1.Location = new Point(((this.Width / 2) - label1.Width / 2), label1.Location.Y);
            label1.Visible = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            StopRecording();

            if (checkBox2.Checked && checkBox3.Checked)
            {
                checkBox3.Checked = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            StopRecording();

            if (checkBox2.Checked && checkBox3.Checked)
            {
                checkBox2.Checked = false;
            }
        }

        string RepeatedKeybinds;
        DialogResult AddKeybindResult;
        private void button1_Click(object sender, EventArgs e)
        {
            StopRecording();
            RepeatedKeybinds = "";
            for (int i1 = 0; i1 < Form3.AllKeyBinds.Count; i1++)
            {
                for (int i2 = 0; i2 < NewWindowKeysList.Count; i2++)
                {
                    if (!Form3.AllKeyBinds[i1].Contains(NewWindowKeysList[i2]))
                    {
                        break;
                    }
                    else
                    {
                        if (i2 == NewWindowKeysList.Count - 1)
                        {
                            RepeatedKeybinds += "\n\"" + Form3.AllTitles[i1] + "\"";
                            Debug.WriteLine("TRUE");
                            break;
                        }
                    }
                }
            }

            if (!(RepeatedKeybinds == ""))
            {
               AddKeybindResult = MessageBox.Show("Your current keybind contains keys that can open other keybinds!\nYou can continue if you would like, but it is not recommended.\nWould you like to continue?\n\nEffected Keybinds:" + RepeatedKeybinds,
                   "Program Hide - Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if (AddKeybindResult == DialogResult.No)
            {
                return;
            }


            if (keybindMethod == "WIN")
            {
                Form3.Maximize.Add(checkBox1.Checked);
                Form3.AllHandles.Add(inHandle);
                Form3.AllTitles.Add(inTitle);

                Form3.ProgramType.Add("WIN");
            }

            if (keybindMethod == "APP")
            {
                Form3.Maximize.Add(checkBox1.Checked);
                Form3.CIOnce.Add(checkBox2.Checked);
                Form3.CIAlways.Add(checkBox3.Checked);
                Form3.AllTitles.Add(fileName);

                Form3.ProgramType.Add("APP");
            }

            Form3.ShowingWindow.Add(false);
            Form3.AllKeyBinds.Add(NewWindowKeysList);

            MessageBox.Show("Successfully added keybind!", "Program Hide - Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            OpenFormThree();
            this.Close();
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
                    Form1.f3 = new Form3();
                }
            }

            Form1.f3.Show();
            Form1.f3.BringToFront();
        }

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

        private void button2_Click(object sender, EventArgs e)
        {
            Form2ForceRecord = true;
            Form1.Form1ForceRecord = false;


            this.ActiveControl = null;
            if (!NewWindowRecording)
            {
                timer1.Enabled = true;
                UpdateKeyRecording(true);
                TempKeysList.Clear();
                NewWindowRecording = true;
                button2.Text = "Stop Recording";
                button4.Visible = true;
            }
            else
            {
                timer1.Enabled = false;
                UpdateKeyRecording(false);
                alreadyShownMB = false;
                NewWindowRecording = false;
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
                    UpdateKeybindLabel();
                }

                currentKeys.Clear();
            }
        }

        public void SetNewKeys() 
        {
            NewWindowKeysList = TempKeysList.ToList();
            button1.Enabled = true;
        }

        public void UpdateKeybindLabel()
        {

            label2.Text = "";
            label2.Text += "Current:\n";
            for (int i = 0; i < NewWindowKeysList.Count; i++)
            {
                if (i < 1)
                {
                    label2.Text += NewWindowKeysList[i].ToString();
                }
                else
                {
                    label2.Text += " + " + NewWindowKeysList[i].ToString();
                }
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (keyListenerInstalled)
            {
                listener.Uninstall();
            }

            listener.KeyDown -= listener_KeyDown;
            listener.KeyUp -= listener_KeyUp;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        public void StopRecording()
        {
            Form1.Form1ForceRecord = false;
            if (button4.Visible)
            {
                button4.Visible = false;
            }

            NewWindowRecording = false;
            button2.Text = "Record Keys";
            listener.Uninstall();
            button4.Visible = false;
            textBox1.Text = "";
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
            if (NewWindowRecording)
            {
                if (Form1.Form1ForceRecord)
                {
                    StopRecording();
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            StopRecording();
        }
    }
}
