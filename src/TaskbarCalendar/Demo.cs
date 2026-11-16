using System;
using System.Collections.Generic;

namespace TaskbarCalendar
{
    /// <summary>Made-up local data for screenshots and visual testing. It never writes settings or touches Outlook.</summary>
    static class Demo
    {
        public static Config ScreenshotConfig(bool privacyPreview = false) => ScreenshotConfig(Config.Load(), privacyPreview);

        // Demo data must be synthetic, but its chrome should be exactly what the
        // user has chosen for the live widget.  The loaded config is used only in
        // memory and is never saved by demo mode.
        internal static Config ScreenshotConfig(Config config, bool privacyPreview = false)
        {
            // Keep screenshot mode independent of Outlook, Teams state and startup.
            // Demo normally ignores the user's live Teams state. The --demo
            // --privacy variant intentionally exercises the Busy renderer.
            config.TeamsPrivacyEnabled = false;
            config.ManualPrivacy = privacyPreview;
            config.ClickAction = WidgetAction.Details;
            config.DoubleClickAction = WidgetAction.Details;
            config.HoverAction = WidgetAction.Details;
            return config;
        }

        public static List<Meeting> Meetings(DateTime now)
        {
            // Keep the sample plausible beside the real taskbar clock. The current
            // review starts in this five-minute block, so it is green, not late.
            var start = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0);
            Meeting Make(string title, int minutes, int length, string link = null, string location = null) => new Meeting { Title = title, Start = start.AddMinutes(minutes), End = start.AddMinutes(minutes + length), Link = link, Location = location };
            return new List<Meeting>
            {
                Make("Cancelled: daily planning recap", -150, 25),
                Make("Product design review", -105, 45, null, "Willow 2B"),
                Make("Release readiness stand-up", -50, 25),
                Make("Platform architecture review", 0, 50, "https://teams.microsoft.com/l/meetup-join/demo-screenshot", "Microsoft Teams Meeting; Harbour 4B"),
                Make("Customer workflow review", 2, 30, "https://teams.microsoft.com/l/meetup-join/demo-screenshot"),
                Make("Roadmap working session", 60, 45, null, "Cedar 3A"),
                Make("Weekly 1:1", 120, 30),
                Make("Incident follow-up", 180, 30),
                Make("Cancelled: documentation review", 240, 30),
                Make("Workshop: writing better requirements", 300, 60, null, "Juniper 1C"),
            };
        }
    }
}
