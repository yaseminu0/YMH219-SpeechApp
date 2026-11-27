
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using YMH219_SpeechApp.Models;
using YMH219_SpeechApp.Services;

namespace YMH219_SpeechApp
{
    public class MainForm : XtraForm
    {
        // ==========================================
        // FONT
        // ==========================================

        private readonly string uiFont = GetUiFontFamily();

        private static string GetUiFontFamily()
        {
            return FontFamily.Families.Any(
                f => f.Name == "Segoe UI Variable")
                ? "Segoe UI Variable"
                : "Segoe UI";
        }

        // ==========================================
        // COLORS
        // ==========================================

        private readonly Color backgroundColor =
            Color.FromArgb(12, 19, 33);

        private readonly Color sidebarColor =
            Color.FromArgb(17, 29, 48);

        private readonly Color cardColor =
            Color.FromArgb(27, 43, 66);

        private readonly Color inputColor =
            Color.FromArgb(35, 54, 79);

        private readonly Color primaryColor =
            Color.FromArgb(139, 124, 255);

        private readonly Color secondaryColor =
            Color.FromArgb(66, 199, 216);

        private readonly Color textColor =
            Color.FromArgb(245, 247, 252);

        private readonly Color mutedColor =
            Color.FromArgb(166, 181, 204);

        private readonly Color successColor =
            Color.FromArgb(57, 201, 138);

        private readonly Color dangerColor =
            Color.FromArgb(224, 93, 117);

        private readonly Color borderColor =
            Color.FromArgb(55, 75, 105);

        // ==========================================
        // MAIN CONTROLS
        // ==========================================

        private PanelControl sidebar;
        private PanelControl mainPanel;
        private PanelControl header;
        private PanelControl content;

        private Label lblPageTitle;
        private Label lblPageDescription;
        private Label lblHeaderStatus;

        // ==========================================
        // PAGES
        // ==========================================

        private PanelControl dashboardPage;
        private PanelControl ttsPage;
        private PanelControl sttPage;
        private PanelControl historyPage;
        private PanelControl settingsPage;

        // ==========================================
        // SPEECH CONTROLS
        // ==========================================

        private MemoEdit txtSpeech;
        private MemoEdit txtRecognized;

        private ComboBoxEdit cmbVoices;
        private ComboBoxEdit cmbLanguages;

        // ==========================================
        // STATUS LABELS
        // ==========================================

        private Label lblTtsStatus;
        private Label lblSttStatus;
        private Label lblHistoryCount;
        private Label lblDashboardVoiceCount;
        private Label lblDashboardStatus;
        private Label lblDashboardLastAction;
        private Label lblCharacterCount;

        private ListBoxControl lstHistory;

        private FlowLayoutPanel dashboardRecentPanel;

        // ==========================================
        // NAVIGATION
        // ==========================================

        private readonly Dictionary<string, SimpleButton>
            navigationButtons =
                new Dictionary<string, SimpleButton>();

        // ==========================================
        // SERVICES
        // ==========================================

        private SpeechSynthesisService synthesisService;

        private SpeechRecognitionService recognitionService;

        private readonly List<string> history =
            new List<string>();

        private string currentRecognitionCulture = "";

        private bool isClosing;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        public MainForm()
        {
            Text = "Speech Assistant";

            StartPosition = FormStartPosition.CenterScreen;

            Size = new Size(1320, 850);

            MinimumSize = new Size(1000, 680);

            BackColor = backgroundColor;

            ForeColor = textColor;

            Font = new Font(uiFont, 10F);

            AutoScaleMode = AutoScaleMode.Dpi;

            BuildInterface();

            InitializeSpeechServices();

            FormClosing += MainForm_FormClosing;
        }

        // ==========================================
        // MAIN INTERFACE
        // ==========================================

        private void BuildInterface()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 2,

                RowCount = 1,

                BackColor = backgroundColor,

                Margin = Padding.Empty,

                Padding = Padding.Empty
            };

            root.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 245));

            root.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            sidebar = CreatePanel(sidebarColor);

            mainPanel = CreatePanel(backgroundColor);

            root.Controls.Add(sidebar, 0, 0);

            root.Controls.Add(mainPanel, 1, 0);

            Controls.Add(root);

            BuildSidebar();

            BuildMainArea();

            BuildPages();

            ShowPage("Genel Bakış");
        }

        private PanelControl CreatePanel(Color color)
        {
            return new PanelControl
            {
                Dock = DockStyle.Fill,

                BorderStyle = BorderStyles.NoBorder,

                Appearance =
                {
                    BackColor = color,

                    Options =
                    {
                        UseBackColor = true
                    }
                },

                Margin = Padding.Empty,

                Padding = Padding.Empty
            };
        }

        // ==========================================
        // SIDEBAR
        // ==========================================

        private void BuildSidebar()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 9,

                BackColor = sidebarColor,

                Padding = new Padding(15, 20, 15, 15)
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 85));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 32));

            for (int i = 0; i < 5; i++)
            {
                layout.RowStyles.Add(
                    new RowStyle(SizeType.Absolute, 55));
            }

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 75));

            sidebar.Controls.Add(layout);

            var logo = new Label
            {
                Text = "SPEECH\nASSISTANT",

                Dock = DockStyle.Fill,

                ForeColor = textColor,

                Font = new Font(
                    uiFont, 17F, FontStyle.Bold),

                TextAlign = ContentAlignment.MiddleLeft,

                BackColor = sidebarColor,

                Padding = new Padding(8, 0, 0, 0)
            };

            layout.Controls.Add(logo, 0, 0);

            var menuLabel = new Label
            {
                Text = "ANA MENÜ",

                Dock = DockStyle.Fill,

                ForeColor = mutedColor,

                Font = new Font(
                    uiFont, 8.5F, FontStyle.Bold),

                TextAlign = ContentAlignment.MiddleLeft,

                Padding = new Padding(8, 0, 0, 0),

                BackColor = sidebarColor
            };

            layout.Controls.Add(menuLabel, 0, 1);

            AddNavigationButton(
                layout, "⌂", "Genel Bakış", 2);

            AddNavigationButton(
                layout, "♫", "Metinden Konuşmaya", 3);

            AddNavigationButton(
                layout, "◉", "Konuşmadan Metne", 4);

            AddNavigationButton(
                layout, "▤", "Konuşma Geçmişi", 5);

            AddNavigationButton(
                layout, "⚙", "Ayarlar", 6);

            var footer = new Panel
            {
                Dock = DockStyle.Fill,

                BackColor = cardColor,

                Padding = new Padding(10),

                Margin = new Padding(3, 5, 3, 5)
            };

            var footerTitle = new Label
            {
                Text = "SPEECH ASSISTANT",

                Dock = DockStyle.Top,

                Height = 23,

                ForeColor = textColor,

                Font = new Font(
                    uiFont, 8.5F, FontStyle.Bold)
            };

            var footerVersion = new Label
            {
                Text = "Sürüm 1.0  •  .NET 10",

                Dock = DockStyle.Fill,

                ForeColor = mutedColor,

                Font = new Font(uiFont, 8F)
            };

            footer.Controls.Add(footerVersion);

            footer.Controls.Add(footerTitle);

            layout.Controls.Add(footer, 0, 8);
        }

        private void AddNavigationButton(
            TableLayoutPanel layout,
            string icon,
            string title,
            int row)
        {
            var button = new SimpleButton
            {
                Text = icon + "     " + title,

                Dock = DockStyle.Fill,

                Margin = new Padding(3, 5, 3, 5),

                Height = 45,

                Font = new Font(uiFont, 9.5F),

                ForeColor = mutedColor,

                Tag = title,

                Appearance =
                {
                    BackColor = sidebarColor,

                    ForeColor = mutedColor,

                    Options =
                    {
                        UseBackColor = true,

                        UseForeColor = true
                    }
                }
            };

            button.Click += (s, e) => ShowPage(title);

            navigationButtons[title] = button;

            layout.Controls.Add(button, 0, row);
        }

        private void UpdateNavigationStyle(string activePage)
        {
            foreach (var item in navigationButtons)
            {
                bool selected = item.Key == activePage;

                item.Value.Appearance.BackColor =
                    selected ? cardColor : sidebarColor;

                item.Value.Appearance.ForeColor =
                    selected ? primaryColor : mutedColor;

                item.Value.Appearance.Options.UseBackColor = true;

                item.Value.Appearance.Options.UseForeColor = true;
            }
        }

        // ==========================================
        // HEADER
        // ==========================================

        private void BuildMainArea()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 2,

                BackColor = backgroundColor,

                Margin = Padding.Empty,

                Padding = Padding.Empty
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 112));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            header = CreatePanel(backgroundColor);

            content = CreatePanel(backgroundColor);

            layout.Controls.Add(header, 0, 0);

            layout.Controls.Add(content, 0, 1);

            mainPanel.Controls.Add(layout);

            BuildHeader();
        }

        private void BuildHeader()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 2,

                RowCount = 2,

                Padding = new Padding(28, 15, 28, 10),

                BackColor = backgroundColor
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 75));

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 25));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 65));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 35));

            lblPageTitle = new Label
            {
                Text = "Genel Bakış",

                Dock = DockStyle.Fill,

                ForeColor = textColor,

                Font = new Font(
                    uiFont, 21F, FontStyle.Bold),

                TextAlign = ContentAlignment.MiddleLeft
            };

            lblPageDescription = new Label
            {
                Text = "Ses işlemlerinizi tek bir yerden yönetin.",

                Dock = DockStyle.Fill,

                ForeColor = mutedColor,

                Font = new Font(uiFont, 9.5F),

                TextAlign = ContentAlignment.MiddleLeft
            };

            lblHeaderStatus = new Label
            {
                Text = "●  SİSTEM HAZIR",

                Dock = DockStyle.Fill,

                ForeColor = successColor,

                Font = new Font(
                    uiFont, 9F, FontStyle.Bold),

                TextAlign = ContentAlignment.MiddleRight
            };

            layout.Controls.Add(lblPageTitle, 0, 0);

            layout.Controls.Add(lblPageDescription, 0, 1);

            layout.Controls.Add(lblHeaderStatus, 1, 0);

            header.Controls.Add(layout);
        }

        // ==========================================
        // PAGES
        // ==========================================

        private void BuildPages()
        {
            dashboardPage = CreatePage();

            ttsPage = CreatePage();

            sttPage = CreatePage();

            historyPage = CreatePage();

            settingsPage = CreatePage();

            content.Controls.Add(dashboardPage);

            content.Controls.Add(ttsPage);

            content.Controls.Add(sttPage);

            content.Controls.Add(historyPage);

            content.Controls.Add(settingsPage);

            BuildDashboard();

            BuildTtsPage();

            BuildSttPage();

            BuildHistoryPage();

            BuildSettingsPage();
        }

        private PanelControl CreatePage()
        {
            var page = CreatePanel(backgroundColor);

            page.AutoScroll = true;

            page.Padding = new Padding(22);

            page.Visible = false;

            return page;
        }

        // ==========================================
        // PAGE LAYOUT
        // ==========================================

        private FlowLayoutPanel CreatePageLayout(
            PanelControl page)
        {
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,

                AutoSize = true,

                AutoSizeMode = AutoSizeMode.GrowAndShrink,

                FlowDirection = FlowDirection.TopDown,

                WrapContents = false,

                Padding = new Padding(4),

                Margin = Padding.Empty,

                BackColor = backgroundColor,

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Left |
                    AnchorStyles.Right
            };

            page.Controls.Add(flow);

            void UpdateWidth()
            {
                int availableWidth =
                    page.ClientSize.Width -
                    page.Padding.Horizontal - 15;

                flow.Width = Math.Max(300, availableWidth);

                foreach (Control control in flow.Controls)
                {
                    control.Width = Math.Max(
                        280, flow.ClientSize.Width - 20);
                }
            }

            page.Resize += (s, e) => UpdateWidth();

            flow.ControlAdded += (s, e) => UpdateWidth();

            UpdateWidth();

            return flow;
        }

        private PanelControl CreateCard(
            FlowLayoutPanel parent,
            int height)
        {
            var card = new PanelControl
            {
                Height = height,

                Width = Math.Max(
                    300, parent.ClientSize.Width - 20),

                Margin = new Padding(4, 7, 4, 14),

                Padding = new Padding(18),

                BorderStyle = BorderStyles.NoBorder,

                Appearance =
                {
                    BackColor = cardColor,

                    Options =
                    {
                        UseBackColor = true
                    }
                }
            };

            parent.Controls.Add(card);

            parent.Resize += (s, e) =>
            {
                card.Width = Math.Max(
                    300, parent.ClientSize.Width - 20);
            };

            return card;
        }

        private Label CreateLabel(
            string text,
            float size = 10,
            bool bold = false,
            Color? color = null)
        {
            return new Label
            {
                Text = text,

                AutoSize = true,

                ForeColor = color ?? textColor,

                Font = new Font(
                    uiFont,
                    size,
                    bold ? FontStyle.Bold : FontStyle.Regular),

                BackColor = Color.Transparent,

                Margin = new Padding(0, 5, 0, 8)
            };
        }

        // ==========================================
        // INPUT DESIGN
        // ==========================================

        private PanelControl CreateInputShell(Control input)
        {
            var shell = new PanelControl
            {
                Dock = DockStyle.Fill,

                Padding = new Padding(12),

                Margin = new Padding(0, 5, 0, 8),

                BorderStyle = BorderStyles.Simple,

                Appearance =
                {
                    BackColor = inputColor,

                    Options =
                    {
                        UseBackColor = true
                    }
                }
            };

            input.Dock = DockStyle.Fill;

            input.BackColor = inputColor;

            input.ForeColor = textColor;

            input.Font = new Font(uiFont, 11F);

            shell.Controls.Add(input);

            return shell;
        }

        private void StyleComboBox(ComboBoxEdit combo)
        {
            combo.Dock = DockStyle.Fill;

            combo.Font = new Font(uiFont, 10F);

            combo.Properties.TextEditStyle =
                TextEditStyles.DisableTextEditor;

            combo.Properties.Appearance.BackColor = inputColor;

            combo.Properties.Appearance.ForeColor = textColor;

            combo.Properties.Appearance.Font =
                new Font(uiFont, 10F);

            combo.Properties.Appearance.Options.UseBackColor = true;

            combo.Properties.Appearance.Options.UseForeColor = true;

            combo.Properties.Appearance.Options.UseFont = true;
        }

        // ==========================================
        // DASHBOARD
        // ==========================================

        private void BuildDashboard()
        {
            var flow = CreatePageLayout(dashboardPage);

            // WELCOME CARD

            var welcomeCard = CreateCard(flow, 155);

            var welcomeLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 3,

                BackColor = cardColor
            };

            welcomeLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 43));

            welcomeLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 38));

            welcomeLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            welcomeLayout.Controls.Add(
                CreateLabel(
                    "Speech Assistant'a Hoş Geldiniz",
                    17, true), 0, 0);

            welcomeLayout.Controls.Add(
                CreateLabel(
                    "Ses ve metin işlemlerinizi kolayca yönetin.",
                    11, false, secondaryColor), 0, 1);

            welcomeLayout.Controls.Add(
                CreateLabel(
                    "Yazılı metinleri sese dönüştürün ve konuşmalarınızı metin olarak tanıyın.",
                    10, false, mutedColor), 0, 2);

            welcomeCard.Controls.Add(welcomeLayout);

            // STATISTICS CARD

            var statsCard = CreateCard(flow, 150);

            var statsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 3,

                RowCount = 2,

                BackColor = cardColor,

                Padding = new Padding(5)
            };

            for (int i = 0; i < 3; i++)
            {
                statsLayout.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 33.33F));
            }

            statsLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 45));

            statsLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 55));

            statsLayout.Controls.Add(
                CreateLabel(
                    "TOPLAM İŞLEM", 8.5F, true, mutedColor),
                0, 0);

            statsLayout.Controls.Add(
                CreateLabel(
                    "SES ÇIKTISI", 8.5F, true, mutedColor),
                1, 0);

            statsLayout.Controls.Add(
                CreateLabel(
                    "SİSTEM DURUMU", 8.5F, true, mutedColor),
                2, 0);

            lblHistoryCount = CreateLabel("0", 23, true);

            lblDashboardVoiceCount = CreateLabel("0", 23, true);

            lblDashboardStatus = CreateLabel(
                "Hazır", 17, true, successColor);

            statsLayout.Controls.Add(lblHistoryCount, 0, 1);

            statsLayout.Controls.Add(lblDashboardVoiceCount, 1, 1);

            statsLayout.Controls.Add(lblDashboardStatus, 2, 1);

            statsCard.Controls.Add(statsLayout);

            // QUICK ACTIONS CARD

            var actionsCard = CreateCard(flow, 165);

            var actionsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 3,

                BackColor = cardColor
            };

            actionsLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            actionsLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50));

            actionsLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50));

            actionsLayout.Controls.Add(
                CreateLabel("Hızlı İşlemler", 15, true),
                0, 0);

            var ttsButton = CreateActionButton(
                "♫    Metinden Konuşmaya", primaryColor);

            ttsButton.Dock = DockStyle.Left;

            ttsButton.Width = 245;

            ttsButton.Click += (s, e) =>
                ShowPage("Metinden Konuşmaya");

            var sttButton = CreateActionButton(
                "◉    Konuşmadan Metne", secondaryColor);

            sttButton.Dock = DockStyle.Left;

            sttButton.Width = 245;

            sttButton.Click += (s, e) =>
                ShowPage("Konuşmadan Metne");

            actionsLayout.Controls.Add(ttsButton, 0, 1);

            actionsLayout.Controls.Add(sttButton, 0, 2);

            actionsCard.Controls.Add(actionsLayout);

            // RECENT ACTIVITY CARD

            var recentCard = CreateCard(flow, 190);

            var recentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 3,

                BackColor = cardColor
            };

            recentLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            recentLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            recentLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 25));

            recentLayout.Controls.Add(
                CreateLabel("Son İşlemler", 14, true),
                0, 0);

            dashboardRecentPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,

                FlowDirection = FlowDirection.TopDown,

                WrapContents = false,

                AutoScroll = true,

                BackColor = cardColor
            };

            dashboardRecentPanel.Controls.Add(
                CreateLabel(
                    "Henüz işlem yapılmadı.",
                    9, false, mutedColor));

            recentLayout.Controls.Add(
                dashboardRecentPanel, 0, 1);

            lblDashboardLastAction = CreateLabel(
                "İlk işleminiz bekleniyor...",
                8.5F, false, mutedColor);

            recentLayout.Controls.Add(
                lblDashboardLastAction, 0, 2);

            recentCard.Controls.Add(recentLayout);
        }

        // ==========================================
        // TEXT TO SPEECH
        // ==========================================

        private void BuildTtsPage()
        {
            var flow = CreatePageLayout(ttsPage);

            // INTRO CARD

            var introCard = CreateCard(flow, 105);

            var introLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 2,

                BackColor = cardColor
            };

            introLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 55));

            introLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 45));

            introLayout.Controls.Add(
                CreateLabel(
                    "Metinden Konuşmaya", 17, true),
                0, 0);

            introLayout.Controls.Add(
                CreateLabel(
                    "Yazdığınız metni sesli çıktıya dönüştürün.",
                    10, false, mutedColor),
                0, 1);

            introCard.Controls.Add(introLayout);

            // EDITOR CARD

            var editorCard = CreateCard(flow, 565);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 7,

                BackColor = cardColor,

                Padding = new Padding(5)
            };

            // 0 - TEXT LABEL
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 32));

            // 1 - TEXT INPUT
            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            // 2 - CHARACTER COUNT
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 25));

            // 3 - VOICE LABEL
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 30));

            // 4 - VOICE COMBOBOX
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 43));

            // 5 - BUTTONS
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            // 6 - STATUS
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 30));

            layout.Controls.Add(
                CreateLabel("Metin Alanı", 12, true),
                0, 0);

            txtSpeech = new MemoEdit();

            txtSpeech.Properties.NullValuePrompt =
                "Konuşmaya dönüştürmek istediğiniz metni buraya yazın...";

            txtSpeech.Properties.BorderStyle =
                BorderStyles.NoBorder;

            txtSpeech.Properties.Appearance.BackColor = inputColor;

            txtSpeech.Properties.Appearance.ForeColor = textColor;

            txtSpeech.Properties.Appearance.Font =
                new Font(uiFont, 11F);

            txtSpeech.Properties.Appearance.Options.UseBackColor = true;

            txtSpeech.Properties.Appearance.Options.UseForeColor = true;

            txtSpeech.Properties.Appearance.Options.UseFont = true;

            var speechShell = CreateInputShell(txtSpeech);

            layout.Controls.Add(speechShell, 0, 1);

            lblCharacterCount = CreateLabel(
                "0 karakter", 8.5F, false, mutedColor);

            lblCharacterCount.TextAlign =
                ContentAlignment.MiddleRight;

            lblCharacterCount.Dock = DockStyle.Fill;

            txtSpeech.TextChanged += (s, e) =>
            {
                int count = txtSpeech.Text.Length;

                lblCharacterCount.Text =
                    count.ToString() + " karakter";
            };

            layout.Controls.Add(lblCharacterCount, 0, 2);

            // VOICE LABEL - SEPARATE ROW

            layout.Controls.Add(
                CreateLabel(
                    "SES SEÇİMİ", 9, true, mutedColor),
                0, 3);

            // VOICE COMBOBOX - SEPARATE ROW

            cmbVoices = new ComboBoxEdit();

            StyleComboBox(cmbVoices);

            layout.Controls.Add(cmbVoices, 0, 4);

            // BUTTONS

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,

                FlowDirection = FlowDirection.LeftToRight,

                WrapContents = true,

                BackColor = cardColor
            };

            var speakButton = CreateActionButton(
                "▶   Konuşmayı Başlat", primaryColor);

            speakButton.Click += SpeakButton_Click;

            var pauseButton = CreateActionButton(
                "Ⅱ   Duraklat / Sürdür", secondaryColor);

            pauseButton.Click += PauseButton_Click;

            var stopButton = CreateActionButton(
                "■   Durdur", dangerColor);

            stopButton.Click += (s, e) =>
            {
                synthesisService?.Stop();

                lblTtsStatus.Text = "Konuşma durduruldu.";
            };

            buttons.Controls.Add(speakButton);

            buttons.Controls.Add(pauseButton);

            buttons.Controls.Add(stopButton);

            layout.Controls.Add(buttons, 0, 5);

            // STATUS

            lblTtsStatus = CreateLabel(
                "● Hazır", 9, false, successColor);

            layout.Controls.Add(lblTtsStatus, 0, 6);

            editorCard.Controls.Add(layout);
        }

        private void SpeakButton_Click(
            object sender,
            EventArgs e)
        {
            if (synthesisService == null)
            {
                lblTtsStatus.Text =
                    "Ses hizmeti kullanılamıyor.";

                return;
            }

            string text = txtSpeech.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                lblTtsStatus.Text =
                    "Lütfen önce metin girin.";

                return;
            }

            try
            {
                if (cmbVoices.SelectedItem != null)
                {
                    string voiceName =
                        cmbVoices.SelectedItem.ToString();

                    if (!string.IsNullOrWhiteSpace(voiceName))
                    {
                        synthesisService.SelectVoice(voiceName);
                    }
                }

                synthesisService.Speak(text);

                lblTtsStatus.Text = "● Konuşuyor...";

                AddHistory("Metinden Konuşmaya: " + text);
            }
            catch (Exception ex)
            {
                lblTtsStatus.Text = "Hata: " + ex.Message;
            }
        }

        private void PauseButton_Click(
            object sender,
            EventArgs e)
        {
            if (synthesisService == null)
                return;

            try
            {
                if (synthesisService.GetState() ==
                    SynthesizerState.Paused)
                {
                    synthesisService.Resume();

                    lblTtsStatus.Text =
                        "● Konuşma sürdürülüyor...";
                }
                else if (synthesisService.GetState() ==
                    SynthesizerState.Speaking)
                {
                    synthesisService.Pause();

                    lblTtsStatus.Text =
                        "Konuşma duraklatıldı.";
                }
                else
                {
                    lblTtsStatus.Text =
                        "Şu anda aktif bir konuşma yok.";
                }
            }
            catch (Exception ex)
            {
                lblTtsStatus.Text = "Hata: " + ex.Message;
            }
        }

        // ==========================================
        // SPEECH TO TEXT
        // ==========================================

        private void BuildSttPage()
        {
            var flow = CreatePageLayout(sttPage);

            // INTRO CARD

            var introCard = CreateCard(flow, 105);

            var introLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 2,

                BackColor = cardColor
            };

            introLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 55));

            introLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 45));

            introLayout.Controls.Add(
                CreateLabel(
                    "Konuşmadan Metne", 17, true),
                0, 0);

            introLayout.Controls.Add(
                CreateLabel(
                    "Konuşmanızı algılayın ve metin olarak görüntüleyin.",
                    10, false, mutedColor),
                0, 1);

            introCard.Controls.Add(introLayout);

            // EDITOR CARD

            var editorCard = CreateCard(flow, 565);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 7,

                BackColor = cardColor,

                Padding = new Padding(5)
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 32));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 43));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 32));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 30));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 45));

            // LANGUAGE LABEL

            layout.Controls.Add(
                CreateLabel("Tanıma Dili", 10, true),
                0, 0);

            // LANGUAGE COMBOBOX

            cmbLanguages = new ComboBoxEdit();

            StyleComboBox(cmbLanguages);

            layout.Controls.Add(cmbLanguages, 0, 1);

            // RECOGNIZED TEXT LABEL

            layout.Controls.Add(
                CreateLabel(
                    "ALGILANAN METİN", 9, true, mutedColor),
                0, 2);

            // RECOGNIZED TEXT INPUT

            txtRecognized = new MemoEdit
            {
                ReadOnly = true
            };

            txtRecognized.Properties.NullValuePrompt =
                "Algılanan konuşma burada görüntülenecek...";

            txtRecognized.Properties.BorderStyle =
                BorderStyles.NoBorder;

            txtRecognized.Properties.Appearance.BackColor =
                inputColor;

            txtRecognized.Properties.Appearance.ForeColor =
                textColor;

            txtRecognized.Properties.Appearance.Font =
                new Font(uiFont, 11F);

            txtRecognized.Properties.Appearance.Options.UseBackColor =
                true;

            txtRecognized.Properties.Appearance.Options.UseForeColor =
                true;

            txtRecognized.Properties.Appearance.Options.UseFont =
                true;

            var recognizedShell =
                CreateInputShell(txtRecognized);

            layout.Controls.Add(recognizedShell, 0, 3);

            // BUTTONS

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,

                FlowDirection = FlowDirection.LeftToRight,

                WrapContents = true,

                BackColor = cardColor
            };

            var startButton = CreateActionButton(
                "●   Dinlemeyi Başlat", primaryColor);

            startButton.Click += StartRecognition_Click;

            var stopButton = CreateActionButton(
                "■   Dinlemeyi Durdur", dangerColor);

            stopButton.Click += (s, e) =>
            {
                try
                {
                    recognitionService?.StopListening();

                    lblSttStatus.Text =
                        "Dinleme durduruluyor...";
                }
                catch (Exception ex)
                {
                    lblSttStatus.Text =
                        "Hata: " + ex.Message;
                }
            };

            buttons.Controls.Add(startButton);

            buttons.Controls.Add(stopButton);

            layout.Controls.Add(buttons, 0, 4);

            // STATUS

            lblSttStatus = CreateLabel(
                "● Hazır", 9, false, successColor);

            layout.Controls.Add(lblSttStatus, 0, 5);

            // CLEAR BUTTON

            var clearButton = CreateActionButton(
                "Metni Temizle", secondaryColor);

            clearButton.Dock = DockStyle.Left;

            clearButton.Width = 160;

            clearButton.Click += (s, e) =>
            {
                txtRecognized.Text = "";
            };

            layout.Controls.Add(clearButton, 0, 6);

            editorCard.Controls.Add(layout);
        }

        private void StartRecognition_Click(
            object sender,
            EventArgs e)
        {
            if (recognitionService == null)
            {
                lblSttStatus.Text =
                    "Konuşma tanıma hizmeti kullanılamıyor.";

                return;
            }

            try
            {
                if (cmbLanguages.SelectedItem is SpeechLanguage language)
                {
                    if (currentRecognitionCulture !=
                        language.CultureName)
                    {
                        recognitionService.SelectLanguage(
                            language.CultureName);

                        currentRecognitionCulture =
                            language.CultureName;
                    }
                }

                recognitionService.StartListening();

                lblSttStatus.Text = "● Dinleniyor...";
            }
            catch (Exception ex)
            {
                lblSttStatus.Text = "Hata: " + ex.Message;
            }
        }

        // ==========================================
        // HISTORY
        // ==========================================

        private void BuildHistoryPage()
        {
            var flow = CreatePageLayout(historyPage);

            var card = CreateCard(flow, 530);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 3,

                BackColor = cardColor
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 45));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            layout.Controls.Add(
                CreateLabel("İşlem Geçmişi", 16, true),
                0, 0);

            lstHistory = new ListBoxControl
            {
                Dock = DockStyle.Fill,

                Font = new Font(uiFont, 10),

                Margin = new Padding(0, 5, 0, 10)
            };

            lstHistory.Appearance.BackColor = inputColor;

            lstHistory.Appearance.ForeColor = textColor;

            lstHistory.Appearance.Options.UseBackColor = true;

            lstHistory.Appearance.Options.UseForeColor = true;

            layout.Controls.Add(lstHistory, 0, 1);

            var clearButton = CreateActionButton(
                "Geçmişi Temizle", dangerColor);

            clearButton.Click += (s, e) =>
            {
                history.Clear();

                lstHistory.Items.Clear();

                dashboardRecentPanel.Controls.Clear();

                dashboardRecentPanel.Controls.Add(
                    CreateLabel(
                        "Henüz işlem yapılmadı.",
                        9, false, mutedColor));

                lblDashboardLastAction.Text =
                    "İlk işleminiz bekleniyor...";

                UpdateHistoryCount();
            };

            layout.Controls.Add(clearButton, 0, 2);

            card.Controls.Add(layout);
        }

        private void AddHistory(string item)
        {
            string entry =
                DateTime.Now.ToString("HH:mm:ss") +
                "  •  " + item;

            history.Insert(0, entry);

            if (lstHistory != null)
            {
                lstHistory.Items.Insert(0, entry);
            }

            UpdateHistoryCount();

            UpdateDashboardRecent(entry);
        }

        private void UpdateHistoryCount()
        {
            if (lblHistoryCount != null)
            {
                lblHistoryCount.Text =
                    history.Count.ToString();
            }

            if (lblDashboardVoiceCount != null)
            {
                int voiceOperations = history.Count(
                    x => x.Contains("Metinden Konuşmaya"));

                lblDashboardVoiceCount.Text =
                    voiceOperations.ToString();
            }
        }

        private void UpdateDashboardRecent(string entry)
        {
            if (dashboardRecentPanel == null)
                return;

            dashboardRecentPanel.Controls.Clear();

            foreach (string item in history.Take(4))
            {
                dashboardRecentPanel.Controls.Add(
                    CreateLabel(
                        item, 9, false, mutedColor));
            }

            lblDashboardLastAction.Text =
                "Son işlem: " +
                DateTime.Now.ToString("HH:mm:ss");
        }

        // ==========================================
        // SETTINGS
        // ==========================================

        private void BuildSettingsPage()
        {
            var flow = CreatePageLayout(settingsPage);

            var card = CreateCard(flow, 330);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 6,

                BackColor = cardColor,

                Padding = new Padding(5)
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 50));

            for (int i = 0; i < 4; i++)
            {
                layout.RowStyles.Add(
                    new RowStyle(SizeType.Absolute, 42));
            }

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(
                CreateLabel(
                    "Uygulama Bilgileri", 16, true),
                0, 0);

            layout.Controls.Add(
                CreateLabel(
                    "Uygulama Adı: Speech Assistant", 10),
                0, 1);

            layout.Controls.Add(
                CreateLabel("Sürüm: 1.0", 10),
                0, 2);

            layout.Controls.Add(
                CreateLabel("Framework: .NET 10", 10),
                0, 3);

            layout.Controls.Add(
                CreateLabel(
                    "Arayüz: DevExpress WinForms", 10),
                0, 4);

            layout.Controls.Add(
                CreateLabel(
                    "Konuşma tanıma dilleri, Windows sistemine yüklenmiş tanıma motorlarına bağlıdır.",
                    9, false, mutedColor),
                0, 5);

            card.Controls.Add(layout);
        }

        // ==========================================
        // BUTTON DESIGN
        // ==========================================

        private SimpleButton CreateActionButton(
            string text,
            Color color)
        {
            return new SimpleButton
            {
                Text = text,

                Height = 39,

                Width = 190,

                Margin = new Padding(3, 5, 8, 5),

                Font = new Font(
                    uiFont, 9F, FontStyle.Bold),

                ForeColor = Color.White,

                Appearance =
                {
                    BackColor = color,

                    ForeColor = Color.White,

                    Options =
                    {
                        UseBackColor = true,

                        UseForeColor = true
                    }
                }
            };
        }

        // ==========================================
        // NAVIGATION
        // ==========================================

        private void ShowPage(string pageName)
        {
            dashboardPage.Visible = false;

            ttsPage.Visible = false;

            sttPage.Visible = false;

            historyPage.Visible = false;

            settingsPage.Visible = false;

            switch (pageName)
            {
                case "Genel Bakış":

                    dashboardPage.Visible = true;

                    lblPageTitle.Text = "Genel Bakış";

                    lblPageDescription.Text =
                        "Ses işlemlerinizi tek bir yerden yönetin.";

                    break;

                case "Metinden Konuşmaya":

                    ttsPage.Visible = true;

                    lblPageTitle.Text = "Metinden Konuşmaya";

                    lblPageDescription.Text =
                        "Yazılı metinlerinizi sesli çıktıya dönüştürün.";

                    break;

                case "Konuşmadan Metne":

                    sttPage.Visible = true;

                    lblPageTitle.Text = "Konuşmadan Metne";

                    lblPageDescription.Text =
                        "Konuşmalarınızı metin olarak görüntüleyin.";

                    break;

                case "Konuşma Geçmişi":

                    historyPage.Visible = true;

                    lblPageTitle.Text = "Konuşma Geçmişi";

                    lblPageDescription.Text =
                        "Önceki işlemlerinizi inceleyin.";

                    break;

                case "Ayarlar":

                    settingsPage.Visible = true;

                    lblPageTitle.Text = "Ayarlar";

                    lblPageDescription.Text =
                        "Uygulama bilgileri ve ayarları.";

                    break;
            }

            UpdateNavigationStyle(pageName);

            foreach (Control page in content.Controls)
            {
                if (page.Visible)
                {
                    page.BringToFront();

                    break;
                }
            }
        }

        // ==========================================
        // SPEECH SERVICES
        // ==========================================

        private void InitializeSpeechServices()
        {
            // TEXT TO SPEECH

            try
            {
                synthesisService = new SpeechSynthesisService();

                foreach (var voice in
                    synthesisService.GetAvailableVoices())
                {
                    cmbVoices.Properties.Items.Add(voice.Name);
                }

                if (cmbVoices.Properties.Items.Count > 0)
                {
                    cmbVoices.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                lblTtsStatus.Text =
                    "Ses hizmeti hatası: " + ex.Message;
            }

            // SPEECH TO TEXT

            try
            {
                recognitionService =
                    new SpeechRecognitionService();

                var languages =
                    recognitionService.GetAvailableLanguages();

                foreach (var language in languages)
                {
                    cmbLanguages.Properties.Items.Add(language);
                }

                if (languages.Count > 0)
                {
                    cmbLanguages.SelectedIndex = 0;

                    currentRecognitionCulture =
                        languages[0].CultureName;
                }
                else
                {
                    lblSttStatus.Text =
                        "Yüklü konuşma tanıma dili bulunamadı.";
                }

                recognitionService.SpeechRecognized +=
                    RecognitionService_SpeechRecognized;

                recognitionService.RecognitionCompleted +=
                    RecognitionService_RecognitionCompleted;
            }
            catch (Exception ex)
            {
                lblSttStatus.Text =
                    "Tanıma hizmeti hatası: " + ex.Message;
            }
        }

        // ==========================================
        // SPEECH RECOGNIZED EVENT
        // ==========================================

        private void RecognitionService_SpeechRecognized(
            object sender,
            SpeechRecognizedEventArgs e)
        {
            string recognizedText = e.Result.Text;

            if (string.IsNullOrWhiteSpace(recognizedText))
                return;

            RunOnUiThread(() =>
            {
                txtRecognized.Text +=
                    recognizedText + Environment.NewLine;

                AddHistory(
                    "Konuşmadan Metne: " + recognizedText);

                lblSttStatus.Text =
                    "● Konuşma algılandı.";
            });
        }

        // ==========================================
        // RECOGNITION COMPLETED EVENT
        // ==========================================

        private void RecognitionService_RecognitionCompleted(
            object sender,
            RecognizeCompletedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                lblSttStatus.Text =
                    "Dinleme tamamlandı.";
            });
        }

        // ==========================================
        // UI THREAD
        // ==========================================

        private void RunOnUiThread(Action action)
        {
            if (isClosing || IsDisposed || !IsHandleCreated)
                return;

            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (InvalidOperationException)
            {
                // Form kapanırken UI güncellemelerini yok say.
            }
        }

        // ==========================================
        // FORM CLOSING
        // ==========================================

        private void MainForm_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            isClosing = true;

            try
            {
                recognitionService?.Dispose();
            }
            catch
            {
                // Tanıma motoru zaten durmuş olabilir.
            }

            try
            {
                synthesisService?.Dispose();
            }
            catch
            {
                // Ses motoru zaten durmuş olabilir.
            }
        }
    }
}
