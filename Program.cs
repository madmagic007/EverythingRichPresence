using EverythingRichPresence.LuaStuff;
using EverythingRichPresence.Modules;
using Microsoft.Win32;
using System.Diagnostics;
using System.Resources;

namespace EverythingRichPresence {

    public class Program : ApplicationContext {

        [STAThread]
        static void Main() {
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)
               .SetValue("ERPC", Application.ExecutablePath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }

        public static NotifyIcon trayIcon;

        public Program() {
            trayIcon = new() {
                Text = "Everything RPC",
                ContextMenuStrip = new(),
                //Icon = Resources.AppIcon,
                Visible = true
            };
            trayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Refresh modules", null, (_, _) => {
                    LuaHandler.Init();
                }),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit", null, (_, _) => {
                    trayIcon.Visible = false;
                    Environment.Exit(0);
                })
            });

            LuaHandler.Init();
            ModuleHandler.Init();
        }

        public static void SendNotif(string text) {
            trayIcon.ShowBalloonTip(0, "ERPC", text, ToolTipIcon.Info);
        }
    }
}