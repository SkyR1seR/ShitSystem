using System.Collections.Generic;
using UnityEngine;

namespace ShitSystem.Registerers
{
    public static class ScriptableRegisterer
    {
        private static readonly HashSet<RegScriptable> RegisteredScriptables = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnGameStart()
        {
            var findedScriptables = Resources.LoadAll<ScriptableObject>("");
            
            foreach (var scriptable in findedScriptables)
                if (scriptable is RegScriptable regScriptable)
                    TryRegisterScriptable(regScriptable);
        }
        
        public static bool TryRegisterScriptable(RegScriptable scriptable)
        {
            if (!RegisteredScriptables.Add(scriptable))
                return false;
            
            Injector.Register(scriptable);
            return true;
        }
    }
}