using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TaskbarCalendar
{
    sealed class PeekForm : Form
    {
        public enum Kind { Heading, Row, Note }
        public sealed class Row
        {
            public Kind Kind; public string Left, Right; public Color LeftColor, RightColor; public ColorState? TimeState; public MeetingIcon Icon; public int GapAbove;
            // A per-row action is stored with the rendered row so concurrent
            // meetings always open their own invitation link.
            public string JoinLink;
            // Secondary information belonging to a meeting aligns beneath its title,
            // never beneath the time column.
            public bool IndentToTitle;
            public bool IsPreviousToggle;
            public bool IsPreviousMeeting;
        }

        readonly Font headingFont, timeFont, titleFont, noteFont;
        List<Row> rows = new List<Row>();
        // During a collapse we keep the expanded rows long enough to fold them
        // away beneath the binder, then replace them with the collapsed list.
        List<Row> rowsAfterResize;
        WidgetTheme theme;
        Config config;
        float scale = 1;
        PeekCardRenderer renderer;
        readonly Timer appearTimer = new Timer { Interval = 15 };
        readonly Timer resizeTimer = new Timer { Interval = 15 };
        Rectangle appearStartBounds, appearTargetBounds;
        Rectangle resizeStartBounds, resizeTargetBounds, previousToggleBounds;
        sealed class JoinTarget { public int RowIndex; public Rectangle RowBounds, IconBounds; public string Link; }
        readonly List<JoinTarget> joinTargets = new List<JoinTarget>();
        int hoveredJoinRow = -1;
        int appearStep, appearSteps, resizeStep, resizeSteps;
        bool appearing, resizing;
        float previousExpansion = 1, previousExpansionStart = 1, previousExpansionEnd = 1;
        Bitmap layeredFrame;
        int shadowLeft, shadowTop, shadowRight, shadowBottom, calloutHeight;
        double visualOpacity = 1;
        public bool IsAppearing => appearing || resizing;
        public event EventHandler PointerEntered;
        public event EventHandler PointerLeft;
        public event EventHandler PreviousMeetingsToggled;

        public PeekForm(Font headingFont, Font timeFont, Font titleFont, Font noteFont)
        {
            this.headingFont = headingFont; this.timeFont = timeFont; this.titleFont = titleFont; this.noteFont = noteFont;
            FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; TopMost = true; StartPosition = FormStartPosition.Manual;
            DoubleBuffered = true; ResizeRedraw = true; AutoScaleMode = AutoScaleMode.None;
            appearTimer.Tick += (s, e) => AppearTick(); resizeTimer.Tick += (s, e) => ResizeTick();
        }

        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams { get { var cp = base.CreateParams; cp.ExStyle |= WinApi.WS_EX_TOOLWINDOW | WinApi.WS_EX_LAYERED | WinApi.WS_EX_NOACTIVATE | WinApi.WS_EX_TOPMOST; return cp; } }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); PointerEntered?.Invoke(this, EventArgs.Empty); }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoveredJoinRow >= 0) { hoveredJoinRow = -1; if (Visible) RenderLayered(); }
            if (!IsAppearing) PointerLeft?.Invoke(this, EventArgs.Empty);
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left)
            {
                var join = joinTargets.FirstOrDefault(target => target.IconBounds.Contains(e.Location));
                if (join != null)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(join.Link) { UseShellExecute = true });
                    return;
                }
            }
            if (e.Button == MouseButtons.Left && !previousToggleBounds.IsEmpty && previousToggleBounds.Contains(e.Location)) PreviousMeetingsToggled?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var target = joinTargets.FirstOrDefault(candidate => candidate.RowBounds.Contains(e.Location));
            int rowIndex = target == null ? -1 : target.RowIndex;
            if (hoveredJoinRow != rowIndex)
            {
                hoveredJoinRow = rowIndex;
                RenderLayered();
            }
            Cursor = joinTargets.Any(candidate => candidate.RowIndex == hoveredJoinRow && candidate.IconBounds.Contains(e.Location)) ? Cursors.Hand : Cursors.Default;
        }

        public static List<Row> BuildRows(IEnumerable<Meeting> allMeetings, DateTime now, Config cfg, bool privateMode, bool showPreviousToday = false)
        {
            var rows = new List<Row>(); var previous = allMeetings.Where(m => m.End <= now && m.Start.Date == now.Date).OrderBy(m => m.Start).Take(8).ToList(); DateTime? day = null;
            if (!privateMode && previous.Count > 0)
            {
                rows.Add(new Row { Kind = Kind.Note, Left = (showPreviousToday ? "▼" : "▶") + " Earlier today (" + previous.Count + ")", LeftColor = WidgetForm.Grey, IsPreviousToggle = true });
                if (showPreviousToday)
                    foreach (var meeting in previous) AddMeetingRows(rows, meeting, now, cfg, privateMode, true);
            }
            foreach (var meeting in allMeetings.Where(m => m.End > now).OrderBy(m => m.Start).Take(8))
            {
                if (meeting.Start.Date != day)
                {
                    var offset = (meeting.Start.Date - now.Date).Days;
                    rows.Add(new Row { Kind = Kind.Heading, Left = offset == 0 ? "Today" : offset == 1 ? "Tomorrow" : meeting.Start.ToString("dddd, MMM dd"), LeftColor = WidgetForm.Grey });
                    day = meeting.Start.Date;
                }
                AddMeetingRows(rows, meeting, now, cfg, privateMode);
            }
            if (rows.Count == 0) rows.Add(new Row { Kind = Kind.Note, Left = WidgetForm.EmptyMessage(cfg), LeftColor = WidgetForm.Dim });
            return rows;
        }

        static void AddMeetingRows(List<Row> rows, Meeting meeting, DateTime now, Config cfg, bool privateMode, bool isPreviousMeeting = false)
        {
            var title = privateMode ? "Busy" : meeting.Title;
            // Past, cancelled, masked, and linkless meetings never present a
            // misleading join affordance. Current and upcoming rows retain
            // the exact invitation URL Outlook supplied.
            var joinLink = !privateMode && !isPreviousMeeting && !CalendarFeed.IsCancelled(meeting) && !string.IsNullOrWhiteSpace(meeting.Link) ? meeting.Link : null;
            // The paper card is a readable schedule.  Green and late-alert colours
            // belong on the compact taskbar strip, where they convey urgency.
            rows.Add(new Row { Kind = Kind.Row, Left = CalendarFeed.FmtRange(meeting, cfg.Use24HourClock), Right = title, Icon = privateMode ? MeetingIcon.None : LegacyIcons.For(meeting), RightColor = privateMode ? WidgetForm.Dim : Color.Empty, IsPreviousMeeting = isPreviousMeeting, JoinLink = joinLink });
            var room = privateMode ? null : CalendarFeed.RoomLocation(meeting);
            if (room != null) rows.Add(new Row { Kind = Kind.Note, Left = "Room: " + room, LeftColor = WidgetForm.Dim, IndentToTitle = true, IsPreviousMeeting = isPreviousMeeting });
        }

        int Px(float value) => PeekCardRenderer.Scaled(value, scale);
        public void ShowRows(List<Row> values, Rectangle anchor, float dpiScale, WidgetTheme newTheme, Config newConfig, bool animateResize = false)
        {
            var oldRows = rows;
            hoveredJoinRow = -1;
            scale = dpiScale; theme = newTheme; config = newConfig;
            renderer?.Dispose();
            renderer = new PeekCardRenderer(headingFont, timeFont, titleFont, noteFont, theme, config, scale);
            var size = renderer.Measure(values); int width = size.Width, height = size.Height;
            calloutHeight = config.ShowCallout ? Px(10) : 0;
            int shadow = theme.Shadow ? Math.Max(Px(8), 8) : 0;
            shadowLeft = shadow; shadowTop = Math.Max(0, shadow - Px(2)); shadowRight = shadow; shadowBottom = shadow + Px(2);
            var screen = Screen.FromRectangle(anchor).Bounds;
            var sheet = new Rectangle(Math.Max(screen.Left + Px(8), Math.Min(anchor.Right - width, screen.Right - width - Px(8))), anchor.Top - height - calloutHeight - Px(8), width, height + calloutHeight);
            var target = new Rectangle(sheet.X - shadowLeft, sheet.Y - shadowTop, sheet.Width + shadowLeft + shadowRight, sheet.Height + shadowTop + shadowBottom);
            if (Visible)
            {
                appearTimer.Stop(); appearing = false;
                if (animateResize && target.Height != Bounds.Height)
                {
                    // Keep the callout and the existing schedule rows anchored
                    // to the widget.  The card grows upward to make room for
                    // history, so expanding history never pushes the schedule
                    // down the screen.  Collapse is the exact reverse.
                    // On collapse, draw the existing history rows while their
                    // height contracts, then switch to the collapsed row list.
                    if (target.Height > Bounds.Height)
                    {
                        rows = values;
                        rowsAfterResize = null;
                        previousExpansion = previousExpansionStart = 0;
                        previousExpansionEnd = 1;
                    }
                    else
                    {
                        rows = oldRows;
                        rowsAfterResize = values;
                        previousExpansion = previousExpansionStart = 1;
                        previousExpansionEnd = 0;
                    }
                    BeginResize(target);
                }
                else { resizeTimer.Stop(); resizing = false; rows = values; rowsAfterResize = null; previousExpansion = previousExpansionStart = previousExpansionEnd = 1; Bounds = target; visualOpacity = 1; }
                RenderLayered(); WinApi.RaiseTopmost(Handle); return;
            }
            rows = values; rowsAfterResize = null;
            BeginAppear(target);
        }

        void BeginAppear(Rectangle target)
        {
            appearTimer.Stop(); previousExpansion = previousExpansionStart = previousExpansionEnd = 1; appearTargetBounds = target;
            int offset = (int)Math.Round(target.Height * (100 - config.AppearStartPercent) / 100d);
            appearStartBounds = new Rectangle(target.X, target.Y + offset, target.Width, target.Height);
            appearSteps = Math.Max(1, (int)Math.Ceiling(config.AppearDurationMilliseconds / 15d)); appearStep = 0;
            appearing = config.AppearDurationMilliseconds > 0;
            Bounds = appearing ? appearStartBounds : appearTargetBounds; visualOpacity = appearing ? 0.05 : 1; Show(); RenderLayered(); WinApi.RaiseTopmost(Handle);
            if (appearing) appearTimer.Start();
        }

        void AppearTick()
        {
            if (!Visible) { appearTimer.Stop(); appearing = false; return; }
            double linear = Math.Min(1, ++appearStep / (double)appearSteps), eased = 1 - (1 - linear) * (1 - linear);
            int y = (int)Math.Round(appearStartBounds.Y + (appearTargetBounds.Y - appearStartBounds.Y) * eased);
            Bounds = new Rectangle(appearTargetBounds.X, y, appearTargetBounds.Width, appearTargetBounds.Height); visualOpacity = 0.05 + 0.95 * eased; RenderLayered();
            if (linear < 1) return;
            appearTimer.Stop(); appearing = false; Bounds = appearTargetBounds; visualOpacity = 1; RenderLayered();
            if (Bounds.Contains(Cursor.Position)) PointerEntered?.Invoke(this, EventArgs.Empty); else PointerLeft?.Invoke(this, EventArgs.Empty);
        }

        void BeginResize(Rectangle target)
        {
            resizeTimer.Stop(); resizeStartBounds = Bounds; resizeTargetBounds = target;
            resizeSteps = Math.Max(1, (int)Math.Ceiling(Math.Min(360, Math.Max(120, config.AppearDurationMilliseconds)) / 15d)); resizeStep = 0; resizing = true; resizeTimer.Start();
        }

        void ResizeTick()
        {
            if (!Visible) { resizeTimer.Stop(); resizing = false; return; }
            double linear = Math.Min(1, ++resizeStep / (double)resizeSteps), eased = 1 - (1 - linear) * (1 - linear);
            previousExpansion = (float)(previousExpansionStart + (previousExpansionEnd - previousExpansionStart) * eased);
            Bounds = new Rectangle(
                (int)Math.Round(resizeStartBounds.X + (resizeTargetBounds.X - resizeStartBounds.X) * eased),
                (int)Math.Round(resizeStartBounds.Y + (resizeTargetBounds.Y - resizeStartBounds.Y) * eased),
                (int)Math.Round(resizeStartBounds.Width + (resizeTargetBounds.Width - resizeStartBounds.Width) * eased),
                (int)Math.Round(resizeStartBounds.Height + (resizeTargetBounds.Height - resizeStartBounds.Height) * eased));
            RenderLayered();
            if (linear < 1) return;
            resizeTimer.Stop(); resizing = false; Bounds = resizeTargetBounds;
            if (rowsAfterResize != null) { rows = rowsAfterResize; rowsAfterResize = null; }
            previousExpansion = previousExpansionStart = previousExpansionEnd = 1;
            RenderLayered();
        }

        public void StopAppearance()
        {
            appearTimer.Stop(); resizeTimer.Stop(); appearing = false; resizing = false; rowsAfterResize = null; previousExpansion = previousExpansionStart = previousExpansionEnd = 1;
        }

        public void SetVisualOpacity(double value)
        {
            visualOpacity = Math.Max(0, Math.Min(1, value));
            if (Visible) RenderLayered();
        }

        void RenderLayered()
        {
            if (!IsHandleCreated || renderer == null || Width < 2 || Height < 2) return;
            if (layeredFrame == null || layeredFrame.Width != Width || layeredFrame.Height != Height)
            {
                layeredFrame?.Dispose();
                layeredFrame = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            }
            // Rectangle.Right/Bottom are exclusive.  Do not subtract an extra
            // pixel here: that makes the frame asymmetric and leaves one side of
            // the layered bitmap to show only the shadow.
            var card = new Rectangle(shadowLeft, shadowTop, Math.Max(1, Width - shadowLeft - shadowRight), Math.Max(1, Height - shadowTop - shadowBottom - calloutHeight));
            int calloutX = Math.Max(card.Left + Px(16), Math.Min(card.Right - Px(16), card.Right - Px(25)));
            int calloutTipY = card.Bottom + calloutHeight;
            using (var graphics = Graphics.FromImage(layeredFrame))
            using (var silhouette = renderer.CreateSilhouettePath(card, calloutTipY, calloutX))
            {
                graphics.Clear(Color.Transparent);
                renderer.DrawOuterShadow(graphics, silhouette);
                previousToggleBounds = Rectangle.Empty;
                joinTargets.Clear();
                renderer.Draw(graphics, card, rows, calloutTipY, calloutX, bounds => previousToggleBounds = bounds, previousExpansion, null,
                    (rowIndex, rowBounds, iconBounds, link) => joinTargets.Add(new JoinTarget { RowIndex = rowIndex, RowBounds = rowBounds, IconBounds = iconBounds, Link = link }), hoveredJoinRow);
            }
            WinApi.SetLayeredBitmap(Handle, layeredFrame, Location, (byte)Math.Round(255 * visualOpacity));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // UpdateLayeredWindow owns the pixels.  Normal WinForms painting would
            // briefly expose the form's rectangular background during animation.
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { layeredFrame?.Dispose(); renderer?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
