using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskbarCalendar
{
    /// <summary>Settings behavior and persistence. The visual layout is in SettingsForm.Designer.cs.</summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public sealed partial class SettingsForm : Form
    {
        readonly Config cfg;
        readonly bool demo;
        readonly Action<Config> onSaved;
        readonly Func<System.Collections.Generic.IEnumerable<Meeting>> previewMeetings;
        readonly Func<bool> previewPrivate;
        readonly System.Collections.Generic.List<Action> colourRefreshers = new System.Collections.Generic.List<Action>();
        bool updatingTextOptions;

        // Visual Studio instantiates this constructor for the Windows Forms Designer.
        // It uses demo data and cannot write the user's configuration.
        public SettingsForm()
        {
            cfg = new Config(); demo = true; onSaved = null;
            previewMeetings = () => Enumerable.Empty<Meeting>(); previewPrivate = () => false;
            InitializeComponent();
            InitializeFormState();
            ApplyRuntimeWindowProperties();
        }

        internal SettingsForm(Config source, bool firstRun, bool demo, Action<Config> onSaved, Func<System.Collections.Generic.IEnumerable<Meeting>> previewMeetings = null, Func<bool> previewPrivate = null)
        {
            cfg = source.Clone(); this.demo = demo; this.onSaved = onSaved;
            this.previewMeetings = previewMeetings ?? (() => Enumerable.Empty<Meeting>()); this.previewPrivate = previewPrivate ?? (() => false);
            InitializeComponent();
            InitializeFormState();
            ApplyRuntimeWindowProperties();
            if (firstRun) testResult.Text = "Classic Outlook is required. Use Test Outlook after signing in.";
        }

        void ApplyRuntimeWindowProperties()
        {
            // Program.AppIcon touches application resources. Keep it out of
            // InitializeComponent so the Visual Studio designer can create the form.
            try { Icon = Program.AppIcon; } catch { }
        }

        void InitializeFormState()
        {
            Font = new Font("MS Sans Serif", 8.25F);
            testResult.Text = "";
            layout.Items.Clear(); tone.Items.Clear(); privacyDisplay.Items.Clear(); preset.Items.Clear(); textElement.Items.Clear(); clickAction.Items.Clear(); doubleClickAction.Items.Clear(); hoverAction.Items.Clear();
            layout.Items.AddRange(Enum.GetNames(typeof(StripLayout))); tone.Items.AddRange(Enum.GetNames(typeof(EmptyDayTone))); privacyDisplay.Items.AddRange(Enum.GetNames(typeof(PrivacyDisplay)));
            preset.Items.AddRange(new object[] { "Notes", "Taskbar", "Midnight" });
            textElement.Items.AddRange(new object[] { "Taskbar top line", "Taskbar bottom line", "Card headings", "Card meeting times", "Card meeting titles", "Card notes", "Card date number", "Card date month", "Card date day" });
            AddActions(clickAction, true); AddActions(doubleClickAction, true); AddActions(hoverAction, false);
            SetValue(lookahead, cfg.LookaheadDays); SetValue(refresh, cfg.RefreshMinutes); SetValue(width, cfg.Width); SetValue(gap, cfg.RightGap); SetValue(amber, cfg.AmberThresholdMinutes); SetValue(fadeDelay, cfg.FadeDelayMilliseconds); SetValue(fadeDuration, cfg.FadeDurationMilliseconds); SetValue(appearStart, cfg.AppearStartPercent); SetValue(appearDuration, cfg.AppearDurationMilliseconds); SetValue(opacity, cfg.BackgroundOpacity); SetValue(ruleThickness, cfg.RuleLineThickness); SetValue(marginThickness, cfg.MarginLineThickness); cornerRadius.Value = Math.Max(cornerRadius.Minimum, Math.Min(cornerRadius.Maximum, cfg.CardCornerRadius)); UpdateCornerRadiusLabel();
            layout.SelectedItem = cfg.Layout.ToString(); tone.SelectedItem = cfg.EmptyDayTone.ToString(); privacyDisplay.SelectedItem = cfg.PrivacyDisplay.ToString(); preset.SelectedItem = cfg.ThemePreset;
            SelectAction(clickAction, cfg.ClickAction); SelectAction(doubleClickAction, cfg.DoubleClickAction); SelectAction(hoverAction, cfg.HoverAction);
            autoWidth.Checked = cfg.AutoWidth; twentyFourHour.Checked = cfg.Use24HourClock; lateAlert.Checked = cfg.LateAlertEnabled; skipCancelledMeetings.Checked = cfg.SkipCancelledMeetings; noStripBackground.Checked = cfg.NoStripBackground; autoTaskbarTextColors.Checked = cfg.AutoTaskbarTextColors; shadow.Checked = cfg.CardShadow; callout.Checked = cfg.ShowCallout; doubleMargin.Checked = cfg.DoubleMarginLine; automaticPrivacy.Checked = cfg.TeamsPrivacyEnabled; startup.Checked = demo || Installer.StartupEnabled; emptyMessage.Text = cfg.EmptyDayMessage;
            textElement.SelectedIndex = 0; LoadTextOptions();
            ConfigureColourButton(colourStrip, "Strip background", () => cfg.StripBackground, value => cfg.StripBackground = value);
            ConfigureColourButton(colourGreen, "Time: started", () => cfg.TimeGreenColor, value => cfg.TimeGreenColor = value);
            ConfigureColourButton(colourAmber, "Late alert", () => cfg.TimeAmberColor, value => cfg.TimeAmberColor = value);
            ConfigureColourButton(colourPaper, "Card paper", () => cfg.CardBackground, value => cfg.CardBackground = value);
            ConfigureColourButton(colourRule, "Rule lines", () => cfg.CardLine, value => cfg.CardLine = value);
            ConfigureColourButton(colourAccent, "Accent and margin", () => cfg.AccentColor, value => cfg.AccentColor = value);
            SetButtonIcon(testOutlook, LegacyIcons.CalendarImage); SetButtonIcon(fontPicker, LegacyIcons.DocumentImage); SetButtonIcon(textColour, LegacyIcons.ColorImage);
            SetButtonIcon(pickAnother, LegacyIcons.RefreshImage); SetButtonIcon(settingsFolder, LegacyIcons.GearImage); SetButtonIcon(logFolder, LegacyIcons.ClockImage);
            SetButtonIcon(applyButton, LegacyIcons.TickImage); SetButtonIcon(okButton, LegacyIcons.TickImage); SetButtonIcon(cancelButton, LegacyIcons.CrossImage);
            testOutlook.Click += (s, e) => TestOutlook(); fontPicker.Click += (s, e) => PickFont(); textColour.Click += (s, e) => PickTextColour(); applyButton.Click += (s, e) => Save(false); okButton.Click += (s, e) => Save(true); cancelButton.Click += (s, e) => Close();
            autoWidth.CheckedChanged += (s, e) => width.Enabled = !autoWidth.Checked;
            twentyFourHour.CheckedChanged += (s, e) => { cfg.Use24HourClock = twentyFourHour.Checked; RefreshAppearancePreview(); };
            lateAlert.CheckedChanged += (s, e) => amber.Enabled = lateAlert.Checked;
            preset.SelectedIndexChanged += (s, e) => { Theme.ApplyPreset(cfg, preset.SelectedItem.ToString()); RefreshAppearancePreview(); };
            opacity.ValueChanged += (s, e) => { cfg.BackgroundOpacity = (int)opacity.Value; RefreshAppearancePreview(); };
            noStripBackground.CheckedChanged += (s, e) => { cfg.NoStripBackground = noStripBackground.Checked; opacity.Enabled = !noStripBackground.Checked; RefreshAppearancePreview(); };
            autoTaskbarTextColors.CheckedChanged += (s, e) => { cfg.AutoTaskbarTextColors = autoTaskbarTextColors.Checked; RefreshAppearancePreview(); };
            shadow.CheckedChanged += (s, e) => { cfg.CardShadow = shadow.Checked; RefreshAppearancePreview(); };
            callout.CheckedChanged += (s, e) => { cfg.ShowCallout = callout.Checked; RefreshAppearancePreview(); };
            doubleMargin.CheckedChanged += (s, e) => { cfg.DoubleMarginLine = doubleMargin.Checked; RefreshAppearancePreview(); };
            ruleThickness.ValueChanged += (s, e) => { cfg.RuleLineThickness = (int)ruleThickness.Value; RefreshAppearancePreview(); };
            marginThickness.ValueChanged += (s, e) => { cfg.MarginLineThickness = (int)marginThickness.Value; RefreshAppearancePreview(); };
            cornerRadius.ValueChanged += (s, e) => { cfg.CardCornerRadius = cornerRadius.Value; UpdateCornerRadiusLabel(); RefreshAppearancePreview(); };
            textElement.SelectedIndexChanged += (s, e) => LoadTextOptions(); textBold.CheckedChanged += (s, e) => SaveTextOptions(); textItalic.CheckedChanged += (s, e) => SaveTextOptions(); textUnderline.CheckedChanged += (s, e) => SaveTextOptions();
            appearancePreview.DateStampElementMoved += (element, offset) => cfg.SetDateStampOffset(element, offset.X, offset.Y);
            tone.SelectedIndexChanged += (s, e) => { cfg.EmptyDayTone = (EmptyDayTone)Enum.Parse(typeof(EmptyDayTone), tone.SelectedItem.ToString()); emptyMessage.Enabled = cfg.EmptyDayTone == EmptyDayTone.Custom; pickAnother.Enabled = cfg.EmptyDayTone == EmptyDayTone.Playful; UpdateEmptyMessagePreview(); };
            emptyMessage.TextChanged += (s, e) => { cfg.EmptyDayMessage = emptyMessage.Text.Trim(); UpdateEmptyMessagePreview(); }; pickAnother.Click += (s, e) => { WidgetForm.PickAnotherPlayfulMessage(); UpdateEmptyMessagePreview(); };
            settingsFolder.Click += (s, e) => OpenFolder(Paths.ConfigDir); logFolder.Click += (s, e) => OpenFolder(Paths.DataDir);
            width.Enabled = !autoWidth.Checked; amber.Enabled = lateAlert.Checked; opacity.Enabled = !noStripBackground.Checked; emptyMessage.Enabled = cfg.EmptyDayTone == EmptyDayTone.Custom; pickAnother.Enabled = cfg.EmptyDayTone == EmptyDayTone.Playful;
            UpdateEmptyMessagePreview(); RefreshAppearancePreview();
        }

        static void SetValue(NumericUpDown spin, int value) { spin.Value = Math.Max(spin.Minimum, Math.Min(spin.Maximum, value)); }
        void UpdateCornerRadiusLabel() { cornerRadiusValue.Text = cornerRadius.Value + " px"; }
        static void AddActions(ComboBox box, bool allowOutlook) { box.Items.Add("Show details card"); if (allowOutlook) box.Items.Add("Open Outlook calendar"); box.Items.Add("Do nothing"); }
        static string ActionLabel(WidgetAction action) { return action == WidgetAction.Details ? "Show details card" : action == WidgetAction.Outlook ? "Open Outlook calendar" : "Do nothing"; }
        static WidgetAction ActionValue(string label) { return label == "Show details card" ? WidgetAction.Details : label == "Open Outlook calendar" ? WidgetAction.Outlook : WidgetAction.Nothing; }
        static void SelectAction(ComboBox box, WidgetAction action) { box.SelectedItem = ActionLabel(action); if (box.SelectedIndex < 0) box.SelectedIndex = 0; }

        static void SetButtonIcon(Button button, Image image)
        {
            button.Image = image; button.ImageAlign = ContentAlignment.MiddleLeft; button.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        void ConfigureColourButton(Button button, string caption, Func<string> read, Action<string> write)
        {
            SetButtonIcon(button, LegacyIcons.PaintCanImage);
            Action refresh = () => button.Text = read();
            button.Click += (s, e) => PickColour(read, write, refresh); colourRefreshers.Add(refresh); refresh();
        }

        void UpdateEmptyMessagePreview() { emptyPreview.Text = "Current widget message: " + WidgetForm.EmptyMessage(cfg); }

        void PickColour(Func<string> read, Action<string> write, Action refresh)
        {
            using (var dialog = new ColorDialog { Color = ReadColor(read()), FullOpen = true })
            if (dialog.ShowDialog(this) == DialogResult.OK) { write(ColorTranslator.ToHtml(dialog.Color)); refresh(); RefreshAppearancePreview(); }
        }

        TextElement SelectedTextElement => (TextElement)textElement.SelectedIndex;

        void LoadTextOptions()
        {
            if (textElement == null || textElement.SelectedIndex < 0) return;
            updatingTextOptions = true; var style = cfg.TextStyle(SelectedTextElement); textBold.Checked = style.HasFlag(FontStyle.Bold); textItalic.Checked = style.HasFlag(FontStyle.Italic); textUnderline.Checked = style.HasFlag(FontStyle.Underline); textColour.Text = cfg.TextColor(SelectedTextElement); fontPicker.Text = cfg.TextFontFamily(SelectedTextElement) + ", " + (cfg.TextFontTenths(SelectedTextElement) / 10f).ToString("0.#") + " pt"; updatingTextOptions = false;
        }

        void SaveTextOptions()
        {
            if (updatingTextOptions || textElement == null || textElement.SelectedIndex < 0) return;
            cfg.SetTextStyle(SelectedTextElement, textBold.Checked, textItalic.Checked, textUnderline.Checked); RefreshAppearancePreview();
        }

        void PickTextColour()
        {
            var selected = SelectedTextElement; PickColour(() => cfg.TextColor(selected), value => cfg.SetTextColor(selected, value), () => textColour.Text = cfg.TextColor(selected));
        }

        void PickFont()
        {
            var selected = SelectedTextElement; var family = FontFamily.Families.FirstOrDefault(item => string.Equals(item.Name, cfg.TextFontFamily(selected), StringComparison.OrdinalIgnoreCase)) ?? SystemFonts.MessageBoxFont.FontFamily;
            using (var dialog = new FontDialog { Font = new Font(family, cfg.TextFontTenths(selected) / 10f, cfg.TextStyle(selected)), ShowColor = false, ShowEffects = false })
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                cfg.SetTextFont(selected, dialog.Font.FontFamily.Name, dialog.Font.SizeInPoints);
                cfg.SetTextStyle(selected, dialog.Font.Bold, dialog.Font.Italic, dialog.Font.Underline);
                LoadTextOptions(); RefreshAppearancePreview();
            }
        }

        void RefreshAppearancePreview()
        {
            if (appearancePreview == null) return;
            appearancePreview.SetPreview(cfg, previewMeetings(), previewPrivate());
            foreach (var refresh in colourRefreshers) refresh();
            if (textElement != null && textElement.SelectedIndex >= 0) textColour.Text = cfg.TextColor(SelectedTextElement);
        }

        void TestOutlook()
        {
            testResult.Text = "Checking..."; var days = (int)lookahead.Value;
            Task.Run(() => { try { return (ok: true, count: OutlookCalendar.Fetch(days).Count); } catch { return (ok: false, count: 0); } })
                .ContinueWith(t => BeginInvoke(new Action(() => testResult.Text = t.Result.ok ? $"Connected. {t.Result.count} meetings found." : "Could not read classic Outlook.")));
        }

        static Color ReadColor(string value) { try { return ColorTranslator.FromHtml(value); } catch { return Color.White; } }
        static void OpenFolder(string path) { Directory.CreateDirectory(path); System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); }

        void Save(bool closeAfter)
        {
            cfg.LookaheadDays = (int)lookahead.Value; cfg.RefreshMinutes = (int)refresh.Value; cfg.Width = (int)width.Value; cfg.AutoWidth = autoWidth.Checked; cfg.RightGap = (int)gap.Value;
            cfg.Layout = (StripLayout)Enum.Parse(typeof(StripLayout), layout.SelectedItem.ToString()); cfg.Use24HourClock = twentyFourHour.Checked; cfg.AmberThresholdMinutes = (int)amber.Value; cfg.LateAlertEnabled = lateAlert.Checked; cfg.SkipCancelledMeetings = skipCancelledMeetings.Checked;
            cfg.FadeDelayMilliseconds = (int)fadeDelay.Value; cfg.FadeDurationMilliseconds = (int)fadeDuration.Value; cfg.AppearStartPercent = (int)appearStart.Value; cfg.AppearDurationMilliseconds = (int)appearDuration.Value;
            cfg.ClickAction = ActionValue(clickAction.SelectedItem.ToString()); cfg.DoubleClickAction = ActionValue(doubleClickAction.SelectedItem.ToString()); cfg.HoverAction = ActionValue(hoverAction.SelectedItem.ToString());
            cfg.EmptyDayTone = (EmptyDayTone)Enum.Parse(typeof(EmptyDayTone), tone.SelectedItem.ToString()); cfg.EmptyDayMessage = emptyMessage.Text.Trim();
            cfg.ThemePreset = preset.SelectedItem.ToString(); cfg.BackgroundOpacity = (int)opacity.Value; cfg.NoStripBackground = noStripBackground.Checked; cfg.AutoTaskbarTextColors = autoTaskbarTextColors.Checked; cfg.CardShadow = shadow.Checked; cfg.ShowCallout = callout.Checked;
            cfg.RuleLineThickness = (int)ruleThickness.Value; cfg.MarginLineThickness = (int)marginThickness.Value; cfg.DoubleMarginLine = doubleMargin.Checked; cfg.CardCornerRadius = cornerRadius.Value;
            cfg.TeamsPrivacyEnabled = automaticPrivacy.Checked; cfg.PrivacyDisplay = (PrivacyDisplay)Enum.Parse(typeof(PrivacyDisplay), privacyDisplay.SelectedItem.ToString());
            if (!demo) cfg.Save();
            try { if (!demo && startup.Checked != Installer.StartupEnabled) Installer.SetStartup(startup.Checked); } catch (Exception ex) { Log.Error("Startup shortcut", ex); }
            onSaved?.Invoke(cfg.Clone());
            if (closeAfter) Close();
        }

        private void appearancePage_Click(object sender, EventArgs e)
        {

        }

        private void appearancePreview_Click(object sender, EventArgs e)
        {

        }

        private void l20_Click(object sender, EventArgs e)
        {

        }
    }
}
