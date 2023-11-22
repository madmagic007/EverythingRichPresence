using EverythingRichPresence.LuaStuff;
using EverythingRichPresence.Modules;
using EverythingRichPresence.Properties;
using Microsoft.Win32;
using Neo.IronLua;
using System.Diagnostics;
using System.Resources;
using System.Security.Policy;

namespace EverythingRichPresence {

    public class Program : ApplicationContext {

        public static readonly DirectoryInfo modulesDir = Directory.CreateDirectory(Environment.GetEnvironmentVariable("APPDATA") + "/MadMagic/Everything Rich Presence/modules");

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
                Icon = Resources.icon,
                Visible = true
            };
            trayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Refresh modules", null, (_, _) => {
                    LuaHandler.Init();
                    ModuleHandler.Init();
                }),
                new ToolStripMenuItem("Open Modules Folder", null, (_, _) => {
                    ProcessStartInfo psi = new() {
                        FileName = modulesDir.FullName,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
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