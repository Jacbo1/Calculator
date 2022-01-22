using System;
using System.ComponentModel;
using System.Diagnostics;
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

        public Form1()
        {
            Process current = Process.GetCurrentProcess();
            string path = current.MainModule.FileName;
            string name = current.ProcessName;
            int id = current.Id;
            bool found = false;
            foreach (Process process in Process.GetProcessesByName(name))
            {
                if (process.Id != id && process.MainModule.FileName == path)
                {
                    process.Kill();
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

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("data.txt"))
            {
                input = TextInput.Text = File.ReadAllText("data.txt");
                inputCounter++;
            }
            TextInput.Focus();
            TextInput.TextChanged += new EventHandler(TextInput_TextChanged);
            worker.RunWorkerAsync();
            timer.Start();
        }

        private void TextInput_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllText("data.txt", TextInput.Text);
            inputCounter++;
            input = TextInput.Text;
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
                if (inputCounter == workerCounter)
                {
                    Thread.Sleep(20);
                }
                else
                {
                    workerCounter = inputCounter;
                    answer = new Formula(input).Calculate(out work);
                    outputCounter++;
                }
            }
        }
    }
}
