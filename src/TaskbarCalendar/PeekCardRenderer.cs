using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace TaskbarCalendar
{
    // The detail-card form and the Settings preview deliberately share this renderer.
    // Keep all card geometry here so a font or theme change cannot make the preview lie.
    sealed class PeekCardRenderer : IDisposable
    {
        internal sealed class DateStampLayout
        {
            public Rectangle Number, Month, Day;
            public Rectangle Bounds
            {
                get
                {
                    var bounds = Rectangle.Union(Number, Month);
                    return Rectangle.Union(bounds, Day);
                }
            }
            public TextElement HitTest(Point location)
            {
                if (Number.Contains(location)) return TextElement.CardDateNumber;
                if (Month.Contains(location)) return TextElement.CardDateMonth;
                return Day.Contains(location) ? TextElement.CardDateDay : TextElement.CardNote;
            }
        }
        static readonly Bitmap LeatherTile = CreateLeatherTile();
        static readonly Bitmap PaperGrainTile = CreatePaperGrainTile();
        static readonly Random TornEdgeRandom = new Random();
        readonly Font headingFont, timeFont, titleFont, noteFont;
        readonly Font dateNumberFont, dateMonthFont, dateDayFont;
        readonly WidgetTheme theme;
        readonly Config config;
        readonly float scale;
        readonly int tornSeed;

        public PeekCardRenderer(Font headingFont, Font timeFont, Font titleFont, Font noteFont, WidgetTheme theme, Config config, float scale)
        {
            this.headingFont = headingFont; this.timeFont = timeFont; this.titleFont = titleFont; this.noteFont = noteFont;
            this.theme = theme; this.config = config; this.scale = scale;
            dateNumberFont = CreateConfiguredFont(TextElement.CardDateNumber);
            dateMonthFont = CreateConfiguredFont(TextElement.CardDateMonth);
            dateDayFont = CreateConfiguredFont(TextElement.CardDateDay);
            lock (TornEdgeRandom) tornSeed = TornEdgeRandom.Next();
        }

        Font CreateConfiguredFont(TextElement element)
        {
            var requested = config.TextFontFamily(element);
            FontFamily family = SystemFonts.MessageBoxFont.FontFamily;
            foreach (var candidate in FontFamily.Families)
                if (string.Equals(candidate.Name, requested, StringComparison.OrdinalIgnoreCase)) { family = candidate; break; }
            return new Font(family, config.TextFontTenths(element) / 10f, config.TextStyle(element));
        }

        int Px(float value) => Scaled(value, scale);
        Font FontFor(PeekForm.Kind kind) => kind == PeekForm.Kind.Heading ? headingFont : kind == PeekForm.Kind.Note ? noteFont : titleFont;
        // Date headings live on the same ruled-paper rhythm as meetings.  Their
        // smaller font should not make the Today/Monday rows visibly shorter.
        int PitchFor(PeekForm.Row row) => row.Kind == PeekForm.Kind.Row || row.Kind == PeekForm.Kind.Heading
            ? RulePitch(timeFont, titleFont, scale, true)
            : RulePitch(FontFor(row.Kind), null, scale);
        int TimeColumn => LeftColumn(timeFont, scale, config.Use24HourClock);
        int BinderHeight => theme.IsNotes ? Px(28) : 0;
        int BinderLipHeight => theme.IsNotes ? Math.Max(Px(3), 3) : 0;
        int PaperContentTopPadding => theme.IsNotes ? BinderLipHeight + Px(11) : Px(7);
        int CalloutHeight => config.ShowCallout ? Px(10) : 0;
        int DateStampContentWidth => TextRenderer.MeasureText(DateTime.Today.ToString("%d"), dateNumberFont, Size.Empty, TextFormatFlags.NoPadding).Width + Px(3) + Math.Max(TextRenderer.MeasureText(DateTime.Today.ToString("MMM"), dateMonthFont, Size.Empty, TextFormatFlags.NoPadding).Width, TextRenderer.MeasureText(DateTime.Today.ToString("dddd"), dateDayFont, Size.Empty, TextFormatFlags.NoPadding).Width);
        const TextFormatFlags Flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;

        internal static int Scaled(float value, float dpiScale) => (int)Math.Round(value * dpiScale);
        internal static int TextHeight(Font value) => TextRenderer.MeasureText("Ag", value, Size.Empty, TextFormatFlags.NoPadding).Height;
        internal static int RulePitch(Font first, Font second, float dpiScale, bool hasIcon = false)
        {
            var textHeight = Math.Max(TextHeight(first), second == null ? 0 : TextHeight(second));
            return hasIcon ? Math.Max(Scaled(18, dpiScale), textHeight) + Scaled(6, dpiScale) : Math.Max(Scaled(18, dpiScale), textHeight + Scaled(6, dpiScale));
        }
        internal static int LeftColumn(Font time, float dpiScale, bool use24HourClock = true)
        {
            // Measure the longest real range for the selected clock format.
            // In 12-hour mode the AM/PM suffix needs its own column space.
            var sample = use24HourClock ? "00:00 → 00:00 " : "12:00 AM → 12:00 PM ";
            return TextRenderer.MeasureText(sample, time, Size.Empty, TextFormatFlags.NoPadding).Width + Scaled(10, dpiScale);
        }

        public Size Measure(IReadOnlyList<PeekForm.Row> rows)
        {
            int width = Px(250), height = BinderHeight + PaperContentTopPadding + Px(7);
            if (theme.IsNotes)
                width = Math.Max(width, Px(44) + TimeColumn + Px(60) + Px(8) + DateStampContentWidth + Px(14));
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var f = FontFor(row.Kind); var size = TextRenderer.MeasureText(row.Left ?? "", f, Size.Empty, TextFormatFlags.NoPadding);
                int measuredWidth = row.Kind == PeekForm.Kind.Row
                    // The title begins after the card's ruled-paper left inset and
                    // needs a real right inset too.  The older +30 estimate omitted
                    // those 56 pixels, so the renderer measured a title as fitting
                    // and then drew it into a narrower rectangle with an ellipsis.
                    ? TimeColumn + Math.Min(Px(300), TextRenderer.MeasureText(row.Right ?? "", titleFont, Size.Empty, TextFormatFlags.NoPadding).Width) + Px(56)
                    : (row.IndentToTitle ? TimeColumn + Px(6) : 0) + size.Width + Px(54);
                width = Math.Max(width, measuredWidth);
                height += Px(row.GapAbove) + PitchFor(row);
            }
            return new Size(width, height);
        }

        public void Draw(Graphics g, Rectangle card, IReadOnlyList<PeekForm.Row> rows, int calloutTipY, int calloutX, Action<Rectangle> previousToggleBounds = null, float previousExpansion = 1, Action<DateStampLayout> dateStampLayoutChanged = null, Action<int, Rectangle, Rectangle, string> joinTargetBounds = null, int hoveredJoinRow = -1)
        {
            if (card.Width < 2 || card.Height < 2) return;
            var frameState = g.Save();
            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var outline = CreateSilhouettePath(card, calloutTipY, calloutX))
                {
                    if (!theme.IsNotes)
                    {
                        DrawPaper(g, outline, card);
                        using (var border = new Pen(theme.CardLine, Px(1))) g.DrawPath(border, outline);
                    }
                    else
                    {
                        using (var paperBody = CreatePaperBodyPath(card, calloutTipY, calloutX))
                        using (var leatherCap = CreateLeatherCapPath(card))
                        {
                            DrawPaper(g, paperBody, card);
                            DrawLeatherJoinShadow(g, paperBody, card);
                            DrawLeather(g, leatherCap, card);
                            DrawLeatherLowerLip(g, paperBody, card);
                            DrawTornPaperEdge(g, paperBody, card);
                            DrawPaperPerimeter(g, card, calloutX);
                        }
                    }
                }
            }
            finally { g.Restore(frameState); }
            g.SmoothingMode = SmoothingMode.None;
            DrawMarginRules(g, card);
            var dateStampLayout = theme.IsNotes ? DateStampLayoutFor(card) : null;
            var dateStampBounds = dateStampLayout == null ? Rectangle.Empty : dateStampLayout.Bounds;
            dateStampLayoutChanged?.Invoke(dateStampLayout);

            previousExpansion = Math.Max(0, Math.Min(1, previousExpansion));
            int left = card.Left + Px(44), yPos = card.Top + BinderHeight + PaperContentTopPadding;
            using (var line = new Pen(Color.FromArgb(112, theme.CardLine), Math.Max(1, Px(config.RuleLineThickness))))
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                yPos += (int)Math.Round(Px(row.GapAbove) * (row.IsPreviousMeeting ? previousExpansion : 1)); var f = FontFor(row.Kind); int textHeight = row.Kind == PeekForm.Kind.Row ? Math.Max(TextHeight(timeFont), TextHeight(titleFont)) : TextHeight(f); int rowHeight = row.Kind == PeekForm.Kind.Row ? Math.Max(Px(18), textHeight) : textHeight; int pitch = (int)Math.Round(PitchFor(row) * (row.IsPreviousMeeting ? previousExpansion : 1)); int lineY = yPos + pitch - Px(1);
                if (pitch <= 0) continue;
                if (lineY >= card.Bottom) break;
                int textY = lineY - rowHeight - Px(1);
                int contentRight = dateStampBounds.IsEmpty || textY >= dateStampBounds.Bottom || textY + rowHeight <= dateStampBounds.Top
                    ? card.Right - Px(12)
                    : dateStampBounds.Left - Px(8);
                GraphicsState rowState = null;
                if (row.IsPreviousMeeting && previousExpansion < 1)
                {
                    rowState = g.Save(); g.SetClip(new Rectangle(card.Left, yPos, card.Width, pitch));
                }
                if (row.Kind == PeekForm.Kind.Row)
                {
                    var image = LegacyIcons.ImageFor(row.Icon);
                    var iconSize = Math.Max(Px(16), Px(18)); if (image != null) g.DrawImage(image, new Rectangle(card.Left + Px(13), textY + Math.Max(0, (rowHeight - iconSize) / 2), iconSize, iconSize));
                    DrawCardText(g, row.Left, timeFont, new Rectangle(left, textY, TimeColumn, rowHeight), TimeColor(row));
                    int titleLeft = left + TimeColumn, titleRight = contentRight;
                    Rectangle joinIcon = Rectangle.Empty;
                    if (!string.IsNullOrWhiteSpace(row.JoinLink))
                    {
                        int availableTitleWidth = Math.Max(0, contentRight - titleLeft - JoinIconSize - Px(4));
                        int renderedTitleWidth = Math.Min(availableTitleWidth, TextRenderer.MeasureText(row.Right ?? "", titleFont, Size.Empty, TextFormatFlags.NoPadding).Width);
                        joinIcon = new Rectangle(titleLeft + renderedTitleWidth + Px(4), textY + Math.Max(0, (rowHeight - JoinIconSize) / 2), JoinIconSize, JoinIconSize);
                        if (!row.IsPreviousMeeting || previousExpansion >= 1)
                            joinTargetBounds?.Invoke(rowIndex, new Rectangle(card.Left + Px(10), textY, Math.Max(0, contentRight - card.Left - Px(20)), rowHeight), joinIcon, row.JoinLink);
                        if (rowIndex == hoveredJoinRow) titleRight = joinIcon.Left - Px(4);
                    }
                    DrawCardText(g, row.Right, titleFont, new Rectangle(titleLeft, textY, Math.Max(0, titleRight - titleLeft), rowHeight), row.RightColor.IsEmpty ? theme.CardText : row.RightColor);
                    if (!joinIcon.IsEmpty && rowIndex == hoveredJoinRow) DrawJoinIcon(g, joinIcon);
                }
                else
                {
                    int textLeft = left + (row.IndentToTitle ? TimeColumn + Px(6) : 0);
                    // Headings and notes share the same pitch as their rule,
                    // so centre the smaller text in that full space instead of
                    // pinning it immediately above the bottom line.
                    var textBounds = new Rectangle(textLeft, yPos, Math.Max(0, contentRight - textLeft), pitch);
                    DrawCardText(g, row.Left, f, textBounds, row.Kind == PeekForm.Kind.Heading ? theme.CardHeading : row.Kind == PeekForm.Kind.Note ? theme.CardNote : row.LeftColor);
                    if (row.IsPreviousToggle) previousToggleBounds?.Invoke(new Rectangle(card.Left + Px(10), yPos, card.Width - Px(20), pitch));
                }
                // Rules separate content rows.  Leaving one below the final
                // row reads as an accidental extra line at the bottom of the
                // note, especially with a short last meeting.
                bool hasVisibleRowAfter = false;
                for (int nextIndex = rowIndex + 1; nextIndex < rows.Count; nextIndex++)
                {
                    var next = rows[nextIndex];
                    var nextScale = next.IsPreviousMeeting ? previousExpansion : 1;
                    if ((int)Math.Round(PitchFor(next) * nextScale) > 0) { hasVisibleRowAfter = true; break; }
                }
                if (hasVisibleRowAfter)
                {
                    if (row.Kind == PeekForm.Kind.Heading)
                    {
                        using (var dateRule = new Pen(Color.FromArgb(164, Darken(theme.CardLine, 0.28f)), Math.Max(1, Px(config.RuleLineThickness))))
                            g.DrawLine(dateRule, card.Left + Px(12), lineY, RuleRightFor(lineY, card, dateStampBounds), lineY);
                    }
                    else g.DrawLine(line, card.Left + Px(12), lineY, RuleRightFor(lineY, card, dateStampBounds), lineY);
                }
                if (rowState != null) g.Restore(rowState);
                yPos += pitch;
            }
            if (dateStampLayout != null) DrawDateStamp(g, dateStampLayout);
        }

        int JoinIconSize => Math.Max(Px(16), 16);

        void DrawJoinIcon(Graphics g, Rectangle bounds)
        {
            var image = LegacyIcons.JoinImage;
            if (image != null) g.DrawImage(image, bounds);
        }

        int RuleRightFor(int lineY, Rectangle card, Rectangle dateStampBounds)
        {
            if (dateStampBounds.IsEmpty || lineY < dateStampBounds.Top || lineY > dateStampBounds.Bottom) return card.Right - Px(12);
            return Math.Max(card.Left + Px(12), dateStampBounds.Left - Px(8));
        }

        DateStampLayout DateStampLayoutFor(Rectangle card)
        {
            var today = DateTime.Today;
            string number = today.ToString("%d"), month = today.ToString("MMM"), day = today.ToString("dddd");
            int numberWidth = TextRenderer.MeasureText(number, dateNumberFont, Size.Empty, TextFormatFlags.NoPadding).Width;
            int monthWidth = TextRenderer.MeasureText(month, dateMonthFont, Size.Empty, TextFormatFlags.NoPadding).Width;
            int dayWidth = TextRenderer.MeasureText(day, dateDayFont, Size.Empty, TextFormatFlags.NoPadding).Width;
            int numberHeight = TextHeight(dateNumberFont);
            int monthHeight = TextHeight(dateMonthFont), dayHeight = TextHeight(dateDayFont);
            int height = Math.Max(numberHeight, monthHeight + dayHeight + Px(1));
            int defaultLeft = card.Right - Px(14) - DateStampContentWidth;
            int defaultTop = card.Top + BinderHeight + PaperContentTopPadding - Px(4);
            int labelLeft = defaultLeft + numberWidth + Px(3);
            int numberTop = defaultTop + (height - numberHeight) / 2;
            return new DateStampLayout
            {
                Number = ClampDateStampElement(card, new Rectangle(defaultLeft, numberTop, numberWidth, numberHeight), config.DateStampOffset(TextElement.CardDateNumber)),
                Month = ClampDateStampElement(card, new Rectangle(labelLeft, defaultTop, monthWidth, monthHeight), config.DateStampOffset(TextElement.CardDateMonth)),
                Day = ClampDateStampElement(card, new Rectangle(labelLeft, defaultTop + monthHeight + Px(1), dayWidth, dayHeight), config.DateStampOffset(TextElement.CardDateDay))
            };
        }

        Rectangle ClampDateStampElement(Rectangle card, Rectangle bounds, Point offset)
        {
            int left = bounds.Left + Px(offset.X), top = bounds.Top + Px(offset.Y);
            left = Math.Max(card.Left + Px(44), Math.Min(left, card.Right - bounds.Width - Px(8)));
            top = Math.Max(card.Top + BinderHeight + BinderLipHeight + Px(8), Math.Min(top, card.Bottom - bounds.Height - Px(8)));
            return new Rectangle(left, top, bounds.Width, bounds.Height);
        }

        void DrawDateStamp(Graphics g, DateStampLayout layout)
        {
            var today = DateTime.Today;
            string number = today.ToString("%d"), month = today.ToString("MMM"), day = today.ToString("dddd");
            DrawCardText(g, number, dateNumberFont, layout.Number, theme.CardDateNumber);
            DrawCardText(g, month, dateMonthFont, layout.Month, theme.CardDateMonth);
            DrawCardText(g, day, dateDayFont, layout.Day, theme.CardDateDay);
        }

        // TextRenderer uses GDI/ClearType.  It looks good on an opaque form but
        // produces jagged, overly dark glyphs once that form becomes a per-pixel
        // alpha layered window.  Grayscale antialiasing composes cleanly with the
        // card bitmap and is also what the settings preview now shows.
        static void DrawCardText(Graphics g, string text, Font font, Rectangle bounds, Color colour)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0 || string.IsNullOrEmpty(text)) return;
            var saved = g.Save();
            try
            {
                g.SetClip(bounds);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using (var brush = new SolidBrush(colour))
                using (var format = new StringFormat(StringFormat.GenericTypographic)
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox
                })
                    g.DrawString(text, font, brush, bounds, format);
            }
            finally { g.Restore(saved); }
        }

        /// <summary>
        /// One outer silhouette for the complete card.  This is used by the
        /// shadow and, for the non-Notes themes, by both fill and border.
        /// Notes draws its leather cap and paper body separately inside this
        /// exact outline, so their edges cannot drift apart.
        /// </summary>
        internal GraphicsPath CreateSilhouettePath(Rectangle card, int calloutTipY, int calloutX)
        {
            var path = new GraphicsPath();
            if (!theme.IsNotes)
            {
                AddClassicSilhouette(path, card, calloutTipY, calloutX);
                return path;
            }

            int radius = CornerRadius(card);
            int cutLeft = CalloutLeft(card, calloutX, radius);
            int cutRight = CalloutRight(card, calloutX, radius);
            path.StartFigure();
            if (radius == 0)
            {
                path.AddLine(card.Left, card.Top, card.Right, card.Top);
                path.AddLine(card.Right, card.Top, card.Right, card.Bottom);
                AddCalloutBase(path, card, calloutTipY, calloutX, cutRight, cutLeft);
                path.AddLine(card.Left, card.Bottom, card.Left, card.Top);
            }
            else
            {
                path.AddLine(card.Left + radius, card.Top, card.Right - radius, card.Top);
                path.AddArc(card.Right - 2 * radius, card.Top, 2 * radius, 2 * radius, 270, 90);
                path.AddLine(card.Right, card.Top + radius, card.Right, card.Bottom - radius);
                path.AddArc(card.Right - 2 * radius, card.Bottom - 2 * radius, 2 * radius, 2 * radius, 0, 90);
                AddCalloutBase(path, card, calloutTipY, calloutX, cutRight, cutLeft);
                AddLowerLeftCorner(path, card.Left, card.Bottom, radius);
                path.AddLine(card.Left, card.Top + radius, card.Left + radius, card.Top);
            }
            path.CloseFigure();
            return path;
        }

        void AddClassicSilhouette(GraphicsPath path, Rectangle card, int calloutTipY, int calloutX)
        {
            if (CalloutHeight == 0) { path.AddRectangle(card); return; }
            int cutLeft = Math.Max(card.Left, calloutX - Px(7));
            int cutRight = Math.Min(card.Right, calloutX + Px(7));
            path.StartFigure();
            path.AddLine(card.Left, card.Top, card.Right, card.Top);
            path.AddLine(card.Right, card.Top, card.Right, card.Bottom);
            AddCalloutBase(path, card, calloutTipY, calloutX, cutRight, cutLeft);
            path.AddLine(card.Left, card.Bottom, card.Left, card.Top);
            path.CloseFigure();
        }

        GraphicsPath CreatePaperBodyPath(Rectangle card, int calloutTipY, int calloutX)
        {
            var path = new GraphicsPath();
            int radius = CornerRadius(card);
            int top = PaperBodyTop(card);
            int cutLeft = CalloutLeft(card, calloutX, radius);
            int cutRight = CalloutRight(card, calloutX, radius);
            path.StartFigure();
            if (radius == 0)
            {
                path.AddLine(card.Left, top, card.Right, top);
                path.AddLine(card.Right, top, card.Right, card.Bottom);
                AddCalloutBase(path, card, calloutTipY, calloutX, cutRight, cutLeft);
                path.AddLine(card.Left, card.Bottom, card.Left, top);
            }
            else
            {
                path.AddLine(card.Left, top, card.Right, top);
                path.AddLine(card.Right, top, card.Right, card.Bottom - radius);
                path.AddArc(card.Right - 2 * radius, card.Bottom - 2 * radius, 2 * radius, 2 * radius, 0, 90);
                AddCalloutBase(path, card, calloutTipY, calloutX, cutRight, cutLeft);
                AddLowerLeftCorner(path, card.Left, card.Bottom, radius);
                path.AddLine(card.Left, top, card.Left, top);
            }
            path.CloseFigure();
            return path;
        }

        GraphicsPath CreateLeatherCapPath(Rectangle card)
        {
            int radius = CornerRadius(card);
            int bottom = card.Top + BinderHeight;
            var path = new GraphicsPath();
            if (radius == 0) { path.AddRectangle(new Rectangle(card.Left, card.Top, card.Width, BinderHeight)); return path; }
            path.StartFigure();
            path.AddLine(card.Left + radius, card.Top, card.Right - radius, card.Top);
            path.AddArc(card.Right - 2 * radius, card.Top, 2 * radius, 2 * radius, 270, 90);
            path.AddLine(card.Right, card.Top + radius, card.Right, bottom);
            path.AddLine(card.Right, bottom, card.Left, bottom);
            path.AddLine(card.Left, bottom, card.Left, card.Top + radius);
            path.AddArc(card.Left, card.Top, 2 * radius, 2 * radius, 180, 90);
            path.CloseFigure();
            return path;
        }

        void AddCalloutBase(GraphicsPath path, Rectangle card, int calloutTipY, int calloutX, int cutRight, int cutLeft)
        {
            path.AddLine(card.Right - CornerRadius(card), card.Bottom, cutRight, card.Bottom);
            if (CalloutHeight > 0)
            {
                path.AddLine(cutRight, card.Bottom, calloutX, calloutTipY);
                path.AddLine(calloutX, calloutTipY, cutLeft, card.Bottom);
            }
            path.AddLine(cutLeft, card.Bottom, card.Left + CornerRadius(card), card.Bottom);
        }

        // GDI+'s lower-left AddArc can rasterize toward the inside of a tiny
        // antialiased shape.  A Bezier has the same circle tangent but gives
        // us the exact start/end winding, so this corner always bows outward.
        static void AddLowerLeftCorner(GraphicsPath path, float left, float bottom, float radius)
        {
            const float kappa = 0.55228475f;
            float control = radius * kappa;
            path.AddBezier(left + radius, bottom, left + radius - control, bottom, left, bottom - radius + control, left, bottom - radius);
        }

        static void AddLowerLeftCornerReverse(GraphicsPath path, float left, float bottom, float radius)
        {
            const float kappa = 0.55228475f;
            float control = radius * kappa;
            path.AddBezier(left, bottom - radius, left, bottom - radius + control, left + radius - control, bottom, left + radius, bottom);
        }

        int CornerRadius(Rectangle card) => Math.Max(0, Math.Min(Px(config.CardCornerRadius), Math.Min(card.Width / 2, card.Height / 2)));
        int PaperBodyTop(Rectangle card) => card.Top + BinderHeight - Math.Max(1, Px(1));
        int CalloutLeft(Rectangle card, int calloutX, int radius) => Math.Max(card.Left + radius, calloutX - Px(7));
        int CalloutRight(Rectangle card, int calloutX, int radius) => Math.Min(card.Right - radius, calloutX + Px(7));

        /// <summary>Draws the details-card shadow into the same alpha surface as
        /// the paper.  This replaces Windows' rectangular window shadow.</summary>
        internal void DrawOuterShadow(Graphics g, GraphicsPath silhouette)
        {
            if (!theme.Shadow || silhouette == null) return;
            var saved = g.Save();
            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var shadow = (GraphicsPath)silhouette.Clone())
                using (var offset = new Matrix())
                {
                    offset.Translate(Px(1), Px(2));
                    shadow.Transform(offset);
                    int radius = Math.Max(Px(6), 6);
                    for (int spread = radius; spread >= 1; spread -= 2)
                    {
                        float fraction = 1f - (spread - 1) / (float)radius;
                        int alpha = Math.Max(2, (int)Math.Round(2 + 16 * fraction * fraction));
                        // A widened outline uses miter joins by default.  At the
                        // lower paper corners that produced a tiny diagonal spur,
                        // which read as a folded "dog-ear" below the card.  The
                        // shadow follows the same rounded silhouette instead.
                        using (var pen = new Pen(Color.FromArgb(alpha, 38, 29, 18), 2 * spread + 1) { LineJoin = LineJoin.Round })
                            g.DrawPath(pen, shadow);
                    }
                }
            }
            finally { g.Restore(saved); }
        }

        void DrawPaper(Graphics g, GraphicsPath paperBody, Rectangle card)
        {
            using (var paper = new SolidBrush(theme.CardBackground)) g.FillPath(paper, paperBody);
            if (!theme.IsNotes) return;

            // Lighting is clipped by the same antialiased paper shape as the
            // base fill.  No rectangular clip means no hard side edge.
            var bounds = new RectangleF(card.Left, PaperBodyTop(card), card.Width, card.Bottom - PaperBodyTop(card) + CalloutHeight);
            var centreGlow = Color.FromArgb(28, 255, 252, 225);
            var sideShade = Color.FromArgb(26, 139, 91, 19);
            using (var sides = new LinearGradientBrush(bounds, sideShade, sideShade, LinearGradientMode.Horizontal))
            {
                sides.InterpolationColors = new ColorBlend
                {
                    Colors = new[] { sideShade, centreGlow, centreGlow, sideShade },
                    Positions = new[] { 0f, 0.20f, 0.80f, 1f }
                };
                g.FillPath(sides, paperBody);
            }
            using (var bottom = new LinearGradientBrush(bounds, Color.FromArgb(0, 62, 42, 13), Color.FromArgb(28, 62, 42, 13), LinearGradientMode.Vertical)) g.FillPath(bottom, paperBody);
            using (var grain = new TextureBrush(PaperGrainTile, WrapMode.Tile)) g.FillPath(grain, paperBody);
        }

        void DrawPaperPerimeter(Graphics g, Rectangle card, int calloutX)
        {
            int radius = CornerRadius(card);
            int top = card.Top + BinderHeight;
            int cutLeft = CalloutLeft(card, calloutX, radius);
            int cutRight = CalloutRight(card, calloutX, radius);
            using (var perimeter = new GraphicsPath())
            using (var border = new Pen(theme.CardLine, Px(1)) { LineJoin = LineJoin.Round })
            {
                // Fill paths use the rectangle's outer bounds.  A one-pixel
                // outline must sit half a pixel inside those bounds, otherwise
                // GDI clips the right/bottom half of the pen while the left/top
                // half remains visible.  That was the visibly heavier right edge.
                float left = card.Left + 0.5f;
                float right = card.Right - 0.5f;
                float bottom = card.Bottom - 0.5f;
                if (radius == 0)
                {
                    perimeter.AddLine(left, top, left, bottom);
                    if (CalloutHeight > 0)
                    {
                        perimeter.AddLine(left, bottom, calloutX, card.Bottom + CalloutHeight - 0.5f);
                        perimeter.AddLine(calloutX, card.Bottom + CalloutHeight - 0.5f, right, bottom);
                    }
                    perimeter.AddLine(right, bottom, right, top);
                    g.DrawPath(border, perimeter);
                    return;
                }
                float innerRadius = radius - 0.5f;
                float cutLeftInner = Math.Max(left + innerRadius, cutLeft);
                float cutRightInner = Math.Min(right - innerRadius, cutRight);
                perimeter.StartFigure();
                perimeter.AddLine(left, top, left, bottom - innerRadius);
                AddLowerLeftCornerReverse(perimeter, left, bottom, innerRadius);
                perimeter.AddLine(left + innerRadius, bottom, cutLeftInner, bottom);
                if (CalloutHeight > 0)
                {
                    perimeter.AddLine(cutLeftInner, bottom, calloutX, card.Bottom + CalloutHeight - 0.5f);
                    perimeter.AddLine(calloutX, card.Bottom + CalloutHeight - 0.5f, cutRightInner, bottom);
                }
                perimeter.AddLine(cutRightInner, bottom, right - innerRadius, bottom);
                perimeter.AddArc(right - 2 * innerRadius, bottom - 2 * innerRadius, 2 * innerRadius, 2 * innerRadius, 90, -90);
                perimeter.AddLine(right, bottom - innerRadius, right, top);
                g.DrawPath(border, perimeter);
            }
        }

        void DrawMarginRules(Graphics g, Rectangle card)
        {
            int x = card.Left + Px(34), top = card.Top + BinderHeight + PaperContentTopPadding + Px(1), bottom = card.Bottom - Px(8);
            int width = Math.Max(1, Px(config.MarginLineThickness));
            using (var primary = new Pen(theme.Accent, width)) g.DrawLine(primary, x, top, x, bottom);
            if (!config.DoubleMarginLine) return;
            int secondX = x + width + Px(config.MarginLineGap);
            using (var secondary = new Pen(Color.FromArgb(118, theme.Accent), width)) g.DrawLine(secondary, secondX, top, secondX, bottom);
        }

        Color TimeColor(PeekForm.Row row)
        {
            if (row.TimeState == ColorState.Green) return theme.TimeGreen;
            if (row.TimeState == ColorState.Amber) return theme.TimeAmber;
            return theme.CardTime;
        }

        static Color Darken(Color colour, float amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromArgb(colour.A,
                (int)Math.Round(colour.R * (1 - amount)),
                (int)Math.Round(colour.G * (1 - amount)),
                (int)Math.Round(colour.B * (1 - amount)));
        }

        void DrawLeather(Graphics g, GraphicsPath leatherCap, Rectangle card)
        {
            var binder = new Rectangle(card.Left, card.Top, card.Width, BinderHeight);
            var saved = g.Save();
            try
            {
                using (var leather = new TextureBrush(LeatherTile, WrapMode.Tile)) g.FillPath(leather, leatherCap);
                g.SetClip(leatherCap);
                DrawBinderLighting(g, binder);
                // One quiet upper highlight gives the leather its old-school depth.
                using (var topEdge = new Pen(Color.FromArgb(150, 214, 157, 103), Px(1)))
                    g.DrawLine(topEdge, binder.Left + CornerRadius(card), binder.Top, binder.Right - CornerRadius(card), binder.Top);
                DrawStitching(g, binder);
            }
            finally { g.Restore(saved); }
        }

        void DrawLeatherJoinShadow(Graphics g, GraphicsPath paperBody, Rectangle card)
        {
            // The hard leather lip sits above this pass.  The shadow begins just
            // below its lower edge, leaving a real thickness before it falls into
            // the paper instead of reading as a single dark separator line.
            int top = card.Top + BinderHeight + BinderLipHeight - Math.Max(1, Px(1));
            int height = Math.Max(Px(7), 7);
            var saved = g.Save();
            try
            {
                g.SetClip(paperBody);
                using (var join = new LinearGradientBrush(new Rectangle(card.Left, top, card.Width, height), Color.Transparent, Color.Transparent, LinearGradientMode.Vertical))
                {
                    join.InterpolationColors = new ColorBlend
                    {
                        Colors = new[]
                        {
                            Color.FromArgb(82, 42, 25, 12),
                            Color.FromArgb(40, 42, 25, 12),
                            Color.FromArgb(10, 42, 25, 12),
                            Color.FromArgb(0, 42, 25, 12)
                        },
                        Positions = new[] { 0f, 0.16f, 0.55f, 1f }
                    };
                    g.FillRectangle(join, card.Left, top, card.Width, height);
                }
            }
            finally { g.Restore(saved); }
        }

        void DrawLeatherLowerLip(Graphics g, GraphicsPath paperBody, Rectangle card)
        {
            int height = BinderLipHeight + Math.Max(1, Px(1));
            int top = card.Top + BinderHeight - Math.Max(1, Px(1));
            var saved = g.Save();
            try
            {
                g.SetClip(paperBody);
                using (var lip = new LinearGradientBrush(new Rectangle(card.Left, top, card.Width, height), Color.Transparent, Color.Transparent, LinearGradientMode.Vertical))
                {
                    lip.InterpolationColors = new ColorBlend
                    {
                        Colors = new[]
                        {
                            Color.FromArgb(172, 77, 42, 24),
                            Color.FromArgb(136, 57, 29, 16),
                            Color.FromArgb(72, 44, 23, 12),
                            Color.FromArgb(0, 44, 23, 12)
                        },
                        Positions = new[] { 0f, 0.38f, 0.76f, 1f }
                    };
                    g.FillRectangle(lip, card.Left, top, card.Width, height);
                }
                // A faint lower catch-light separates the leather underside from
                // its cast shadow without becoming another horizontal rule.
                using (var edge = new Pen(Color.FromArgb(36, 243, 204, 142), Math.Max(1, Px(1))))
                    g.DrawLine(edge, card.Left + Px(1), top + BinderLipHeight, card.Right - Px(1), top + BinderLipHeight);
            }
            finally { g.Restore(saved); }
        }

        void DrawTornPaperEdge(Graphics g, GraphicsPath paperBody, Rectangle card)
        {
            // Each renderer receives a new irregular tear profile, while that
            // profile stays fixed for its lifetime so hover/fade redraws do not
            // make it dance. It remains below the leather lip rather than
            // becoming a second binding seam.
            int left = card.Left + Px(1), right = card.Right - Px(1);
            int top = card.Top + BinderHeight + BinderLipHeight + Px(1);
            int minStep = Math.Max(Px(2), 2), maxStep = Math.Max(Px(7), 7), maxDepth = Math.Max(Px(4), 4);
            int lowerMaxDepth = maxDepth + Math.Max(Px(2), 2);
            var upperPoints = new List<PointF> { new PointF(left, top) };
            var lowerPoints = new List<PointF> { new PointF(left, top) };
            int index = 0, x = left;
            while (x < right)
            {
                int value = TornValue(index++);
                x += minStep + value % (maxStep - minStep + 1);
                if (x >= right) break;
                int upperDepth = 1 + ((value >> 8) % maxDepth);
                // A second independently torn page sits below the first. It
                // shares each X sample so we can hide its edge wherever the
                // upper page hangs lower at that spot.
                int lowerDepth = 1 + ((TornValue(index + 409) >> 8) % lowerMaxDepth);
                upperPoints.Add(new PointF(x, top + upperDepth));
                lowerPoints.Add(new PointF(x, top + lowerDepth));
            }
            upperPoints.Add(new PointF(right, top));
            lowerPoints.Add(new PointF(right, top));
            var saved = g.Save();
            try
            {
                g.SetClip(paperBody);
                using (var lowerTear = new Pen(Color.FromArgb(145, 61, 37, 17), Math.Max(1, Px(1))) { LineJoin = LineJoin.Round })
                    DrawVisibleLowerTornEdge(g, lowerTear, upperPoints, lowerPoints);
                using (var tear = new Pen(Color.FromArgb(108, 83, 54, 25), Math.Max(1, Px(1))) { LineJoin = LineJoin.Round })
                    // Keep the original busy profile, but round the sharpest
                    // joins just enough to stop the pinking-shear look.
                    g.DrawCurve(tear, upperPoints.ToArray(), 0.10f);
            }
            finally { g.Restore(saved); }
        }

        static void DrawVisibleLowerTornEdge(Graphics g, Pen pen, IReadOnlyList<PointF> upperPoints, IReadOnlyList<PointF> lowerPoints)
        {
            for (int pointIndex = 0; pointIndex < upperPoints.Count - 1; pointIndex++)
            {
                var upperA = upperPoints[pointIndex]; var upperB = upperPoints[pointIndex + 1];
                var lowerA = lowerPoints[pointIndex]; var lowerB = lowerPoints[pointIndex + 1];
                float differenceA = lowerA.Y - upperA.Y, differenceB = lowerB.Y - upperB.Y;
                if (differenceA <= 0 && differenceB <= 0) continue;

                if (differenceA > 0 && differenceB > 0)
                {
                    g.DrawLine(pen, lowerA, lowerB);
                    continue;
                }

                // The two torn profiles cross inside this segment. Draw only
                // the part of the lower edge which actually protrudes.
                float crossing = differenceA / (differenceA - differenceB);
                var intersection = new PointF(
                    lowerA.X + (lowerB.X - lowerA.X) * crossing,
                    lowerA.Y + (lowerB.Y - lowerA.Y) * crossing);
                if (differenceA > 0) g.DrawLine(pen, lowerA, intersection);
                else g.DrawLine(pen, intersection, lowerB);
            }
        }

        int TornValue(int index)
        {
            unchecked
            {
                int value = tornSeed ^ (index * 0x45d9f3b);
                value ^= value >> 16; value *= 0x45d9f3b; value ^= value >> 16;
                return value & int.MaxValue;
            }
        }

        void DrawBinderLighting(Graphics g, Rectangle binder)
        {
            // Broad warm centre-light, then a barely-there curved reflection. This
            // gives the leather the same shallow, bowed highlight as the old iOS bar.
            var band = new RectangleF(binder.Left, binder.Top + Px(2), binder.Width, Math.Max(Px(8), BinderHeight - Px(5)));
            using (var glow = new LinearGradientBrush(band, Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal))
            {
                glow.InterpolationColors = new ColorBlend
                {
                    Colors = new[] { Color.FromArgb(0, 255, 202, 143), Color.FromArgb(30, 255, 205, 150), Color.FromArgb(30, 255, 205, 150), Color.FromArgb(0, 255, 202, 143) },
                    Positions = new[] { 0f, 0.28f, 0.72f, 1f }
                };
                g.FillRectangle(glow, band);
            }
            using (var reflection = new Pen(Color.FromArgb(36, 255, 220, 174), Math.Max(1, Px(1))))
            {
                float y = binder.Top + Px(3), middle = binder.Top + Px(7);
                g.DrawBezier(reflection,
                    new PointF(binder.Left + Px(4), y), new PointF(binder.Left + binder.Width * 0.30f, middle),
                    new PointF(binder.Right - binder.Width * 0.30f, middle), new PointF(binder.Right - Px(4), y));
            }
        }

        void DrawStitching(Graphics g, Rectangle binder)
        {
            int stitchY = binder.Bottom - Px(6), step = Math.Max(Px(9), 8), length = Math.Max(Px(6), 5);
            using (var dark = new Pen(Color.FromArgb(77, 38, 20), Math.Max(1, Px(2))))
            using (var light = new Pen(Color.FromArgb(224, 157, 96), Math.Max(1, Px(1))))
            for (int x = binder.Left + Px(3); x < binder.Right; x += step)
            {
                int end = Math.Min(binder.Right - Px(1), x + length);
                g.DrawLine(dark, x, stitchY, end, stitchY + Px(2));
                g.DrawLine(light, x, stitchY - Px(1), end, stitchY + Px(1));
            }
        }

        static Bitmap CreateLeatherTile()
        {
            const int width = 48, height = 24;
            var bitmap = new Bitmap(width, height); var random = new Random(2010);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int grain = random.Next(-8, 9) + (int)(5 * Math.Sin((x * 0.43) + (y * 0.71))) + (int)(3 * Math.Sin((x * 1.27) - (y * 0.24)));
                bitmap.SetPixel(x, y, Color.FromArgb(Clamp(105 + grain), Clamp(57 + grain), Clamp(35 + grain)));
            }
            using (var g = Graphics.FromImage(bitmap))
            using (var pore = new Pen(Color.FromArgb(30, 48, 22, 12), 1))
            using (var sheen = new Pen(Color.FromArgb(22, 230, 174, 115), 1))
            for (int x = -12; x < width + 12; x += 11)
            {
                g.DrawBezier(pore, x, 2, x + 4, 7, x - 3, 15, x + 6, 22);
                g.DrawBezier(sheen, x + 2, 1, x + 5, 8, x - 1, 14, x + 8, 23);
            }
            return bitmap;
        }

        static Bitmap CreatePaperGrainTile()
        {
            const int width = 48, height = 48;
            var bitmap = new Bitmap(width, height); var random = new Random(1987);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var alpha = random.Next(0, 13);
                var colour = ((x + y) % 5 == 0) ? Color.FromArgb(alpha, 111, 82, 31) : Color.FromArgb(alpha, 255, 249, 204);
                bitmap.SetPixel(x, y, colour);
            }
            return bitmap;
        }

        public void Dispose()
        {
            dateNumberFont.Dispose(); dateMonthFont.Dispose(); dateDayFont.Dispose();
        }

        static int Clamp(int value) => Math.Max(0, Math.Min(255, value));
    }
}
