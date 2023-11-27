using EverythingRichPresence.Modules;
using Microsoft.VisualBasic;
using Neo.IronLua;
using System.IO;

namespace EverythingRichPresence.LuaStuff {

    public class LuaHandler {

        public static void Init() {
            ModuleHandler.loadedModules.Clear();
            Directory.GetFiles(Program.modulesDir.FullName, "*", SearchOption.AllDirectories).OfType<string>().ToList().ForEach(f => {
                Lua lua = new();
                dynamic env = lua.CreateEnvironment<LuaGlobal>();

                Globalhandler.LoadGlobals(env);
                env.RegisterModule = new Action<LuaTable, Func<object>>((table, loop) => {
                    Module module = new() {
                        appName = table.GetValue("appName") as string,
                        appId = table.GetValue("discordAppId") as string,
                        titleContains = table.GetValue("titleContains") as string,
                        updateUrl = table.GetValue("updateUrl") as string,
                        filePath = f,
                        lua = lua,
                        env = env,
                        loop = loop,
                        fileName = Path.GetFileNameWithoutExtension(f)
                    };
                    module.Update();
                    ModuleHandler.AddModule(module);
                });

                env.dochunk(lua.CompileChunk(f, new LuaCompileOptions()));
            });
        }
    }
}
