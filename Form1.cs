using System;
using System.ComponentModel;
using System.Diagnostics;
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
        private bool showWork = true;
        private bool oneInstance = false;
        private bool topmost = false;
        private OutputMode outputMode = OutputMode.Default;

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
                if (found) Close();
                else InitializeComponent();
            }
            else InitializeComponent(); // Open
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Restore state
            switch (Properties.Settings.Default.State)
            {
                case 1: WindowState = FormWindowState.Maximized; break;
                case 2: WindowState = FormWindowState.Minimized; break;
            }
            
            Location = Properties.Settings.Default.Pos;
            Size = Properties.Settings.Default.Size;
            showWork = Properties.Settings.Default.ShowWork;
            input = TextInput.Text = Properties.Settings.Default.Input;
            TopMost = topmost = MenuTopMost.Checked = Properties.Settings.Default.TopMost;
            SetOutputMode((OutputMode)Properties.Settings.Default.OutputMode);

            inputCounter++;
            MenuToggleWork.Checked = showWork;
            Menu1Instance.Checked = oneInstance;

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

            MenuOutputBinary.Click += (_, __) => SetOutputMode(OutputMode.Binary);
            MenuOutputColorHex.Click += (_, __) => SetOutputMode(OutputMode.ColorHex);
            MenuOutputDefault.Click += (_, __) => SetOutputMode(OutputMode.Default);
            MenuOutputFractions.Click += (_, __) => SetOutputMode(OutputMode.Fractions);
            MenuOutputHex.Click += (_, __) => SetOutputMode(OutputMode.Hex);
            MenuOutputScientific.Click += (_, __) => SetOutputMode(OutputMode.Scientific);
        }

        private void AnswerOutput_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(AnswerOutput.Text);
        }

        private void TextInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
                Clipboard.SetText(Clipboard.GetText());
        }

        private void TextInput_TextChanged(object sender, EventArgs e)
        {
            inputCounter++;
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
            for (; ; )
            {
                if (inputCounter != workerCounter)
                {
                    workerCounter = inputCounter;
                    try
                    {
                        answer = FormulaGroup.Calculate(input, out work, outputMode);
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
            switch (WindowState)
            {
                case FormWindowState.Maximized:
                    Properties.Settings.Default.Pos = RestoreBounds.Location;
                    Properties.Settings.Default.Size = RestoreBounds.Size;
                    Properties.Settings.Default.State = 1;
                    break;

                case FormWindowState.Normal:
                    Properties.Settings.Default.State = 0;
                    Properties.Settings.Default.Pos = Location;
                    Properties.Settings.Default.Size = Size;
                    break;

                default:
                    Properties.Settings.Default.Pos = RestoreBounds.Location;
                    Properties.Settings.Default.Size = RestoreBounds.Size;
                    Properties.Settings.Default.State = 0; // 2
                    break;
            }
            Properties.Settings.Default.Save();
        }

        private void ToggleOutputButton(OutputMode mode, bool state)
        {
            switch (mode)
            {
                case OutputMode.Default: MenuOutputDefault.Checked = state; break;
                case OutputMode.Scientific: MenuOutputScientific.Checked = state; break;
                case OutputMode.Fractions: MenuOutputFractions.Checked = state; break;
                case OutputMode.Binary: MenuOutputBinary.Checked = state; break;
                case OutputMode.Hex: MenuOutputHex.Checked = state; break;
                case OutputMode.ColorHex: MenuOutputColorHex.Checked = state; break;
            }
        }

        private void MenuButtonPressed_ShowWork(object sender, EventArgs e)
        {
            showWork = !showWork;
            Properties.Settings.Default.ShowWork = showWork;
            MenuToggleWork.Checked = showWork;
            SizeComponents();
            Properties.Settings.Default.Save();
        }

        private void MenuButtonPressed_1Instance(object sender, EventArgs e)
        {
            oneInstance = !oneInstance;
            Properties.Settings.Default.OneInstance = oneInstance;
            Menu1Instance.Checked = oneInstance;
            Properties.Settings.Default.Save();
        }

        private void MenuTopMost_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.TopMost = MenuTopMost.Checked = TopMost = topmost = !topmost;
            Properties.Settings.Default.Save();
        }

        private void SetOutputMode(OutputMode mode)
        {
            ToggleOutputButton(outputMode, false);
            outputMode = mode;
            ToggleOutputButton(outputMode, true);
            Properties.Settings.Default.OutputMode = (byte)outputMode;
            Properties.Settings.Default.Save();
            inputCounter++;
        }
    }
}
