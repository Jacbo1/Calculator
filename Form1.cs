using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Calculator
{
    public partial class Form1 : Form
    {
        private int inputCounter = 0;
        private int outputCounter = 0;
        private int workerCounter = 0;
        private int displayCounter = 0;
        private string input = "";
        private string answer = "";
        private string work = "";
        private bool showWork = true,
            oneInstance = false,
            scientific = false,
            showFractions = false;

        public Form1()
        {
            oneInstance = Properties.Settings.Default.OneInstance;
            if (oneInstance)
            {
                // Close other instances and if there are none already then open
                Process current = Process.GetCurrentProcess();
                string path = current.MainModule.FileName;
                string name = current.ProcessName;
                int id = current.Id;
                bool found = false;
                foreach (Process process in Process.GetProcessesByName(name))
                {
                    if (process.Id != id && process.MainModule.FileName == path)
                    {
                        process.CloseMainWindow();
                        found = true;
                    }
                }
                if (found)
                {
                    Close();
                }
                else
                {
                    InitializeComponent();
                }
            }
            else
            {
                // Open
                InitializeComponent();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Restore state
            switch (Properties.Settings.Default.State)
            {
                case 1:
                    WindowState = FormWindowState.Maximized;
                    break;
                case 2:
                    WindowState = FormWindowState.Minimized;
                    break;
            }
            
            Location = Properties.Settings.Default.Pos;
            Size = Properties.Settings.Default.Size;
            showWork = Properties.Settings.Default.ShowWork;
            input = TextInput.Text = Properties.Settings.Default.Input;
            scientific = Properties.Settings.Default.Scientific;
            showFractions = Properties.Settings.Default.ShowFractions;

            inputCounter++;
            MenuToggleWork.Checked = showWork;
            Menu1Instance.Checked = oneInstance;
            MenuFractions.Checked = showFractions;
            MenuScientific.Checked = scientific;

            //
            AnswerOutput.Click += AnswerOutput_Click;
            TextInput.KeyDown += TextInput_KeyDown;
            TextInput.Focus();
            worker.RunWorkerAsync();
            timer.Start();

            TextInput.TextChanged += TextInput_TextChanged;
            ClientSizeChanged += WindowResized;
            FormClosed += WindowClosed;
            MenuToggleWork.Click += MenuButtonPressed_ShowWork;
            Menu1Instance.Click += MenuButtonPressed_1Instance;

            SizeComponents();
        }

        private void AnswerOutput_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(AnswerOutput.Text);
        }

        private void TextInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                TextInput.Text += (string)Clipboard.GetData("Text");
                e.Handled = true;
            }
        }

        private void TextInput_TextChanged(object sender, EventArgs e)
        {
            inputCounter++;
            Font x = TextInput.Font;
            input = TextInput.Text;
        }

        private void SizeComponents()
        {
            if (showWork)
            {
                int width = ClientSize.Width / 2;
                TextInput.SetBounds(0, 0, width, TextInput.Height);
                WorkOutput.SetBounds(width, 0, width, WorkOutput.Height);
            }
            else
            {
                TextInput.SetBounds(0, 0, ClientSize.Width, TextInput.Height);
                WorkOutput.SetBounds(ClientSize.Width, 0, 0, WorkOutput.Height);
            }
        }

        private void WindowResized(object sender, EventArgs e)
        {
            SizeComponents();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (displayCounter != outputCounter)
            {
                displayCounter = outputCounter;
                AnswerOutput.Text = answer;
                WorkOutput.Text = work;
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (inputCounter != workerCounter)
                {
                    workerCounter = inputCounter;
                    try
                    {
                        answer = FormulaGroup.Calculate(input, out work, scientific, showFractions);
                    }
                    catch (Exception error)
                    {
                        answer = error.Message;
                        work = error.ToString();
                    }
                    finally
                    {
                        outputCounter++;
                    }
                }
                Thread.Sleep(20);
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Input = TextInput.Text;
            if (WindowState == FormWindowState.Maximized)
            {
                Properties.Settings.Default.Pos = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.State = 1;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.State = 0;
                Properties.Settings.Default.Pos = Location;
                Properties.Settings.Default.Size = Size;
            }
            else
            {
                Properties.Settings.Default.Pos = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.State = 2;
            }
            Properties.Settings.Default.Save();
        }

        private void MenuFractions_Click(object sender, EventArgs e)
        {
            showFractions = !showFractions;
            if (showFractions && scientific)
            {
                scientific = false;
                MenuScientific.Checked = false;
                Properties.Settings.Default.Scientific = false;
            }
            Properties.Settings.Default.ShowFractions = showFractions;
            MenuFractions.Checked = showFractions;
            Properties.Settings.Default.Save();
            inputCounter++;
        }

        private void MenuButtonPressed_ShowWork(object sender, EventArgs e)
        {
            showWork = !showWork;
            Properties.Settings.Default.ShowWork = showWork;
            MenuToggleWork.Checked = showWork;
            SizeComponents();
            Properties.Settings.Default.Save();
        }

        private void MenuScientific_Click(object sender, EventArgs e)
        {
            scientific = !scientific;
            if (showFractions && scientific)
            {
                showFractions = false;
                MenuFractions.Checked = false;
                Properties.Settings.Default.ShowFractions = false;
            }
            Properties.Settings.Default.Scientific = scientific;
            MenuScientific.Checked = scientific;
            Properties.Settings.Default.Save();
            inputCounter++;
        }

        private void MenuButtonPressed_1Instance(object sender, EventArgs e)
        {
            oneInstance = !oneInstance;
            Properties.Settings.Default.OneInstance = oneInstance;
            Menu1Instance.Checked = oneInstance;
            Properties.Settings.Default.Save();
        }
    }
}
