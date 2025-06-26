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

        private Bitmap backgroundImage;

        private Font selectedFont = new Font("Arial", 12);

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

            try
            {
                serialPort = new SerialPort("COM3", 9600);
                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verbinden met Arduino (COM-poort): " + ex.Message);
            }


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
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Geen camera gevonden. Controleer of je webcam is aangesloten.");
                return;
            }

            try
            {
                videoSource = new VideoCaptureDevice(videoDevices[1].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);
                videoSource.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij het starten van de camera: " + ex.Message);
            }
        }

        private void StartMicrophone(int deviceIndex = 0)
        {
            if (radioBtnMic.Checked)
            {
                try
                {
                    waveIn?.StopRecording();
                    waveIn?.Dispose();

                    waveIn = new WaveInEvent
                    {
                        DeviceNumber = deviceIndex,
                        WaveFormat = new WaveFormat(44100, 1)
                    };
                    waveIn.DataAvailable += WaveIn_DataAvailable;
                    waveIn.StartRecording();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Microfoon kon niet worden gestart: " + ex.Message);
                }
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

                this.BeginInvoke(() =>
                {
                    progressBarMic.Value = Math.Min(progressBarMic.Maximum, volumeLevel);

                    if (fullscreenForm != null && !fullscreenForm.IsDisposed)
                    {
                        fullscreenForm.UpdateVolumeBar(volumeLevel);
                    }
                });

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

            string customText = txtOndertekst.Text;
            Font font = selectedFont;
            int textHeight;

            using (Graphics g = Graphics.FromImage(new Bitmap(1, 1)))
            {
                SizeF size = g.MeasureString(customText, font);
                textHeight = (int)Math.Ceiling(size.Height);
            }

            Bitmap strip = new Bitmap(width, height * 3 + margin * 2 + textHeight);
            using (Graphics g = Graphics.FromImage(strip))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent); // Geen wit!

                for (int i = 0; i < 3; i++)
                {
                    g.DrawImage(photos[i], 0, i * (height + margin));
                }

                g.DrawString(customText, font, Brushes.Black, new PointF(0, strip.Height - textHeight));
            }

            return strip;
        }


        private Bitmap CombineTwoStripsIntoPage(List<Bitmap> photos)
        {
            Bitmap strip1 = CreateStrip(photos);
            Bitmap strip2 = CreateStrip(photos);

            // A4 formaat in pixels bij 96 DPI
            int pageWidth = 794;
            int pageHeight = 1123;

            Bitmap page = new Bitmap(pageWidth, pageHeight);
            using (Graphics g = Graphics.FromImage(page))
            {
                g.Clear(Color.White);

                // Achtergrond tekenen (proportioneel centreren en schalen)
                if (backgroundImage != null)
                {
                    float bgRatio = Math.Min((float)pageWidth / backgroundImage.Width, (float)pageHeight / backgroundImage.Height);
                    int bgWidth = (int)(backgroundImage.Width * bgRatio);
                    int bgHeight = (int)(backgroundImage.Height * bgRatio);
                    int bgX = (pageWidth - bgWidth) / 2;
                    int bgY = (pageHeight - bgHeight) / 2;

                    g.DrawImage(backgroundImage, bgX, bgY, bgWidth, bgHeight);
                }

                // Strips schalen naar 80% van de hoogte
                float maxStripHeight = pageHeight * 0.8f;
                float scaleFactor = Math.Min(maxStripHeight / strip1.Height, (pageWidth * 0.45f) / strip1.Width); // max 45% breedte per strip
                int newStripWidth = (int)(strip1.Width * scaleFactor);
                int newStripHeight = (int)(strip1.Height * scaleFactor);

                int marginBetween = 20;
                int totalWidth = newStripWidth * 2 + marginBetween;
                int startX = (pageWidth - totalWidth) / 2;
                int startY = (pageHeight - newStripHeight) / 2;

                g.DrawImage(strip1, startX, startY, newStripWidth, newStripHeight);
                g.DrawImage(strip2, startX + newStripWidth + marginBetween, startY, newStripWidth, newStripHeight);
            }

            return page;
        }

        private void DrawCenteredImage(Graphics g, Image image, Rectangle bounds)
        {

            double ratioX = (double)bounds.Width / image.Width;
            double ratioY = (double)bounds.Height / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            int posX = bounds.X + (bounds.Width - newWidth) / 2;
            int posY = bounds.Y + (bounds.Height - newHeight) / 2;




            g.DrawImage(image, posX, posY, newWidth, newHeight);
        }


        private void SaveBitmapAsPdfBackup(Bitmap bitmap, string fileName)
        {
            string folderPath = Path.Combine(Application.StartupPath, "strippenbackup");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);

            using var document = new PdfDocument();
            var page = document.AddPage();

            // Stel paginaformaat in (A4 bijv.)
            page.Size = PdfSharp.PageSize.A4;
            page.Orientation = PdfSharp.PageOrientation.Portrait;

            using var gfx = XGraphics.FromPdfPage(page);

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            using var xImage = XImage.FromStream(ms);

            double ratioX = page.Width.Point / bitmap.Width;
            double ratioY = page.Height.Point / bitmap.Height;
            double ratio = Math.Min(ratioX, ratioY);

            double width = bitmap.Width * ratio;
            double height = bitmap.Height * ratio;

            double x = (page.Width.Point - width) / 2;
            double y = (page.Height.Point - height) / 2;

            gfx.DrawImage(xImage, x, y, width, height);
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
                DrawCenteredImage(e.Graphics, imageToPrint, m);
                e.HasMorePages = false;
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

            if (fullscreenForm != null && !fullscreenForm.IsDisposed)
            {
                fullscreenForm.SetThreshold((int)numMicThreshold.Value);
            }
        }
        private void FullscreenForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            fullscreenForm = null;
        }

        private void btnSelectBackground_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Afbeeldingen|*.jpg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                backgroundImage = new Bitmap(openFileDialog.FileName);
                MessageBox.Show("Achtergrond geladen!");
            }
        }

        private void btnSelectFont_Click(object sender, EventArgs e)
        {
            fontDialog1.Font = selectedFont;

            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedFont = fontDialog1.Font;
                MessageBox.Show($"Lettertype ingesteld op: {selectedFont.Name}, grootte: {selectedFont.Size}");
            }
        }

    }
}