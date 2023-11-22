using EverythingRichPresence.LuaStuff;
using EverythingRichPresence.LuaStuff.Globals;
using Flurl.Http;
using Neo.IronLua;
using System.IO;
using System.Net;

namespace EverythingRichPresence.Modules {

    public class Module {

        public Lua lua;
        public dynamic env;
        public string appName, appId, titleContains, updateUrl, filePath;
        public Func<object> loop;

        public void Update() {
            if (string.IsNullOrEmpty(updateUrl)) return;
            lua.Dispose();

            string s = updateUrl.GetStringAsync().Result;
            File.WriteAllText(filePath, s);

            lua.Dispose();
            env = lua.CreateEnvironment<LuaGlobal>();

            Globalhandler.LoadGlobals(env);
            env.RegisterModule = new Action<LuaTable, Func<object>>((table, loop) => {
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
