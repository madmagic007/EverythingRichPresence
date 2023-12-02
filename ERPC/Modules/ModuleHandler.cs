using System.Diagnostics;
using Timer = System.Threading.Timer;

namespace ERPC.Modules {

    public class ModuleHandler {

        public static readonly List<Module> loadedModules = new();
        public static Module activeModule;
        private static Timer timer;
        public static WebMemory mem;

        public static void AddModule(Module module) {
            if (loadedModules.FindIndex(m => m.fileName == module.fileName) >= 0) return;
            loadedModules.Add(module);
            Program.Log("Loaded module: " + module.fileName);
        }

        public static void Init() {
            StopAll();
            timer = new(_ => {
                if (mem == null || mem.p == null) ScanModules();
                else if (mem.p.HasExited) {
                    PresenceHandler.StopPresence();
                    mem.CloseProcess();
                    ScanModules();
                } else activeModule.loop.Invoke();
            }, null, 0, 1000);
        }

        public static void StopAll() {
            timer?.Dispose();
            PresenceHandler.StopPresence();
        }

        private static void ScanModules() {
            Module module = CheckModules(out Process p);
            if (module == null) return;

            activeModule = module;
            mem = new(p);
            Console.WriteLine("found");
            Program.Log("Found app for module: " + module.fileName);
            PresenceHandler.Init(module.appId);
        }

        private static Module CheckModules(out Process p) {
            foreach (Module module in loadedModules) {
                Process[] procs = Process.GetProcessesByName(module.appName);
                if (procs.Length < 1) continue;

                if (module.titleContains != null) {
                    foreach (Process proc in procs) {
                        if (!proc.MainWindowTitle.ToLower().Contains(module.titleContains.ToLower())) continue;

                        p = proc;
                        return module;
                    }
                } else {
                    p = procs[0];
                    return module;
                }
            }
            p = null;
            return null;
        }
    }
}
