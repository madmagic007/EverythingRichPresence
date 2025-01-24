using ERPC.LuaStuff;
using ERPC.Modules;
using ERPC.Properties;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.Json;

namespace ERPC {
    public class Program : ApplicationContext {

        public static readonly DirectoryInfo modulesDir = Directory.CreateDirectory(Environment.GetEnvironmentVariable("APPDATA") + "/MadMagic/Everything Rich Presence/modules");
        private static readonly FileInfo loggerExe = new(modulesDir.Parent.FullName + "/ConsoleLogger.exe");

        private static readonly FileInfo x86 = new(modulesDir.Parent.FullName + "/ERPCx86.exe");
        private static readonly FileInfo x64 = new(modulesDir.Parent.FullName + "/ERPCx64.exe");

        [STAThread]
        static void Main() {
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)
               .SetValue("ERPC", Application.ExecutablePath);

            Mutex mutex = new(false, "ERPC");

            try {
                if (mutex.WaitOne(0, false)) {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Program());
                }
            } finally {
                mutex.Close();
            }
        }

        public static NotifyIcon trayIcon;
        private static readonly ToolStripMenuItem consoleItem = new("Enable Module Debug Console", null, (_, _) => {
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
            trayIcon.ContextMenuStrip.Items.AddRange([
                new ToolStripMenuItem("Refresh modules", null, (_, _) => {
                    ModuleHandler.StopAll();
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
                new ToolStripMenuItem ("Download Module", null, (_, _) => {
                    ModuleDownloader.ShowDialog();
                }),
                new ToolStripSeparator(),
                consoleItem,
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit", null, (_, _) => {
                    trayIcon.Visible = false;
                    Environment.Exit(0);
                })
            ]);

			LuaHandler.Init();
            ModuleHandler.Init();

            List<Process> processes = [
                Process.Start(x64.FullName),
                Process.Start(x86.FullName)
            ];

            AppDomain.CurrentDomain.ProcessExit += (_, _) => {
                processes.ForEach(p => p.Kill());
            };
        }

        public static void SendNotif(string text) {
            trayIcon.ShowBalloonTip(0, "Everything Rich Presence", text, ToolTipIcon.Info);
        }

        private static Process? p;

        private static void ToggleConsole() {
            if (p == null) {
                p = Process.Start(new ProcessStartInfo {
                    FileName = loggerExe.FullName,
                    UseShellExecute = false,
                    RedirectStandardInput = true
                });

                Log("Warning:", ConsoleColor.Red);
                Log("Closing this console window will exit ERPC! You can close this window in the same way you opened it.", ConsoleColor.Yellow);
            } else {
                p.Kill();
                p = null;
            }

            consoleItem.Text = (p == null ? "Enable" : "Disable") + " Module Debug Console";
        }

        public static void Log(string message, ConsoleColor color = ConsoleColor.White) {
            string s = JsonSerializer.Serialize(new MessageContent {
                message = message,
                color = color
            });
            p?.StandardInput.WriteLine(s);

            Console.ForegroundColor = color;
        }
    }

    class MessageContent {
        public string message { get; set; }
        public ConsoleColor color { get; set; }
    }
}