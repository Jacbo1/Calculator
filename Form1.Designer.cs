namespace Calculator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.MenuToggleWork = new System.Windows.Forms.MenuItem();
            this.Menu1Instance = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.MenuOutputDefault = new System.Windows.Forms.MenuItem();
            this.MenuOutputScientific = new System.Windows.Forms.MenuItem();
            this.MenuOutputFractions = new System.Windows.Forms.MenuItem();
            this.MenuOutputBinary = new System.Windows.Forms.MenuItem();
            this.MenuOutputHex = new System.Windows.Forms.MenuItem();
            this.TextInput = new System.Windows.Forms.RichTextBox();
            this.WorkOutput = new System.Windows.Forms.RichTextBox();
            this.AnswerOutput = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // timer
            // 
            this.timer.Interval = 20;
            this.timer.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // worker
            // 
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Worker_DoWork);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuToggleWork,
            this.Menu1Instance});
            this.menuItem1.Text = "Settings";
            // 
            // MenuToggleWork
            // 
            this.MenuToggleWork.Checked = true;
            this.MenuToggleWork.Index = 0;
            this.MenuToggleWork.Text = "Toggle Work";
            // 
            // Menu1Instance
            // 
            this.Menu1Instance.Index = 1;
            this.Menu1Instance.Text = "Only 1 instance";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuOutputDefault,
            this.MenuOutputScientific,
            this.MenuOutputFractions,
            this.MenuOutputBinary,
            this.MenuOutputHex});
            this.menuItem2.Text = "Output";
            // 
            // MenuOutputDefault
            // 
            this.MenuOutputDefault.Index = 0;
            this.MenuOutputDefault.Text = "Default";
            this.MenuOutputDefault.Click += new System.EventHandler(this.MenuOutputDefault_Click);
            // 
            // MenuOutputScientific
            // 
            this.MenuOutputScientific.Index = 1;
            this.MenuOutputScientific.Text = "Scientific";
            this.MenuOutputScientific.Click += new System.EventHandler(this.MenuOutputScientific_Click);
            // 
            // MenuOutputFractions
            // 
            this.MenuOutputFractions.Index = 2;
            this.MenuOutputFractions.Text = "Fractions";
            this.MenuOutputFractions.Click += new System.EventHandler(this.MenuOutputFractions_Click);
            // 
            // MenuOutputBinary
            // 
            this.MenuOutputBinary.Index = 3;
            this.MenuOutputBinary.Text = "Binary";
            this.MenuOutputBinary.Click += new System.EventHandler(this.MenuOutputBinary_Click);
            // 
            // MenuOutputHex
            // 
            this.MenuOutputHex.Index = 4;
            this.MenuOutputHex.Text = "Hexadecimal";
            this.MenuOutputHex.Click += new System.EventHandler(this.MenuOutputHex_Click);
            // 
            // TextInput
            // 
            this.TextInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.TextInput.BackColor = System.Drawing.Color.Black;
            this.TextInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextInput.Font = new System.Drawing.Font("Bahnschrift Light", 18F);
            this.TextInput.ForeColor = System.Drawing.Color.White;
            this.TextInput.Location = new System.Drawing.Point(0, 0);
            this.TextInput.Margin = new System.Windows.Forms.Padding(0);
            this.TextInput.Name = "TextInput";
            this.TextInput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.TextInput.Size = new System.Drawing.Size(492, 715);
            this.TextInput.TabIndex = 7;
            this.TextInput.Text = "";
            // 
            // WorkOutput
            // 
            this.WorkOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.WorkOutput.BackColor = System.Drawing.Color.Black;
            this.WorkOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WorkOutput.Font = new System.Drawing.Font("Bahnschrift Light", 18F);
            this.WorkOutput.ForeColor = System.Drawing.Color.White;
            this.WorkOutput.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.WorkOutput.Location = new System.Drawing.Point(492, 0);
            this.WorkOutput.Margin = new System.Windows.Forms.Padding(0);
            this.WorkOutput.Name = "WorkOutput";
            this.WorkOutput.ReadOnly = true;
            this.WorkOutput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.WorkOutput.Size = new System.Drawing.Size(492, 715);
            this.WorkOutput.TabIndex = 8;
            this.WorkOutput.Text = "...";
            // 
            // AnswerOutput
            // 
            this.AnswerOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AnswerOutput.BackColor = System.Drawing.Color.Black;
            this.AnswerOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AnswerOutput.Font = new System.Drawing.Font("Bahnschrift Light", 18F);
            this.AnswerOutput.ForeColor = System.Drawing.Color.White;
            this.AnswerOutput.Location = new System.Drawing.Point(0, 715);
            this.AnswerOutput.Margin = new System.Windows.Forms.Padding(0);
            this.AnswerOutput.Multiline = false;
            this.AnswerOutput.Name = "AnswerOutput";
            this.AnswerOutput.ReadOnly = true;
            this.AnswerOutput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Horizontal;
            this.AnswerOutput.Size = new System.Drawing.Size(984, 36);
            this.AnswerOutput.TabIndex = 9;
            this.AnswerOutput.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.BlanchedAlmond;
            this.ClientSize = new System.Drawing.Size(984, 751);
            this.Controls.Add(this.AnswerOutput);
            this.Controls.Add(this.WorkOutput);
            this.Controls.Add(this.TextInput);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "Calculator";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem MenuToggleWork;
        private System.Windows.Forms.MenuItem Menu1Instance;
        private System.Windows.Forms.RichTextBox TextInput;
        private System.Windows.Forms.RichTextBox WorkOutput;
        private System.Windows.Forms.RichTextBox AnswerOutput;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem MenuOutputDefault;
        private System.Windows.Forms.MenuItem MenuOutputScientific;
        private System.Windows.Forms.MenuItem MenuOutputFractions;
        private System.Windows.Forms.MenuItem MenuOutputBinary;
        private System.Windows.Forms.MenuItem MenuOutputHex;
    }
}

