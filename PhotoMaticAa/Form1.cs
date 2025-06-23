using System.Drawing.Printing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;
using System.Media;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Drawing;


namespace PhotoMaticAa
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection? videoDevices;
        private VideoCaptureDevice? videoSource;

        private int currentPhotoIndex = 0;
        private int totalPhotosToTake = 3;
        private bool isTakingPictures = false; // flag om meerdere triggers van takepicture te voorkomen


        private WaveInEvent waveIn;
        private bool isTriggered = false;
        private int triggerThreshold => (int)numMicThreshold.Value;
        private int cooldownTime = 5000;

        private List<Bitmap> capturedPhotos = new();
        private int photoCount = 0;
        private System.Windows.Forms.Timer? photoTimer;

        private SerialPort serialPort;

        private bool isFullscreen = false;
        private bool isInFullscreen = false;
        private bool isTogglingFullscreen = false;
        private FullscreenPreviewForm fullscreenForm;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
            pictureBox1.Click += pictureBox1_Click;

            radioBtnMic.CheckedChanged += RadioButtons_CheckedChanged;
            radioBtnClick.CheckedChanged += RadioButtons_CheckedChanged;

            // Stel standaard trigger in
            radioBtnClick.Checked = true;

            UpdateTriggerMode(); // zet juiste trigger op basis van radio buttons

            numMicThreshold.ValueChanged += numMicThreshold_ValueChanged;

            StartCamera();
            StartMicrophone();

            serialPort = new SerialPort("COM3", 9600); // vervang COM3 met juiste poort
            serialPort.Open();
            serialPort.DataReceived += SerialPort_DataReceived;
            
            numIntervalLed.ValueChanged += Interval_ValueChanged;
            numIntervalPic.ValueChanged += Interval_ValueChanged;
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
            if (radioBtnMic.Checked)
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
        }
        private void StopMicrophone()
        {
            if (waveIn != null)
            {
                waveIn.DataAvailable -= WaveIn_DataAvailable;
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                int maxVolume = 0;
                for (int i = 0; i < e.BytesRecorded; i += 2)
                {
                    if (i + 1 >= e.BytesRecorded) break; // voorkom index out of range

                    short sample = BitConverter.ToInt16(e.Buffer, i);

                    maxVolume = Math.Max(maxVolume, Math.Abs(sample));
                }

                int volumeLevel = (int)((float)maxVolume / short.MaxValue * 100);

                this.BeginInvoke(new Action(() =>
                {
                    progressBarMic.Value = Math.Min(progressBarMic.Maximum, volumeLevel);
                }));

                if (volumeLevel > numMicThreshold.Value && !isTriggered)
                {
                    isTriggered = true;

                    this.BeginInvoke(() =>
                    {
                        TakePicture();
                    });

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

        private async void TakePicture()
        {
            if (isTakingPictures) return;
            isTakingPictures = true;

            if (videoSource == null || !videoSource.IsRunning)
            {
                MessageBox.Show("Camera is niet actief.");
                isTakingPictures = false;
                return;
            }

            btnTakePictures.Enabled = false;
            capturedPhotos.Clear();
            currentPhotoIndex = 0;
            totalPhotosToTake = 3;

            SendCountdownToArduino();
        }
        private void CapturePhoto()
        {
            if (videoSource == null || pictureBox1.Image == null) return;

            Bitmap photo = new Bitmap(pictureBox1.Image);
            capturedPhotos.Add(photo);
            PlayClickSound();
        }

        private void SendCountdownToArduino()
        {
            if (serialPort?.IsOpen == true && currentPhotoIndex < totalPhotosToTake)
            {
                // interval in ms naar Arduino sturen (led interval)
                int ledIntervalMs = (int)(numIntervalLed.Value * 1000);
                string cmd = $"COUNTDOWN;{ledIntervalMs}\n";
                serialPort.Write(cmd);
            }
        }

        private Bitmap CreateStrip(List<Bitmap> photos)
        {
            int width = photos[0].Width;
            int height = photos[0].Height;
            int margin = 10;
            int textHeight = 40;

            Bitmap strip = new Bitmap(width + 2 * margin, height * 3 + margin * 4 + textHeight);
            using (Graphics g = Graphics.FromImage(strip))
            {
                g.Clear(Color.White);

                for (int i = 0; i < 3; i++)
                {
                    g.DrawImage(photos[i], margin, margin + i * (height + margin));
                    g.DrawRectangle(Pens.Black, margin, margin + i * (height + margin), width - 1, height - 1);
                }

                Font font = new Font("Arial", 12);
                string customText = txtOndertekst.Text;
                g.DrawString(customText, font, Brushes.Black, new PointF(margin, strip.Height - textHeight));
            }
            return strip;
        }

        private Bitmap CombineTwoStripsIntoPage(List<Bitmap> photos)
        {
            Bitmap strip1 = CreateStrip(photos);
            Bitmap strip2 = CreateStrip(photos);

            int marginBetween = 20;
            int pageWidth = strip1.Width * 2 + marginBetween;
            int pageHeight = strip1.Height;

            Bitmap page = new Bitmap(pageWidth, pageHeight);
            using (Graphics g = Graphics.FromImage(page))
            {
                g.Clear(Color.White);
                g.DrawImage(strip1, 0, 0);
                g.DrawImage(strip2, strip1.Width + marginBetween, 0);
            }

            return page;
        }

        private void SaveBitmapAsPdfBackup(Bitmap bitmap, string fileName)
        {
            string folderPath = Path.Combine(Application.StartupPath, "strippenbackup");

            // Maak de map aan als deze nog niet bestaat
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            using var document = new PdfDocument();
            var page = document.AddPage();

            page.Width = XUnit.FromPoint(bitmap.Width);
            page.Height = XUnit.FromPoint(bitmap.Height);

            using var gfx = XGraphics.FromPdfPage(page);

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            using var xImage = XImage.FromStream(ms);

            gfx.DrawImage(xImage, 0, 0, bitmap.Width, bitmap.Height);

            document.Save(filePath);
        }

        private void CombinePhotosIntoStripsAndSave()
        {
            if (capturedPhotos.Count < 3) return;

            Bitmap pageBitmap = CombineTwoStripsIntoPage(capturedPhotos);

            // Toon in PictureBox
            pictureBox1.Image = pageBitmap;

            // Opslaan als PDF (backup)
            string fileName = $"FotoStrippenBackup_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            SaveBitmapAsPdfBackup(pageBitmap, fileName);

            // Printen
            PrintImage(pageBitmap);

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

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine().Trim();
            Console.WriteLine("Ontvangen via COM: " + data); // debug

            if (data == "BUTTON")
            {
                this.BeginInvoke(() =>
                {
                    if (radioBtnClick.Checked) // alleen als klik-modus aanstaat
                    {
                        TakePicture(); // zelfde functie als digitale knop
                    }
                });
            }


            if (data == "READY")
            {
                this.BeginInvoke(new Action(async () =>
                {
                    CapturePhoto();
                    currentPhotoIndex++;

                    if (currentPhotoIndex < totalPhotosToTake)
                    {
                        int intervalLed = (int)(numIntervalLed.Value * 1000);
                        int intervalPic = (int)(numIntervalPic.Value * 1000);
                        await Task.Delay(intervalPic);
                        SendCountdownToArduino();
                    }
                    else
                    {
                        CombinePhotosIntoStripsAndSave();
                        isTakingPictures = false;
                    }
                }));
            }
        }


        private void Interval_ValueChanged(object sender, EventArgs e)
        {
            UpdateTotalIntervalLabel();
        }
        private void UpdateTotalIntervalLabel()
        {
            decimal ledInterval = numIntervalLed.Value;
            decimal pictureInterval = numIntervalPic.Value;

            // 3 keer led interval + picture interval (sec)
            decimal totalInterval = ledInterval * 3 + pictureInterval;

            lblTotalInt.Text = $"{totalInterval:F1} Sec";
        }
        private async void pictureBox1_Click(object sender, EventArgs e)
        {
            if (isTogglingFullscreen)
                return;

            isTogglingFullscreen = true;

            if (fullscreenForm == null || fullscreenForm.IsDisposed)
            {
                if (pictureBox1.Image == null)
                {
                    isTogglingFullscreen = false;
                    return;
                }

                fullscreenForm = new FullscreenPreviewForm(pictureBox1.Image);
                fullscreenForm.FormClosed += FullscreenForm_FormClosed;
                fullscreenForm.Show();
            }
            else
            {
                fullscreenForm.Close();
                fullscreenForm = null;
            }

            await Task.Delay(300);
            isTogglingFullscreen = false;
        }

        private void RadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTriggerMode();
        }

        private void UpdateTriggerMode()
        {
            if (radioBtnMic.Checked)
            {
                StartMicrophone();        // microfoon aanzetten
                btnTakePictures.Enabled = false;  // handmatige knop uitzetten
            }
            else
            {
                StopMicrophone();         // microfoon stoppen
                btnTakePictures.Enabled = true;   // handmatige knop aanzetten
            }
        }

        private void numMicThreshold_ValueChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Microfoon threshold aangepast naar: " + numMicThreshold.Value);
        }
        private void FullscreenForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            fullscreenForm = null;
        }
    }
}