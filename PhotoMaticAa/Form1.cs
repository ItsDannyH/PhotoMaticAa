using System.Drawing.Printing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace PhotoMaticAa
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection? videoDevices;
        private VideoCaptureDevice? videoSource;

        private List<Bitmap> capturedPhotos = new();
        private int photoCount = 0;
        private System.Windows.Forms.Timer? photoTimer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
            StartCamera();
        }

        private void StartCamera()
        {
            // Zoek beschikbare camera's
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Geen camera gevonden!");
                return;
            }

            // Kies de eerste camera
            videoSource = new VideoCaptureDevice(videoDevices[1].MonikerString);

            // Elke frame verwerken
            videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);

            // Start camera
            videoSource.Start();
        }

        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Kopieer het beeld en toon het in de PictureBox
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bitmap;
        }


        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.NewFrame -= Video_NewFrame;
                videoSource.WaitForStop();
                videoSource = null;
            }

            Application.Exit(); // ensures full exit
        }

        private void btnTakePictures_Click(object sender, EventArgs e)
        {
            if (videoSource == null || !videoSource.IsRunning)
            {
                MessageBox.Show("Camera is niet actief.");
                return;
            }

            capturedPhotos.Clear();
            photoCount = 0;

            photoTimer = new System.Windows.Forms.Timer();
            photoTimer.Interval = 2000; // 2 seconden tussen foto's
            photoTimer.Tick += PhotoTimer_Tick;
            photoTimer.Start();
        }
        private void PhotoTimer_Tick(object? sender, EventArgs e)
        {
            if (videoSource == null || pictureBox1.Image == null) return;

            // Capture huidige frame
            Bitmap photo = new Bitmap(pictureBox1.Image);
            capturedPhotos.Add(photo);
            photoCount++;

            if (photoCount >= 3)
            {
                photoTimer?.Stop();
                photoTimer?.Dispose();
                photoTimer = null;

                CombinePhotosIntoStrip();
            }
        }
        private void CombinePhotosIntoStrip()
        {
            if (capturedPhotos.Count < 3) return;

            int width = capturedPhotos[0].Width;
            int height = capturedPhotos[0].Height;
            int margin = 10;
            int textHeight = 40;

            Bitmap strip = new Bitmap(width + 2 * margin, height * 3 + margin * 4 + textHeight);
            using Graphics g = Graphics.FromImage(strip);

            g.Clear(Color.White);

            for (int i = 0; i < 3; i++)
            {
                g.DrawImage(capturedPhotos[i], margin, margin + i * (height + margin));
                g.DrawRectangle(Pens.Black, margin, margin + i * (height + margin), width - 1, height - 1);
            }

            // Voeg tekst toe onderaan
            Font font = new Font("Arial", 12);
            string customText = "Bedankt voor het poseren!";
            g.DrawString(customText, font, Brushes.Black, new PointF(margin, strip.Height - textHeight));

            // Toon de strip in een nieuwe PictureBox of sla op
            pictureBox1.Image = strip;

            // Als je meteen wilt printen:
            PrintImage(strip);
        }
        private void PrintImage(Image imageToPrint)
        {
            PrintDocument printDoc = new();
            printDoc.PrintPage += (s, e) =>
            {
                Rectangle m = e.MarginBounds;

                // Zorg dat de afbeelding in de marges past
                if ((double)imageToPrint.Width / (double)imageToPrint.Height > (double)m.Width / (double)m.Height)
                {
                    m.Height = (int)((double)imageToPrint.Height / imageToPrint.Width * m.Width);
                }
                else
                {
                    m.Width = (int)((double)imageToPrint.Width / imageToPrint.Height * m.Height);
                }

                e.Graphics.DrawImage(imageToPrint, m);
            };

            try
            {
                printDoc.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij printen: " + ex.Message);
            }
        }
    }
}
