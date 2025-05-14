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
            numInterval = new NumericUpDown();
            button1 = new Button();
            label1 = new Label();
            progressBarMic = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numInterval).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(30, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(758, 398);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // btnTakePictures
            // 
            btnTakePictures.Location = new Point(265, 557);
            btnTakePictures.Name = "btnTakePictures";
            btnTakePictures.Size = new Size(292, 34);
            btnTakePictures.TabIndex = 1;
            btnTakePictures.Text = "Take Picture";
            btnTakePictures.UseVisualStyleBackColor = true;
            btnTakePictures.Click += btnTakePictures_Click;
            // 
            // txtOndertekst
            // 
            txtOndertekst.Location = new Point(30, 426);
            txtOndertekst.Name = "txtOndertekst";
            txtOndertekst.Size = new Size(758, 31);
            txtOndertekst.TabIndex = 2;
            // 
            // numInterval
            // 
            numInterval.Location = new Point(265, 505);
            numInterval.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numInterval.Name = "numInterval";
            numInterval.Size = new Size(180, 31);
            numInterval.TabIndex = 3;
            numInterval.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numInterval.ValueChanged += numInterval_ValueChanged;
            // 
            // button1
            // 
            button1.Location = new Point(462, 505);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 4;
            button1.Text = "Save";
            button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(372, 477);
            label1.Name = "label1";
            label1.Size = new Size(122, 25);
            label1.TabIndex = 5;
            label1.Text = "Photo interval";
            // 
            // progressBarMic
            // 
            progressBarMic.Location = new Point(54, 515);
            progressBarMic.Name = "progressBarMic";
            progressBarMic.RightToLeft = RightToLeft.Yes;
            progressBarMic.Size = new Size(205, 21);
            progressBarMic.Style = ProgressBarStyle.Continuous;
            progressBarMic.TabIndex = 6;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 603);
            Controls.Add(progressBarMic);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(numInterval);
            Controls.Add(txtOndertekst);
            Controls.Add(btnTakePictures);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numInterval).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Button btnTakePictures;
        private TextBox txtOndertekst;
        private NumericUpDown numInterval;
        private Button button1;
        private Label label1;
        private ProgressBar progressBarMic;
    }
}
