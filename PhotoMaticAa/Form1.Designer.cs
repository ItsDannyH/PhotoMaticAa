namespace PhotoMaticAa
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            btnTakePictures = new Button();
            txtOndertekst = new TextBox();
            numIntervalLed = new NumericUpDown();
            progressBarMic = new ProgressBar();
            groupBox1 = new GroupBox();
            lblTotalInt = new Label();
            label5 = new Label();
            label3 = new Label();
            label4 = new Label();
            numIntervalPic = new NumericUpDown();
            label2 = new Label();
            label1 = new Label();
            groupBox2 = new GroupBox();
            label8 = new Label();
            label7 = new Label();
            numMicThreshold = new NumericUpDown();
            radioBtnClick = new RadioButton();
            radioBtnMic = new RadioButton();
            label6 = new Label();
            btnSelectBackground = new Button();
            groupBox3 = new GroupBox();
            btnSelectFont = new Button();
            fontDialog1 = new FontDialog();
            txtEmail = new TextBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numIntervalLed).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numIntervalPic).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMicThreshold).BeginInit();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(25, 7);
            pictureBox1.Margin = new Padding(2, 2, 2, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(647, 239);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // btnTakePictures
            // 
            btnTakePictures.Location = new Point(21, 390);
            btnTakePictures.Margin = new Padding(2, 2, 2, 2);
            btnTakePictures.Name = "btnTakePictures";
            btnTakePictures.Size = new Size(204, 20);
            btnTakePictures.TabIndex = 1;
            btnTakePictures.Text = "Take Picture";
            btnTakePictures.UseVisualStyleBackColor = true;
            btnTakePictures.Click += btnTakePictures_Click;
            // 
            // txtOndertekst
            // 
            txtOndertekst.Location = new Point(21, 274);
            txtOndertekst.Margin = new Padding(2, 2, 2, 2);
            txtOndertekst.Name = "txtOndertekst";
            txtOndertekst.Size = new Size(648, 23);
            txtOndertekst.TabIndex = 2;
            // 
            // numIntervalLed
            // 
            numIntervalLed.DecimalPlaces = 1;
            numIntervalLed.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numIntervalLed.Location = new Point(155, 19);
            numIntervalLed.Margin = new Padding(2, 2, 2, 2);
            numIntervalLed.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numIntervalLed.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numIntervalLed.Name = "numIntervalLed";
            numIntervalLed.Size = new Size(45, 23);
            numIntervalLed.TabIndex = 3;
            numIntervalLed.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numIntervalLed.ValueChanged += Interval_ValueChanged;
            // 
            // progressBarMic
            // 
            progressBarMic.Location = new Point(524, 406);
            progressBarMic.Margin = new Padding(2, 2, 2, 2);
            progressBarMic.Name = "progressBarMic";
            progressBarMic.RightToLeft = RightToLeft.Yes;
            progressBarMic.Size = new Size(144, 13);
            progressBarMic.Style = ProgressBarStyle.Continuous;
            progressBarMic.TabIndex = 6;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblTotalInt);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(numIntervalPic);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(numIntervalLed);
            groupBox1.Location = new Point(21, 296);
            groupBox1.Margin = new Padding(2, 2, 2, 2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(2, 2, 2, 2);
            groupBox1.Size = new Size(210, 90);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Interval";
            // 
            // lblTotalInt
            // 
            lblTotalInt.AutoSize = true;
            lblTotalInt.Location = new Point(158, 67);
            lblTotalInt.Margin = new Padding(2, 0, 2, 0);
            lblTotalInt.Name = "lblTotalInt";
            lblTotalInt.RightToLeft = RightToLeft.No;
            lblTotalInt.Size = new Size(12, 15);
            lblTotalInt.TabIndex = 10;
            lblTotalInt.Text = "-";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(4, 67);
            label5.Margin = new Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new Size(81, 15);
            label5.TabIndex = 9;
            label5.Text = "Total interval :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(123, 43);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(25, 15);
            label3.TabIndex = 8;
            label3.Text = "Sec";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(4, 43);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(86, 15);
            label4.TabIndex = 7;
            label4.Text = "Interval Picture";
            // 
            // numIntervalPic
            // 
            numIntervalPic.DecimalPlaces = 1;
            numIntervalPic.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numIntervalPic.Location = new Point(155, 42);
            numIntervalPic.Margin = new Padding(2, 2, 2, 2);
            numIntervalPic.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numIntervalPic.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numIntervalPic.Name = "numIntervalPic";
            numIntervalPic.Size = new Size(45, 23);
            numIntervalPic.TabIndex = 6;
            numIntervalPic.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(123, 20);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(25, 15);
            label2.TabIndex = 5;
            label2.Text = "Sec";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 20);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(68, 15);
            label1.TabIndex = 4;
            label1.Text = "Interval Led";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(numMicThreshold);
            groupBox2.Controls.Add(radioBtnClick);
            groupBox2.Controls.Add(radioBtnMic);
            groupBox2.Location = new Point(458, 296);
            groupBox2.Margin = new Padding(2, 2, 2, 2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(2, 2, 2, 2);
            groupBox2.Size = new Size(210, 90);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "Take Picture";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(187, 60);
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new Size(17, 15);
            label8.TabIndex = 13;
            label8.Text = "%";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(14, 60);
            label7.Margin = new Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new Size(83, 15);
            label7.TabIndex = 12;
            label7.Text = "Mic Threshold";
            // 
            // numMicThreshold
            // 
            numMicThreshold.Location = new Point(138, 59);
            numMicThreshold.Margin = new Padding(2, 2, 2, 2);
            numMicThreshold.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMicThreshold.Name = "numMicThreshold";
            numMicThreshold.Size = new Size(45, 23);
            numMicThreshold.TabIndex = 11;
            numMicThreshold.Value = new decimal(new int[] { 80, 0, 0, 0 });
            // 
            // radioBtnClick
            // 
            radioBtnClick.AutoSize = true;
            radioBtnClick.Location = new Point(14, 41);
            radioBtnClick.Margin = new Padding(2, 2, 2, 2);
            radioBtnClick.Name = "radioBtnClick";
            radioBtnClick.Size = new Size(91, 19);
            radioBtnClick.TabIndex = 1;
            radioBtnClick.TabStop = true;
            radioBtnClick.Text = "Button Press";
            radioBtnClick.UseVisualStyleBackColor = true;
            // 
            // radioBtnMic
            // 
            radioBtnMic.AutoSize = true;
            radioBtnMic.Location = new Point(14, 20);
            radioBtnMic.Margin = new Padding(2, 2, 2, 2);
            radioBtnMic.Name = "radioBtnMic";
            radioBtnMic.Size = new Size(90, 19);
            radioBtnMic.TabIndex = 0;
            radioBtnMic.TabStop = true;
            radioBtnMic.Text = "Microphone";
            radioBtnMic.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(21, 257);
            label6.Margin = new Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new Size(129, 15);
            label6.TabIndex = 11;
            label6.Text = "Text Input Picture Strip:";
            // 
            // btnSelectBackground
            // 
            btnSelectBackground.Location = new Point(4, 18);
            btnSelectBackground.Margin = new Padding(2, 2, 2, 2);
            btnSelectBackground.Name = "btnSelectBackground";
            btnSelectBackground.Size = new Size(202, 20);
            btnSelectBackground.TabIndex = 12;
            btnSelectBackground.Text = "Import Background";
            btnSelectBackground.UseVisualStyleBackColor = true;
            btnSelectBackground.Click += btnSelectBackground_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnSelectFont);
            groupBox3.Controls.Add(btnSelectBackground);
            groupBox3.Location = new Point(235, 296);
            groupBox3.Margin = new Padding(2, 2, 2, 2);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(2, 2, 2, 2);
            groupBox3.Size = new Size(210, 90);
            groupBox3.TabIndex = 11;
            groupBox3.TabStop = false;
            groupBox3.Text = "Style";
            // 
            // btnSelectFont
            // 
            btnSelectFont.Location = new Point(4, 42);
            btnSelectFont.Margin = new Padding(2, 2, 2, 2);
            btnSelectFont.Name = "btnSelectFont";
            btnSelectFont.Size = new Size(202, 20);
            btnSelectFont.TabIndex = 13;
            btnSelectFont.Text = "Kies Lettertype";
            btnSelectFont.UseVisualStyleBackColor = true;
            btnSelectFont.Click += btnSelectFont_Click;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(235, 390);
            txtEmail.Margin = new Padding(2, 2, 2, 2);
            txtEmail.Name = "txtEmail";
            txtEmail.PlaceholderText = "vul hier uw email adress in";
            txtEmail.Size = new Size(211, 23);
            txtEmail.TabIndex = 7;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(711, 441);
            Controls.Add(pictureBox1);
            Controls.Add(groupBox3);
            Controls.Add(label6);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(progressBarMic);
            Controls.Add(txtOndertekst);
            Controls.Add(txtEmail);
            Controls.Add(btnTakePictures);
            Margin = new Padding(2, 2, 2, 2);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numIntervalLed).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numIntervalPic).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMicThreshold).EndInit();
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Button btnTakePictures;
        private TextBox txtOndertekst;
        private NumericUpDown numIntervalLed;
        private ProgressBar progressBarMic;
        private GroupBox groupBox1;
        private Label lblTotalInt;
        private Label label5;
        private Label label3;
        private Label label4;
        private NumericUpDown numIntervalPic;
        private Label label2;
        private Label label1;
        private GroupBox groupBox2;
        private RadioButton radioBtnClick;
        private RadioButton radioBtnMic;
        private Label label6;
        private NumericUpDown numMicThreshold;
        private Label label8;
        private Label label7;
        private Button btnSelectBackground;
        private TextBox txtEmail;
        private GroupBox groupBox3;
        private Button btnSelectFont;
        private FontDialog fontDialog1;
    }
}
