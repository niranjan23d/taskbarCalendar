using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace TaskbarCalendar
{
    sealed class WidgetForm : Form
    {
        public static readonly Color Grey = Color.FromArgb(118, 118, 118), Dim = Color.FromArgb(180, 180, 180), Green = Color.FromArgb(131, 230, 156), Amber = Color.FromArgb(255, 211, 106);
        static readonly string[] PlayfulMessages =
        {
            "No meetings. The calendar has left the chat.",
            "The tiny rectangles have been defeated.",
            "Schedule empty. Please enjoy this side quest.",
            "No calls. Your headphones may rest.",
            "Plot twist: the rest of the day is yours.",
            "Meeting-free. Pretend you planned it.",
            "No one can say 'quick sync' for a while.",
            "The calendar is clear. Suspiciously clear.",
            "This is not a drill. You have time.",
            "No invites remain. Excellent work, probably.",
            "Free-range human hours unlocked.",
            "The next meeting is currently a rumour.",
            "Your calendar has achieved inner peace.",
            "No calls. Hydration arc unlocked.",
            "The meeting gods have granted a quiet afternoon.",
            "You are free. Do not make eye contact with Outlook.",
        };
        static readonly Random PlayfulRandom = new Random();
        static int playfulMessageIndex = PlayfulRandom.Next(PlayfulMessages.Length);
        static string playfulMessage = PlayfulMessages[playfulMessageIndex];
        readonly bool demo, openSettingsOnStart;
        readonly ITeamsShareDetector teamsDetector;
        Config cfg;
        List<Meeting> meetings = new List<Meeting>();
        Meeting current;
        string calendarError;
        bool calendarLoaded, stripHover, cardHover, shown, menuOpen, privacyCheckRunning;
        TeamsShareState teamsState = TeamsShareState.NotPresenting;
        PeekForm peek;
        SettingsForm settings;
        ShellWatcher shell;
        Font topFont, bottomFont, headingFont, timeFont, titleFont, noteFont;
        readonly Timer minuteTimer = new Timer(), refreshTimer = new Timer(), privacyTimer = new Timer { Interval = 1000 }, leaveTimer = new Timer(), fadeTimer = new Timer { Interval = 30 }, layoutTimer = new Timer { Interval = 30 }, clickTimer = new Timer(), hoverTimer = new Timer { Interval = 40 };
        int fadeTicks;
        bool pointerInHoverRegion, detailsShownForHover, showPreviousToday;
        Color stripSurface;

        public WidgetForm(bool demo, Action restartApp, bool openSettings, ITeamsShareDetector detector = null, bool demoPrivacy = false)
        {
            this.demo = demo; openSettingsOnStart = openSettings; teamsDetector = detector ?? new TeamsShareDetector(); cfg = demo ? Demo.ScreenshotConfig(demoPrivacy) : Config.Load();
            Text = "Taskbar Calendar"; Icon = Program.AppIcon; FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; TopMost = true; StartPosition = FormStartPosition.Manual; DoubleBuffered = true; Cursor = Cursors.Hand; AutoScaleMode = AutoScaleMode.None;
            ResetFonts();
            minuteTimer.Tick += (s, e) => Render(); refreshTimer.Tick += (s, e) => RefreshCalendar(); privacyTimer.Tick += (s, e) => RefreshPrivacy();
            leaveTimer.Tick += (s, e) => { leaveTimer.Stop(); BeginFadeIfAway(); }; fadeTimer.Tick += (s, e) => FadeTick(); layoutTimer.Tick += (s, e) => { layoutTimer.Stop(); ApplyLayout(); };
            clickTimer.Tick += (s, e) => { clickTimer.Stop(); RunAction(cfg.ClickAction); }; hoverTimer.Tick += (s, e) => UpdateHoverRegion();
        }

        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams { get { var cp = base.CreateParams; cp.ExStyle |= WinApi.WS_EX_TOOLWINDOW | WinApi.WS_EX_NOACTIVATE | WinApi.WS_EX_TOPMOST; return cp; } }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); WinApi.RoundCorners(Handle); }

        public void Start()
        {
            var unused = Handle;
            if (openSettingsOnStart) BeginInvoke(new Action(() => OpenSettings(Config.IsFirstRun)));
            shell = new ShellWatcher(); shell.Event += name => BeginInvoke(new Action(() => OnShellEvent(name)));
            refreshTimer.Interval = cfg.RefreshMinutes * 60_000; minuteTimer.Interval = 30_000;
            refreshTimer.Start(); minuteTimer.Start(); privacyTimer.Start(); ApplyLayout(); RefreshCalendar(); RefreshPrivacy();
        }

        void ResetFonts()
        {
            topFont?.Dispose(); bottomFont?.Dispose(); headingFont?.Dispose(); timeFont?.Dispose(); titleFont?.Dispose(); noteFont?.Dispose();
            topFont = CreateTextFont(TextElement.TaskbarTop); bottomFont = CreateTextFont(TextElement.TaskbarBottom);
            headingFont = CreateTextFont(TextElement.CardHeading); timeFont = CreateTextFont(TextElement.CardTime);
            titleFont = CreateTextFont(TextElement.CardTitle); noteFont = CreateTextFont(TextElement.CardNote);
        }
        Font CreateTextFont(TextElement element)
        {
            var family = cfg.TextFontFamily(element); if (!FontFamily.Families.Any(f => string.Equals(f.Name, family, StringComparison.OrdinalIgnoreCase))) family = "Segoe UI";
            return new Font(family, cfg.TextFontTenths(element) / 10f, cfg.TextStyle(element));
        }
        float S => DeviceDpi / 96f; int Px(float value) => (int)Math.Round(value * S); int LineHeight => Math.Max(TextRenderer.MeasureText("Ag", topFont, Size.Empty, TextFormatFlags.NoPadding).Height, TextRenderer.MeasureText("Ag", bottomFont, Size.Empty, TextFormatFlags.NoPadding).Height);
        int Measure(string text) => TextRenderer.MeasureText(text, topFont, Size.Empty, TextFormatFlags.NoPadding).Width;
        WidgetTheme CurrentTheme => Theme.From(cfg);
        bool IsPrivate => PrivacyState.IsPrivate(cfg.ManualPrivacy, cfg.TeamsPrivacyEnabled, teamsState);
        bool IsHidden => IsPrivate && cfg.PrivacyDisplay == PrivacyDisplay.Hidden;

        void RefreshCalendar()
        {
            if (!cfg.ShowCalendar) return;
            Task.Run(() =>
            {
                try { return (items: demo ? Demo.Meetings(DateTime.Now) : CalendarService.Fetch(cfg), error: (string)null); }
                catch { return (items: new List<Meeting>(), error: "Outlook calendar unavailable"); }
            }).ContinueWith(task => Post(() => { meetings = task.Result.items; calendarError = task.Result.error; calendarLoaded = task.Result.error == null; Render(); }));
        }
        void Post(Action action) { try { if (IsHandleCreated && !IsDisposed) BeginInvoke(action); } catch (InvalidOperationException) { } }

        void RefreshPrivacy()
        {
            if (!cfg.TeamsPrivacyEnabled || privacyCheckRunning) return;
            privacyCheckRunning = true;
            Task.Run(() => teamsDetector.Detect()).ContinueWith(task => Post(() => { privacyCheckRunning = false; var next = task.Status == TaskStatus.RanToCompletion ? task.Result : TeamsShareState.Unknown; if (teamsState != next) { teamsState = next; ApplyLayout(); Render(); } }));
        }

        void Render()
        {
            current = CalendarFeed.PickToday(meetings, DateTime.Now, cfg.SkipCancelledMeetings); if (cfg.AutoWidth) ApplyLayout(); Invalidate();
            if (peek != null && peek.Visible && !IsHidden) ShowPeek();
        }

        static string TrimMessage(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        public static string EmptyMessage(Config cfg)
        {
            if (cfg.EmptyDayTone == EmptyDayTone.Minimal) return "No more meetings";
            if (cfg.EmptyDayTone == EmptyDayTone.Playful) return playfulMessage;
            if (cfg.EmptyDayTone == EmptyDayTone.Custom) return TrimMessage(cfg.EmptyDayMessage, "Done for the day");
            return "Done for the day";
        }
        public static string PickAnotherPlayfulMessage()
        {
            playfulMessageIndex = PlayfulRandom.Next(PlayfulMessages.Length - 1);
            if (playfulMessageIndex >= Array.IndexOf(PlayfulMessages, playfulMessage)) playfulMessageIndex++;
            return playfulMessage = PlayfulMessages[playfulMessageIndex];
        }
        public static bool IsPlayfulMessage(string value) => PlayfulMessages.Contains(value);
        public static Color ColorFor(ColorState state) => state == ColorState.Green ? Green : state == ColorState.Amber ? Amber : Dim;
        TaskbarTextPalette StripTextPalette => TaskbarTextPalette.From(cfg, CurrentTheme, stripSurface.IsEmpty ? CurrentTheme.StripBackground : stripSurface);

        (string Top, string Bottom, Color BottomColor, MeetingIcon Icon) CalendarText(DateTime now)
        {
            if (IsPrivate && cfg.PrivacyDisplay == PrivacyDisplay.Busy)
            {
                var range = current == null ? "" : CalendarFeed.FmtRange(current, cfg.Use24HourClock);
                return cfg.Layout == StripLayout.TimeFirst ? (range, "Busy", Dim, MeetingIcon.None) : ("Busy", range, Dim, MeetingIcon.None);
            }
            if (calendarError != null) return ("Classic Outlook unavailable", "Open Settings", Dim, MeetingIcon.None);
            if (!calendarLoaded) return ("Loading Outlook calendar", "", Dim, MeetingIcon.None);
            if (current == null) return (EmptyMessage(cfg), "No more meetings today", Dim, MeetingIcon.None);
            var rangeText = CalendarFeed.FmtRange(current, cfg.Use24HourClock); var state = TimeColor(CalendarFeed.StartState(current, now, cfg.AmberThresholdMinutes, cfg.LateAlertEnabled));
            if (cfg.Layout == StripLayout.Private) return (rangeText, "Calendar active", state, MeetingIcon.None);
            return cfg.Layout == StripLayout.TimeFirst ? (rangeText, current.Title, state, LegacyIcons.For(current)) : (current.Title, rangeText, state, LegacyIcons.For(current));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var theme = CurrentTheme; var palette = StripTextPalette; var g = e.Graphics; g.Clear(stripSurface.IsEmpty ? theme.StripBackground : stripSurface); var now = DateTime.Now; var text = CalendarText(now); var line = LineHeight; var top = (Height - 2 * line) / 2;
            const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.Right;
            var image = LegacyIcons.ImageFor(text.Icon); var iconSize = Math.Max(Px(16), Px(18)); var iconWidth = image == null ? 0 : iconSize + Px(5); if (image != null) g.DrawImage(image, new Rectangle(Px(8), top + Math.Max(0, (2 * line - iconSize) / 2), iconSize, iconSize));
            var bounds = new Rectangle(Px(8) + iconWidth, top, Width - Px(16) - iconWidth, line);
            var extra = ConcurrentSuffix(now); var extraWidth = Measure(extra);
            var first = bounds; first.Width = Math.Max(0, first.Width - extraWidth);
            TextRenderer.DrawText(g, text.Top, topFont, first, palette.Primary, flags);
            if (extraWidth > 0) TextRenderer.DrawText(g, extra, topFont, new Rectangle(bounds.Right - extraWidth, top, extraWidth, line), palette.Primary, flags);
            TextRenderer.DrawText(g, text.Bottom, bottomFont, new Rectangle(Px(8), top + line, Width - Px(16), line), text.BottomColor, flags);
        }

        string ConcurrentSuffix(DateTime now)
        {
            var active = cfg.SkipCancelledMeetings ? meetings.Where(m => !CalendarFeed.IsCancelled(m)) : meetings;
            var suffix = current != null ? CalendarFeed.ActiveMeetings(active, now).Count - 1 : 0;
            return suffix > 0 ? $" + {suffix} more" : "";
        }

        int RequiredWidth(DateTime now)
        {
            var text = CalendarText(now); var iconWidth = LegacyIcons.ImageFor(text.Icon) == null ? 0 : Math.Max(Px(16), Px(18)) + Px(5);
            var top = TextRenderer.MeasureText(text.Top + ConcurrentSuffix(now), topFont, Size.Empty, TextFormatFlags.NoPadding).Width;
            var bottom = TextRenderer.MeasureText(text.Bottom, bottomFont, Size.Empty, TextFormatFlags.NoPadding).Width;
            return Math.Max(Px(120), Math.Max(top, bottom) + iconWidth + Px(16));
        }

        Color TimeColor(ColorState state)
        {
            var palette = StripTextPalette;
            if (state == ColorState.Green) return palette.Green;
            if (state == ColorState.Amber) return palette.Amber;
            return palette.Secondary;
        }

        void ApplyLayout()
        {
            var taskbar = WinApi.Taskbar();
            if (!cfg.ShowCalendar || IsHidden || WinApi.ForegroundIsFullscreen()) { if (shown) { Hide(); shown = false; } HidePeek(); return; }
            if (taskbar == null) { if (shown) { Hide(); shown = false; } ScheduleLayout(200); return; }
            int width = cfg.AutoWidth ? Math.Min(taskbar.Bounds.Width - Px(16), RequiredWidth(DateTime.Now)) : Px(cfg.Width), height = Math.Max(Px(28), 2 * LineHeight + Px(4)); int limit = taskbar.Limit;
            int x = cfg.X ?? limit - Px(cfg.RightGap) - width; x = Math.Max(taskbar.Bounds.Left, Math.Min(x, limit - width)); int y = taskbar.Bounds.Top + (taskbar.Bounds.Height - height) / 2;
            ApplyStripSurface(x, y, width, height); shell?.Watch(taskbar.Hwnd, taskbar.Neighbours);
            Bounds = new Rectangle(x, y, width, height); if (!shown) { Show(); shown = true; } WinApi.RaiseTopmost(Handle); Invalidate();
        }
        void ApplyStripSurface(int x, int y, int width, int height)
        {
            if (cfg.NoStripBackground)
            {
                // A transparency key also removes those pixels from the native hit-test
                // region.  The result is a widget that only reacts over its glyphs.
                // Paint an exact taskbar-coloured surface instead, retaining a full-width
                // hover and click target without a visible strip background.
                var sampleX = x > 3 ? x - 3 : x + width + 3;
                stripSurface = WinApi.ScreenPixel(sampleX, y + Math.Max(1, height / 2)) ?? Color.FromArgb(32, 32, 32);
                TransparencyKey = Color.Empty; BackColor = stripSurface; Opacity = 1;
            }
            else
            {
                // Normal WinForms opacity fades the entire window, including text. Approximate
                // background-only alpha against a nearby taskbar pixel instead, so text stays crisp.
                var sampleX = x > 3 ? x - 3 : x + width + 3;
                var backdrop = WinApi.ScreenPixel(sampleX, y + Math.Max(1, height / 2)) ?? Color.FromArgb(32, 32, 32);
                stripSurface = Blend(backdrop, CurrentTheme.StripBackground, cfg.BackgroundOpacity);
                TransparencyKey = Color.Empty; BackColor = stripSurface; Opacity = 1;
            }
        }
        static Color Blend(Color backdrop, Color foreground, int alpha)
        {
            alpha = Math.Max(0, Math.Min(255, alpha)); int inverse = 255 - alpha;
            return Color.FromArgb((foreground.R * alpha + backdrop.R * inverse) / 255, (foreground.G * alpha + backdrop.G * inverse) / 255, (foreground.B * alpha + backdrop.B * inverse) / 255);
        }
        void OnShellEvent(string name)
        {
            if (name == "open-settings") OpenSettings();
            else if (name == "resume") { RefreshCalendar(); RefreshPrivacy(); }
            else ScheduleLayout();
        }
        void ScheduleLayout(int delay = 30) { layoutTimer.Stop(); layoutTimer.Interval = Math.Max(1, delay); layoutTimer.Start(); }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            bool newHoverSession = !detailsShownForHover;
            stripHover = true; pointerInHoverRegion = true; CancelFade();
            if (cfg.HoverAction == WidgetAction.Details)
            {
                // Moving the popup exposes the strip beneath it for a moment, which
                // can produce an extra MouseEnter. Do not restart a card already
                // animating for this same hover session.
                if (newHoverSession) ShowPeek(true);
            }
            else RunAction(cfg.HoverAction);
        }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); stripHover = false; UpdateHoverRegion(); }
        void ScheduleHide() { if (menuOpen) return; leaveTimer.Interval = Math.Max(1, cfg.FadeDelayMilliseconds); leaveTimer.Stop(); leaveTimer.Start(); }
        bool PointerInHoverRegion()
        {
            var point = Cursor.Position; stripHover = shown && Bounds.Contains(point); cardHover = peek != null && peek.Visible && peek.Bounds.Contains(point); return stripHover || cardHover;
        }
        void UpdateHoverRegion()
        {
            if (peek == null || !peek.Visible) { hoverTimer.Stop(); pointerInHoverRegion = false; detailsShownForHover = false; return; }
            if (PointerInHoverRegion()) { pointerInHoverRegion = true; CancelFade(); return; }
            if (!pointerInHoverRegion) return;
            pointerInHoverRegion = false; detailsShownForHover = false; ScheduleHide();
        }
        void BeginFadeIfAway()
        {
            if (PointerInHoverRegion() || menuOpen) return;
            if (peek != null && peek.IsAppearing) { leaveTimer.Interval = 30; leaveTimer.Start(); return; }
            if (cfg.FadeDurationMilliseconds == 0) { HidePeek(); return; }
            fadeTicks = Math.Max(1, (int)Math.Ceiling(cfg.FadeDurationMilliseconds / 30d)); fadeTimer.Start();
        }
        void FadeTick()
        {
            if (PointerInHoverRegion() || menuOpen) { CancelFade(); return; }
            if (peek == null || !peek.Visible || --fadeTicks <= 0) { HidePeek(); return; }
            if (peek.IsAppearing) { fadeTimer.Stop(); return; }
            peek.SetVisualOpacity(Math.Max(0.05, fadeTicks / Math.Max(1d, Math.Ceiling(cfg.FadeDurationMilliseconds / 30d))));
        }
        void CancelFade() { leaveTimer.Stop(); fadeTimer.Stop(); if (peek != null && peek.Visible && !peek.IsAppearing) peek.SetVisualOpacity(1); }

        void ShowPeek(bool replayAppearance = false, bool animatePreviousExpansion = false)
        {
            if (IsHidden || !shown) return;
            if (peek == null)
            {
                peek = new PeekForm(headingFont, timeFont, titleFont, noteFont);
                peek.PointerEntered += (s, e) => UpdateHoverRegion(); peek.PointerLeft += (s, e) => UpdateHoverRegion();
                peek.PreviousMeetingsToggled += (s, e) => { showPreviousToday = !showPreviousToday; ShowPeek(false, true); };
            }
            if (replayAppearance && peek.Visible) { peek.StopAppearance(); peek.Hide(); }
            peek.ShowRows(PeekForm.BuildRows(meetings, DateTime.Now, cfg, IsPrivate, showPreviousToday), Bounds, S, CurrentTheme, cfg, animatePreviousExpansion);
            hoverTimer.Start(); pointerInHoverRegion = true; detailsShownForHover = true; UpdateHoverRegion();
        }
        void HidePeek() { leaveTimer.Stop(); fadeTimer.Stop(); hoverTimer.Stop(); pointerInHoverRegion = false; detailsShownForHover = false; showPreviousToday = false; if (peek != null) { peek.StopAppearance(); peek.Hide(); } }

        void RunAction(WidgetAction action)
        {
            if (action == WidgetAction.Details) ShowPeek();
            else if (action == WidgetAction.Outlook && !demo && !IsPrivate) OpenOutlookCalendar();
        }
        void OpenOutlookCalendar()
        {
            try { OutlookCalendar.Open(); }
            catch (Exception ex)
            {
                Log.Warn($"Couldn't open classic Outlook calendar: {ex.Message}");
                MessageBox.Show(this, "Couldn't open the classic Outlook calendar.\r\n\r\nStart classic Outlook once and make sure its profile is signed in.", "Taskbar Calendar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Right) ShowMenu(PointToScreen(e.Location));
            else if (e.Button == MouseButtons.Left)
            {
                clickTimer.Stop(); clickTimer.Interval = Math.Max(1, SystemInformation.DoubleClickTime); clickTimer.Start();
            }
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Left) { clickTimer.Stop(); RunAction(cfg.DoubleClickAction); }
        }
        void ShowMenu(Point at)
        {
            var menu = new ContextMenuStrip(); menu.Opened += (s, e) => { menuOpen = true; CancelFade(); }; menu.Closed += (s, e) => { menuOpen = false; ScheduleHide(); };
            if (current?.Link != null && !demo && !IsPrivate) menu.Items.Add("Join video call", null, (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(current.Link) { UseShellExecute = true }));
            if (!demo) menu.Items.Add("Open Outlook Calendar", null, (s, e) => OpenOutlookCalendar());
            menu.Items.Add("Refresh calendar", null, (s, e) => RefreshCalendar()); menu.Items.Add(new ToolStripSeparator());
            var privacy = new ToolStripMenuItem("Privacy mode", null, (s, e) => { cfg.ManualPrivacy = !cfg.ManualPrivacy; if (!demo) cfg.Save(); ApplyLayout(); Render(); }) { Checked = cfg.ManualPrivacy, CheckOnClick = false };
            menu.Items.Add(privacy);
            menu.Items.Add($"Teams: {teamsState}", null, (s, e) => RefreshPrivacy()).Enabled = cfg.TeamsPrivacyEnabled;
            menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("Settings…", null, (s, e) => OpenSettings()); menu.Items.Add("Quit", null, (s, e) => Close()); menu.Show(at);
        }

        public void OpenSettings(bool firstRun = false)
        {
            try
            {
                if (settings != null && !settings.IsDisposed)
                {
                    if (settings.WindowState == FormWindowState.Minimized) settings.WindowState = FormWindowState.Normal;
                    if (!settings.Visible) settings.Show(this);
                    settings.BringToFront(); settings.Activate(); WinApi.SetForegroundWindow(settings.Handle);
                    return;
                }
                settings = new SettingsForm(cfg, firstRun, demo, updated => { cfg = updated; peek?.Close(); peek = null; ResetFonts(); refreshTimer.Interval = cfg.RefreshMinutes * 60_000; ApplyLayout(); Render(); }, () => meetings?.ToArray() ?? Array.Empty<Meeting>(), () => IsPrivate);
                settings.FormClosed += (s, e) => settings = null;
                // The widget itself is a topmost no-activate window. Giving the
                // normal settings form an owner keeps it visible and foregrounded.
                settings.Show(this); settings.BringToFront(); settings.Activate(); WinApi.SetForegroundWindow(settings.Handle);
            }
            catch (Exception ex)
            {
                settings?.Dispose(); settings = null;
                Log.Error("Couldn't open settings", ex);
                MessageBox.Show(this, "The settings window could not be opened. Check the app log for details.", "Taskbar Calendar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public IReadOnlyList<Meeting> UpcomingMeetings(DateTime now) => meetings.Where(m => m.End > now).OrderBy(m => m.Start).ToList();
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (var timer in new[] { minuteTimer, refreshTimer, privacyTimer, leaveTimer, fadeTimer, layoutTimer, clickTimer }) timer.Stop();
            shell?.Dispose(); peek?.Close(); topFont?.Dispose(); bottomFont?.Dispose(); headingFont?.Dispose(); timeFont?.Dispose(); titleFont?.Dispose(); noteFont?.Dispose(); base.OnFormClosed(e);
        }
    }
}
