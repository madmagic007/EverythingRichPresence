using Memory;
using System.Diagnostics;
using Timer = System.Threading.Timer;

namespace EverythingRichPresence.Modules {

    public class ModuleHandler {

        public static readonly List<Module> loadedModules = new();
        public static Module activeModule;
        private static Timer timer;
        public static Mem mem;

        public static void AddModule(Module module) {
            if (loadedModules.FindIndex(m => m.appName == module.appName) >= 0) return;
            loadedModules.Add(module);
        }

        public static void Init() {
            mem = new();
            StopAll();
            timer = new(_ => {
                if (mem.theProc == null) ScanModules();
                else if (mem.theProc.HasExited) {
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
            Module module = CheckModules(out int id);
            if (module == null) return;

            activeModule = module;
            mem = new();
            mem.OpenProcess(id);
            PresenceHandler.Init(module.appId);
        }

        private static Module CheckModules(out int id) {
            foreach (Module module in loadedModules) {
                Process[] procs = Process.GetProcessesByName(module.appName);
                if (procs.Length < 1) continue;

                if (module.titleContains != null) {
                    foreach (Process proc in procs) {
                        if (!proc.MainWindowTitle.ToLower().Contains(module.titleContains.ToLower())) continue;

                        id = proc.Id;
                        return module;
                    }
                } else {
                    id = procs[0].Id;
                    return module;
                }
            }
            id = 0;
            return null;
        }
    }
}
