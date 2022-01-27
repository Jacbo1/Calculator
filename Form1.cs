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
        private string dataPath = "";
        private float ogWorkOutputFontSize;
        private const double WORK_OUTPUT_HEIGHT_MULT = 1030.0 / 1080.0;

        public Form1()
        {
            try
            {
                dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Calculator\";
            }
            catch { }
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
            ogWorkOutputFontSize = WorkOutput.Font.Size;
            try
            {
                if (File.Exists($"{dataPath}data.txt"))
                {
                    input = TextInput.Text = File.ReadAllText($"{dataPath}data.txt");
                    inputCounter++;
                }
            }
            catch { }
            TextInput.Focus();
            TextInput.TextChanged += new EventHandler(TextInput_TextChanged);
            worker.RunWorkerAsync();
            timer.Start();
        }

        private void TextInput_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(dataPath);
                File.WriteAllText($"{dataPath}data.txt", TextInput.Text);
            }
            catch { }
            inputCounter++;
            input = TextInput.Text;
        }

        private void ScaleFont(Label label)
        {
            Font font = new Font(label.Font.FontFamily, ogWorkOutputFontSize, label.Font.Style);
            double maxHeight = Height * WORK_OUTPUT_HEIGHT_MULT;
            while (maxHeight < TextRenderer.MeasureText(label.Text, font).Height)
            {
                font = new Font(font.FontFamily, font.Size - 0.5f, font.Style);
            }
            label.Font = font;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (displayCounter != outputCounter)
            {
                displayCounter = outputCounter;
                AnswerOutput.Text = answer;
                WorkOutput.Text = work;
                ScaleFont(WorkOutput);
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
                        answer = FormulaGroup.Calculate(input, out work);
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
    }
}
