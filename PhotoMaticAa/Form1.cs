using System.Drawing.Printing;
using System.Diagnostics;
using System.Media;
using System.IO.Ports;
using Microsoft.VisualBasic;
using Microsoft.Data.SqlClient;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Management;
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
        private string connectionString = "Data Source=localhost\\sqlexpress;Initial Catalog=PhotoMaticAa;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

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

        // Printer selection
        private List<string> installedPrinters = new List<string>();
        private string? currentPrinterName;
        // Printing state flag
        private bool isPrinting = false;

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

            // Populate device lists and wire selection events
            PopulateCameraList();
            PopulateMicrophoneList();

            cmbCamera.SelectedIndexChanged += CmbCamera_SelectedIndexChanged;
            cmbMicrophone.SelectedIndexChanged += CmbMicrophone_SelectedIndexChanged;

            // Start camera and microphone using selected devices
            StartCamera(cmbCamera.SelectedIndex >= 0 ? cmbCamera.SelectedIndex : 0);
            StartMicrophone(cmbMicrophone.SelectedIndex >= 0 ? cmbMicrophone.SelectedIndex : 0);

            // Onthoud originele grootte en positie van PictureBox
            pictureBox1OriginalLocation = pictureBox1.Location;
            pictureBox1OriginalSize = pictureBox1.Size;

            // Arduino verbinden via COM
            try
            {
                serialPort = new SerialPort("COM4", 9600);
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

            // Populate printers and wire selection
            PopulatePrinterList();
            cmbPrinterSelect.SelectedIndexChanged += CmbPrinterSelect_SelectedIndexChanged;

            InitializeFlashSettings();

            //Reset paper level 
            btnResetPaper.Click += btnResetPaper_Click;

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

        // Populate available camera devices into combo box
        private void PopulateCameraList()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                cmbCamera.Items.Clear();

                for (int i = 0; i < videoDevices.Count; i++)
                {
                    cmbCamera.Items.Add(videoDevices[i].Name);
                }

                if (cmbCamera.Items.Count > 0)
                    cmbCamera.SelectedIndex = 0;
                else
                {
                    cmbCamera.Items.Add("No camera");
                    cmbCamera.SelectedIndex = 0;
                    cmbCamera.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                LogSystem("Error populating camera list: " + ex.Message);
            }
        }

        // Populate available microphone devices into combo box
        private void PopulateMicrophoneList()
        {
            try
            {
                cmbMicrophone.Items.Clear();
                int deviceCount = NAudio.Wave.WaveIn.DeviceCount;
                for (int i = 0; i < deviceCount; i++)
                {
                    var caps = NAudio.Wave.WaveIn.GetCapabilities(i);
                    cmbMicrophone.Items.Add(caps.ProductName);
                }

                if (cmbMicrophone.Items.Count > 0)
                    cmbMicrophone.SelectedIndex = 0;
                else
                {
                    cmbMicrophone.Items.Add("No microphone");
                    cmbMicrophone.SelectedIndex = 0;
                    cmbMicrophone.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                LogSystem("Error populating microphone list: " + ex.Message);
            }
        }

        private void CmbCamera_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // switch camera while running
            int idx = cmbCamera.SelectedIndex;
            if (idx >= 0)
            {
                StartCamera(idx);
            }
        }

        private void CmbMicrophone_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int idx = cmbMicrophone.SelectedIndex;
            if (idx >= 0)
            {
                if (radioBtnMic.Checked)
                {
                    StopMicrophone();
                    StartMicrophone(idx);
                }
            }
        }

        // Start webcam (optionally select device index)
        private void StartCamera(int deviceIndex = 0)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Geen camera gevonden.");
                return;
            }

            // Clamp deviceIndex
            if (deviceIndex < 0) deviceIndex = 0;
            if (deviceIndex >= videoDevices.Count) deviceIndex = 0;

            try
            {
                // Stop previous source if running
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.NewFrame -= Video_NewFrame;
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                    videoSource = null;
                }

                videoSource = new VideoCaptureDevice(videoDevices[deviceIndex].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);
                videoSource.Start();
                LogSystem($"Camera gestart: {videoDevices[deviceIndex].Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij camera: " + ex.Message);
                LogSystem("Fout bij starten camera: " + ex.Message);
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
            // Clone the incoming frame to own a separate Bitmap instance
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

            // Ensure UI update and previous-image disposal happen on the UI thread to avoid GDI+ concurrency issues
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.BeginInvoke(new Action(() =>
                {
                    var old = pictureBox1.Image;
                    pictureBox1.Image = bitmap;
                    // Dispose previous image to free GDI+ resources (do NOT dispose 'bitmap' we've just assigned)
                    old?.Dispose();
                }));
            }
            else
            {
                var old = pictureBox1.Image;
                pictureBox1.Image = bitmap;
                old?.Dispose();
            }
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
            bool hasValidEmail = !string.IsNullOrWhiteSpace(email) && email.Contains(@"@");

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
                        else if (!input.Contains(@"@"))
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
            // Prevent starting a photo session if paper is empty
            if (numPaperLeft.Value <= 0)
            {
                LogSystem("Cannot start fotosessie: paper empty. Button blocked.");
                btnTakePictures.Enabled = false;

                try
                {
                    if (serialPort?.IsOpen == true)
                    {
                        serialPort.WriteLine("DONE;9999999"); // effectively lock the Arduino button
                        LogCSharp(">> DONE;9999999 (paper empty, Arduino button locked)");
                    }
                }
                catch (Exception ex)
                {
                    LogSystem("Error sending lock command to Arduino: " + ex.Message);
                }

                isTakingPictures = false;
                return;
            }

            if (isTakingPictures) return;
            isTakingPictures = true;

            if (videoSource == null || !videoSource.IsRunning)
            {
                MessageBox.Show("Camera is niet active.");
                isTakingPictures = false;
                return;
            }

            // Blokkeer knop direct bij start fotosessie (local + Arduino)
            btnTakePictures.Enabled = false;
            try
            {
                // Send a long lock to Arduino so the physical button is blocked for the whole session
                if (serialPort?.IsOpen == true)
                {
                    serialPort.WriteLine("DONE;9999999");
                    LogCSharp(">> DONE;9999999 sent to Arduino - knop geblokkeerd tijdens fotosessie");
                }
            }
            catch (Exception ex)
            {
                LogSystem("Error sending lock to Arduino: " + ex.Message);
            }

            // Immediately reserve/consume one sheet of paper when session starts
            try
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new Action(() => numPaperLeft.Value = Math.Max(0, numPaperLeft.Value - 1)));
                else
                    numPaperLeft.Value = Math.Max(0, numPaperLeft.Value - 1);

                LogSystem($"Paper reserved/consumed at session start. Remaining sheets: {numPaperLeft.Value}");
            }
            catch (Exception ex)
            {
                LogSystem("Error decrementing paper counter: " + ex.Message);
            }

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
                    if (!radioBtnClick.Checked)
                        return;

                    // If paper is empty, ignore button requests and ensure Arduino is locked
                    if (numPaperLeft.Value <= 0)
                    {
                        LogSystem("BUTTON pressed but paper empty — ignoring and locking Arduino button.");
                        btnTakePictures.Enabled = false;
                        try
                        {
                            if (serialPort?.IsOpen == true)
                            {
                                serialPort.WriteLine("DONE;9999999");
                                LogCSharp(">> DONE;9999999 (paper empty, Arduino button locked)");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogSystem("Error sending lock command to Arduino: " + ex.Message);
                        }

                        return;
                    }

                    // If we're already taking photos or printing, keep the button locked and ignore presses
                    if (isTakingPictures || isPrinting)
                    {
                        LogSystem("BUTTON pressed while session/printing active — ignored and Arduino kept locked.");
                        try
                        {
                            if (serialPort?.IsOpen == true)
                            {
                                serialPort.WriteLine("DONE;9999999");
                            }
                        }
                        catch { }
                        return;
                    }

                    // Otherwise start a new session
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

                        // Do not send DONE to Arduino here: printing is started in CombinePhotosIntoStripsAndSave
                        // PrintImageAsync will send DONE after the print job finishes and after the configured cooldown.
                        LogCSharp("Fotosessie voltooid - printopdracht gestart (Arduino wordt vrijgegeven na print + cooldown)");
                    }
                }));
            }

            // Alleen deze handler voor knop vrijgeven
            if (data == "LOG:BUTTON_ENABLED")
            {
                this.BeginInvoke(() =>
                {
                    // Only enable the UI button if we're not currently printing
                    if (!isPrinting)
                    {
                        btnTakePictures.Enabled = true;
                    }
                    else
                    {
                        LogSystem("Ignored BUTTON_ENABLED from Arduino because printing is in progress.");
                    }
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

        // Combineert 2 strips op een A4-pagina (fixed pixel size 794 x 1123)
        private Bitmap CombineTwoStripsIntoPage(List<Bitmap> photos)
        {
            Bitmap strip1 = CreateStrip(photos);
            Bitmap strip2 = CreateStrip(photos);

            // Use exact pixel dimensions requested by user
            int pageWidth = 794;
            int pageHeight = 1123;

            Bitmap page = new Bitmap(pageWidth, pageHeight);
            using (Graphics g = Graphics.FromImage(page))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                if (backgroundImage != null)
                {
                    float bgRatio = Math.Min((float)pageWidth / backgroundImage.Width, (float)pageHeight / backgroundImage.Height);
                    int bgWidth = (int)(backgroundImage.Width * bgRatio);
                    int bgHeight = (int)(backgroundImage.Height * bgRatio);
                    int bgX = (pageWidth - bgWidth) / 2;
                    int bgY = (pageHeight - bgHeight) / 2;
                    g.DrawImage(backgroundImage, bgX, bgY, bgWidth, bgHeight);
                }

                // Calculate scaling so two strips fit side by side with a margin
                int marginBetween = 20;

                float maxStripHeight = pageHeight * 0.8f;
                float availableHalfWidth = (pageWidth - marginBetween) / 2f;

                float scaleFactor = Math.Min(maxStripHeight / strip1.Height, availableHalfWidth / strip1.Width);

                int newStripWidth = Math.Max(1, (int)(strip1.Width * scaleFactor));
                int newStripHeight = Math.Max(1, (int)(strip1.Height * scaleFactor));

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

        // Save bitmap as PDF to an explicit path (temporary PDF for printing)
        private void SaveBitmapAsPdfToPath(Bitmap bitmap, string fullPath)
        {
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
            document.Save(fullPath);
        }

        // Maak finale pagina, toon preview, sla op en print
        private async void CombinePhotosIntoStripsAndSave()
        {
            // Note: paper consumption already handled when session started in TakePicture()
            if (capturedPhotos.Count < 3) return;

            Bitmap pageBitmap = CombineTwoStripsIntoPage(capturedPhotos);
            pictureBox1.Image = pageBitmap;

            string fileName = $"FotoStrippenBackup_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string folderPath = Path.Combine(Application.StartupPath, "strippenbackup");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);

            // Optional: keep a PDF backup of the strip
            try
            {
                SaveBitmapAsPdfBackup(pageBitmap, fileName);
            }
            catch (Exception ex)
            {
                LogSystem("Warning: failed to save PDF backup: " + ex.Message);
            }

            // Ensure UI button remains blocked until print finishes and cooldown handling is done inside PrintImageAsync
            btnTakePictures.Enabled = false;

            // Print the bitmap directly (no PDF round-trip) — this preserves the exact rendered output
            await PrintImageAsync(pageBitmap);

            // After PrintImageAsync returns, print finished and cooldown elapsed; session fully complete
            isTakingPictures = false;
        }

        // Print a PDF file using the system default print verb and monitor the Windows print queue
        private async Task PrintPdfAsync(string pdfPath, Image? fallbackImage = null)
        {
            isPrinting = true;
            if (this.InvokeRequired) this.BeginInvoke(new Action(() => btnTakePictures.Enabled = false)); else btnTakePictures.Enabled = false;

            string printerNameToUse = currentPrinterName ?? new PrinterSettings().PrinterName;

            // Try to start the default associated application with the "print" verb
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = pdfPath,
                    Verb = "print",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                try
                {
                    Process.Start(psi);
                    LogSystem($"Started print process for PDF: {pdfPath}");
                }
                catch (System.ComponentModel.Win32Exception winEx)
                {
                    // No application associated with .pdf that supports the print verb
                    LogSystem("Failed to start print process for PDF (no handler): " + winEx.Message);
                    if (fallbackImage != null)
                    {
                        LogSystem("Falling back to image-based printing.");
                        await PrintImageAsync(fallbackImage);
                        return; // PrintImageAsync will handle DONE/cooldown and isPrinting state
                    }
                    else
                    {
                        MessageBox.Show("Geen toepassing gevonden om PDF te printen. Installeer een PDF-reader of gebruik fallback printing.");
                        // release printing state and re-enable UI
                        isPrinting = false;
                        if (this.InvokeRequired) this.BeginInvoke(new Action(() => btnTakePictures.Enabled = true)); else btnTakePictures.Enabled = true;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LogSystem("Failed to start print process for PDF: " + ex.Message);
                    if (fallbackImage != null)
                    {
                        LogSystem("Falling back to image-based printing.");
                        await PrintImageAsync(fallbackImage);
                        return;
                    }
                }

                // Give the application a moment to hand the job to the spooler
                await Task.Delay(1500);
            }
            catch (Exception ex)
            {
                LogSystem("Error launching PDF print: " + ex.Message);
            }

            // Monitor the print queue for the job
            try
            {
                LogSystem("PDF afdruktaak gestart. Wachten op afdruktaak in de wachtrij...");

                await Task.Run(async () =>
                {
                    int pollInterval = 500;
                    int timeoutMs = 120000;
                    int elapsed = 0;
                    bool jobSeen = false;
                    bool loggedJobDetected = false;
                    string expectedDocumentName = Path.GetFileName(pdfPath);

                    while (true)
                    {
                        try
                        {
                            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PrintJob");
                            var jobs = searcher.Get();

                            bool jobExists = false;
                            foreach (ManagementObject job in jobs)
                            {
                                string name = (job["Name"] ?? string.Empty).ToString();
                                string document = (job["Document"] ?? string.Empty).ToString();
                                string owner = (job["Owner"] ?? string.Empty).ToString();

                                if (!string.IsNullOrEmpty(name) && name.StartsWith(printerNameToUse, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (string.Equals(document, expectedDocumentName, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(owner, Environment.UserName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        jobExists = true;
                                        jobSeen = true;
                                        break;
                                    }

                                    if (!jobSeen)
                                    {
                                        jobExists = true;
                                        jobSeen = true;
                                        break;
                                    }
                                }
                            }

                            if (jobSeen && !jobExists)
                            {
                                LogSystem("PDF afdruktaak voltooid.");
                                break;
                            }

                            if (!jobSeen && elapsed >= timeoutMs)
                            {
                                LogSystem("Time-out: geen afdruktaak gevonden binnen de wachttijd (PDF).");
                                break;
                            }

                            if (jobSeen && elapsed >= timeoutMs)
                            {
                                LogSystem("Time-out tijdens het wachten op afronding van de afdruktaak (PDF).");
                                break;
                            }

                            if (jobSeen && !loggedJobDetected)
                            {
                                LogSystem("Afdruktaak gevonden - wachten tot deze is voltooid...");
                                loggedJobDetected = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogSystem("Fout bij het controleren van de printwachtrij (PDF): " + ex.Message);
                            break;
                        }

                        await Task.Delay(pollInterval);
                        elapsed += pollInterval;
                    }
                });
            }
            catch { }
            finally
            {
                // After print job finished (or timed out), send DONE with cooldown to Arduino and wait cooldown before re-enabling
                try
                {
                    int cooldownMs = (int)(numKnopCooldown.Value * 1000);

                    if (serialPort?.IsOpen == true)
                    {
                        serialPort.WriteLine($"DONE;{cooldownMs}");
                        LogCSharp($">> DONE;{cooldownMs} (Arduino button state updated)");
                    }

                    if (cooldownMs > 0)
                    {
                        LogSystem($"Wachten {numKnopCooldown.Value} seconden voordat knoppen worden vrijgegeven...");
                        await Task.Delay(cooldownMs);
                    }
                }
                catch (Exception ex)
                {
                    LogSystem("Fout bij het versturen van DONE naar Arduino of wachten op cooldown (PDF): " + ex.Message);
                }

                // Re-enable button on UI thread
                LogSystem("Printmonitoring gestopt. Knop heringeven.");
                if (this.InvokeRequired) this.BeginInvoke(new Action(() => btnTakePictures.Enabled = true)); else btnTakePictures.Enabled = true;

                isPrinting = false;
            }
        }

        // Print an Image using PrintDocument and monitor the print queue (used as fallback)
        private async Task PrintImageAsync(Image imageToPrint)
        {
            isPrinting = true;
            if (this.InvokeRequired) this.BeginInvoke(new Action(() => btnTakePictures.Enabled = false)); else btnTakePictures.Enabled = false;

            // Save debug copy
            try
            {
                string debugFolder = Path.Combine(Application.StartupPath, "print_debug");
                if (!Directory.Exists(debugFolder)) Directory.CreateDirectory(debugFolder);
                string debugPath = Path.Combine(debugFolder, $"print_debug_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
                using (var bmp = new Bitmap(imageToPrint.Width, imageToPrint.Height))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imageToPrint, 0, 0, imageToPrint.Width, imageToPrint.Height);
                    bmp.Save(debugPath, System.Drawing.Imaging.ImageFormat.Png);
                }
                LogSystem($"Saved debug print image: {debugPath}");
            }
            catch (Exception ex)
            {
                LogSystem("Failed to save debug print image: " + ex.Message);
            }

            PrintDocument printDoc = new()
            {
                DocumentName = $"PhotoMatic_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}_{Environment.ProcessId}"
            };

            printDoc.PrintPage += (s, e) =>
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                Rectangle target = e.MarginBounds;
                if (target.Width <= 0 || target.Height <= 0) target = e.PageBounds;

                DrawCenteredImage(e.Graphics, imageToPrint, target);
                e.HasMorePages = false;
            };

            string printerNameToUse = currentPrinterName ?? new PrinterSettings().PrinterName;
            try { printDoc.PrinterSettings.PrinterName = printerNameToUse; } catch { printDoc.PrinterSettings.PrinterName = new PrinterSettings().PrinterName; }

            try
            {
                printDoc.Print();
            }
            catch (Exception ex)
            {
                LogSystem("Fout bij printen (image fallback): " + ex.Message);
                if (this.InvokeRequired) this.BeginInvoke(new Action(() => btnTakePictures.Enabled = true)); else btnTakePictures.Enabled = true;
                isTakingPictures = false;
                isPrinting = false;
                return;
            }

            // Monitor the print queue similar to PrintImageAsync earlier
            try
            {
                LogSystem("Image print opgestuurd naar spooler. Wachten op afdruktaak...");

                await Task.Run(async () =>
                {
                    int pollInterval = 500;
                    int timeoutMs = 120000;
                    int elapsed = 0;
                    bool jobSeen = false;
                    bool loggedJobDetected = false;
                    string expectedDocumentName = printDoc.DocumentName;

                    while (true)
                    {
                        try
                        {
                            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PrintJob");
                            var jobs = searcher.Get();

                            bool jobExists = false;
                            foreach (ManagementObject job in jobs)
                            {
                                string name = (job["Name"] ?? string.Empty).ToString();
                                string document = (job["Document"] ?? string.Empty).ToString();
                                string owner = (job["Owner"] ?? string.Empty).ToString();

                                if (!string.IsNullOrEmpty(name) && name.StartsWith(printerNameToUse, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (string.Equals(document, expectedDocumentName, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(owner, Environment.UserName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        jobExists = true;
                                        jobSeen = true;
                                        break;
                                    }

                                    if (!jobSeen)
                                    {
                                        jobExists = true;
                                        jobSeen = true;
                                        break;
                                    }
                                }
                            }

                            if (jobSeen && !jobExists)
                            {
                                LogSystem("Afdruktaak voltooid.");
                                break;
                            }

                            if (!jobSeen && elapsed >= timeoutMs)
                            {
                                LogSystem("Time-out: geen afdruktaak gevonden binnen de wachttijd.");
                                break;
                            }

                            if (jobSeen && elapsed >= timeoutMs)
                            {
                                LogSystem("Time-out tijdens het wachten op afronding van de afdruktaak.");
                                break;
                            }

                            if (jobSeen && !loggedJobDetected)
                            {
                                LogSystem("Afdruktaak is gestart - wachten tot deze is voltooid...");
                                loggedJobDetected = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogSystem("Fout bij het controleren van de printwachtrij: " + ex.Message);
                            break;
                        }

                        await Task.Delay(pollInterval);
                        elapsed += pollInterval;
                    }
                });
            }
            catch { }
            finally
            {
                try
                {
                    int cooldownMs = (int)(numKnopCooldown.Value * 1000);

                    if (serialPort?.IsOpen == true)
                    {
                        serialPort.WriteLine($"DONE;{cooldownMs}");
                        LogCSharp($">> DONE;{cooldownMs} (Arduino button state updated)");
                    }

                    if (cooldownMs > 0)
                    {
                        LogSystem($"Wachten {numKnopCooldown.Value} seconden voordat knoppen worden vrijgegeven...");
                        await Task.Delay(cooldownMs);
                    }
                }
                catch (Exception ex)
                {
                    LogSystem("Fout bij het versturen van DONE naar Arduino of wachten op cooldown: " + ex.Message);
                }

                LogSystem("Printmonitoring gestopt. Knop heringeven.");
                if (this.InvokeRequired) this.BeginInvoke(new Action(() => btnTakePictures.Enabled = true)); else btnTakePictures.Enabled = true;

                isPrinting = false;
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
                if (control != pnlVolumeTrackBackground) // Behalve de volume panel die we gaan verwijder
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
                // start selected microphone
                StartMicrophone(cmbMicrophone.SelectedIndex >= 0 ? cmbMicrophone.SelectedIndex : 0);
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
            int userId;

            // Create a new SQL connection
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Check if a user with this email already exists
                string checkUser = "SELECT UserID FROM Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(checkUser, conn))
                {
                    // Add the email parameter to prevent SQL injection
                    cmd.Parameters.AddWithValue("@Email", email);

                    // Execute the query and get the first column of the first row (UserID)
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // User already exists → retrieve the existing UserID
                        userId = Convert.ToInt32(result);
                    }
                    else
                    {
                        // User does not exist → insert a new one and return the new ID
                        string insertUser = "INSERT INTO Users (Email) OUTPUT INSERTED.UserID VALUES (@Email)";
                        using (SqlCommand insertCmd = new SqlCommand(insertUser, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@Email", email);

                            // Execute the insert query and get the newly created UserID
                            userId = (int)insertCmd.ExecuteScalar();
                        }
                    }
                }
            }

            // Return the found or newly created UserID
            return userId;
        }

        private void SavePhoto(int userId, string photoPath)
        {
            // Create a new SQL connection
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Insert a new photo record and link it to the user
                string insertPhoto = "INSERT INTO Photos (UserID, PhotoPath) VALUES (@UserID, @PhotoPath)";
                using (SqlCommand cmd = new SqlCommand(insertPhoto, conn))
                {
                    // Add parameters for UserID and the file path of the photo
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@PhotoPath", photoPath);

                    // Execute the insert query (no return value expected)
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // Reset papierniveau en heractiveer knoppen
        private void btnResetPaper_Click(object sender, EventArgs e)
        {
            numPaperLeft.Value = 40;
            LogSystem("Paper counter reset to 40 (new paper cartridge inserted)");

            btnTakePictures.Enabled = true;

            try
            {
                if (serialPort?.IsOpen == true)
                {
                    serialPort.WriteLine("DONE;0");
                    LogCSharp(">> DONE;0 (Arduino button re-enabled)");
                }
            }
            catch (Exception ex)
            {
                LogSystem("Error re-enabling Arduino button: " + ex.Message);
            }
        }

        private void PopulatePrinterList()
        {
            try
            {
                installedPrinters.Clear();
                cmbPrinterSelect.Items.Clear();
                foreach (string p in PrinterSettings.InstalledPrinters)
                {
                    installedPrinters.Add(p);
                    cmbPrinterSelect.Items.Add(p);
                }

                if (installedPrinters.Count == 0)
                {
                    currentPrinterName = null;
                    cmbPrinterSelect.Items.Add("No printers");
                    cmbPrinterSelect.SelectedIndex = 0;
                    cmbPrinterSelect.Enabled = false;
                    LogSystem("No printers found on the system.");
                }
                else
                {
                    cmbPrinterSelect.Enabled = true;
                    cmbPrinterSelect.SelectedIndex = 0;
                    currentPrinterName = installedPrinters[0];
                    LogSystem($"Default printer set to: {currentPrinterName}");
                }
            }
            catch (Exception ex)
            {
                LogSystem("Error populating printers: " + ex.Message);
            }
        }

        private void CmbPrinterSelect_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                int idx = cmbPrinterSelect.SelectedIndex;
                if (idx >= 0 && idx < installedPrinters.Count)
                {
                    currentPrinterName = installedPrinters[idx];
                    LogCSharp($"Selected printer: {currentPrinterName}");
                }
            }
            catch (Exception ex)
            {
                LogSystem("Error selecting printer: " + ex.Message);
            }
        }
    }
}
