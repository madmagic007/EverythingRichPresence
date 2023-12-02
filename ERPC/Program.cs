using ERPC.LuaStuff;
using ERPC.Modules;
using ERPC.Properties;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ERPC {

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
        private static ToolStripMenuItem consoleItem = new("Enable Module Debug Console", null, (_, _) => {
            ToggleConsole();
        });

        public Program() {
            WebMemory.CreateAuth();

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
                consoleItem,
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

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static int hide = 0;
        private static int show = 5;
        private static int currentState = 0;

        private static void ToggleConsole() {
            IntPtr handle = GetConsoleWindow();
            currentState = currentState == hide ? show : hide;

            if (handle == IntPtr.Zero) AllocConsole();
            else {
                ShowWindow(handle, currentState);
                Console.WriteLine("showing");
            }

            consoleItem.Text = $"{(currentState == hide ? "Enable" : "Disable")} Module Debug Console";

            if (currentState == show) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Warning: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Closing this window will exit the app, disable the console from the system tray.");
                Console.ResetColor();
            }
        }

        public static void Log(string s) {
            //if (currentState != show) return;
            Console.WriteLine(s);
        }
    }
}