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
            var findedScriptables = Resources.LoadAll<RegScriptable>("");
            
            foreach (var scriptable in findedScriptables)
                TryRegisterScriptable(scriptable);
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