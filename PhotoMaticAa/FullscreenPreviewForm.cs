using System;
using System.Drawing;
using System.Windows.Forms;

namespace PhotoMaticAa
{
    public partial class FullscreenPreviewForm : Form
    {
        private Panel pnlVolumeTrack;
        private Panel pnlVolumeFill;
        private Panel pnlThresholdLine;

        private int currentThreshold = 50; // standaardwaarde, wordt vanuit Form1 gezet
        public FullscreenPreviewForm(Image imageToShow)
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackgroundImage = imageToShow;
            this.BackgroundImageLayout = ImageLayout.Zoom;
            this.KeyPreview = true;
            this.Click += (s, e) => this.Close();
            this.KeyDown += FullscreenForm_KeyDown;

            SetupVolumeBar();
        }

        private void FullscreenForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void SetupVolumeBar()
        {
            int barWidth = 30;
            int totalHeight = (int)(this.ClientSize.Height * 0.8);
            int topMargin = (this.ClientSize.Height - totalHeight) / 2;

            pnlVolumeTrack = new Panel
            {
                Width = barWidth,
                Height = totalHeight,
                BackColor = Color.DarkGray,
                Left = 20,
                Top = topMargin
            };
            this.Controls.Add(pnlVolumeTrack);

            pnlVolumeFill = new Panel
            {
                Width = barWidth,
                Height = 0,
                BackColor = Color.LimeGreen,
                Left = 0,
                Top = pnlVolumeTrack.Height
            };
            pnlVolumeTrack.Controls.Add(pnlVolumeFill);

            pnlThresholdLine = new Panel
            {
                Width = barWidth,
                Height = 3,
                BackColor = Color.Red,
                Left = 0,
                Top = 0 // wordt berekend
            };
            pnlVolumeTrack.Controls.Add(pnlThresholdLine);

            UpdateThresholdLine();
        }

        public void UpdateVolumeBar(int volumePercent)
        {
            volumePercent = Math.Clamp(volumePercent, 0, 100);

            int maxHeight = pnlVolumeTrack.Height;
            int fillHeight = (int)(maxHeight * volumePercent / 100.0);

            pnlVolumeFill.Height = fillHeight;
            pnlVolumeFill.Top = maxHeight - fillHeight;
        }

        public void SetThreshold(int thresholdPercent)
        {
            currentThreshold = Math.Clamp(thresholdPercent, 0, 100);
            UpdateThresholdLine();
        }

        private void UpdateThresholdLine()
        {
            if (pnlVolumeTrack == null || pnlThresholdLine == null) return;

            int maxHeight = pnlVolumeTrack.Height;
            int y = (int)(maxHeight * (1 - currentThreshold / 100.0)) - pnlThresholdLine.Height / 2;
            pnlThresholdLine.Top = y;
        }
    }
}
