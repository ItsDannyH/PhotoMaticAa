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
            numFlashTime = new NumericUpDown();
            lblTotalInt = new Label();
            label10 = new Label();
            label5 = new Label();
            numKnopCooldown = new NumericUpDown();
            label3 = new Label();
            label9 = new Label();
            txtEmail = new TextBox();
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
            rtbLog = new RichTextBox();
            label11 = new Label();
            numPaperLeft = new NumericUpDown();
            btnResetPaper = new Button();
            cmbCamera = new ComboBox();
            label12 = new Label();
            label13 = new Label();
            cmbMicrophone = new ComboBox();
            cmbPrinterSelect = new ComboBox();
            label14 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numIntervalLed).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numFlashTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numKnopCooldown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numIntervalPic).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMicThreshold).BeginInit();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPaperLeft).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(36, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(924, 398);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // btnTakePictures
            // 
            btnTakePictures.Location = new Point(16, 178);
            btnTakePictures.Name = "btnTakePictures";
            btnTakePictures.Size = new Size(278, 34);
            btnTakePictures.TabIndex = 1;
            btnTakePictures.Text = "Take Picture";
            btnTakePictures.UseVisualStyleBackColor = true;
            btnTakePictures.Click += btnTakePictures_Click;
            // 
            // txtOndertekst
            // 
            txtOndertekst.Location = new Point(30, 457);
            txtOndertekst.Name = "txtOndertekst";
            txtOndertekst.Size = new Size(924, 31);
            txtOndertekst.TabIndex = 2;
            // 
            // numIntervalLed
            // 
            numIntervalLed.DecimalPlaces = 1;
            numIntervalLed.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numIntervalLed.Location = new Point(221, 31);
            numIntervalLed.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numIntervalLed.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numIntervalLed.Name = "numIntervalLed";
            numIntervalLed.Size = new Size(64, 31);
            numIntervalLed.TabIndex = 3;
            numIntervalLed.Value = new decimal(new int[] { 3, 0, 0, 65536 });
            numIntervalLed.ValueChanged += Interval_ValueChanged;
            // 
            // progressBarMic
            // 
            progressBarMic.Location = new Point(20, 147);
            progressBarMic.Name = "progressBarMic";
            progressBarMic.RightToLeft = RightToLeft.Yes;
            progressBarMic.Size = new Size(241, 21);
            progressBarMic.Style = ProgressBarStyle.Continuous;
            progressBarMic.TabIndex = 6;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(numFlashTime);
            groupBox1.Controls.Add(lblTotalInt);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(numKnopCooldown);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(txtEmail);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(numIntervalPic);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(numIntervalLed);
            groupBox1.Location = new Point(30, 494);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(300, 249);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Interval";
            // 
            // numFlashTime
            // 
            numFlashTime.Location = new Point(228, 171);
            numFlashTime.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numFlashTime.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numFlashTime.Name = "numFlashTime";
            numFlashTime.Size = new Size(66, 31);
            numFlashTime.TabIndex = 14;
            numFlashTime.Value = new decimal(new int[] { 150, 0, 0, 0 });
            numFlashTime.ValueChanged += numFlashTime_ValueChanged;
            // 
            // lblTotalInt
            // 
            lblTotalInt.AutoSize = true;
            lblTotalInt.Location = new Point(226, 112);
            lblTotalInt.Name = "lblTotalInt";
            lblTotalInt.RightToLeft = RightToLeft.No;
            lblTotalInt.Size = new Size(19, 25);
            lblTotalInt.TabIndex = 10;
            lblTotalInt.Text = "-";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(6, 173);
            label10.Margin = new Padding(0);
            label10.Name = "label10";
            label10.Size = new Size(229, 25);
            label10.TabIndex = 15;
            label10.Text = "Set time Flash Duration: Ms";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 112);
            label5.Name = "label5";
            label5.Size = new Size(120, 25);
            label5.TabIndex = 9;
            label5.Text = "Total interval :";
            // 
            // numKnopCooldown
            // 
            numKnopCooldown.DecimalPlaces = 1;
            numKnopCooldown.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numKnopCooldown.Location = new Point(240, 138);
            numKnopCooldown.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numKnopCooldown.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numKnopCooldown.Name = "numKnopCooldown";
            numKnopCooldown.Size = new Size(53, 31);
            numKnopCooldown.TabIndex = 11;
            numKnopCooldown.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(176, 72);
            label3.Name = "label3";
            label3.Size = new Size(39, 25);
            label3.TabIndex = 8;
            label3.Text = "Sec";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 140);
            label9.Margin = new Padding(0);
            label9.Name = "label9";
            label9.Size = new Size(237, 25);
            label9.TabIndex = 11;
            label9.Text = "Block Input after picture: Sec";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(6, 208);
            txtEmail.Name = "txtEmail";
            txtEmail.PlaceholderText = "vul hier uw email adress in";
            txtEmail.Size = new Size(288, 31);
            txtEmail.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 72);
            label4.Name = "label4";
            label4.Size = new Size(128, 25);
            label4.TabIndex = 7;
            label4.Text = "Interval Picture";
            // 
            // numIntervalPic
            // 
            numIntervalPic.DecimalPlaces = 1;
            numIntervalPic.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numIntervalPic.Location = new Point(221, 70);
            numIntervalPic.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numIntervalPic.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numIntervalPic.Name = "numIntervalPic";
            numIntervalPic.Size = new Size(64, 31);
            numIntervalPic.TabIndex = 6;
            numIntervalPic.Value = new decimal(new int[] { 3, 0, 0, 65536 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(176, 33);
            label2.Name = "label2";
            label2.Size = new Size(39, 25);
            label2.TabIndex = 5;
            label2.Text = "Sec";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 33);
            label1.Name = "label1";
            label1.Size = new Size(103, 25);
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
            groupBox2.Controls.Add(progressBarMic);
            groupBox2.Controls.Add(btnTakePictures);
            groupBox2.Location = new Point(654, 494);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(300, 249);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "Take Picture";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(267, 100);
            label8.Name = "label8";
            label8.Size = new Size(27, 25);
            label8.TabIndex = 13;
            label8.Text = "%";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(20, 100);
            label7.Name = "label7";
            label7.Size = new Size(123, 25);
            label7.TabIndex = 12;
            label7.Text = "Mic Threshold";
            // 
            // numMicThreshold
            // 
            numMicThreshold.Location = new Point(197, 98);
            numMicThreshold.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMicThreshold.Name = "numMicThreshold";
            numMicThreshold.Size = new Size(64, 31);
            numMicThreshold.TabIndex = 11;
            numMicThreshold.Value = new decimal(new int[] { 80, 0, 0, 0 });
            // 
            // radioBtnClick
            // 
            radioBtnClick.AutoSize = true;
            radioBtnClick.Location = new Point(20, 68);
            radioBtnClick.Name = "radioBtnClick";
            radioBtnClick.Size = new Size(136, 29);
            radioBtnClick.TabIndex = 1;
            radioBtnClick.TabStop = true;
            radioBtnClick.Text = "Button Press";
            radioBtnClick.UseVisualStyleBackColor = true;
            // 
            // radioBtnMic
            // 
            radioBtnMic.AutoSize = true;
            radioBtnMic.Location = new Point(20, 33);
            radioBtnMic.Name = "radioBtnMic";
            radioBtnMic.Size = new Size(133, 29);
            radioBtnMic.TabIndex = 0;
            radioBtnMic.TabStop = true;
            radioBtnMic.Text = "Microphone";
            radioBtnMic.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(30, 429);
            label6.Name = "label6";
            label6.Size = new Size(192, 25);
            label6.TabIndex = 11;
            label6.Text = "Text Input Picture Strip:";
            // 
            // btnSelectBackground
            // 
            btnSelectBackground.Location = new Point(6, 30);
            btnSelectBackground.Name = "btnSelectBackground";
            btnSelectBackground.Size = new Size(288, 34);
            btnSelectBackground.TabIndex = 12;
            btnSelectBackground.Text = "Import Background";
            btnSelectBackground.UseVisualStyleBackColor = true;
            btnSelectBackground.Click += btnSelectBackground_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnSelectFont);
            groupBox3.Controls.Add(btnSelectBackground);
            groupBox3.Location = new Point(336, 494);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(300, 113);
            groupBox3.TabIndex = 11;
            groupBox3.TabStop = false;
            groupBox3.Text = "Style";
            // 
            // btnSelectFont
            // 
            btnSelectFont.Location = new Point(6, 70);
            btnSelectFont.Name = "btnSelectFont";
            btnSelectFont.Size = new Size(288, 34);
            btnSelectFont.TabIndex = 13;
            btnSelectFont.Text = "Kies Lettertype";
            btnSelectFont.UseVisualStyleBackColor = true;
            btnSelectFont.Click += btnSelectFont_Click;
            // 
            // rtbLog
            // 
            rtbLog.Location = new Point(30, 749);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbLog.Size = new Size(930, 359);
            rtbLog.TabIndex = 13;
            rtbLog.Text = "";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(342, 610);
            label11.Margin = new Padding(0);
            label11.Name = "label11";
            label11.Size = new Size(158, 25);
            label11.TabIndex = 11;
            label11.Text = "Printer paper level:";
            // 
            // numPaperLeft
            // 
            numPaperLeft.Increment = new decimal(new int[] { 0, 0, 0, 0 });
            numPaperLeft.Location = new Point(503, 608);
            numPaperLeft.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            numPaperLeft.Name = "numPaperLeft";
            numPaperLeft.Size = new Size(53, 31);
            numPaperLeft.TabIndex = 11;
            numPaperLeft.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnResetPaper
            // 
            btnResetPaper.Location = new Point(560, 606);
            btnResetPaper.Name = "btnResetPaper";
            btnResetPaper.Size = new Size(68, 34);
            btnResetPaper.TabIndex = 14;
            btnResetPaper.Text = "Reset";
            btnResetPaper.UseVisualStyleBackColor = true;
            btnResetPaper.Click += btnResetPaper_Click;
            // 
            // cmbCamera
            // 
            cmbCamera.FormattingEnabled = true;
            cmbCamera.Location = new Point(457, 639);
            cmbCamera.Name = "cmbCamera";
            cmbCamera.Size = new Size(173, 33);
            cmbCamera.TabIndex = 15;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(342, 641);
            label12.Margin = new Padding(0);
            label12.Name = "label12";
            label12.Size = new Size(76, 25);
            label12.TabIndex = 16;
            label12.Text = "Camera:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(342, 672);
            label13.Margin = new Padding(0);
            label13.Name = "label13";
            label13.Size = new Size(112, 25);
            label13.TabIndex = 16;
            label13.Text = "Microphone:";
            // 
            // cmbMicrophone
            // 
            cmbMicrophone.FormattingEnabled = true;
            cmbMicrophone.Location = new Point(457, 670);
            cmbMicrophone.Name = "cmbMicrophone";
            cmbMicrophone.Size = new Size(173, 33);
            cmbMicrophone.TabIndex = 15;
            // 
            // cmbPrinterSelect
            // 
            cmbPrinterSelect.Enabled = false;
            cmbPrinterSelect.Location = new Point(412, 702);
            cmbPrinterSelect.Name = "cmbPrinterSelect";
            cmbPrinterSelect.Size = new Size(218, 33);
            cmbPrinterSelect.TabIndex = 16;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(342, 705);
            label14.Margin = new Padding(0);
            label14.Name = "label14";
            label14.Size = new Size(67, 25);
            label14.TabIndex = 16;
            label14.Text = "Printer:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(982, 1113);
            Controls.Add(cmbMicrophone);
            Controls.Add(cmbPrinterSelect);
            Controls.Add(cmbCamera);
            Controls.Add(label14);
            Controls.Add(label13);
            Controls.Add(label12);
            Controls.Add(btnResetPaper);
            Controls.Add(rtbLog);
            Controls.Add(pictureBox1);
            Controls.Add(groupBox3);
            Controls.Add(label6);
            Controls.Add(numPaperLeft);
            Controls.Add(groupBox2);
            Controls.Add(label11);
            Controls.Add(groupBox1);
            Controls.Add(txtOndertekst);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numIntervalLed).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numFlashTime).EndInit();
            ((System.ComponentModel.ISupportInitialize)numKnopCooldown).EndInit();
            ((System.ComponentModel.ISupportInitialize)numIntervalPic).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMicThreshold).EndInit();
            groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numPaperLeft).EndInit();
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
        private NumericUpDown numKnopCooldown;
        private Label label9;
        private RichTextBox rtbLog;
        private NumericUpDown numFlashTime;
        private Label label10;
        private Label label11;
        private NumericUpDown numPaperLeft;
        private Button btnResetPaper;
        private ComboBox cmbCamera;
        private Label label12;
        private Label label13;
        private ComboBox cmbMicrophone;
        private ComboBox cmbPrinterSelect;
        private Label label14;
    }
}
