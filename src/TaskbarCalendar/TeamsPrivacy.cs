using System;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Forms;

namespace TaskbarCalendar
{
    enum TeamsShareState { NotPresenting, Presenting, Unknown }

    interface ITeamsShareDetector { TeamsShareState Detect(); }

    /// <summary>Local UI heuristic only. Unknown is returned only for a readable active call without a clear presentation state.</summary>
    sealed class TeamsShareDetector : ITeamsShareDetector
    {
        static readonly string[] PresentingLabels = { "Stop presenting", "Stop sharing", "Stop screen sharing", "End presentation" };
        static readonly string[] InCallLabels = { "Leave", "Leave call", "Hang up", "End call" };

        public TeamsShareState Detect()
        {
            var processes = Process.GetProcesses().Where(p => string.Equals(p.ProcessName, "ms-teams", StringComparison.OrdinalIgnoreCase) ||
                                                               string.Equals(p.ProcessName, "Teams", StringComparison.OrdinalIgnoreCase)).ToList();
            try
            {
                foreach (var process in processes)
                {
                    if (process.MainWindowHandle == IntPtr.Zero) continue;
                    var root = AutomationElement.FromHandle(process.MainWindowHandle);
                    if (root == null) continue;
                    var names = root.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button))
                                    .Cast<AutomationElement>().Select(e => SafeName(e)).Where(n => n.Length > 0).ToList();
                    if (names.Any(n => PresentingLabels.Any(label => n.IndexOf(label, StringComparison.OrdinalIgnoreCase) >= 0))) return TeamsShareState.Presenting;
                    if (names.Any(n => InCallLabels.Any(label => string.Equals(n, label, StringComparison.OrdinalIgnoreCase)))) return TeamsShareState.Unknown;
                }

                // New Teams sometimes paints call controls inside a WebView without
                // exposing their accessible names. Its red share outline stays on
                // the display even if the call is reduced to the compact mini window.
                if (TeamsPresentationBorder.IsVisible()) return TeamsShareState.Presenting;
            }
            catch { return TeamsShareState.Unknown; }
            finally { foreach (var process in processes) process.Dispose(); }
            return TeamsShareState.NotPresenting;
        }

        static string SafeName(AutomationElement element) { try { return element.Current.Name ?? ""; } catch { return ""; } }
    }

    /// <summary>Detects Teams' full-display red sharing outline without OCR or saved screen captures.</summary>
    static class TeamsPresentationBorder
    {
        const int BorderPixels = 2;

        internal static bool IsTeamsPresentationRed(Color colour) =>
            colour.R >= 130 && colour.G <= 115 && colour.B <= 115 && colour.R - Math.Max(colour.G, colour.B) >= 40;

        public static bool IsVisible()
        {
            try
            {
                // A genuine presentation outline runs around all sides of a screen.
                // Requiring three red edges rejects a red notification or taskbar icon.
                return Screen.AllScreens.Any(screen => CountRedEdges(screen.Bounds) >= 3);
            }
            catch { return false; }
        }

        static int CountRedEdges(Rectangle screen)
        {
            var edges = new[]
            {
                new Rectangle(screen.Left, screen.Top, screen.Width, BorderPixels),
                new Rectangle(screen.Left, screen.Bottom - BorderPixels, screen.Width, BorderPixels),
                new Rectangle(screen.Left, screen.Top, BorderPixels, screen.Height),
                new Rectangle(screen.Right - BorderPixels, screen.Top, BorderPixels, screen.Height),
            };
            return edges.Count(EdgeLooksRed);
        }

        static bool EdgeLooksRed(Rectangle area)
        {
            if (area.Width <= 0 || area.Height <= 0) return false;
            using (var image = new Bitmap(area.Width, area.Height))
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.CopyFromScreen(area.Location, Point.Empty, area.Size, CopyPixelOperation.SourceCopy);
                var redPixels = 0;
                var total = area.Width * area.Height;
                for (var y = 0; y < area.Height; y++)
                    for (var x = 0; x < area.Width; x++)
                        if (IsTeamsPresentationRed(image.GetPixel(x, y))) redPixels++;
                return redPixels >= total * 0.20;
            }
        }
    }

    static class PrivacyState
    {
        public static bool IsPrivate(bool manualOn, bool automaticOn, TeamsShareState teams) =>
            manualOn || (automaticOn && teams != TeamsShareState.NotPresenting);
    }
}
