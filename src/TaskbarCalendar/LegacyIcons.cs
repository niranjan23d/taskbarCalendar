using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace TaskbarCalendar
{
    enum MeetingIcon
    {
        None, Calendar, Online, Cancelled, Sync, Team, Training, Debug, Emulator, Operations,
        Security, Data, Development, Presentation, Research, Release, Finance, Planning,
        Customer, Interview, Call, Social, Wellness, Break, Home, Global, Metrics, ActionItems
    }

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
        static readonly Bitmap Shield = Load("fugue.shield.png");
        static readonly Bitmap Database = Load("fugue.database.png");
        static readonly Bitmap Terminal = Load("fugue.terminal.png");
        static readonly Bitmap Presentation = Load("fugue.projection-screen-presentation.png");
        static readonly Bitmap Beaker = Load("fugue.beaker.png");
        static readonly Bitmap Rocket = Load("fugue.rocket.png");
        static readonly Bitmap Money = Load("fugue.money.png");
        static readonly Bitmap Target = Load("fugue.target.png");
        static readonly Bitmap Briefcase = Load("fugue.briefcase.png");
        static readonly Bitmap UserBusiness = Load("fugue.user-business.png");
        static readonly Bitmap Telephone = Load("fugue.telephone.png");
        static readonly Bitmap Music = Load("fugue.music.png");
        static readonly Bitmap Heart = Load("fugue.heart.png");
        static readonly Bitmap Cup = Load("fugue.cup.png");
        static readonly Bitmap Home = Load("fugue.home.png");
        static readonly Bitmap Globe = Load("fugue.globe.png");
        static readonly Bitmap Chart = Load("fugue.chart.png");
        static readonly Bitmap ClipboardTask = Load("fugue.clipboard-task.png");

        static Bitmap Load(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            using (var image = Image.FromStream(stream)) return new Bitmap(image);
        }

        public static MeetingIcon For(Meeting meeting)
        {
            if (meeting == null) return MeetingIcon.None;
            // Only the subject drives classification.  URLs and location data are not
            // inspected, retained, or used to infer meeting context.
            var text = meeting.Title ?? "";
            if (CalendarFeed.IsCancelled(meeting)) return MeetingIcon.Cancelled;
            if (Contains(text, "debug", "debugging", "bug", "bugs")) return MeetingIcon.Debug;
            if (Contains(text, "security", "privacy", "threat", "compliance", "audit", "vulnerability")) return MeetingIcon.Security;
            if (Contains(text, "database", "data", "analytics", "sql", "telemetry")) return MeetingIcon.Data;
            if (Contains(text, "docker", "code", "coding", "software", "compiler", "terminal", "git", "rtl", "cadt")) return MeetingIcon.Development;
            if (Contains(text, "yoga", "fitness", "wellness", "pranayama", "health")) return MeetingIcon.Wellness;
            if (Contains(text, "bootcamp", "bootcamps", "training", "workshop", "workshops", "course", "courses", "class", "classes")) return MeetingIcon.Training;
            if (Contains(text, "research", "experiment", "lab", "study")) return MeetingIcon.Research;
            if (Contains(text, "ops", "operations")) return MeetingIcon.Operations;
            if (Contains(text, "emulation", "emulator", "emu", "veloce")) return MeetingIcon.Emulator;
            if (Contains(text, "sync", "standup", "scrum")) return MeetingIcon.Sync;
            if (Contains(text, "team", "teams", "1:1", "one on one")) return MeetingIcon.Team;
            if (Contains(text, "release", "launch", "tapeout", "ship", "milestone")) return MeetingIcon.Release;
            if (Contains(text, "budget", "finance", "financial", "cost", "pricing")) return MeetingIcon.Finance;
            if (Contains(text, "customer", "client", "vendor", "partner", "external")) return MeetingIcon.Customer;
            if (Contains(text, "interview", "hiring", "recruiting", "candidate")) return MeetingIcon.Interview;
            if (Contains(text, "action items", "task", "tasks")) return MeetingIcon.ActionItems;
            if (Contains(text, "plan", "planning", "project", "roadmap")) return MeetingIcon.Planning;
            if (Contains(text, "metrics", "kpi", "performance", "report")) return MeetingIcon.Metrics;
            if (Contains(text, "work from home", "wfh", "remote work")) return MeetingIcon.Home;
            if (Contains(text, "global", "international", "travel", "onsite")) return MeetingIcon.Global;
            if (Contains(text, "presentation", "demo", "walkthrough", "review", "showcase")) return MeetingIcon.Presentation;
            if (Contains(text, "call", "phone", "dial in")) return MeetingIcon.Call;
            if (Contains(text, "teams", "zoom", "webex", "meet")) return MeetingIcon.Online;
            if (Contains(text, "open house", "social", "happy hour", "club", "birthday")) return MeetingIcon.Social;
            if (Contains(text, "lunch", "coffee", "break")) return MeetingIcon.Break;
            return MeetingIcon.Calendar;
        }

        static bool Contains(string text, params string[] words)
        {
            foreach (var word in words)
            {
                for (var start = 0; start < text.Length;)
                {
                    var index = text.IndexOf(word, start, StringComparison.OrdinalIgnoreCase);
                    if (index < 0) break;
                    var end = index + word.Length;
                    var leftBoundary = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
                    var rightBoundary = end == text.Length || !char.IsLetterOrDigit(text[end]);
                    if (leftBoundary && rightBoundary) return true;
                    start = index + 1;
                }
            }
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
                case MeetingIcon.Security: return Shield;
                case MeetingIcon.Data: return Database;
                case MeetingIcon.Development: return Terminal;
                case MeetingIcon.Presentation: return Presentation;
                case MeetingIcon.Research: return Beaker;
                case MeetingIcon.Release: return Rocket;
                case MeetingIcon.Finance: return Money;
                case MeetingIcon.Planning: return Target;
                case MeetingIcon.Customer: return Briefcase;
                case MeetingIcon.Interview: return UserBusiness;
                case MeetingIcon.Call: return Telephone;
                case MeetingIcon.Social: return Music;
                case MeetingIcon.Wellness: return Heart;
                case MeetingIcon.Break: return Cup;
                case MeetingIcon.Home: return Home;
                case MeetingIcon.Global: return Globe;
                case MeetingIcon.Metrics: return Chart;
                case MeetingIcon.ActionItems: return ClipboardTask;
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
