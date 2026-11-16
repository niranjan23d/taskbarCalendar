using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TaskbarCalendar
{
    /// <summary>Reads the default calendar from classic Outlook through the signed-in local MAPI profile.
    /// This uses no network API, app registration, or stored credential.</summary>
    static class OutlookCalendar
    {
        const int OlFolderCalendar = 9;
        const int OlAppointment = 26;
        const int OlResponseDeclined = 4;

        public static List<Meeting> Fetch(int lookaheadDays)
        {
            dynamic app = null, ns = null, folder = null, items = null;
            var result = new List<Meeting>();
            var start = DateTime.Now.AddDays(-1);
            var end = DateTime.Now.AddDays(lookaheadDays);
            try
            {
                var type = Type.GetTypeFromProgID("Outlook.Application");
                if (type == null) throw new InvalidOperationException("Classic Outlook is not installed.");
                app = Activator.CreateInstance(type);
                ns = app.GetNamespace("MAPI");
                folder = ns.GetDefaultFolder(OlFolderCalendar);
                items = folder.Items;
                items.IncludeRecurrences = true;
                items.Sort("[Start]");
                dynamic item = items.Find(Filter(start, end));
                var seen = new HashSet<string>(StringComparer.Ordinal);
                for (var examined = 0; item != null && examined < 500; examined++)
                {
                    var repeated = false;
                    try
                    {
                        // FindNext can repeat one changed recurrence indefinitely. Entry ID plus occurrence start
                        // is stable for a real occurrence and stops that Outlook bug from stalling the widget.
                        var key = ((string)item.EntryID ?? "") + "|" + ((DateTime)item.Start).Ticks;
                        if (!seen.Add(key)) { repeated = true; continue; }
                        if ((int)item.Class != OlAppointment || (bool)item.AllDayEvent) continue;
                        if ((int)item.ResponseStatus == OlResponseDeclined) continue;
                        var meetingStart = (DateTime)item.Start;
                        var meetingEnd = (DateTime)item.End;
                        if (meetingEnd <= meetingStart) meetingEnd = meetingStart.AddMinutes(30);
                        if (meetingEnd < start || meetingStart > end) continue;
                        var title = ((string)item.Subject ?? "").Trim();
                        var location = ((string)item.Location ?? "").Trim();
                        result.Add(new Meeting
                        {
                            Title = title.Length > 0 ? title : "(No title)",
                            Start = meetingStart,
                            End = meetingEnd,
                            Location = location,
                            Link = CalendarFeed.FindLink(location, (string)item.Body),
                        });
                    }
                    finally
                    {
                        var next = repeated ? null : items.FindNext();
                        Release(item);
                        item = next;
                    }
                }
            }
            catch (COMException ex)
            {
                throw new InvalidOperationException("Couldn't read the default Outlook calendar. Start Outlook once and make sure its profile is signed in.", ex);
            }
            finally
            {
                Release(items); Release(folder); Release(ns); Release(app);
            }
            return result;
        }

        static string Filter(DateTime start, DateTime end) =>
            "[Start] >= '" + start.ToString("g", CultureInfo.CurrentCulture) + "' AND [Start] <= '" + end.ToString("g", CultureInfo.CurrentCulture) + "'";

        static void Release(object value)
        {
            if (value != null && Marshal.IsComObject(value)) Marshal.FinalReleaseComObject(value);
        }

        public static void Open()
        {
            var path = FindOutlookExecutable();
            if (path != null)
            {
                Process.Start(new ProcessStartInfo(path, "/select outlook:calendar") { UseShellExecute = true });
                return;
            }
            throw new InvalidOperationException("Couldn't find classic Outlook. Start classic Outlook once and make sure its profile is signed in.");
        }

        static string FindOutlookExecutable()
        {
            var keys = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\OUTLOOK.EXE",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\OUTLOOK.EXE"
            };
            foreach (var keyName in keys)
            using (var key = Registry.LocalMachine.OpenSubKey(keyName))
            {
                var path = key?.GetValue(null) as string;
                if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path)) return path;
            }
            foreach (var root in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) })
            {
                var path = System.IO.Path.Combine(root, "Microsoft Office", "root", "Office16", "OUTLOOK.EXE");
                if (System.IO.File.Exists(path)) return path;
            }
            return null;
        }
    }

    static class CalendarService
    {
        public static List<Meeting> Fetch(Config config) => OutlookCalendar.Fetch(config.LookaheadDays);
        public static void Open(Config config) => OutlookCalendar.Open();
    }
}
