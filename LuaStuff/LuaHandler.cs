using EverythingRichPresence.Modules;
using Neo.IronLua;

namespace EverythingRichPresence.LuaStuff {

    public class LuaHandler {

        public static void Init() {
            LuaCompileOptions lco = new();

            FileInfo f = new(Application.ExecutablePath);
            DirectoryInfo modulesDir = new(f.Directory + "/modules");
            modulesDir.Create();

            Directory.GetFiles(modulesDir.FullName, "*", SearchOption.AllDirectories).OfType<String>().ToList().ForEach(f => {
                Lua lua = new();
                dynamic env = lua.CreateEnvironment<LuaGlobal>();

                Globalhandler.LoadGlobals(env);
                env.RegisterModule = new Action<LuaTable, Func<object>>((table, loop) => {
                    Module module = new() {
                        appName = table.GetValue("appName") as string,
                        appId = table.GetValue("discordAppID") as string,
                        titleContains = table.GetValue("titleContains") as string,
                        updateUrl = table.GetValue("updateUrl") as string,
                        filePath = f,
                        lua = lua,
                        env = env,
                        loop = loop
                    };
                    ModuleHandler.AddModule(module);
                });

                env.DoChunk(lua.CompileChunk(f, lco));
            });
        }
    }
}
