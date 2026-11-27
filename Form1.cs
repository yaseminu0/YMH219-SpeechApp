
using System;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows.Forms;
using YMH219_SpeechApp.Models;
using YMH219_SpeechApp.Services;

namespace YMH219_SpeechApp
{
    public partial class Form1 : Form
    {
        private SpeechSynthesisService synthesisService;
        private SpeechRecognitionService recognitionService;

        private TextBox txtInput;
        private TextBox txtResult;

        private ComboBox cmbVoices;
        private ComboBox cmbLanguages;

        private Label lblVoiceStatus;
        private Label lblMicStatus;
        private Label lblWordCount;

        private Button btnSpeak;
        private Button btnPause;
        private Button btnStop;
        private Button btnStartListening;
        private Button btnStopListening;

        private bool isClosing = false;

        public Form1()
        {
            InitializeComponent();

            BuildInterface();
            InitializeServices();
        }

        // Initialize speech services
        private void InitializeServices()
        {
            try
            {
                synthesisService = new SpeechSynthesisService();

                synthesisService.SpeakCompleted += (s, e) =>
                {
                    if (!isClosing && IsHandleCreated)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            lblVoiceStatus.Text = "Durum: Hazır";
                        }));
                    }
                };

                LoadVoices();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ses sistemi başlatılamadı: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            try
            {
                recognitionService = new SpeechRecognitionService();

                recognitionService.SpeechRecognized +=
                    RecognitionService_SpeechRecognized;

                recognitionService.RecognitionCompleted +=
                    RecognitionService_RecognitionCompleted;

                LoadLanguages();
            }
            catch (Exception ex)
            {
                lblMicStatus.Text = "Durum: Mikrofon kullanılamıyor";

                MessageBox.Show(
                    "Konuşma tanıma sistemi başlatılamadı: " + ex.Message,
                    "Mikrofon Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // Build interface
        private void BuildInterface()
        {
            Text = "Konuşma Asistanı";
            Size = new Size(1180, 800);
            MinimumSize = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;

            BackColor = Color.FromArgb(243, 246, 250);
            Font = new Font("Segoe UI", 10);

            // Header
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 95,
                BackColor = Color.FromArgb(28, 43, 67)
            };

            Label title = new Label
            {
                Text = "KONUŞMA ASİSTANI",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 15)
            };

            Label subtitle = new Label
            {
                Text = "Metinden konuşmaya ve konuşmadan metne dönüştürme uygulaması",
                ForeColor = Color.FromArgb(195, 207, 224),
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(33, 57)
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            Controls.Add(header);

            // Main content
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(22),
                BackColor = BackColor
            };

            mainLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            mainLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            Controls.Add(mainLayout);

            // Text to Speech card
            Panel ttsCard = CreateCard();
            mainLayout.Controls.Add(ttsCard, 0, 0);

            AddCardTitle(ttsCard, "Metinden Konuşmaya",
                "Yazdığınız metni sesli olarak dinleyin.");

            AddLabel(ttsCard, "Metin", 25, 105);

            txtInput = new TextBox
            {
                Location = new Point(25, 132),
                Size = new Size(480, 205),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };

            txtInput.TextChanged += (s, e) =>
            {
                UpdateWordCount();
            };

            ttsCard.Controls.Add(txtInput);

            AddLabel(ttsCard, "Ses Seçimi", 25, 355);

            cmbVoices = CreateComboBox(25, 382);
            ttsCard.Controls.Add(cmbVoices);

            btnSpeak = CreateButton("Seslendir", 25, 445);
            btnPause = CreateButton("Duraklat / Devam", 165, 445);
            btnStop = CreateButton("Durdur", 335, 445);

            ttsCard.Controls.AddRange(new Control[]
            {
                btnSpeak,
                btnPause,
                btnStop
            });

            lblVoiceStatus = CreateStatusLabel(
                "Durum: Hazır", 25, 515, Color.DarkGreen);

            ttsCard.Controls.Add(lblVoiceStatus);

            // Speech to Text card
            Panel sttCard = CreateCard();
            mainLayout.Controls.Add(sttCard, 1, 0);

            AddCardTitle(sttCard, "Konuşmadan Metne",
                "Mikrofon aracılığıyla konuşmanızı metne dönüştürün.");

            AddLabel(sttCard, "Algılama Sonucu", 25, 105);

            txtResult = new TextBox
            {
                Location = new Point(25, 132),
                Size = new Size(480, 205),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };

            ttsCard.Controls.Add(txtInput);
            sttCard.Controls.Add(txtResult);

            AddLabel(sttCard, "Tanıma Dili", 25, 355);

            cmbLanguages = CreateComboBox(25, 382);
            sttCard.Controls.Add(cmbLanguages);

            btnStartListening =
                CreateButton("Dinlemeyi Başlat", 25, 445);

            btnStopListening =
                CreateButton("Dinlemeyi Durdur", 195, 445);

            Button btnClear = CreateButton("Temizle", 365, 445);

            sttCard.Controls.AddRange(new Control[]
            {
                btnStartListening,
                btnStopListening,
                btnClear
            });

            lblMicStatus = CreateStatusLabel(
                "Durum: Başlatılmadı", 25, 515, Color.DarkRed);

            sttCard.Controls.Add(lblMicStatus);

            lblWordCount = new Label
            {
                Text = "Kelime Sayısı: 0",
                Location = new Point(25, 555),
                AutoSize = true,
                ForeColor = Color.FromArgb(90, 105, 125),
                Font = new Font("Segoe UI", 9)
            };

            sttCard.Controls.Add(lblWordCount);

            // Save button
            Button btnSave = CreateButton("Metni Kaydet", 25, 595);
            btnSave.Size = new Size(150, 42);
            sttCard.Controls.Add(btnSave);

            // Events
            btnSpeak.Click += (s, e) => SpeakText();

            btnPause.Click += (s, e) => PauseResume();

            btnStop.Click += (s, e) =>
            {
                synthesisService?.Stop();
                lblVoiceStatus.Text = "Durum: Durduruldu";
            };

            btnStartListening.Click += (s, e) => StartListening();

            btnStopListening.Click += (s, e) => StopListening();

            btnClear.Click += (s, e) =>
            {
                txtResult.Clear();
            };

            btnSave.Click += (s, e) => SaveText();

            cmbLanguages.SelectedIndexChanged +=
                CmbLanguages_SelectedIndexChanged;

            cmbVoices.SelectedIndexChanged +=
                CmbVoices_SelectedIndexChanged;
        }

        // Create card
        private Panel CreateCard()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };
        }

        // Add card title
        private void AddCardTitle(
            Panel panel, string heading, string description)
        {
            Label title = new Label
            {
                Text = heading,
                Location = new Point(25, 22),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 43, 67)
            };

            Label subtitle = new Label
            {
                Text = description,
                Location = new Point(27, 62),
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            panel.Controls.Add(title);
            panel.Controls.Add(subtitle);
        }

        // Add label
        private void AddLabel(
            Panel panel, string text, int x, int y)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 60, 80)
            });
        }

        // Create combo box
        private ComboBox CreateComboBox(int x, int y)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(480, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Left |
                         AnchorStyles.Right
            };
        }

        // Create button
        private Button CreateButton(
            string text, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(145, 42),
                BackColor = Color.FromArgb(43, 105, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        // Create status label
        private Label CreateStatusLabel(
            string text, int x, int y, Color color)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = color,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
        }

        // Load installed voices
        private void LoadVoices()
        {
            var voices = synthesisService.GetAvailableVoices();

            cmbVoices.Items.Clear();

            foreach (var voice in voices)
            {
                cmbVoices.Items.Add(voice);
            }

            cmbVoices.DisplayMember = "Name";

            if (cmbVoices.Items.Count > 0)
            {
                cmbVoices.SelectedIndex = 0;
            }
        }

        // Load installed recognition languages
        private void LoadLanguages()
        {
            var languages = recognitionService.GetAvailableLanguages();

            cmbLanguages.DataSource = languages;
            cmbLanguages.DisplayMember = "DisplayName";

            if (languages.Count == 0)
            {
                lblMicStatus.Text =
                    "Durum: Tanıma dili bulunamadı";
            }
        }

        // Select voice
        private void CmbVoices_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            if (cmbVoices.SelectedItem is VoiceInfo voice)
            {
                try
                {
                    synthesisService?.SelectVoice(voice.Name);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ses seçilemedi: " + ex.Message,
                        "Hata");
                }
            }
        }

        // Select recognition language
        private void CmbLanguages_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            if (cmbLanguages.SelectedItem is SpeechLanguage language)
            {
                try
                {
                    if (recognitionService != null &&
                        !recognitionService.IsListening)
                    {
                        recognitionService.SelectLanguage(
                            language.CultureName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Dil değiştirilemedi: " + ex.Message,
                        "Hata");
                }
            }
        }

        // Text to Speech
        private void SpeakText()
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show(
                    "Lütfen önce bir metin girin.",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                synthesisService?.Speak(txtInput.Text);
                lblVoiceStatus.Text = "Durum: Konuşuyor";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Seslendirme hatası: " + ex.Message,
                    "Hata");
            }
        }

        // Pause and Resume
        private void PauseResume()
        {
            try
            {
                if (synthesisService == null)
                    return;

                if (synthesisService.GetState() ==
                    SynthesizerState.Speaking)
                {
                    synthesisService.Pause();
                    lblVoiceStatus.Text = "Durum: Duraklatıldı";
                }
                else if (synthesisService.GetState() ==
                         SynthesizerState.Paused)
                {
                    synthesisService.Resume();
                    lblVoiceStatus.Text = "Durum: Konuşuyor";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata");
            }
        }

        // Start listening
        private void StartListening()
        {
            try
            {
                if (recognitionService == null)
                {
                    MessageBox.Show(
                        "Konuşma tanıma sistemi kullanılamıyor.",
                        "Hata");
                    return;
                }

                recognitionService.StartListening();

                lblMicStatus.Text = "Durum: Dinleniyor";
                lblMicStatus.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                lblMicStatus.Text = "Durum: Başlatılamadı";

                MessageBox.Show(
                    "Mikrofon başlatılamadı: " + ex.Message,
                    "Mikrofon Hatası");
            }
        }

        // Stop listening
        private void StopListening()
        {
            try
            {
                recognitionService?.StopListening();

                lblMicStatus.Text = "Durum: Durduruluyor...";
                lblMicStatus.ForeColor = Color.DarkOrange;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Dinleme durdurulamadı: " + ex.Message,
                    "Hata");
            }
        }

        // Recognized speech event
        private void RecognitionService_SpeechRecognized(
            object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            if (isClosing || IsDisposed || !IsHandleCreated)
                return;

            BeginInvoke(new Action(() =>
            {
                txtResult.AppendText(
                    e.Result.Text + Environment.NewLine);

                UpdateWordCount();
            }));
        }

        // Recognition completed event
        private void RecognitionService_RecognitionCompleted(
            object sender,
            System.Speech.Recognition.RecognizeCompletedEventArgs e)
        {
            if (isClosing || IsDisposed || !IsHandleCreated)
                return;

            BeginInvoke(new Action(() =>
            {
                lblMicStatus.Text = "Durum: Hazır";
                lblMicStatus.ForeColor = Color.DarkRed;
            }));
        }

        // Update word counter
        private void UpdateWordCount()
        {
            string text = txtInput.Text.Trim();

            int count = string.IsNullOrWhiteSpace(text)
                ? 0
                : text.Split(
                    new[] { ' ', '\r', '\n', '\t' },
                    StringSplitOptions.RemoveEmptyEntries).Length;

            lblWordCount.Text = "Kelime Sayısı: " + count;
        }

        // Save recognized text
        private void SaveText()
        {
            if (string.IsNullOrWhiteSpace(txtResult.Text))
            {
                MessageBox.Show(
                    "Kaydedilecek metin bulunamadı.",
                    "Bilgi");
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Metin Dosyası (*.txt)|*.txt";
                dialog.Title = "Metni Kaydet";
                dialog.FileName = "KonusmaSonucu.txt";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(
                        dialog.FileName,
                        txtResult.Text,
                        System.Text.Encoding.UTF8);

                    MessageBox.Show(
                        "Metin başarıyla kaydedildi.",
                        "İşlem Tamamlandı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        // Close application
        protected override void OnFormClosing(
            FormClosingEventArgs e)
        {
            isClosing = true;

            try
            {
                synthesisService?.Dispose();
                recognitionService?.Dispose();
            }
            catch
            {
                // Resources are being released.
            }

            base.OnFormClosing(e);
        }
    }
}
