using System.Drawing.Printing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;
using System.Media;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoMaticAa
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection? videoDevices;
        private VideoCaptureDevice? videoSource;

        private WaveInEvent waveIn;
        private bool isTriggered = false;
        private int triggerThreshold = 70; // Gevoeligheid (0–100)
        private int cooldownTime = 5000;

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
            StartMicrophone();
        }
        private void PlayClickSound()
        {
            string path = Path.Combine(Application.StartupPath, "Sounds", "camera.wav");

            // Controleer of het bestand bestaat
            if (File.Exists(path))
            {
                SoundPlayer player = new SoundPlayer(path);
                player.Play();
            }
            else
            {
                MessageBox.Show("Het geluid bestand is niet gevonden.");
            }
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
        private void StartMicrophone(int deviceIndex = 0)
        {
            if (waveIn != null)
            {
                waveIn.DataAvailable -= WaveIn_DataAvailable;
                waveIn.StopRecording();
                waveIn.Dispose();
            }

            waveIn = new WaveInEvent
            {
                DeviceNumber = deviceIndex,
                WaveFormat = new WaveFormat(44100, 1)
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.StartRecording();
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                int maxVolume = 0;
                for (int i = 0; i < e.BytesRecorded; i += 2)
                {
                    short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                    maxVolume = Math.Max(maxVolume, Math.Abs(sample));
                }

                int volumeLevel = (int)((float)maxVolume / short.MaxValue * 100);

                // Update UI met microfoonvolume
                this.BeginInvoke(new Action(() =>
                {
                    progressBarMic.Value = Math.Min(progressBarMic.Maximum, volumeLevel);
                }));

                if (volumeLevel > triggerThreshold && !isTriggered)
                {
                    isTriggered = true;

                    // Start actie bij trigger
                    this.BeginInvoke(() =>
                    {
                        PlayClickSound(); // Je eigen methode
                        TakePicture();      // Je eigen methode
                    });

                    // Cooldown wachten
                    Task.Run(async () =>
                    {
                        await Task.Delay(cooldownTime);
                        isTriggered = false;
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fout in mic detectie: " + ex.Message);
            }
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
            TakePicture();
        }

        private void TakePicture()
        {
            if (videoSource == null || !videoSource.IsRunning)
            {
                MessageBox.Show("Camera is niet actief.");
                return;
            }
            btnTakePictures.Enabled = false;
            capturedPhotos.Clear();
            photoCount = 0;

            photoTimer = new System.Windows.Forms.Timer();
            int intervalSeconds = (int)numInterval.Value;
            photoTimer.Interval = intervalSeconds * 1000;
            photoTimer.Tick += PhotoTimer_Tick;
            photoTimer.Start();
        }

        private void PhotoTimer_Tick(object? sender, EventArgs e)
        {
            if (videoSource == null || pictureBox1.Image == null) return;

            // Capture huidige frame
            Bitmap photo = new Bitmap(pictureBox1.Image);
            capturedPhotos.Add(photo);
            PlayClickSound();
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
            string customText = txtOndertekst.Text;
            g.DrawString(customText, font, Brushes.Black, new PointF(margin, strip.Height - textHeight));

            // Toon de strip in een nieuwe PictureBox of sla op
            pictureBox1.Image = strip;

            // Als je meteen wilt printen:
            PrintImage(strip);
            btnTakePictures.Enabled = true;
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

        private void numInterval_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
