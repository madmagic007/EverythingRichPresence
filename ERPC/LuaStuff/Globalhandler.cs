﻿using ERPC.LuaStuff.Globals;

namespace ERPC.LuaStuff {

    public class Globalhandler {

        public static void LoadGlobals(dynamic env) {
            globals.ForEach(g => g.LoadGlobal(env));
        }

        private static List<IGlobal> globals = new() {
           new LuaGlobalMem(), new LuaGlobalSetPresence()
        };
    }
}
