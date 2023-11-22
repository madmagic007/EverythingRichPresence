using EverythingRichPresence.Modules;
using Neo.IronLua;

namespace EverythingRichPresence.LuaStuff.Globals {

    internal class LuaGlobalMem : IGlobal {

        public void LoadGlobal(dynamic env) {
            dynamic mem = ((LuaGlobal)env).Lua.CreateEnvironment<LuaGlobal>();

            mem.readString = new Func<string, string>(s => ModuleHandler.mem?.ReadString(s, length: 100) ?? "");
            mem.readInt = new Func<string, int>(s => ModuleHandler.mem?.ReadInt(s) ?? 0);
            mem.readFloat = new Func<string, float>(s => ModuleHandler.mem?.ReadFloat(s) ?? 0);
            mem.readInt = new Func<string, double>(s => ModuleHandler.mem?.ReadDouble(s) ?? 0);
            env.Mem = mem;
        }
    }
}
