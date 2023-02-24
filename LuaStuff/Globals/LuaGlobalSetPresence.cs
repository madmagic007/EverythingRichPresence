using Neo.IronLua;

namespace EverythingRichPresence.LuaStuff.Globals
{

    public class LuaGlobalSetPresence : IGlobal {

        public void LoadGlobal(dynamic env) {
            env.SetPresence = new Action<LuaTable>(tbl => PresenceHandler.Handle(tbl));
        }
    }
}
