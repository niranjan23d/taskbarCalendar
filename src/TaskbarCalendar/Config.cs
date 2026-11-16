using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TaskbarCalendar
{
    enum StripLayout { TitleFirst, TimeFirst, Private }
    enum PrivacyDisplay { Busy, Hidden }
    enum EmptyDayTone { Minimal, Casual, Playful, Custom }
    enum TextElement { TaskbarTop, TaskbarBottom, CardHeading, CardTime, CardTitle, CardNote, CardDateNumber, CardDateMonth, CardDateDay }
    enum WidgetAction { Details, Outlook, Nothing }

    /// <summary>Local-only settings. Legacy Google, Claude, updater and Chrome keys are deliberately ignored.</summary>
    class Config
    {
        public int RefreshMinutes = 5;
        public int LookaheadDays = 7;
        public bool ShowCalendar = true;
        public int Width = 300;
        public bool AutoWidth = true;
        public int RightGap = 15;
        public int? X;

        public StripLayout Layout = StripLayout.TitleFirst;
        public bool Use24HourClock = true;
        public int AmberThresholdMinutes = 3;
        public bool LateAlertEnabled = false;
        public bool SkipCancelledMeetings = true;
        public int FadeDelayMilliseconds = 250;
        public int FadeDurationMilliseconds = 180;
        // The detail card starts this fraction of its final height below its resting point.
        public int AppearStartPercent = 85;
        public int AppearDurationMilliseconds = 250;
        public WidgetAction ClickAction = WidgetAction.Outlook;
        public WidgetAction DoubleClickAction = WidgetAction.Outlook;
        public WidgetAction HoverAction = WidgetAction.Details;
        public EmptyDayTone EmptyDayTone = EmptyDayTone.Casual;
        public string EmptyDayMessage = "Done for the day";

        public bool TeamsPrivacyEnabled = true;
        public bool ManualPrivacy = false;
        public PrivacyDisplay PrivacyDisplay = PrivacyDisplay.Busy;

        public string ThemePreset = "Notes";
        public string StripBackground = "#4B392C";
        public string StripForeground = "#FFF7E1";
        // Use the actual taskbar surface under the strip to keep the compact text readable
        // when Windows is using a light or accent-coloured taskbar.
        public bool AutoTaskbarTextColors = true;
        // Sampled from the supplied iOS Notes reference. Text stays independently configurable.
        public string CardBackground = "#FFF6CF";
        public string CardLine = "#989758";
        public string AccentColor = "#BF975B";
        public int RuleLineThickness = 1;
        public int MarginLineThickness = 1;
        public int MarginLineGap = 2;
        public bool DoubleMarginLine = true;
        // Notes card outer-corner radius in physical pixels before DPI scaling.
        // Zero keeps the paper square; the default keeps the subtle original curve.
        public int CardCornerRadius = 5;
        // Logical-pixel offsets from the Notes date stamp's default positions.
        public int CardDateNumberOffsetX = 16, CardDateNumberOffsetY = -29;
        public int CardDateMonthOffsetX = -1, CardDateMonthOffsetY = 12;
        public int CardDateDayOffsetX = -1, CardDateDayOffsetY = 10;
        // Legacy configurations can still fall back to this single family, so
        // it must agree with the per-element Notes defaults below.
        public string FontFamily = "Segoe UI";
        public string TaskbarTopFontFamily = "Segoe UI", TaskbarBottomFontFamily = "Segoe UI", CardHeadingFontFamily = "Segoe UI", CardTimeFontFamily = "Segoe UI", CardTitleFontFamily = "Segoe UI", CardNoteFontFamily = "Segoe UI", CardDateNumberFontFamily = "Segoe UI", CardDateMonthFontFamily = "Segoe UI", CardDateDayFontFamily = "Segoe UI";
        public int TaskbarTopFontTenths = 82, TaskbarBottomFontTenths = 82, CardHeadingFontTenths = 82, CardTimeFontTenths = 82, CardTitleFontTenths = 82, CardNoteFontTenths = 82, CardDateNumberFontTenths = 240, CardDateMonthFontTenths = 90, CardDateDayFontTenths = 90;
        public string StripSecondaryColor = "#F3E5B4";
        public string TimeGreenColor = "#83E69C";
        public string TimeAmberColor = "#A8442F";
        public string CardHeadingColor = "#694C2C";
        public string CardTimeColor = "#3A2B1D";
        public string CardTextColor = "#372D1E";
        public string CardNoteColor = "#60533A";
        public string CardDateNumberColor = "#372D1E";
        public string CardDateMonthColor = "#694C2C";
        public string CardDateDayColor = "#60533A";
        public bool TaskbarTopBold = true, TaskbarTopItalic, TaskbarTopUnderline;
        public bool TaskbarBottomBold, TaskbarBottomItalic = true, TaskbarBottomUnderline;
        public bool CardHeadingBold = true, CardHeadingItalic, CardHeadingUnderline;
        public bool CardTimeBold, CardTimeItalic, CardTimeUnderline;
        public bool CardTitleBold = true, CardTitleItalic, CardTitleUnderline;
        public bool CardNoteBold, CardNoteItalic = true, CardNoteUnderline;
        public bool CardDateNumberBold, CardDateNumberItalic, CardDateNumberUnderline;
        public bool CardDateMonthBold = true, CardDateMonthItalic, CardDateMonthUnderline;
        public bool CardDateDayBold, CardDateDayItalic = true, CardDateDayUnderline;
        public int BackgroundOpacity = 40;
        public bool NoStripBackground = true;
        public bool CardShadow = true;
        public bool ShowCallout = true;

        public static bool IsFirstRun => !File.Exists(Paths.ConfigFile);
        public static Config Load() => Load(Paths.ConfigFile);

        internal static Config Load(string path)
        {
            var cfg = new Config();
            Dictionary<string, object> d = null;
            try { if (File.Exists(path)) d = Json.ParseObject(FileUtil.ReadText(path)); }
            catch (Exception ex) { Log.Warn($"Settings file is invalid; using defaults ({ex.Message})"); }
            if (d == null) return cfg;

            cfg.RefreshMinutes = Limit(Json.Num(d, "refresh_minutes"), cfg.RefreshMinutes, 1, 60);
            cfg.LookaheadDays = Limit(Json.Num(d, "lookahead_days"), cfg.LookaheadDays, 1, 30);
            cfg.ShowCalendar = Json.Bool(d, "show_calendar") ?? cfg.ShowCalendar;
            cfg.Width = Limit(Json.Num(d, "width"), cfg.Width, 120, 500);
            cfg.AutoWidth = Json.Bool(d, "auto_width") ?? cfg.AutoWidth;
            cfg.RightGap = Limit(Json.Num(d, "right_gap"), cfg.RightGap, 0, 400);
            cfg.X = (int?)Json.Num(d, "x");
            cfg.Layout = ReadEnum(Json.Str(d, "layout"), cfg.Layout);
            cfg.Use24HourClock = Json.Bool(d, "use_24_hour_clock") ?? cfg.Use24HourClock;
            cfg.AmberThresholdMinutes = Limit(Json.Num(d, "amber_threshold_minutes"), cfg.AmberThresholdMinutes, 0, 60);
            cfg.LateAlertEnabled = Json.Bool(d, "late_alert_enabled") ?? cfg.LateAlertEnabled;
            cfg.SkipCancelledMeetings = Json.Bool(d, "skip_cancelled_meetings") ?? Json.Bool(d, "cancelled_final_meeting_ends_day") ?? cfg.SkipCancelledMeetings;
            cfg.FadeDelayMilliseconds = Limit(Json.Num(d, "fade_delay_ms"), cfg.FadeDelayMilliseconds, 0, 3000);
            cfg.FadeDurationMilliseconds = Limit(Json.Num(d, "fade_duration_ms"), cfg.FadeDurationMilliseconds, 0, 3000);
            cfg.AppearStartPercent = Limit(Json.Num(d, "appear_start_percent"), cfg.AppearStartPercent, 50, 100);
            cfg.AppearDurationMilliseconds = Limit(Json.Num(d, "appear_duration_ms"), cfg.AppearDurationMilliseconds, 0, 3000);
            cfg.ClickAction = ReadEnum(Json.Str(d, "click_action"), cfg.ClickAction);
            cfg.DoubleClickAction = ReadEnum(Json.Str(d, "double_click_action"), cfg.DoubleClickAction);
            cfg.HoverAction = ReadEnum(Json.Str(d, "hover_action"), cfg.HoverAction);
            if (cfg.HoverAction == WidgetAction.Outlook) cfg.HoverAction = WidgetAction.Details;
            cfg.EmptyDayTone = ReadEnum(Json.Str(d, "empty_day_tone"), cfg.EmptyDayTone);
            cfg.EmptyDayMessage = Json.Str(d, "empty_day_message") ?? cfg.EmptyDayMessage;
            cfg.TeamsPrivacyEnabled = Json.Bool(d, "teams_privacy_enabled") ?? cfg.TeamsPrivacyEnabled;
            cfg.ManualPrivacy = Json.Bool(d, "manual_privacy") ?? cfg.ManualPrivacy;
            cfg.PrivacyDisplay = ReadEnum(Json.Str(d, "privacy_display"), cfg.PrivacyDisplay);
            cfg.ThemePreset = Json.Str(d, "theme_preset") ?? cfg.ThemePreset;
            cfg.StripBackground = Json.Str(d, "strip_background") ?? cfg.StripBackground;
            cfg.StripForeground = Json.Str(d, "strip_foreground") ?? cfg.StripForeground;
            cfg.AutoTaskbarTextColors = Json.Bool(d, "auto_taskbar_text_colors") ?? cfg.AutoTaskbarTextColors;
            cfg.CardBackground = Json.Str(d, "card_background") ?? cfg.CardBackground;
            cfg.CardLine = Json.Str(d, "card_line") ?? cfg.CardLine;
            cfg.AccentColor = Json.Str(d, "accent_color") ?? cfg.AccentColor;
            cfg.RuleLineThickness = Limit(Json.Num(d, "rule_line_thickness"), cfg.RuleLineThickness, 1, 4);
            cfg.MarginLineThickness = Limit(Json.Num(d, "margin_line_thickness"), cfg.MarginLineThickness, 1, 4);
            cfg.MarginLineGap = Limit(Json.Num(d, "margin_line_gap"), cfg.MarginLineGap, 1, 8);
            cfg.DoubleMarginLine = Json.Bool(d, "double_margin_line") ?? cfg.DoubleMarginLine;
            cfg.CardCornerRadius = Limit(Json.Num(d, "card_corner_radius"), cfg.CardCornerRadius, 0, 16);
            cfg.CardDateNumberOffsetX = Limit(Json.Num(d, "card_date_number_offset_x"), cfg.CardDateNumberOffsetX, -400, 400);
            cfg.CardDateNumberOffsetY = Limit(Json.Num(d, "card_date_number_offset_y"), cfg.CardDateNumberOffsetY, -400, 400);
            cfg.CardDateMonthOffsetX = Limit(Json.Num(d, "card_date_month_offset_x"), cfg.CardDateMonthOffsetX, -400, 400);
            cfg.CardDateMonthOffsetY = Limit(Json.Num(d, "card_date_month_offset_y"), cfg.CardDateMonthOffsetY, -400, 400);
            cfg.CardDateDayOffsetX = Limit(Json.Num(d, "card_date_day_offset_x"), cfg.CardDateDayOffsetX, -400, 400);
            cfg.CardDateDayOffsetY = Limit(Json.Num(d, "card_date_day_offset_y"), cfg.CardDateDayOffsetY, -400, 400);
            cfg.FontFamily = Json.Str(d, "font_family") ?? cfg.FontFamily;
            cfg.TaskbarTopFontFamily = Json.Str(d, "taskbar_top_font_family") ?? cfg.FontFamily;
            cfg.TaskbarBottomFontFamily = Json.Str(d, "taskbar_bottom_font_family") ?? cfg.FontFamily;
            cfg.CardHeadingFontFamily = Json.Str(d, "card_heading_font_family") ?? cfg.FontFamily;
            cfg.CardTimeFontFamily = Json.Str(d, "card_time_font_family") ?? cfg.FontFamily;
            cfg.CardTitleFontFamily = Json.Str(d, "card_title_font_family") ?? cfg.FontFamily;
            cfg.CardNoteFontFamily = Json.Str(d, "card_note_font_family") ?? cfg.FontFamily;
            cfg.CardDateNumberFontFamily = Json.Str(d, "card_date_number_font_family") ?? cfg.FontFamily;
            cfg.CardDateMonthFontFamily = Json.Str(d, "card_date_month_font_family") ?? cfg.FontFamily;
            cfg.CardDateDayFontFamily = Json.Str(d, "card_date_day_font_family") ?? cfg.FontFamily;
            cfg.TaskbarTopFontTenths = Limit(Json.Num(d, "taskbar_top_font_tenths"), cfg.TaskbarTopFontTenths, 50, 240);
            cfg.TaskbarBottomFontTenths = Limit(Json.Num(d, "taskbar_bottom_font_tenths"), cfg.TaskbarBottomFontTenths, 50, 240);
            cfg.CardHeadingFontTenths = Limit(Json.Num(d, "card_heading_font_tenths"), cfg.CardHeadingFontTenths, 50, 240);
            cfg.CardTimeFontTenths = Limit(Json.Num(d, "card_time_font_tenths"), cfg.CardTimeFontTenths, 50, 240);
            cfg.CardTitleFontTenths = Limit(Json.Num(d, "card_title_font_tenths"), cfg.CardTitleFontTenths, 50, 240);
            cfg.CardNoteFontTenths = Limit(Json.Num(d, "card_note_font_tenths"), cfg.CardNoteFontTenths, 50, 240);
            cfg.CardDateNumberFontTenths = Limit(Json.Num(d, "card_date_number_font_tenths"), cfg.CardDateNumberFontTenths, 50, 240);
            cfg.CardDateMonthFontTenths = Limit(Json.Num(d, "card_date_month_font_tenths"), cfg.CardDateMonthFontTenths, 50, 240);
            cfg.CardDateDayFontTenths = Limit(Json.Num(d, "card_date_day_font_tenths"), cfg.CardDateDayFontTenths, 50, 240);
            cfg.StripSecondaryColor = Json.Str(d, "strip_secondary_color") ?? cfg.StripSecondaryColor;
            cfg.TimeGreenColor = Json.Str(d, "time_green_color") ?? cfg.TimeGreenColor;
            cfg.TimeAmberColor = Json.Str(d, "time_amber_color") ?? cfg.TimeAmberColor;
            cfg.CardHeadingColor = Json.Str(d, "card_heading_color") ?? cfg.CardHeadingColor;
            cfg.CardTimeColor = Json.Str(d, "card_time_color") ?? cfg.CardTimeColor;
            cfg.CardTextColor = Json.Str(d, "card_text_color") ?? cfg.CardTextColor;
            cfg.CardNoteColor = Json.Str(d, "card_note_color") ?? cfg.CardNoteColor;
            cfg.CardDateNumberColor = Json.Str(d, "card_date_number_color") ?? cfg.CardDateNumberColor;
            cfg.CardDateMonthColor = Json.Str(d, "card_date_month_color") ?? cfg.CardDateMonthColor;
            cfg.CardDateDayColor = Json.Str(d, "card_date_day_color") ?? cfg.CardDateDayColor;
            cfg.TaskbarTopBold = Json.Bool(d, "taskbar_top_bold") ?? cfg.TaskbarTopBold; cfg.TaskbarTopItalic = Json.Bool(d, "taskbar_top_italic") ?? cfg.TaskbarTopItalic; cfg.TaskbarTopUnderline = Json.Bool(d, "taskbar_top_underline") ?? cfg.TaskbarTopUnderline;
            cfg.TaskbarBottomBold = Json.Bool(d, "taskbar_bottom_bold") ?? cfg.TaskbarBottomBold; cfg.TaskbarBottomItalic = Json.Bool(d, "taskbar_bottom_italic") ?? cfg.TaskbarBottomItalic; cfg.TaskbarBottomUnderline = Json.Bool(d, "taskbar_bottom_underline") ?? cfg.TaskbarBottomUnderline;
            cfg.CardHeadingBold = Json.Bool(d, "card_heading_bold") ?? cfg.CardHeadingBold; cfg.CardHeadingItalic = Json.Bool(d, "card_heading_italic") ?? cfg.CardHeadingItalic; cfg.CardHeadingUnderline = Json.Bool(d, "card_heading_underline") ?? cfg.CardHeadingUnderline;
            cfg.CardTimeBold = Json.Bool(d, "card_time_bold") ?? cfg.CardTimeBold; cfg.CardTimeItalic = Json.Bool(d, "card_time_italic") ?? cfg.CardTimeItalic; cfg.CardTimeUnderline = Json.Bool(d, "card_time_underline") ?? cfg.CardTimeUnderline;
            cfg.CardTitleBold = Json.Bool(d, "card_title_bold") ?? cfg.CardTitleBold; cfg.CardTitleItalic = Json.Bool(d, "card_title_italic") ?? cfg.CardTitleItalic; cfg.CardTitleUnderline = Json.Bool(d, "card_title_underline") ?? cfg.CardTitleUnderline;
            cfg.CardNoteBold = Json.Bool(d, "card_note_bold") ?? cfg.CardNoteBold; cfg.CardNoteItalic = Json.Bool(d, "card_note_italic") ?? cfg.CardNoteItalic; cfg.CardNoteUnderline = Json.Bool(d, "card_note_underline") ?? cfg.CardNoteUnderline;
            cfg.CardDateNumberBold = Json.Bool(d, "card_date_number_bold") ?? cfg.CardDateNumberBold; cfg.CardDateNumberItalic = Json.Bool(d, "card_date_number_italic") ?? cfg.CardDateNumberItalic; cfg.CardDateNumberUnderline = Json.Bool(d, "card_date_number_underline") ?? cfg.CardDateNumberUnderline;
            cfg.CardDateMonthBold = Json.Bool(d, "card_date_month_bold") ?? cfg.CardDateMonthBold; cfg.CardDateMonthItalic = Json.Bool(d, "card_date_month_italic") ?? cfg.CardDateMonthItalic; cfg.CardDateMonthUnderline = Json.Bool(d, "card_date_month_underline") ?? cfg.CardDateMonthUnderline;
            cfg.CardDateDayBold = Json.Bool(d, "card_date_day_bold") ?? cfg.CardDateDayBold; cfg.CardDateDayItalic = Json.Bool(d, "card_date_day_italic") ?? cfg.CardDateDayItalic; cfg.CardDateDayUnderline = Json.Bool(d, "card_date_day_underline") ?? cfg.CardDateDayUnderline;
            cfg.BackgroundOpacity = Limit(Json.Num(d, "background_opacity"), cfg.BackgroundOpacity, 40, 255);
            cfg.NoStripBackground = Json.Bool(d, "no_strip_background") ?? cfg.NoStripBackground;
            cfg.CardShadow = Json.Bool(d, "card_shadow") ?? cfg.CardShadow;
            cfg.ShowCallout = Json.Bool(d, "show_callout") ?? cfg.ShowCallout;
            if (string.Equals(cfg.ThemePreset, "Notes", StringComparison.OrdinalIgnoreCase) && string.Equals(cfg.TimeAmberColor, "#FFD36A", StringComparison.OrdinalIgnoreCase)) cfg.TimeAmberColor = "#A8442F";
            if (string.Equals(cfg.ThemePreset, "Midnight", StringComparison.OrdinalIgnoreCase) && Json.Str(d, "card_text_color") == null)
            {
                cfg.StripSecondaryColor = "#C7DAFF"; cfg.TimeGreenColor = "#90F0AD"; cfg.TimeAmberColor = "#FFD978";
                cfg.CardHeadingColor = "#B1CDF5"; cfg.CardTimeColor = "#D5E4FF"; cfg.CardTextColor = "#F4F7FF"; cfg.CardNoteColor = "#C7DAFF";
            }
            if (string.Equals(cfg.ThemePreset, "Midnight", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.StripBackground, "#1C1C1C", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.StripForeground, "#FFFFFF", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.CardBackground, "#FFFBEA", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.CardLine, "#D8D1BA", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.AccentColor, "#C85A5A", StringComparison.OrdinalIgnoreCase))
                Theme.ApplyPreset(cfg, "Midnight");
            if (string.Equals(cfg.ThemePreset, "Notes", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.StripBackground, "#1C1C1C", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.StripForeground, "#FFFFFF", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.CardBackground, "#FFFBEA", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.CardLine, "#D8D1BA", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.AccentColor, "#C85A5A", StringComparison.OrdinalIgnoreCase))
                Theme.ApplyPreset(cfg, "Notes");
            // Migrate the earlier cream Notes defaults to the current Notes preset
            // without touching the user's text formatting.
            if (string.Equals(cfg.ThemePreset, "Notes", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.CardBackground, "#FFF1BD", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.CardLine, "#D6C17A", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.AccentColor, "#B64A43", StringComparison.OrdinalIgnoreCase))
            {
                cfg.CardBackground = "#FFF6CF"; cfg.CardLine = "#989758"; cfg.AccentColor = "#BF975B";
            }
            // Lighten the shipped Notes paper for existing installations that
            // still use the exact previous preset colours.  Custom paper
            // colours remain untouched.
            if (string.Equals(cfg.ThemePreset, "Notes", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.CardBackground, "#FFF1BD", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.CardLine, "#989758", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cfg.AccentColor, "#BF975B", StringComparison.OrdinalIgnoreCase))
                cfg.CardBackground = "#FFF6CF";
            return cfg;
        }

        static int Limit(double? value, int fallback, int min, int max) =>
            value == null ? fallback : Math.Max(min, Math.Min(max, (int)value.Value));

        static T ReadEnum<T>(string value, T fallback) where T : struct
            => Enum.TryParse(value, true, out T parsed) ? parsed : fallback;

        public void Save() => Save(Paths.ConfigFile);
        internal void Save(string path)
        {
            var d = new Dictionary<string, object>
            {
                ["refresh_minutes"] = RefreshMinutes, ["lookahead_days"] = LookaheadDays,
                ["show_calendar"] = ShowCalendar, ["width"] = Width, ["auto_width"] = AutoWidth, ["right_gap"] = RightGap, ["x"] = X,
                ["layout"] = Layout.ToString(), ["use_24_hour_clock"] = Use24HourClock, ["amber_threshold_minutes"] = AmberThresholdMinutes, ["late_alert_enabled"] = LateAlertEnabled, ["skip_cancelled_meetings"] = SkipCancelledMeetings,
                ["fade_delay_ms"] = FadeDelayMilliseconds, ["fade_duration_ms"] = FadeDurationMilliseconds,
                ["appear_start_percent"] = AppearStartPercent, ["appear_duration_ms"] = AppearDurationMilliseconds,
                ["click_action"] = ClickAction.ToString(), ["double_click_action"] = DoubleClickAction.ToString(), ["hover_action"] = HoverAction.ToString(),
                ["empty_day_tone"] = EmptyDayTone.ToString(), ["empty_day_message"] = EmptyDayMessage,
                ["teams_privacy_enabled"] = TeamsPrivacyEnabled, ["manual_privacy"] = ManualPrivacy,
                ["privacy_display"] = PrivacyDisplay.ToString(), ["theme_preset"] = ThemePreset,
                ["strip_background"] = StripBackground, ["strip_foreground"] = StripForeground,
                ["auto_taskbar_text_colors"] = AutoTaskbarTextColors,
                ["card_background"] = CardBackground, ["card_line"] = CardLine, ["accent_color"] = AccentColor,
                ["rule_line_thickness"] = RuleLineThickness, ["margin_line_thickness"] = MarginLineThickness, ["margin_line_gap"] = MarginLineGap, ["double_margin_line"] = DoubleMarginLine, ["card_corner_radius"] = CardCornerRadius,
                ["card_date_number_offset_x"] = CardDateNumberOffsetX, ["card_date_number_offset_y"] = CardDateNumberOffsetY, ["card_date_month_offset_x"] = CardDateMonthOffsetX, ["card_date_month_offset_y"] = CardDateMonthOffsetY, ["card_date_day_offset_x"] = CardDateDayOffsetX, ["card_date_day_offset_y"] = CardDateDayOffsetY,
                ["font_family"] = FontFamily, ["background_opacity"] = BackgroundOpacity, ["no_strip_background"] = NoStripBackground,
                ["taskbar_top_font_family"] = TextFontFamily(TextElement.TaskbarTop), ["taskbar_bottom_font_family"] = TextFontFamily(TextElement.TaskbarBottom), ["card_heading_font_family"] = TextFontFamily(TextElement.CardHeading), ["card_time_font_family"] = TextFontFamily(TextElement.CardTime), ["card_title_font_family"] = TextFontFamily(TextElement.CardTitle), ["card_note_font_family"] = TextFontFamily(TextElement.CardNote), ["card_date_number_font_family"] = TextFontFamily(TextElement.CardDateNumber), ["card_date_month_font_family"] = TextFontFamily(TextElement.CardDateMonth), ["card_date_day_font_family"] = TextFontFamily(TextElement.CardDateDay),
                ["taskbar_top_font_tenths"] = TextFontTenths(TextElement.TaskbarTop), ["taskbar_bottom_font_tenths"] = TextFontTenths(TextElement.TaskbarBottom), ["card_heading_font_tenths"] = TextFontTenths(TextElement.CardHeading), ["card_time_font_tenths"] = TextFontTenths(TextElement.CardTime), ["card_title_font_tenths"] = TextFontTenths(TextElement.CardTitle), ["card_note_font_tenths"] = TextFontTenths(TextElement.CardNote), ["card_date_number_font_tenths"] = TextFontTenths(TextElement.CardDateNumber), ["card_date_month_font_tenths"] = TextFontTenths(TextElement.CardDateMonth), ["card_date_day_font_tenths"] = TextFontTenths(TextElement.CardDateDay),
                ["strip_secondary_color"] = StripSecondaryColor, ["time_green_color"] = TimeGreenColor, ["time_amber_color"] = TimeAmberColor,
                ["card_heading_color"] = CardHeadingColor, ["card_time_color"] = CardTimeColor, ["card_text_color"] = CardTextColor, ["card_note_color"] = CardNoteColor, ["card_date_number_color"] = CardDateNumberColor, ["card_date_month_color"] = CardDateMonthColor, ["card_date_day_color"] = CardDateDayColor,
                ["taskbar_top_bold"] = TaskbarTopBold, ["taskbar_top_italic"] = TaskbarTopItalic, ["taskbar_top_underline"] = TaskbarTopUnderline,
                ["taskbar_bottom_bold"] = TaskbarBottomBold, ["taskbar_bottom_italic"] = TaskbarBottomItalic, ["taskbar_bottom_underline"] = TaskbarBottomUnderline,
                ["card_heading_bold"] = CardHeadingBold, ["card_heading_italic"] = CardHeadingItalic, ["card_heading_underline"] = CardHeadingUnderline,
                ["card_time_bold"] = CardTimeBold, ["card_time_italic"] = CardTimeItalic, ["card_time_underline"] = CardTimeUnderline,
                ["card_title_bold"] = CardTitleBold, ["card_title_italic"] = CardTitleItalic, ["card_title_underline"] = CardTitleUnderline,
                ["card_note_bold"] = CardNoteBold, ["card_note_italic"] = CardNoteItalic, ["card_note_underline"] = CardNoteUnderline,
                ["card_date_number_bold"] = CardDateNumberBold, ["card_date_number_italic"] = CardDateNumberItalic, ["card_date_number_underline"] = CardDateNumberUnderline,
                ["card_date_month_bold"] = CardDateMonthBold, ["card_date_month_italic"] = CardDateMonthItalic, ["card_date_month_underline"] = CardDateMonthUnderline,
                ["card_date_day_bold"] = CardDateDayBold, ["card_date_day_italic"] = CardDateDayItalic, ["card_date_day_underline"] = CardDateDayUnderline,
                ["card_shadow"] = CardShadow, ["show_callout"] = ShowCallout,
            };
            FileUtil.AtomicWrite(path, Json.Serialize(d, indent: true));
        }

        public Config Clone()
        {
            return (Config)MemberwiseClone();
        }

        public string TextColor(TextElement element)
        {
            switch (element)
            {
                case TextElement.TaskbarTop: return StripForeground;
                case TextElement.TaskbarBottom: return StripSecondaryColor;
                case TextElement.CardHeading: return CardHeadingColor;
                case TextElement.CardTime: return CardTimeColor;
                case TextElement.CardTitle: return CardTextColor;
                case TextElement.CardNote: return CardNoteColor;
                case TextElement.CardDateNumber: return CardDateNumberColor;
                case TextElement.CardDateMonth: return CardDateMonthColor;
                default: return CardDateDayColor;
            }
        }

        public void SetTextColor(TextElement element, string color)
        {
            switch (element)
            {
                case TextElement.TaskbarTop: StripForeground = color; break;
                case TextElement.TaskbarBottom: StripSecondaryColor = color; break;
                case TextElement.CardHeading: CardHeadingColor = color; break;
                case TextElement.CardTime: CardTimeColor = color; break;
                case TextElement.CardTitle: CardTextColor = color; break;
                case TextElement.CardNote: CardNoteColor = color; break;
                case TextElement.CardDateNumber: CardDateNumberColor = color; break;
                case TextElement.CardDateMonth: CardDateMonthColor = color; break;
                default: CardDateDayColor = color; break;
            }
        }

        public string TextFontFamily(TextElement element)
        {
            string value;
            switch (element)
            {
                case TextElement.TaskbarTop: value = TaskbarTopFontFamily; break;
                case TextElement.TaskbarBottom: value = TaskbarBottomFontFamily; break;
                case TextElement.CardHeading: value = CardHeadingFontFamily; break;
                case TextElement.CardTime: value = CardTimeFontFamily; break;
                case TextElement.CardTitle: value = CardTitleFontFamily; break;
                case TextElement.CardNote: value = CardNoteFontFamily; break;
                case TextElement.CardDateNumber: value = CardDateNumberFontFamily; break;
                case TextElement.CardDateMonth: value = CardDateMonthFontFamily; break;
                default: value = CardDateDayFontFamily; break;
            }
            return string.IsNullOrWhiteSpace(value) ? FontFamily : value;
        }

        public int TextFontTenths(TextElement element)
        {
            switch (element)
            {
                case TextElement.TaskbarTop: return TaskbarTopFontTenths;
                case TextElement.TaskbarBottom: return TaskbarBottomFontTenths;
                case TextElement.CardHeading: return CardHeadingFontTenths;
                case TextElement.CardTime: return CardTimeFontTenths;
                case TextElement.CardTitle: return CardTitleFontTenths;
                case TextElement.CardNote: return CardNoteFontTenths;
                case TextElement.CardDateNumber: return CardDateNumberFontTenths;
                case TextElement.CardDateMonth: return CardDateMonthFontTenths;
                default: return CardDateDayFontTenths;
            }
        }

        public void SetTextFont(TextElement element, string family, float size)
        {
            var tenths = Math.Max(50, Math.Min(240, (int)Math.Round(size * 10)));
            switch (element)
            {
                case TextElement.TaskbarTop: TaskbarTopFontFamily = family; TaskbarTopFontTenths = tenths; break;
                case TextElement.TaskbarBottom: TaskbarBottomFontFamily = family; TaskbarBottomFontTenths = tenths; break;
                case TextElement.CardHeading: CardHeadingFontFamily = family; CardHeadingFontTenths = tenths; break;
                case TextElement.CardTime: CardTimeFontFamily = family; CardTimeFontTenths = tenths; break;
                case TextElement.CardTitle: CardTitleFontFamily = family; CardTitleFontTenths = tenths; break;
                case TextElement.CardNote: CardNoteFontFamily = family; CardNoteFontTenths = tenths; break;
                case TextElement.CardDateNumber: CardDateNumberFontFamily = family; CardDateNumberFontTenths = tenths; break;
                case TextElement.CardDateMonth: CardDateMonthFontFamily = family; CardDateMonthFontTenths = tenths; break;
                default: CardDateDayFontFamily = family; CardDateDayFontTenths = tenths; break;
            }
        }

        public FontStyle TextStyle(TextElement element)
        {
            bool bold, italic, underline;
            switch (element)
            {
                case TextElement.TaskbarTop: bold = TaskbarTopBold; italic = TaskbarTopItalic; underline = TaskbarTopUnderline; break;
                case TextElement.TaskbarBottom: bold = TaskbarBottomBold; italic = TaskbarBottomItalic; underline = TaskbarBottomUnderline; break;
                case TextElement.CardHeading: bold = CardHeadingBold; italic = CardHeadingItalic; underline = CardHeadingUnderline; break;
                case TextElement.CardTime: bold = CardTimeBold; italic = CardTimeItalic; underline = CardTimeUnderline; break;
                case TextElement.CardTitle: bold = CardTitleBold; italic = CardTitleItalic; underline = CardTitleUnderline; break;
                case TextElement.CardNote: bold = CardNoteBold; italic = CardNoteItalic; underline = CardNoteUnderline; break;
                case TextElement.CardDateNumber: bold = CardDateNumberBold; italic = CardDateNumberItalic; underline = CardDateNumberUnderline; break;
                case TextElement.CardDateMonth: bold = CardDateMonthBold; italic = CardDateMonthItalic; underline = CardDateMonthUnderline; break;
                default: bold = CardDateDayBold; italic = CardDateDayItalic; underline = CardDateDayUnderline; break;
            }
            return (bold ? FontStyle.Bold : FontStyle.Regular) | (italic ? FontStyle.Italic : FontStyle.Regular) | (underline ? FontStyle.Underline : FontStyle.Regular);
        }

        public void SetTextStyle(TextElement element, bool bold, bool italic, bool underline)
        {
            switch (element)
            {
                case TextElement.TaskbarTop: TaskbarTopBold = bold; TaskbarTopItalic = italic; TaskbarTopUnderline = underline; break;
                case TextElement.TaskbarBottom: TaskbarBottomBold = bold; TaskbarBottomItalic = italic; TaskbarBottomUnderline = underline; break;
                case TextElement.CardHeading: CardHeadingBold = bold; CardHeadingItalic = italic; CardHeadingUnderline = underline; break;
                case TextElement.CardTime: CardTimeBold = bold; CardTimeItalic = italic; CardTimeUnderline = underline; break;
                case TextElement.CardTitle: CardTitleBold = bold; CardTitleItalic = italic; CardTitleUnderline = underline; break;
                case TextElement.CardNote: CardNoteBold = bold; CardNoteItalic = italic; CardNoteUnderline = underline; break;
                case TextElement.CardDateNumber: CardDateNumberBold = bold; CardDateNumberItalic = italic; CardDateNumberUnderline = underline; break;
                case TextElement.CardDateMonth: CardDateMonthBold = bold; CardDateMonthItalic = italic; CardDateMonthUnderline = underline; break;
                default: CardDateDayBold = bold; CardDateDayItalic = italic; CardDateDayUnderline = underline; break;
            }
        }

        public Point DateStampOffset(TextElement element)
        {
            switch (element)
            {
                case TextElement.CardDateNumber: return new Point(CardDateNumberOffsetX, CardDateNumberOffsetY);
                case TextElement.CardDateMonth: return new Point(CardDateMonthOffsetX, CardDateMonthOffsetY);
                case TextElement.CardDateDay: return new Point(CardDateDayOffsetX, CardDateDayOffsetY);
                default: return Point.Empty;
            }
        }

        public void SetDateStampOffset(TextElement element, int x, int y)
        {
            x = Math.Max(-400, Math.Min(400, x)); y = Math.Max(-400, Math.Min(400, y));
            switch (element)
            {
                case TextElement.CardDateNumber: CardDateNumberOffsetX = x; CardDateNumberOffsetY = y; break;
                case TextElement.CardDateMonth: CardDateMonthOffsetX = x; CardDateMonthOffsetY = y; break;
                case TextElement.CardDateDay: CardDateDayOffsetX = x; CardDateDayOffsetY = y; break;
            }
        }
    }
}
