using System;
using System.Drawing;

namespace TaskbarCalendar
{
    sealed class WidgetTheme
    {
        public Color StripBackground, StripForeground, StripSecondary, TimeGreen, TimeAmber, CardBackground, CardLine, Accent, CardText, CardHeading, CardTime, CardNote, CardDateNumber, CardDateMonth, CardDateDay;
        public bool Shadow, Callout;
        public bool IsNotes;
    }

    sealed class TaskbarTextPalette
    {
        const double MinimumContrast = 4.5;
        static readonly Color LightPrimary = Color.FromArgb(32, 29, 24);
        static readonly Color LightSecondary = Color.FromArgb(86, 78, 66);
        static readonly Color LightGreen = Color.FromArgb(28, 112, 62);
        static readonly Color LightAmber = Color.FromArgb(140, 55, 35);

        public Color Primary, Secondary, Green, Amber;

        public static TaskbarTextPalette From(Config cfg, WidgetTheme theme, Color surface)
        {
            if (!cfg.AutoTaskbarTextColors)
                return new TaskbarTextPalette { Primary = theme.StripForeground, Secondary = theme.StripSecondary, Green = theme.TimeGreen, Amber = theme.TimeAmber };

            var lightSurface = RelativeLuminance(surface) >= 0.45;
            return new TaskbarTextPalette
            {
                Primary = Readable(theme.StripForeground, surface, lightSurface ? LightPrimary : Color.White),
                Secondary = Readable(theme.StripSecondary, surface, lightSurface ? LightSecondary : Color.Gainsboro),
                Green = Readable(theme.TimeGreen, surface, lightSurface ? LightGreen : Color.FromArgb(131, 230, 156)),
                Amber = Readable(theme.TimeAmber, surface, lightSurface ? LightAmber : Color.FromArgb(255, 211, 106)),
            };
        }

        public static double ContrastRatio(Color first, Color second)
        {
            var firstLum = RelativeLuminance(first);
            var secondLum = RelativeLuminance(second);
            return (Math.Max(firstLum, secondLum) + 0.05) / (Math.Min(firstLum, secondLum) + 0.05);
        }

        static Color Readable(Color requested, Color surface, Color fallback)
        {
            if (ContrastRatio(requested, surface) >= MinimumContrast) return requested;
            if (ContrastRatio(fallback, surface) >= MinimumContrast) return fallback;
            return ContrastRatio(Color.Black, surface) >= ContrastRatio(Color.White, surface) ? Color.Black : Color.White;
        }

        static double RelativeLuminance(Color color)
        {
            double channel(byte value)
            {
                var normalised = value / 255d;
                return normalised <= 0.03928 ? normalised / 12.92 : Math.Pow((normalised + 0.055) / 1.055, 2.4);
            }

            return 0.2126 * channel(color.R) + 0.7152 * channel(color.G) + 0.0722 * channel(color.B);
        }
    }

    static class Theme
    {
        public static WidgetTheme From(Config cfg)
        {
            var theme = new WidgetTheme
            {
                StripBackground = Parse(cfg.StripBackground, Color.FromArgb(0x4b, 0x39, 0x2c)),
                StripForeground = Parse(cfg.StripForeground, Color.FromArgb(0xff, 0xf7, 0xe1)),
                StripSecondary = Parse(cfg.StripSecondaryColor, Color.FromArgb(0xf3, 0xe5, 0xb4)),
                TimeGreen = Parse(cfg.TimeGreenColor, Color.FromArgb(0x83, 0xe6, 0x9c)),
                TimeAmber = Parse(cfg.TimeAmberColor, Color.FromArgb(0xff, 0xd3, 0x6a)),
                CardBackground = Parse(cfg.CardBackground, Color.FromArgb(0xff, 0xf6, 0xcf)),
                CardLine = Parse(cfg.CardLine, Color.FromArgb(0xd8, 0xd5, 0x8b)),
                Accent = Parse(cfg.AccentColor, Color.FromArgb(0xc8, 0x64, 0x4b)),
                CardText = Parse(cfg.CardTextColor, Color.FromArgb(55, 45, 30)), CardHeading = Parse(cfg.CardHeadingColor, Color.FromArgb(105, 76, 44)),
                CardTime = Parse(cfg.CardTimeColor, Color.FromArgb(58, 43, 29)), CardNote = Parse(cfg.CardNoteColor, Color.FromArgb(96, 83, 58)),
                CardDateNumber = Parse(cfg.CardDateNumberColor, Color.FromArgb(55, 45, 30)), CardDateMonth = Parse(cfg.CardDateMonthColor, Color.FromArgb(105, 76, 44)), CardDateDay = Parse(cfg.CardDateDayColor, Color.FromArgb(96, 83, 58)),
                Shadow = cfg.CardShadow, Callout = cfg.ShowCallout, IsNotes = string.Equals(cfg.ThemePreset, "Notes", StringComparison.OrdinalIgnoreCase),
            };
            if (string.Equals(cfg.ThemePreset, "Midnight", StringComparison.OrdinalIgnoreCase))
            {
                theme.CardText = Parse(cfg.CardTextColor, Color.FromArgb(244, 247, 255)); theme.CardHeading = Parse(cfg.CardHeadingColor, Color.FromArgb(177, 205, 245));
            }
            return theme;
        }

        public static void ApplyPreset(Config cfg, string preset)
        {
            cfg.ThemePreset = preset;
            if (string.Equals(preset, "Midnight", StringComparison.OrdinalIgnoreCase))
            {
                cfg.StripBackground = "#182132"; cfg.StripForeground = "#F4F7FF"; cfg.CardBackground = "#202B40";
                cfg.CardLine = "#5F7BA7"; cfg.AccentColor = "#82D173";
                cfg.StripSecondaryColor = "#C7DAFF"; cfg.TimeGreenColor = "#90F0AD"; cfg.TimeAmberColor = "#FFD978";
                cfg.CardHeadingColor = "#B1CDF5"; cfg.CardTimeColor = "#D5E4FF"; cfg.CardTextColor = "#F4F7FF"; cfg.CardNoteColor = "#C7DAFF"; cfg.CardDateNumberColor = "#F4F7FF"; cfg.CardDateMonthColor = "#B1CDF5"; cfg.CardDateDayColor = "#C7DAFF";
            }
            else if (string.Equals(preset, "Taskbar", StringComparison.OrdinalIgnoreCase))
            {
                cfg.StripBackground = "#1C1C1C"; cfg.StripForeground = "#FFFFFF"; cfg.CardBackground = "#F4F4F4";
                cfg.CardLine = "#B8B8B8"; cfg.AccentColor = "#4A78B8";
                cfg.StripSecondaryColor = "#E7E7E7"; cfg.TimeGreenColor = "#75D88B"; cfg.TimeAmberColor = "#FFD166";
                cfg.CardHeadingColor = "#354A67"; cfg.CardTimeColor = "#333333"; cfg.CardTextColor = "#222222"; cfg.CardNoteColor = "#555555"; cfg.CardDateNumberColor = "#222222"; cfg.CardDateMonthColor = "#354A67"; cfg.CardDateDayColor = "#555555";
            }
            else
            {
                cfg.StripBackground = "#4B392C"; cfg.StripForeground = "#FFF7E1"; cfg.CardBackground = "#FFF6CF";
                cfg.CardLine = "#989758"; cfg.AccentColor = "#BF975B";
                cfg.StripSecondaryColor = "#F3E5B4"; cfg.TimeGreenColor = "#83E69C"; cfg.TimeAmberColor = "#A8442F";
                cfg.CardHeadingColor = "#694C2C"; cfg.CardTimeColor = "#3A2B1D"; cfg.CardTextColor = "#372D1E"; cfg.CardNoteColor = "#60533A"; cfg.CardDateNumberColor = "#372D1E"; cfg.CardDateMonthColor = "#694C2C"; cfg.CardDateDayColor = "#60533A";
                cfg.RuleLineThickness = 1; cfg.MarginLineThickness = 1; cfg.MarginLineGap = 2; cfg.DoubleMarginLine = true; cfg.CardCornerRadius = 5;
                cfg.BackgroundOpacity = 40; cfg.NoStripBackground = true; cfg.CardShadow = true; cfg.ShowCallout = true;
                cfg.FontFamily = "Segoe UI";
                cfg.TaskbarTopFontFamily = cfg.TaskbarBottomFontFamily = cfg.CardHeadingFontFamily = cfg.CardTimeFontFamily = cfg.CardTitleFontFamily = cfg.CardNoteFontFamily = cfg.CardDateNumberFontFamily = cfg.CardDateMonthFontFamily = cfg.CardDateDayFontFamily = "Segoe UI";
                cfg.TaskbarTopFontTenths = cfg.TaskbarBottomFontTenths = cfg.CardHeadingFontTenths = cfg.CardTimeFontTenths = cfg.CardTitleFontTenths = cfg.CardNoteFontTenths = 82; cfg.CardDateNumberFontTenths = 240; cfg.CardDateMonthFontTenths = 90; cfg.CardDateDayFontTenths = 90;
                cfg.TaskbarTopBold = true; cfg.TaskbarTopItalic = false; cfg.TaskbarTopUnderline = false;
                cfg.TaskbarBottomBold = false; cfg.TaskbarBottomItalic = true; cfg.TaskbarBottomUnderline = false;
                cfg.CardHeadingBold = true; cfg.CardHeadingItalic = false; cfg.CardHeadingUnderline = false;
                cfg.CardTimeBold = false; cfg.CardTimeItalic = false; cfg.CardTimeUnderline = false;
                cfg.CardTitleBold = true; cfg.CardTitleItalic = false; cfg.CardTitleUnderline = false;
                cfg.CardNoteBold = false; cfg.CardNoteItalic = true; cfg.CardNoteUnderline = false;
                cfg.CardDateNumberBold = false; cfg.CardDateNumberItalic = false; cfg.CardDateNumberUnderline = false;
                cfg.CardDateMonthBold = true; cfg.CardDateMonthItalic = false; cfg.CardDateMonthUnderline = false;
                cfg.CardDateDayBold = false; cfg.CardDateDayItalic = true; cfg.CardDateDayUnderline = false;
                cfg.CardDateNumberOffsetX = 16; cfg.CardDateNumberOffsetY = -29;
                cfg.CardDateMonthOffsetX = -1; cfg.CardDateMonthOffsetY = 12;
                cfg.CardDateDayOffsetX = -1; cfg.CardDateDayOffsetY = 10;
            }
        }

        static Color Parse(string value, Color fallback) { try { return ColorTranslator.FromHtml(value); } catch { return fallback; } }
    }
}
