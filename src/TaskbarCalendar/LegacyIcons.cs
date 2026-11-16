using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace TaskbarCalendar
{
    enum MeetingIcon { None, Calendar, Online, Cancelled, Sync, Team, Training, Debug, Emulator, Operations }

    static class LegacyIcons
    {
        static readonly Bitmap Calendar = Load("fugue.calendar-day.png");
        static readonly Bitmap Online = Load("fugue.balloon.png");
        static readonly Bitmap Cancelled = Load("fugue.cross-circle.png");
        static readonly Bitmap Sync = Load("fugue.arrow-circle-double.png");
        static readonly Bitmap Team = Load("fugue.category-group.png");
        static readonly Bitmap Training = Load("fugue.book.png");
        static readonly Bitmap Debug = Load("fugue.bug.png");
        static readonly Bitmap Emulator = Load("fugue.processor.png");
        static readonly Bitmap Gear = Load("fugue.gear.png");
        static readonly Bitmap Color = Load("fugue.color.png");
        static readonly Bitmap PaintCan = Load("fugue.paint-can.png");
        static readonly Bitmap Refresh = Load("fugue.arrow-circle-double.png");
        static readonly Bitmap Tick = Load("fugue.tick.png");
        static readonly Bitmap Cross = Load("fugue.cross.png");
        static readonly Bitmap Folder = Load("fugue.folder-open.png");
        static readonly Bitmap Document = Load("fugue.document-text.png");
        static readonly Bitmap Clock = Load("fugue.clock.png");
        static readonly Bitmap DoorOpenIn = Load("fugue.door-open-in.png");

        static Bitmap Load(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            using (var image = Image.FromStream(stream)) return new Bitmap(image);
        }

        public static MeetingIcon For(Meeting meeting)
        {
            if (meeting == null) return MeetingIcon.None;
            var text = (meeting.Title ?? "") + " " + (meeting.Link ?? "");
            if (CalendarFeed.IsCancelled(meeting)) return MeetingIcon.Cancelled;
            if (Contains(text, "debug", "bug")) return MeetingIcon.Debug;
            if (Contains(text, "bootcamp", "training", "workshop", "course")) return MeetingIcon.Training;
            if (Contains(text, "ops", "operations")) return MeetingIcon.Operations;
            if (Contains(text, "emulation", "emulator", " emu", "veloce")) return MeetingIcon.Emulator;
            if (Contains(text, "sync", "standup", "scrum")) return MeetingIcon.Sync;
            if (Contains(text, "team", "1:1", "one on one")) return MeetingIcon.Team;
            if (text.IndexOf("teams", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("zoom", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("webex", StringComparison.OrdinalIgnoreCase) >= 0) return MeetingIcon.Online;
            return MeetingIcon.Calendar;
        }

        static bool Contains(string text, params string[] words)
        {
            foreach (var word in words) if (text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        public static Image ImageFor(MeetingIcon icon)
        {
            switch (icon)
            {
                case MeetingIcon.Calendar: return Calendar;
                case MeetingIcon.Online: return Online;
                case MeetingIcon.Cancelled: return Cancelled;
                case MeetingIcon.Sync: return Sync;
                case MeetingIcon.Team: return Team;
                case MeetingIcon.Training: return Training;
                case MeetingIcon.Debug: return Debug;
                case MeetingIcon.Emulator: return Emulator;
                case MeetingIcon.Operations: return Gear;
                default: return null;
            }
        }

        public static Image CalendarImage => Calendar;
        public static Image OnlineImage => Online;
        public static Image GearImage => Gear;
        public static Image ColorImage => Color;
        public static Image PaintCanImage => PaintCan;
        public static Image RefreshImage => Refresh;
        public static Image TickImage => Tick;
        public static Image CrossImage => Cross;
        public static Image FolderImage => Folder;
        public static Image DocumentImage => Document;
        public static Image ClockImage => Clock;
        public static Image JoinImage => DoorOpenIn;
    }
}
