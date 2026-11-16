using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TaskbarCalendar
{
    class Meeting
    {
        public string Title;
        public DateTime Start;
        public DateTime End;
        public string Link;
        public string Location;
    }

    /// <summary>Pure meeting selection and presentation helpers. Outlook is the only data provider.</summary>
    static class CalendarFeed
    {
        static readonly Regex LinkPattern = new Regex(
            @"https://(?:[\w.-]*teams\.microsoft\.com/(?:l/meetup-join|meet)/[^\s<>\""']+|aka\.ms/JoinTeamsMeeting[^\s<>\""']*|[\w.-]*zoom\.us/j/[^\s<>\""']+|[\w.-]*webex\.com/[^\s<>\""']+)", RegexOptions.IgnoreCase);

        internal static string FindLink(params string[] values)
        {
            foreach (var value in values.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                var match = LinkPattern.Match(value);
                if (match.Success) return match.Value.TrimEnd('.', ',', ';', ')', ']');
            }
            return null;
        }

        /// <summary>Returns the physical room while dropping generic online-meeting markers Outlook adds to Location.</summary>
        internal static string RoomLocation(Meeting meeting)
        {
            if (string.IsNullOrWhiteSpace(meeting?.Location)) return null;
            var parts = meeting.Location.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => !IsOnlineLocationMarker(value))
                .ToList();
            return parts.Count == 0 ? null : string.Join("; ", parts);
        }

        static bool IsOnlineLocationMarker(string value)
        {
            return string.Equals(value, "Microsoft Teams Meeting", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Teams Meeting", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Skype Meeting", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Zoom Meeting", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Webex Meeting", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Online", StringComparison.OrdinalIgnoreCase);
        }

        public static List<Meeting> ActiveMeetings(IEnumerable<Meeting> meetings, DateTime now) =>
            meetings.Where(m => m.Start <= now && m.End > now).OrderBy(m => m.Start).ThenBy(m => m.Title).ToList();

        public static bool IsCancelled(Meeting meeting)
        {
            var title = meeting?.Title ?? "";
            return title.IndexOf("cancelled", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   title.IndexOf("canceled", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Calendar-active: the longest-running active event wins; otherwise return the next scheduled event.</summary>
        public static Meeting PickNext(IEnumerable<Meeting> meetings, DateTime now, bool skipCancelledToday = false)
        {
            var active = ActiveMeetings(meetings, now).OrderBy(m => m.Start).FirstOrDefault();
            var selected = active ?? meetings.Where(m => m.End > now).OrderBy(m => m.Start).ThenBy(m => m.Title).FirstOrDefault();
            if (!skipCancelledToday || selected == null || !IsCancelled(selected) || selected.Start.Date != now.Date) return selected;

            // A cancellation only changes today's strip. It must not pull a meeting
            // from tomorrow into a day that has otherwise become free.
            var remainingToday = meetings.Where(m => m.End > now && m.Start.Date == now.Date && !IsCancelled(m));
            var replacement = ActiveMeetings(remainingToday, now).OrderBy(m => m.Start).FirstOrDefault();
            return replacement ?? remainingToday.OrderBy(m => m.Start).ThenBy(m => m.Title).FirstOrDefault();
        }

        /// <summary>The taskbar strip is about today. The details card can show
        /// future days, but a free current day must not be replaced by tomorrow's
        /// first appointment.</summary>
        public static Meeting PickToday(IEnumerable<Meeting> meetings, DateTime now, bool skipCancelledToday = false)
        {
            var active = ActiveMeetings(meetings, now);
            var candidates = active.Concat(meetings.Where(m => m.End > now && m.Start.Date == now.Date && !active.Contains(m)))
                .OrderBy(m => m.Start).ThenBy(m => m.Title);
            if (skipCancelledToday) candidates = candidates.Where(m => !IsCancelled(m)).OrderBy(m => m.Start).ThenBy(m => m.Title);
            return candidates.FirstOrDefault();
        }

        public static bool CancelledTodayWasSkipped(IEnumerable<Meeting> meetings, DateTime now, bool skipCancelledToday)
        {
            if (!skipCancelledToday) return false;
            var unfiltered = PickToday(meetings, now);
            return unfiltered != null && IsCancelled(unfiltered) && PickToday(meetings, now, true) == null;
        }

        public static string FmtTime(DateTime value, bool use24HourClock = true) => value.ToString(use24HourClock ? "HH:mm" : "h:mm tt");
        public static string FmtRange(Meeting meeting, bool use24HourClock = true) => $"{FmtTime(meeting.Start, use24HourClock)} → {FmtTime(meeting.End, use24HourClock)}";
        public static ColorState StartState(Meeting meeting, DateTime now, int amberThresholdMinutes, bool lateAlertEnabled = true)
        {
            if (now < meeting.Start) return ColorState.Neutral;
            // Completed rows are history, not a live attendance signal.
            if (now >= meeting.End) return ColorState.Neutral;
            if (!lateAlertEnabled) return ColorState.Green;
            return now <= meeting.Start.AddMinutes(amberThresholdMinutes) ? ColorState.Green : ColorState.Amber;
        }
    }

    public enum ColorState { Neutral, Green, Amber }
}
