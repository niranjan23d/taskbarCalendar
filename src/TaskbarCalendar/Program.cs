using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace TaskbarCalendar
{
    static class Program
    {
        static Mutex instanceMutex;
        public static Icon AppIcon { get; } = LoadIcon();

        static Icon LoadIcon()
        {
            try { using (var stream = typeof(Program).Assembly.GetManifestResourceStream("icon.ico")) return stream != null ? new Icon(stream) : SystemIcons.Application; }
            catch { return SystemIcons.Application; }
        }

        [STAThread]
        static int Main(string[] args)
        {
            if (args.Contains("--list-meetings")) return ListMeetings();
            var demo = args.Contains("--demo");
            if (args.Contains("--uninstall")) { Installer.Uninstall(); return 0; }
            // Keep the settings dialog and menus on the system classic-control renderer.
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (s, e) => Log.Error("Unhandled UI error", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Log.Error("Fatal error", e.ExceptionObject as Exception);
            if (!demo)
            {
                if (Installer.ShouldInstall && Installer.InstallAndRelaunch(args)) return 0;
                instanceMutex = new Mutex(false, @"Local\TaskbarCalendarWidget", out var createdNew);
                if (!createdNew) { WinApi.AskRunningInstanceToOpenSettings(); return 0; }
                if (Installer.IsInstalledCopy && Config.IsFirstRun)
                {
                    try { Installer.SetStartup(true); }
                    catch (Exception ex) { Log.Warn($"Couldn't enable startup: {ex.Message}"); }
                }
            }
            Log.Info($"Starting Taskbar Calendar {Paths.VersionText}{(demo ? " (demo)" : "")}");
            var form = new WidgetForm(demo, null, args.Contains("--settings") || (!demo && Config.IsFirstRun), null, demo && args.Contains("--privacy"));
            var context = new ApplicationContext(); form.FormClosed += (s, e) => context.ExitThread(); form.Start(); Application.Run(context); return 0;
        }

        static int ListMeetings()
        {
            try
            {
                foreach (var meeting in CalendarService.Fetch(Config.Load()).Where(m => m.End > DateTime.Now)) Console.WriteLine($"{meeting.Start:yyyy-MM-dd HH:mm}  {meeting.End:HH:mm}  {meeting.Title}");
                return 0;
            }
            catch (Exception ex) { Console.Error.WriteLine(ex.GetBaseException().Message); return 1; }
        }
    }
}
