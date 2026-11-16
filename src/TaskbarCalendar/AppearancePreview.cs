using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TaskbarCalendar
{
    sealed class AppearancePreview : Control
    {
        Config config = new Config();
        IReadOnlyList<Meeting> meetings = Array.Empty<Meeting>();
        bool privateMode;
        PeekCardRenderer.DateStampLayout dateStampLayout;
        TextElement draggingDateElement = TextElement.CardNote;
        Point dateDragStart, dateOffsetStart;
        public event Action<TextElement, Point> DateStampElementMoved;

        public AppearancePreview()
        {
            Height = 140; Width = 500; Margin = new Padding(0, 4, 0, 4);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        public void SetPreview(Config settings, IEnumerable<Meeting> calendarMeetings, bool isPrivate)
        {
            config = settings.Clone(); meetings = (calendarMeetings ?? Enumerable.Empty<Meeting>()).ToList(); privateMode = isPrivate; Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || dateStampLayout == null) return;
            var hit = dateStampLayout.HitTest(e.Location);
            if (hit != TextElement.CardDateNumber && hit != TextElement.CardDateMonth && hit != TextElement.CardDateDay) return;
            draggingDateElement = hit; dateDragStart = e.Location; dateOffsetStart = config.DateStampOffset(hit);
            Capture = true; Cursor = Cursors.SizeAll;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Capture && (draggingDateElement == TextElement.CardDateNumber || draggingDateElement == TextElement.CardDateMonth || draggingDateElement == TextElement.CardDateDay))
            {
                var offset = new Point(dateOffsetStart.X + e.X - dateDragStart.X, dateOffsetStart.Y + e.Y - dateDragStart.Y);
                config.SetDateStampOffset(draggingDateElement, offset.X, offset.Y);
                DateStampElementMoved?.Invoke(draggingDateElement, config.DateStampOffset(draggingDateElement));
                Invalidate();
                return;
            }
            Cursor = dateStampLayout != null && (dateStampLayout.Number.Contains(e.Location) || dateStampLayout.Month.Contains(e.Location) || dateStampLayout.Day.Contains(e.Location)) ? Cursors.SizeAll : Cursors.Default;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && Capture) { Capture = false; draggingDateElement = TextElement.CardNote; Cursor = Cursors.Default; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.Clear(SystemColors.Control); var theme = Theme.From(config); var now = DateTime.Now;
            using (var top = PreviewFont(TextElement.TaskbarTop))
            using (var bottom = PreviewFont(TextElement.TaskbarBottom))
            using (var heading = PreviewFont(TextElement.CardHeading))
            using (var time = PreviewFont(TextElement.CardTime))
            using (var title = PreviewFont(TextElement.CardTitle))
            using (var note = PreviewFont(TextElement.CardNote))
            {
                var stripLine = Math.Max(PeekCardRenderer.TextHeight(top), PeekCardRenderer.TextHeight(bottom));
                var strip = new Rectangle(7, 7, Math.Max(120, Width - 14), Math.Max(28, 2 * stripLine + 4));
                var taskbarSurface = Color.FromArgb(38, 38, 38);
                using (var taskbarBrush = new SolidBrush(taskbarSurface)) g.FillRectangle(taskbarBrush, strip);
                if (!config.NoStripBackground)
                {
                    var alpha = Math.Max(0, Math.Min(255, config.BackgroundOpacity));
                    using (var stripBrush = new SolidBrush(Color.FromArgb(alpha, theme.StripBackground))) g.FillRectangle(stripBrush, strip);
                    using (var stripBorder = new Pen(ControlPaint.Dark(theme.StripBackground))) g.DrawRectangle(stripBorder, strip);
                    taskbarSurface = Blend(taskbarSurface, theme.StripBackground, alpha);
                }

                DrawWidgetStrip(g, strip, now, top, bottom, theme, TaskbarTextPalette.From(config, theme, taskbarSurface), stripLine);

                var callout = config.ShowCallout ? 10 : 0;
                // The preview is deliberately compact, but keep enough vertical room for
                // the first real room/location row rather than clipping it immediately.
                var cardTop = strip.Bottom;
                var card = new Rectangle(24, cardTop, Math.Max(190, Width - 48), Math.Max(0, Height - cardTop - callout - 1));
                var rows = PeekForm.BuildRows(meetings, now, config, privateMode);
                using (var renderer = new PeekCardRenderer(heading, time, title, note, theme, config, 1))
                {
                    using (var silhouette = renderer.CreateSilhouettePath(card, Height - 1, card.Right - 28)) renderer.DrawOuterShadow(g, silhouette);
                    dateStampLayout = null;
                    renderer.Draw(g, card, rows, Height - 1, card.Right - 28, null, 1, layout => dateStampLayout = layout);
                }
            }
        }

        void DrawWidgetStrip(Graphics g, Rectangle strip, DateTime now, Font top, Font bottom, WidgetTheme theme, TaskbarTextPalette palette, int line)
        {
            var current = CalendarFeed.PickToday(meetings, now, config.SkipCancelledMeetings);
            string topText, bottomText; Color bottomColor; MeetingIcon icon;
            if (privateMode && config.PrivacyDisplay == PrivacyDisplay.Busy)
            {
                var range = current == null ? "" : CalendarFeed.FmtRange(current, config.Use24HourClock);
                topText = config.Layout == StripLayout.TimeFirst ? range : "Busy"; bottomText = config.Layout == StripLayout.TimeFirst ? "Busy" : range; bottomColor = WidgetForm.Dim; icon = MeetingIcon.None;
            }
            else if (current == null)
            {
                topText = WidgetForm.EmptyMessage(config); bottomText = "No more meetings today"; bottomColor = WidgetForm.Dim; icon = MeetingIcon.None;
            }
            else
            {
                var range = CalendarFeed.FmtRange(current, config.Use24HourClock); var state = CalendarFeed.StartState(current, now, config.AmberThresholdMinutes, config.LateAlertEnabled);
                bottomColor = state == ColorState.Green ? palette.Green : state == ColorState.Amber ? palette.Amber : palette.Secondary;
                if (config.Layout == StripLayout.Private) { topText = range; bottomText = "Calendar active"; icon = MeetingIcon.None; }
                else if (config.Layout == StripLayout.TimeFirst) { topText = range; bottomText = current.Title; icon = LegacyIcons.For(current); }
                else { topText = current.Title; bottomText = range; icon = LegacyIcons.For(current); }
            }

            const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.Right;
            var stripTop = strip.Top + (strip.Height - 2 * line) / 2;
            var image = LegacyIcons.ImageFor(icon); const int iconSize = 18; var iconWidth = image == null ? 0 : iconSize + 5;
            if (image != null) g.DrawImage(image, new Rectangle(strip.Left + 8, stripTop + Math.Max(0, (2 * line - iconSize) / 2), iconSize, iconSize));
            TextRenderer.DrawText(g, topText, top, new Rectangle(strip.Left + 8 + iconWidth, stripTop, strip.Width - 16 - iconWidth, line), palette.Primary, flags);
            TextRenderer.DrawText(g, bottomText, bottom, new Rectangle(strip.Left + 8, stripTop + line, strip.Width - 16, line), bottomColor, flags);
        }

        static Color Blend(Color backdrop, Color foreground, int alpha)
        {
            var inverse = 255 - alpha;
            return Color.FromArgb((foreground.R * alpha + backdrop.R * inverse) / 255, (foreground.G * alpha + backdrop.G * inverse) / 255, (foreground.B * alpha + backdrop.B * inverse) / 255);
        }

        Font PreviewFont(TextElement element)
        {
            var family = FontFamily.Families.FirstOrDefault(item => string.Equals(item.Name, config.TextFontFamily(element), StringComparison.OrdinalIgnoreCase));
            return new Font(family ?? SystemFonts.MessageBoxFont.FontFamily, config.TextFontTenths(element) / 10f, config.TextStyle(element));
        }
    }
}
