using ERPC.LuaStuff;
using Flurl.Http;
using Neo.IronLua;

namespace ERPC.Modules {

    public class Module {

        public Lua lua;
        public dynamic env;
        public string appName, appId, titleContains, updateUrl, filePath, fileName;
        public Func<object> loop;

        public void Update() {
            if (string.IsNullOrEmpty(updateUrl)) return;
            lua.Dispose();

            string s = updateUrl.GetStringAsync().Result;
            File.WriteAllText(filePath, s);

            lua.Dispose();
            env = lua.CreateEnvironment<LuaGlobal>();

            LuaGlobals.LoadGlobals(env);
            env.RegisterModule = new Action<LuaTable, Func<object>>((table, loop) => {
				Console.WriteLine("created updated module");
				appName = table.GetValue("appName") as string;
                appId = table.GetValue("discordAppId") as string;
                titleContains = table.GetValue("titleContains") as string;
                updateUrl = table.GetValue("updateUrl") as string;
                this.loop = loop;
            });

            env.dochunk(lua.CompileChunk(filePath, new LuaCompileOptions()));
        }
    }
}
