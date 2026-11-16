using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Xunit;

namespace TaskbarCalendar.Tests
{
    public class CalendarAndPrivacyTests
    {
        static Meeting Meeting(string title, int start, int end) => new Meeting { Title = title, Start = new DateTime(2026, 1, 2, 10, 0, 0).AddMinutes(start), End = new DateTime(2026, 1, 2, 10, 0, 0).AddMinutes(end) };

        [Fact]
        public void CalendarActiveMeetingWinsOverUpcomingMeeting()
        {
            var now = new DateTime(2026, 1, 2, 10, 10, 0);
            var picked = CalendarFeed.PickNext(new[] { Meeting("Next", 15, 45), Meeting("Active", -5, 20) }, now);
            Assert.Equal("Active", picked.Title);
        }

        [Fact]
        public void ConcurrentActiveMeetingsAreAllReported()
        {
            var now = new DateTime(2026, 1, 2, 10, 10, 0);
            var active = CalendarFeed.ActiveMeetings(new[] { Meeting("A", -5, 20), Meeting("B", 0, 40), Meeting("Later", 60, 90) }, now);
            Assert.Equal(2, active.Count);
        }

        [Theory]
        [InlineData(-1, ColorState.Neutral)]
        [InlineData(3, ColorState.Green)]
        [InlineData(6, ColorState.Amber)]
        public void StartColourUsesTheConfiguredGracePeriod(int minutesAfterStart, ColorState expected)
        {
            var meeting = Meeting("A", 0, 30); var now = meeting.Start.AddMinutes(minutesAfterStart);
            Assert.Equal(expected, CalendarFeed.StartState(meeting, now, 5));
        }

        [Fact]
        public void DisabledLateAlertKeepsCurrentMeetingGreen()
        {
            var meeting = Meeting("A", 0, 30);
            Assert.Equal(ColorState.Green, CalendarFeed.StartState(meeting, meeting.Start.AddMinutes(20), 0, false));
        }

        [Fact]
        public void CompletedMeetingsUseTheNeutralTimeColour()
        {
            var meeting = Meeting("Earlier", -30, -10);
            Assert.Equal(ColorState.Neutral, CalendarFeed.StartState(meeting, new System.DateTime(2026, 1, 2, 10, 10, 0), 5));
        }

        [Fact]
        public void TeamsPresentationBorderRecognisesTheTeamsRedOutline()
        {
            Assert.True(TeamsPresentationBorder.IsTeamsPresentationRed(System.Drawing.Color.FromArgb(196, 49, 75)));
            Assert.False(TeamsPresentationBorder.IsTeamsPresentationRed(System.Drawing.Color.FromArgb(45, 45, 45)));
        }

        [Fact]
        public void CancelledMeetingsAreSkippedByTheWidgetSelector()
        {
            var now = new DateTime(2026, 1, 2, 10, 10, 0);
            var meetings = new[] { Meeting("Canceled: late sync", 20, 50), Meeting("Review", 60, 90) };
            Assert.Equal("Review", CalendarFeed.PickNext(meetings, now, true).Title);
            var cancelledThenTomorrow = new[] { Meeting("Canceled: late sync", 20, 50), Meeting("Tomorrow review", 24 * 60 + 20, 24 * 60 + 50) };
            Assert.Null(CalendarFeed.PickNext(cancelledThenTomorrow, now, true));
            Assert.True(CalendarFeed.CancelledTodayWasSkipped(cancelledThenTomorrow, now, true));
        }

        [Fact]
        public void TaskbarShowsTheFreeDayMessageInsteadOfTomorrowsCancelledMeeting()
        {
            var now = new DateTime(2026, 1, 2, 16, 0, 0);
            var tomorrow = new Meeting { Title = "Cancelled: tomorrow sync", Start = now.Date.AddDays(1).AddHours(9), End = now.Date.AddDays(1).AddHours(10) };
            Assert.Null(CalendarFeed.PickToday(new[] { tomorrow }, now, true));
            Assert.NotNull(CalendarFeed.PickNext(new[] { tomorrow }, now, true));
        }

        [Fact]
        public void EarlierTodayRowsStayCollapsedUntilRequested()
        {
            var now = new DateTime(2026, 1, 2, 10, 10, 0);
            var meetings = new[] { Meeting("Earlier review", -60, -30), Meeting("Next review", 20, 50) };
            var collapsed = PeekForm.BuildRows(meetings, now, new Config(), false);
            var expanded = PeekForm.BuildRows(meetings, now, new Config(), false, true);
            Assert.Contains(collapsed, row => row.IsPreviousToggle && row.Left.StartsWith("▶ Earlier today"));
            Assert.DoesNotContain(collapsed, row => row.Right == "Earlier review");
            Assert.Contains(expanded, row => row.IsPreviousToggle && row.Left.StartsWith("▼ Earlier today"));
            Assert.Contains(expanded, row => row.Right == "Earlier review");
        }

        [Fact]
        public void CalloutTopIsFilledAsPartOfThePaperSilhouette()
        {
            var cfg = new Config { ThemePreset = "Notes", ShowCallout = true };
            using (var font = new Font("Segoe UI", 8.2f))
            using (var bitmap = new Bitmap(340, 180, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var renderer = new PeekCardRenderer(font, font, font, font, Theme.From(cfg), cfg, 1);
                var card = new Rectangle(8, 8, 300, 140);
                const int calloutX = 275;
                using (var shape = renderer.CreateSilhouettePath(card, card.Bottom + 10, calloutX))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.FillPath(Brushes.Red, shape);
                    Assert.NotEqual(0, bitmap.GetPixel(calloutX, card.Bottom).A);
                }
            }
        }

        [Fact]
        public void NotesOuterSilhouetteUsesTheLeatherCapsRoundedTopCorners()
        {
            var cfg = new Config { ThemePreset = "Notes", ShowCallout = true };
            using (var font = new Font("Segoe UI", 8.2f))
            using (var bitmap = new Bitmap(340, 180, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var renderer = new PeekCardRenderer(font, font, font, font, Theme.From(cfg), cfg, 1);
                var card = new Rectangle(8, 8, 300, 140);
                using (var shape = renderer.CreateSilhouettePath(card, card.Bottom + 10, 275))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.FillPath(Brushes.Red, shape);
                    Assert.Equal(0, bitmap.GetPixel(card.Left, card.Top).A);
                    Assert.Equal(0, bitmap.GetPixel(card.Right - 1, card.Top).A);
                    Assert.NotEqual(0, bitmap.GetPixel(card.Left + cfg.CardCornerRadius, card.Top).A);
                    Assert.NotEqual(0, bitmap.GetPixel(card.Right - cfg.CardCornerRadius - 1, card.Top).A);
                }
            }
        }

        [Fact]
        public void PrivacyFailsClosedForAnUnreadableActiveCall()
        {
            Assert.True(PrivacyState.IsPrivate(false, true, TeamsShareState.Unknown));
            Assert.True(PrivacyState.IsPrivate(false, true, TeamsShareState.Presenting));
            Assert.False(PrivacyState.IsPrivate(false, true, TeamsShareState.NotPresenting));
            Assert.True(PrivacyState.IsPrivate(true, false, TeamsShareState.NotPresenting));
        }

        [Fact]
        public void BusyRowsNeverContainMeetingTitles()
        {
            var cfg = new Config(); var now = new DateTime(2026, 1, 2, 10, 10, 0);
            var rows = PeekForm.BuildRows(new[] { Meeting("Acquisition discussion", -5, 20) }, now, cfg, true);
            Assert.DoesNotContain(rows, row => (row.Right ?? "").Contains("Acquisition"));
            Assert.Contains(rows, row => row.Right == "Busy");
        }

        [Fact]
        public void DetailRowsOnlyExposeJoinForUpcomingOrActiveJoinableMeetings()
        {
            var now = new DateTime(2026, 1, 2, 10, 10, 0);
            var joinable = Meeting("Design review", -5, 20); joinable.Link = "https://teams.microsoft.com/l/meetup-join/example";
            var cancelled = Meeting("Canceled: daily sync", 30, 60); cancelled.Link = "https://teams.microsoft.com/l/meetup-join/cancelled";
            var earlier = Meeting("Earlier review", -60, -30); earlier.Link = "https://teams.microsoft.com/l/meetup-join/earlier";

            var regular = PeekForm.BuildRows(new[] { joinable, cancelled, earlier }, now, new Config(), false, true);
            Assert.Contains(regular, row => row.Right == "Design review" && row.JoinLink == joinable.Link);
            Assert.DoesNotContain(regular, row => row.Right == "Canceled: daily sync" && row.JoinLink != null);
            Assert.DoesNotContain(regular, row => row.Right == "Earlier review" && row.JoinLink != null);

            var privateRows = PeekForm.BuildRows(new[] { joinable }, now, new Config(), true);
            Assert.DoesNotContain(privateRows, row => row.JoinLink != null);
            Assert.Equal(16, LegacyIcons.JoinImage.Width);
        }

        [Fact]
        public void DetailCardShowsPhysicalRoomButNotGenericTeamsLocation()
        {
            var meeting = Meeting("Review", 0, 30); meeting.Location = "Microsoft Teams Meeting; Atlas 3A";
            var cfg = new Config(); var now = meeting.Start.AddMinutes(5);
            Assert.Equal("Atlas 3A", CalendarFeed.RoomLocation(meeting));
            Assert.Contains(PeekForm.BuildRows(new[] { meeting }, now, cfg, false), row => row.Left == "Room: Atlas 3A" && row.IndentToTitle);
            Assert.DoesNotContain(PeekForm.BuildRows(new[] { meeting }, now, cfg, true), row => (row.Left ?? "").Contains("Atlas 3A"));
        }

        [Fact]
        public void DetailCardKeepsTimingInTheNormalPaperColour()
        {
            var meeting = Meeting("Review", -3, 30);
            var rows = PeekForm.BuildRows(new[] { meeting }, meeting.Start.AddMinutes(3), new Config(), false);
            Assert.All(rows.Where(row => row.Kind == PeekForm.Kind.Row), row => Assert.Null(row.TimeState));
        }
    }

    public class ConfigTests
    {
        [Fact]
        public void NotesDefaultsMatchTheShippedSegoeUiStyleAndDatePlacement()
        {
            var cfg = new Config();
            Assert.Equal("Notes", cfg.ThemePreset);
            Assert.Equal("Segoe UI", cfg.FontFamily);
            Assert.All(new[]
            {
                TextElement.TaskbarTop, TextElement.TaskbarBottom, TextElement.CardHeading, TextElement.CardTime, TextElement.CardTitle,
                TextElement.CardNote, TextElement.CardDateNumber, TextElement.CardDateMonth, TextElement.CardDateDay
            }, element => Assert.Equal("Segoe UI", cfg.TextFontFamily(element)));
            Assert.Equal(new Point(16, -29), cfg.DateStampOffset(TextElement.CardDateNumber));
            Assert.Equal(new Point(-1, 12), cfg.DateStampOffset(TextElement.CardDateMonth));
            Assert.Equal(new Point(-1, 10), cfg.DateStampOffset(TextElement.CardDateDay));
        }

        [Fact]
        public void LegacyGoogleAndClaudeKeysAreDiscardedOnSave()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "{\"ics_urls\":[\"https://example.test/private.ics\"],\"show_claude\":true,\"layout\":\"TimeFirst\",\"amber_threshold_minutes\":12}");
            var cfg = Config.Load(path); cfg.Save(path); var saved = File.ReadAllText(path);
            Assert.Equal(StripLayout.TimeFirst, cfg.Layout); Assert.Equal(12, cfg.AmberThresholdMinutes);
            Assert.DoesNotContain("ics_urls", saved); Assert.DoesNotContain("show_claude", saved);
            File.Delete(path);
        }

        [Fact]
        public void MeetingIconsUseTheClassicFugueSet()
        {
            Assert.Equal(MeetingIcon.Cancelled, LegacyIcons.For(new Meeting { Title = "Canceled: daily sync" }));
            Assert.Equal(MeetingIcon.Sync, LegacyIcons.For(new Meeting { Title = "Internal EFR Sync" }));
            Assert.Equal(MeetingIcon.Training, LegacyIcons.For(new Meeting { Title = "Honu V1 SVE Bootcamps" }));
            Assert.Equal(MeetingIcon.Debug, LegacyIcons.For(new Meeting { Title = "R5 Tiger Team Debugging Sessions" }));
            Assert.Equal(MeetingIcon.Emulator, LegacyIcons.For(new Meeting { Title = "Core Emu Sync Up" }));
            Assert.Equal(MeetingIcon.Operations, LegacyIcons.For(new Meeting { Title = "Weekly Veloce Ops" }));
            Assert.Equal(MeetingIcon.Team, LegacyIcons.For(new Meeting { Title = "Teams review" }));
            Assert.Equal(MeetingIcon.Presentation, LegacyIcons.For(new Meeting { Title = "Design review" }));
            Assert.Equal(MeetingIcon.Development, LegacyIcons.For(new Meeting { Title = "Docker Guild 101 - Docker Compose" }));
            Assert.Equal(MeetingIcon.Wellness, LegacyIcons.For(new Meeting { Title = "Weekly Yoga Class" }));
            Assert.Equal(MeetingIcon.Customer, LegacyIcons.For(new Meeting { Title = "Customer roadmap discussion" }));
            Assert.Equal(MeetingIcon.Security, LegacyIcons.For(new Meeting { Title = "Threat model review" }));
            Assert.Equal(MeetingIcon.Data, LegacyIcons.For(new Meeting { Title = "Telemetry data review" }));
            Assert.Equal(MeetingIcon.Research, LegacyIcons.For(new Meeting { Title = "Lab experiment readout" }));
            Assert.Equal(MeetingIcon.Release, LegacyIcons.For(new Meeting { Title = "Release milestone" }));
            Assert.Equal(MeetingIcon.Finance, LegacyIcons.For(new Meeting { Title = "FY27 budget review" }));
            Assert.Equal(MeetingIcon.Planning, LegacyIcons.For(new Meeting { Title = "Project roadmap" }));
            Assert.Equal(MeetingIcon.ActionItems, LegacyIcons.For(new Meeting { Title = "Action items" }));
            Assert.Equal(MeetingIcon.Interview, LegacyIcons.For(new Meeting { Title = "Candidate interview" }));
            Assert.Equal(MeetingIcon.Call, LegacyIcons.For(new Meeting { Title = "Architecture call" }));
            Assert.Equal(MeetingIcon.Break, LegacyIcons.For(new Meeting { Title = "Coffee break" }));
            Assert.Equal(MeetingIcon.Calendar, LegacyIcons.For(new Meeting { Title = "Design huddle" }));
            Assert.Equal(16, LegacyIcons.ImageFor(MeetingIcon.Cancelled).Width);
            Assert.Equal(16, LegacyIcons.ImageFor(MeetingIcon.Security).Width);
        }

        [Fact]
        public void MidnightThemeUsesLightTextOnItsDarkCard()
        {
            var cfg = new Config(); Theme.ApplyPreset(cfg, "Midnight"); var theme = Theme.From(cfg);
            Assert.True(theme.CardText.GetBrightness() - theme.CardBackground.GetBrightness() > 0.45f);
            Assert.True(theme.CardHeading.GetBrightness() - theme.CardBackground.GetBrightness() > 0.30f);
        }

        [Fact]
        public void AutomaticTaskbarColoursRemainReadableOnLightTaskbars()
        {
            var cfg = new Config { AutoTaskbarTextColors = true };
            var palette = TaskbarTextPalette.From(cfg, Theme.From(cfg), Color.FromArgb(242, 242, 242));
            Assert.True(TaskbarTextPalette.ContrastRatio(palette.Primary, Color.FromArgb(242, 242, 242)) >= 4.5);
            Assert.True(TaskbarTextPalette.ContrastRatio(palette.Secondary, Color.FromArgb(242, 242, 242)) >= 4.5);
            Assert.True(TaskbarTextPalette.ContrastRatio(palette.Green, Color.FromArgb(242, 242, 242)) >= 4.5);
            Assert.True(TaskbarTextPalette.ContrastRatio(palette.Amber, Color.FromArgb(242, 242, 242)) >= 4.5);
        }

        [Fact]
        public void ManualTaskbarColoursRemainUntouchedWhenAutomaticContrastIsOff()
        {
            var cfg = new Config { AutoTaskbarTextColors = false, StripForeground = "#123456", StripSecondaryColor = "#234567", TimeGreenColor = "#345678", TimeAmberColor = "#456789" };
            var palette = TaskbarTextPalette.From(cfg, Theme.From(cfg), Color.White);
            Assert.Equal(ColorTranslator.FromHtml("#123456"), palette.Primary);
            Assert.Equal(ColorTranslator.FromHtml("#234567"), palette.Secondary);
            Assert.Equal(ColorTranslator.FromHtml("#345678"), palette.Green);
            Assert.Equal(ColorTranslator.FromHtml("#456789"), palette.Amber);
        }

        [Theory]
        [InlineData("Minimal", "No more meetings")]
        [InlineData("Casual", "Done for the day")]
        [InlineData("Custom", "My calendar is clear")]
        public void EmptyDayTonesUseTheExpectedMessage(string tone, string expected)
        {
            var cfg = new Config { EmptyDayTone = (EmptyDayTone)Enum.Parse(typeof(EmptyDayTone), tone), EmptyDayMessage = "My calendar is clear" };
            Assert.Equal(expected, WidgetForm.EmptyMessage(cfg));
        }

        [Fact]
        public void PlayfulEmptyDayMessageComesFromTheRotatingPool()
        {
            var cfg = new Config { EmptyDayTone = EmptyDayTone.Playful };
            var first = WidgetForm.EmptyMessage(cfg); var second = WidgetForm.PickAnotherPlayfulMessage();
            Assert.True(WidgetForm.IsPlayfulMessage(first));
            Assert.True(WidgetForm.IsPlayfulMessage(second));
            Assert.NotEqual(first, second);
        }

        [Fact]
        public void TextFormattingAndColourCanBeSetPerElement()
        {
            var cfg = new Config(); cfg.SetTextColor(TextElement.CardTitle, "#123456"); cfg.SetTextStyle(TextElement.CardTitle, true, true, true); cfg.SetTextFont(TextElement.CardTitle, "Courier New", 11.5f);
            Assert.Equal("#123456", cfg.TextColor(TextElement.CardTitle));
            Assert.Equal("Courier New", cfg.TextFontFamily(TextElement.CardTitle)); Assert.Equal(115, cfg.TextFontTenths(TextElement.CardTitle));
            Assert.True(cfg.TextStyle(TextElement.CardTitle).HasFlag(System.Drawing.FontStyle.Bold));
            Assert.True(cfg.TextStyle(TextElement.CardTitle).HasFlag(System.Drawing.FontStyle.Italic));
            Assert.True(cfg.TextStyle(TextElement.CardTitle).HasFlag(System.Drawing.FontStyle.Underline));
        }

        [Fact]
        public void LegacyNotesPaletteMigratesToTheCurrentNotesPalette()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "{\"theme_preset\":\"Notes\",\"strip_background\":\"#1C1C1C\",\"strip_foreground\":\"#FFFFFF\",\"card_background\":\"#FFFBEA\",\"card_line\":\"#D8D1BA\",\"accent_color\":\"#C85A5A\"}");
            var cfg = Config.Load(path); File.Delete(path);
            Assert.Equal("#4B392C", cfg.StripBackground); Assert.Equal("#FFF6CF", cfg.CardBackground);
        }

        [Fact]
        public void ScreenshotDemoUsesFictionalDataAndNeverEnablesTeamsPrivacy()
        {
            var cfg = Demo.ScreenshotConfig(new Config());
            var now = new DateTime(2026, 1, 2, 10, 0, 0);
            var meetings = Demo.Meetings(now);
            Assert.False(cfg.TeamsPrivacyEnabled); Assert.False(cfg.ManualPrivacy);
            Assert.Equal(WidgetAction.Details, cfg.ClickAction); Assert.Equal(WidgetAction.Details, cfg.DoubleClickAction);
            Assert.True(meetings.Count >= 8);
            Assert.Contains(meetings, m => m.Start <= now && m.End > now);
            Assert.Contains(meetings, m => CalendarFeed.IsCancelled(m));
        }

        [Fact]
        public void ScreenshotDemoRetainsTheLiveNotesAppearance()
        {
            var live = new Config { ThemePreset = "Notes", CardBackground = "#FDE8A8", CardTitleFontFamily = "Tahoma", CardTitleFontTenths = 104, CardTitleBold = false, CardTitleItalic = true };
            var demo = Demo.ScreenshotConfig(live);
            Assert.Equal("#FDE8A8", demo.CardBackground);
            Assert.Equal("Tahoma", demo.CardTitleFontFamily);
            Assert.Equal(104, demo.CardTitleFontTenths);
            Assert.True(demo.CardTitleItalic);
        }

        [Fact]
        public void PrivacyDemoUsesTheSameBusyRendererPath()
        {
            var cfg = Demo.ScreenshotConfig(new Config(), privacyPreview: true);
            Assert.True(cfg.ManualPrivacy);
            Assert.True(PrivacyState.IsPrivate(cfg.ManualPrivacy, cfg.TeamsPrivacyEnabled, TeamsShareState.NotPresenting));
        }

        [Fact]
        public void NotesPresetResetsEveryVisualTextSettingToTheShippedNotesStyle()
        {
            var cfg = new Config { TaskbarTopFontFamily = "Tahoma", TaskbarTopFontTenths = 120, TaskbarTopBold = false, CardNoteItalic = false };
            Theme.ApplyPreset(cfg, "Notes");
            Assert.Equal("Segoe UI", cfg.TaskbarTopFontFamily);
            Assert.Equal(82, cfg.TaskbarTopFontTenths);
            Assert.True(cfg.TaskbarTopBold);
            Assert.True(cfg.TaskbarBottomItalic);
            Assert.True(cfg.CardTitleBold);
            Assert.True(cfg.CardNoteItalic);
            Assert.Equal(5, cfg.CardCornerRadius);
            Assert.Equal(240, cfg.CardDateNumberFontTenths);
            Assert.Equal(90, cfg.CardDateDayFontTenths);
            Assert.True(cfg.CardDateDayItalic);
            Assert.Equal(new Point(16, -29), cfg.DateStampOffset(TextElement.CardDateNumber));
            Assert.Equal(new Point(-1, 12), cfg.DateStampOffset(TextElement.CardDateMonth));
            Assert.Equal(new Point(-1, 10), cfg.DateStampOffset(TextElement.CardDateDay));
            Assert.Equal("#FFF6CF", cfg.CardBackground);
            Assert.True(cfg.CardShadow);
            Assert.True(cfg.ShowCallout);
        }

        [Fact]
        public void PaperRuleAndDoubleMarginSettingsPersist()
        {
            var path = Path.GetTempFileName();
            var cfg = new Config { RuleLineThickness = 3, MarginLineThickness = 2, MarginLineGap = 4, DoubleMarginLine = false };
            cfg.Save(path); var loaded = Config.Load(path); File.Delete(path);
            Assert.Equal(3, loaded.RuleLineThickness); Assert.Equal(2, loaded.MarginLineThickness); Assert.Equal(4, loaded.MarginLineGap); Assert.False(loaded.DoubleMarginLine);
        }

        [Fact]
        public void DateStampTextSettingsPersistIndependently()
        {
            var path = Path.GetTempFileName();
            var cfg = new Config
            {
                CardDateNumberFontFamily = "Tahoma", CardDateNumberFontTenths = 186, CardDateNumberColor = "#123456", CardDateNumberBold = true,
                CardDateMonthFontFamily = "Arial", CardDateMonthFontTenths = 91, CardDateMonthColor = "#654321", CardDateMonthItalic = true,
                CardDateDayFontFamily = "Verdana", CardDateDayFontTenths = 74, CardDateDayColor = "#ABCDEF", CardDateDayUnderline = true,
                CardDateNumberOffsetX = -52, CardDateNumberOffsetY = 37, CardDateMonthOffsetX = 18, CardDateMonthOffsetY = -21, CardDateDayOffsetX = 4, CardDateDayOffsetY = 31
            };
            cfg.Save(path); var loaded = Config.Load(path); File.Delete(path);
            Assert.Equal("Tahoma", loaded.CardDateNumberFontFamily); Assert.Equal(186, loaded.CardDateNumberFontTenths); Assert.Equal("#123456", loaded.CardDateNumberColor); Assert.True(loaded.CardDateNumberBold);
            Assert.Equal("Arial", loaded.CardDateMonthFontFamily); Assert.Equal(91, loaded.CardDateMonthFontTenths); Assert.Equal("#654321", loaded.CardDateMonthColor); Assert.True(loaded.CardDateMonthItalic);
            Assert.Equal("Verdana", loaded.CardDateDayFontFamily); Assert.Equal(74, loaded.CardDateDayFontTenths); Assert.Equal("#ABCDEF", loaded.CardDateDayColor); Assert.True(loaded.CardDateDayUnderline);
            Assert.Equal(new Point(-52, 37), loaded.DateStampOffset(TextElement.CardDateNumber));
            Assert.Equal(new Point(18, -21), loaded.DateStampOffset(TextElement.CardDateMonth));
            Assert.Equal(new Point(4, 31), loaded.DateStampOffset(TextElement.CardDateDay));
        }

        [Fact]
        public void StripSizesItselfToTextByDefault()
        {
            Assert.True(new Config().AutoWidth);
        }

        [Fact]
        public void NoStripBackgroundPersistsSeparatelyFromOpacity()
        {
            var path = Path.GetTempFileName();
            var cfg = new Config { NoStripBackground = true, BackgroundOpacity = 123 };
            cfg.Save(path); var loaded = Config.Load(path); File.Delete(path);
            Assert.True(loaded.NoStripBackground);
            Assert.Equal(123, loaded.BackgroundOpacity);
        }

        [Fact]
        public void DetailAppearAnimationSettingsPersistAndAreBounded()
        {
            var path = Path.GetTempFileName();
            var cfg = new Config { AppearStartPercent = 88, AppearDurationMilliseconds = 220 };
            cfg.Save(path); var loaded = Config.Load(path); File.Delete(path);
            Assert.Equal(88, loaded.AppearStartPercent);
            Assert.Equal(220, loaded.AppearDurationMilliseconds);
        }

        [Fact]
        public void SkipCancelledMeetingsPreferencePersistsAndMigrates()
        {
            var path = Path.GetTempFileName();
            new Config { SkipCancelledMeetings = false }.Save(path);
            var loaded = Config.Load(path); File.Delete(path);
            Assert.False(loaded.SkipCancelledMeetings);
            path = Path.GetTempFileName(); File.WriteAllText(path, "{\"cancelled_final_meeting_ends_day\":false}");
            loaded = Config.Load(path); File.Delete(path);
            Assert.False(loaded.SkipCancelledMeetings);
        }

        [Fact]
        public void PointerActionsHaveSafeDefaultsAndPersist()
        {
            var path = Path.GetTempFileName();
            var cfg = new Config { ClickAction = WidgetAction.Details, DoubleClickAction = WidgetAction.Nothing, HoverAction = WidgetAction.Details };
            cfg.Save(path); var loaded = Config.Load(path); File.Delete(path);
            Assert.Equal(WidgetAction.Details, loaded.ClickAction);
            Assert.Equal(WidgetAction.Nothing, loaded.DoubleClickAction);
            Assert.Equal(WidgetAction.Details, loaded.HoverAction);
            Assert.Equal(WidgetAction.Outlook, new Config().ClickAction);
            Assert.Equal(WidgetAction.Details, new Config().HoverAction);
        }
    }
}
