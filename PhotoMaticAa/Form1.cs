using System;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Drawing;
using Microsoft.Data.SqlClient;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PhotoMaticAa
{
    public partial class Form1 : Form
    {
        // Webcam gerelateerd
        private FilterInfoCollection? videoDevices;
        private VideoCaptureDevice? videoSource;

        // Fotologica
        private int currentPhotoIndex = 0;
        private int totalPhotosToTake = 3;
        private bool isTakingPictures = false;

        // Microfoon
        private WaveInEvent waveIn;
        private bool isTriggered = false;
        private int triggerThreshold => (int)numMicThreshold.Value;
        private int cooldownTime = 5000;

        // Foto-opslag
        private List<Bitmap> capturedPhotos = new();
        private int photoCount = 0;
        private System.Windows.Forms.Timer? photoTimer;

        // Arduino
        private SerialPort serialPort;

        // Database
        private string Email;
        private string connectionString = "Server=YOUR_SERVER_NAME;Database=PhotoMaticAa;Trusted_Connection=True;";

        // Fullscreen gedrag
        private System.Drawing.Point pictureBox1OriginalLocation;
        private System.Drawing.Size pictureBox1OriginalSize;
        private bool isFullscreen = false;
        private bool isTogglingFullscreen = false;

        // Achtergrondafbeelding
        private Bitmap backgroundImage;

        // Lettertype voor ondertekst
        private Font selectedFont = new Font("Arial", 12);

        // Panels voor volume-indicatie
        private Panel pnlVolumeTrackBackground;
        private Panel pnlVolumeTrackLevel;
        private Panel pnlVolumeThresholdLine;

        //panel voor camera flits
        private Panel flashPanel;


        private int flashDuration = 150; // milliseconden

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Afsluitgedrag en click event instellen
            this.FormClosing += Form1_FormClosing;
            pictureBox1.Click += pictureBox1_Click;

            // Trigger methode wisselen (mic/klik)
            radioBtnMic.CheckedChanged += RadioButtons_CheckedChanged;
            radioBtnClick.CheckedChanged += RadioButtons_CheckedChanged;
            radioBtnClick.Checked = true;
            UpdateTriggerMode();

            // Drempelwijziging
            numMicThreshold.ValueChanged += numMicThreshold_ValueChanged;

            // Start camera en microfoon
            StartCamera();
            StartMicrophone();

            // Onthoud originele grootte en positie van PictureBox
            pictureBox1OriginalLocation = pictureBox1.Location;
            pictureBox1OriginalSize = pictureBox1.Size;

            // Arduino verbinden via COM
            try
            {
                serialPort = new SerialPort("COM3", 9600);
                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;
                LogSystem("Arduino verbonden via COM3");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verbinden met Arduino (COM-poort): " + ex.Message);
                LogSystem($"Arduino verbinding mislukt: {ex.Message}");
            }

            // Eventhandlers voor intervalinstellingen
            numIntervalLed.ValueChanged += Interval_ValueChanged;
            numIntervalPic.ValueChanged += Interval_ValueChanged;

            InitializeFlashSettings();

            // Initialiseer cooldown control
            InitializeKnopCooldown();
        }

        private void InitializeKnopCooldown()
        {
            numKnopCooldown.Minimum = 0;
            numKnopCooldown.Maximum = 30;
            numKnopCooldown.Value = 5;
            numKnopCooldown.DecimalPlaces = 0;
            numKnopCooldown.Increment = 1;
            numKnopCooldown.ValueChanged += NumKnopCooldown_ValueChanged;
        }
        private void InitializeFlashSettings()
        {
            numFlashTime.Minimum = 50;
            numFlashTime.Maximum = 500;
            numFlashTime.Value = 150;
            numFlashTime.Increment = 25;
            numFlashTime.ValueChanged += (s, e) => flashDuration = (int)numFlashTime.Value;
        }

        private void LogMessage(string message, Color color)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => LogMessage(message, color)));
                return;
            }

            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText($"[{DateTime.Now:T}] {message}\n");
            rtbLog.SelectionColor = rtbLog.ForeColor;
            rtbLog.ScrollToCaret();
        }

        private void LogArduino(string message)
        {
            LogMessage($"[Arduino] {message}", Color.Blue);
        }

        private void LogCSharp(string message)
        {
            LogMessage($"[C#] {message}", Color.Red);
        }

        private void LogSystem(string message)
        {
            LogMessage($"[System] {message}", Color.Green);
        }

        // Speelt camerasluiter-geluid af
        private void PlayClickSound()
        {
            string path = Path.Combine(Application.StartupPath, "Sounds", "camera.wav");
            if (File.Exists(path)) new SoundPlayer(path).Play();
            else MessageBox.Show("Het geluid bestand is niet gevonden.");
        }

        // Start webcam
        private void StartCamera()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Geen camera gevonden.");
                return;
            }

            try
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);
                videoSource.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij camera: " + ex.Message);
            }
        }

        // Start microfooninput (volume meten)
        private void StartMicrophone(int deviceIndex = 0)
        {
            if (!radioBtnMic.Checked) return;

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
                MessageBox.Show("Microfoon kon niet starten: " + ex.Message);
            }
        }

        // Stop microfoon
        private void StopMicrophone()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveIn = null;
        }

        // Mic-volume live uitlezen
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                int maxVolume = 0;
                for (int i = 0; i < e.BytesRecorded; i += 2)
                {
                    if (i + 1 >= e.BytesRecorded) break;
                    short sample = BitConverter.ToInt16(e.Buffer, i);
                    maxVolume = Math.Max(maxVolume, Math.Abs(sample));
                }

                int volumeLevel = (int)((float)maxVolume / short.MaxValue * 100);

                // Update progressbar op UI thread
                this.BeginInvoke(() =>
                {
                    progressBarMic.Value = Math.Min(progressBarMic.Maximum, volumeLevel);
                });

                // Volume UI in fullscreen updaten
                if (isFullscreen && pnlVolumeTrackLevel != null && pnlVolumeTrackBackground != null)
                {
                    this.BeginInvoke(() =>
                    {
                        int maxHeight = pnlVolumeTrackBackground.Height;
                        int barHeight = (int)(maxHeight * (volumeLevel / 100.0));
                        pnlVolumeTrackLevel.Height = Math.Max(barHeight, 1);
                        pnlVolumeTrackLevel.Top = maxHeight - pnlVolumeTrackLevel.Height;
                    });
                }

                // Trigger als volume boven drempel is
                if (volumeLevel > triggerThreshold && !isTriggered)
                {
                    isTriggered = true;
                    this.BeginInvoke(() => TakePicture());

                    Task.Run(async () =>
                    {
                        await Task.Delay(cooldownTime);
                        isTriggered = false;
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mic detectie fout: " + ex.Message);
            }
        }

        // Camera levert nieuw beeld: zet in PictureBox
        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bitmap;
        }

        // Proper afsluiten van camera
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.NewFrame -= Video_NewFrame;
                videoSource.WaitForStop();
                videoSource = null;
            }

            Application.Exit();
        }

        // Handmatige fotoknop
        private void btnTakePictures_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            bool hasValidEmail = !string.IsNullOrWhiteSpace(email) && email.Contains("@");

            if (!hasValidEmail)
            {
                var result = MessageBox.Show(
                    "No valid e-mail found. Do you want to provide an e-mail address now?\n\n" +
                    "Yes = enter e-mail\nNo = continue without e-mail",
                    "E-mail?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Allow user a few attempts to enter a valid email; user can cancel by leaving input empty and choosing Cancel/Yes when asked.
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        string input = Interaction.InputBox(
                            "Enter e-mail address (will be saved with the photos):",
                            "Enter E-mail",
                            email);

                        if (string.IsNullOrWhiteSpace(input))
                        {
                            var cancelChoice = MessageBox.Show(
                                "No e-mail entered. Do you want to cancel taking pictures?",
                                "No E-mail",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (cancelChoice == DialogResult.Yes)
                                return; // abort
                                        // else loop again to allow entering email
                        }
                        else if (!input.Contains("@"))
                        {
                            MessageBox.Show("Please enter a valid e-mail address containing '@'.", "Invalid e-mail", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            // loop for another attempt
                        }
                        else
                        {
                            txtEmail.Text = input.Trim();
                            break;
                        }
                    }
                    // if after attempts still no valid email, we continue without email
                }
                // If user chose No, continue without email
            }

            TakePicture();
        }

        // Start fotoreeks
        private void TakePicture()
        {
            if (isTakingPictures) return;
            isTakingPictures = true;

            if (videoSource == null || !videoSource.IsRunning)
            {
                MessageBox.Show("Camera is niet actief.");
                isTakingPictures = false;
                return;
            }

            // Blokkeer knop direct bij start fotosessie
            if (serialPort?.IsOpen == true)
            {
                serialPort.WriteLine("BLOCK_BUTTON");
                LogCSharp("Knop geblokkeerd - fotosessie gestart");
            }

            btnTakePictures.Enabled = false;
            capturedPhotos.Clear();
            currentPhotoIndex = 0;
            totalPhotosToTake = 3;
            LogCSharp($"Fotosessie gestart - {totalPhotosToTake} foto's");
            SendCountdownToArduino();
        }

        // Neemt huidige frame als foto
        private async void CapturePhoto()
        {
            if (videoSource == null || pictureBox1.Image == null) return;

            // Toon flits
            if (isFullscreen)
                ShowFlashFullscreen();
            else
                LogCSharp($"Niet fullscreen - Geen camera flits gestart");

            Bitmap photo = new Bitmap(pictureBox1.Image);
            capturedPhotos.Add(photo);
            PlayClickSound();
        }

        // Start countdown via Arduino
        private void SendCountdownToArduino()
        {
            try
            {
                if (serialPort?.IsOpen == true && currentPhotoIndex < totalPhotosToTake)
                {
                    int ledIntervalMs = (int)(numIntervalLed.Value * 1000);
                    string cmd = $"COUNTDOWN;{ledIntervalMs}";
                    serialPort.WriteLine(cmd);
                    Debug.WriteLine($"[C#] Sent: {cmd}");
                    LogCSharp($"Countdown gestart voor foto {currentPhotoIndex + 1}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[C#] Error sending command: {ex.Message}");
                LogSystem($"FOUT: {ex.Message}");
            }
        }

        // Ontvangt triggers van Arduino
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine().Trim();

            Debug.WriteLine($"[Arduino] {data}");

            // Alle Arduino berichten in blauw
            this.BeginInvoke(() => LogArduino(data));

            if (data == "BUTTON")
            {
                this.BeginInvoke(() =>
                {
                    if (radioBtnClick.Checked)
                        TakePicture();
                });
            }

            if (data == "READY")
            {
                this.BeginInvoke(new Action(async () =>
                {
                    CapturePhoto();

                    if (serialPort?.IsOpen == true)
                    {
                        serialPort.WriteLine("SNAP");
                    }

                    currentPhotoIndex++;

                    if (currentPhotoIndex < totalPhotosToTake)
                    {
                        int intervalPic = (int)(numIntervalPic.Value * 1000);
                        await Task.Delay(intervalPic);
                        SendCountdownToArduino();
                    }
                    else
                    {
                        CombinePhotosIntoStripsAndSave();
                        isTakingPictures = false;

                        if (serialPort?.IsOpen == true)
                        {
                            int cooldownMs = (int)(numKnopCooldown.Value * 1000);
                            serialPort.WriteLine($"DONE;{cooldownMs}");

                            if (cooldownMs > 0)
                            {
                                LogCSharp($"Fotosessie voltooid - cooldown: {numKnopCooldown.Value} seconden");
                            }
                            else
                            {
                                LogCSharp("Fotosessie voltooid - knop direct beschikbaar");
                            }
                        }
                    }
                }));
            }

            // Alleen deze handler voor knop vrijgeven
            if (data == "LOG:BUTTON_ENABLED")
            {
                this.BeginInvoke(() =>
                {
                    btnTakePictures.Enabled = true;
                });
            }
        }

        private void NumKnopCooldown_ValueChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                LogCSharp($"Cooldown tijd ingesteld op: {numKnopCooldown.Value} seconden");
            }
        }
        // Maakt strip (3 foto's onder elkaar + tekst)
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
                SizeF size = g.MeasureString(customText, font, width);
                textHeight = (int)Math.Ceiling(size.Height);
            }

            Bitmap strip = new Bitmap(width, height * 3 + margin * 2 + textHeight);
            using (Graphics g = Graphics.FromImage(strip))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent);

                for (int i = 0; i < 3; i++)
                {
                    g.DrawImage(photos[i], 0, i * (height + margin));
                }

                RectangleF textRect = new RectangleF(0, strip.Height - textHeight, strip.Width, textHeight);
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    Trimming = StringTrimming.Word
                };

                g.DrawString(customText, font, Brushes.Black, textRect, format);
            }

            return strip;
        }

        // Combineert 2 strips op een A4-pagina
        private Bitmap CombineTwoStripsIntoPage(List<Bitmap> photos)
        {
            Bitmap strip1 = CreateStrip(photos);
            Bitmap strip2 = CreateStrip(photos);

            int pageWidth = 794;
            int pageHeight = 1123;

            Bitmap page = new Bitmap(pageWidth, pageHeight);
            using (Graphics g = Graphics.FromImage(page))
            {
                g.Clear(Color.White);

                if (backgroundImage != null)
                {
                    float bgRatio = Math.Min((float)pageWidth / backgroundImage.Width, (float)pageHeight / backgroundImage.Height);
                    int bgWidth = (int)(backgroundImage.Width * bgRatio);
                    int bgHeight = (int)(backgroundImage.Height * bgRatio);
                    int bgX = (pageWidth - bgWidth) / 2;
                    int bgY = (pageHeight - bgHeight) / 2;
                    g.DrawImage(backgroundImage, bgX, bgY, bgWidth, bgHeight);
                }

                float maxStripHeight = pageHeight * 0.8f;
                float scaleFactor = Math.Min(maxStripHeight / strip1.Height, (pageWidth * 0.45f) / strip1.Width);
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

        // Middelt en schaalt een afbeelding netjes in het opgegeven kader
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

        // Sla bitmap op als PDF in backupmap
        private void SaveBitmapAsPdfBackup(Bitmap bitmap, string fileName)
        {
            string folderPath = Path.Combine(Application.StartupPath, "strippenbackup");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);
            using var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

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

        // Maak finale pagina, toon preview, sla op en print
        private void CombinePhotosIntoStripsAndSave()
        {
            if (capturedPhotos.Count < 3) return;

            Bitmap pageBitmap = CombineTwoStripsIntoPage(capturedPhotos);
            pictureBox1.Image = pageBitmap;

            string fileName = $"FotoStrippenBackup_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string folderPath = Path.Combine(Application.StartupPath, "strippenbackup");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);

            SaveBitmapAsPdfBackup(pageBitmap, fileName);

            // 🔹 Save to database
            string email = txtEmail.Text.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    int userId = GetOrCreateUserId(email);
                    SavePhoto(userId, filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fout bij opslaan naar database: " + ex.Message);
                }
            }

            PrintImage(pageBitmap);
            btnTakePictures.Enabled = true;
        }

        // Start printproces
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

        // UI updaten bij intervalwijzigingen
        private void Interval_ValueChanged(object sender, EventArgs e) => UpdateTotalIntervalLabel();

        private void UpdateTotalIntervalLabel()
        {
            decimal totalInterval = numIntervalLed.Value * 3 + numIntervalPic.Value;
            lblTotalInt.Text = $"{totalInterval:F1} Sec";
        }

        // Klik op camera-preview voor fullscreen modus
        private async void pictureBox1_Click(object sender, EventArgs e)
        {
            if (isTogglingFullscreen) return;

            isTogglingFullscreen = true;

            if (!isFullscreen) EnterFullscreen();
            else ExitFullscreen();

            await Task.Delay(300);
            isTogglingFullscreen = false;
        }

        // Zet fullscreen aan
        private void EnterFullscreen()
        {
            isFullscreen = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            // Verberg alle andere controls behalve pictureBox1
            foreach (Control control in this.Controls)
            {
                if (control != pictureBox1)
                {
                    control.Visible = false;
                }
            }

            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Size = this.ClientSize;

            int margin = 20;
            int barWidth = 30;
            int barHeight = (int)(this.ClientSize.Height * 0.8);
            int barTop = (this.ClientSize.Height - barHeight) / 2;
            int barLeft = this.ClientSize.Width - barWidth - margin;

            pnlVolumeTrackBackground = new Panel
            {
                Width = barWidth,
                Height = barHeight,
                Left = barLeft,
                Top = barTop,
                BackColor = Color.DarkGray
            };
            this.Controls.Add(pnlVolumeTrackBackground);
            pnlVolumeTrackBackground.BringToFront();

            pnlVolumeTrackLevel = new Panel
            {
                Width = barWidth,
                Height = 0,
                Left = 0,
                Top = barHeight,
                BackColor = Color.LimeGreen,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            pnlVolumeTrackBackground.Controls.Add(pnlVolumeTrackLevel);

            pnlVolumeThresholdLine = new Panel
            {
                Width = barWidth,
                Height = 2,
                Left = 0,
                BackColor = Color.Red,
                Top = GetThresholdTopPosition()
            };
            pnlVolumeTrackBackground.Controls.Add(pnlVolumeThresholdLine);
        }

        // Zet fullscreen uit
        private void ExitFullscreen()
        {
            isFullscreen = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.WindowState = FormWindowState.Normal;

            pictureBox1.Location = pictureBox1OriginalLocation;
            pictureBox1.Size = pictureBox1OriginalSize;

            // Toon alle controls weer
            foreach (Control control in this.Controls)
            {
                if (control != pnlVolumeTrackBackground) // Behalve de volume panel die we gaan verwijderen
                {
                    control.Visible = true;
                }
            }

            if (pnlVolumeTrackBackground != null)
            {
                this.Controls.Remove(pnlVolumeTrackBackground);
                pnlVolumeTrackBackground.Dispose();
                pnlVolumeTrackBackground = null;
                pnlVolumeTrackLevel = null;
                pnlVolumeThresholdLine = null;
            }
        }

        private async void ShowFlashFullscreen()
        {
            // Maak flash panel dat het hele scherm vult
            flashPanel = new Panel
            {
                BackColor = Color.White,
                Location = new Point(0, 0),
                Size = this.ClientSize,
                Visible = false
            };
            this.Controls.Add(flashPanel);
            flashPanel.BringToFront();
            flashPanel.Visible = true;
            await Task.Delay(flashDuration); // Gebruik instelbare duur
            flashPanel.Visible = false;

            this.Controls.Remove(flashPanel);
            flashPanel.Dispose();
            flashPanel = null;
        }

        // Wisselen tussen mic/klik-modus
        private void RadioButtons_CheckedChanged(object sender, EventArgs e) => UpdateTriggerMode();

        private void UpdateTriggerMode()
        {
            if (radioBtnMic.Checked)
            {
                StartMicrophone();
                btnTakePictures.Enabled = false;
            }
            else
            {
                StopMicrophone();
                btnTakePictures.Enabled = true;
            }
        }

        // Verplaats thresholdlijn als waarde wijzigt
        private void numMicThreshold_ValueChanged(object sender, EventArgs e)
        {
            if (isFullscreen && pnlVolumeThresholdLine != null)
                pnlVolumeThresholdLine.Top = GetThresholdTopPosition();
        }

        // Selecteer achtergrond
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

        // Kies font
        private void btnSelectFont_Click(object sender, EventArgs e)
        {
            fontDialog1.Font = selectedFont;
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedFont = fontDialog1.Font;
                MessageBox.Show($"Lettertype ingesteld op: {selectedFont.Name}, grootte: {selectedFont.Size}");
            }
        }

        // Bereken Y-positie van threshold lijn
        private int GetThresholdTopPosition()
        {
            int maxHeight = pnlVolumeTrackBackground.Height;
            double ratio = (double)numMicThreshold.Value / 100.0;
            return (int)(maxHeight * (1 - ratio));
        }

        private void numFlashTime_ValueChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                LogCSharp($"Flash tijd ingesteld op: {numFlashTime.Value} MilliSeconden");
            }
        }
        
        private int GetOrCreateUserId(string email)
        {
            int userId = -1;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Check if user exists
                string checkUser = "SELECT UserID FROM Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(checkUser, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        userId = Convert.ToInt32(result);
                    }
                    else
                    {
                        // Insert new user
                        string insertUser = "INSERT INTO Users (Email) OUTPUT INSERTED.UserID VALUES (@Email)";
                        using (SqlCommand insertCmd = new SqlCommand(insertUser, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@Email", email);
                            userId = (int)insertCmd.ExecuteScalar();
                        }
                    }
                }
            }

            return userId;
        }

        private void SavePhoto(int userId, string photoPath)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string insertPhoto = "INSERT INTO Photos (UserID, PhotoPath) VALUES (@UserID, @PhotoPath)";
                using (SqlCommand cmd = new SqlCommand(insertPhoto, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@PhotoPath", photoPath);
                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
