using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoMaticAa
{
    public partial class FullscreenPreviewForm : Form
    {
        private void Form2_Load(object sender, EventArgs e)
        {

        }
        public FullscreenPreviewForm(Image image)
        {
            InitializeComponent();
            this.BackgroundImage = image;
            this.BackgroundImageLayout = ImageLayout.Zoom;

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.KeyPreview = true;  // belangrijk om key events te kunnen opvangen

            this.KeyDown += FullscreenPreviewForm_KeyDown;
            this.Click += (s, e) => this.Close();  // klik sluit ook fullscreen
        }

        private void FullscreenPreviewForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

    }
}
