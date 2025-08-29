using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ShitSystem.Attributes;
using ShitSystem.Interfaces;
using UnityEngine.LowLevel;

namespace ShitSystem.Registerers
{
    public static class SystemRegisterer
    {
        private static readonly HashSet<RegSystem> RegisteredSystems = new ();
        
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnGameStart()
        {
            var findedAssembly = AppDomain.CurrentDomain.GetAssemblies();
            
            List<Type> systems = new();

            foreach (var asm in findedAssembly)
            {
                Type[] types;
                
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                if (types == null) 
                    continue;

                foreach (Type t in types)
                {
                    if (t == null) 
                        continue;

                    if (t.IsSubclassOf(typeof(RegSystem)) && !t.IsAbstract)
                        systems.Add(t);
                }
            }
            
            foreach (var system in OrderSystems(systems))
                TryRegisterSystem(system);

            RegisterUpdateSystems();
        }

        public static bool TryRegisterSystem(RegSystem system)
        {
            if (!RegisteredSystems.Add(system))
                return false;

            Injector.Register(system);
            
            if (system is IInitializable initializable)
                initializable.Initialize();
            
            return true;
        }

        private static List<RegSystem> OrderSystems(List<Type> types)
        {
            var sorted = new List<Type>();
            var visited = new HashSet<Type>();

            void Visit(Type type)
            {
                if (!visited.Add(type))
                    return;

                // Находим все зависимости типа
                var dependencies = type
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttribute<Inject>() != null)
                    .Select(f => f.FieldType)
                    .Where(types.Contains);

                foreach (var dep in dependencies)
                    Visit(dep);

                sorted.Add(type);
            }

            foreach (var type in types)
                Visit(type);

            List<RegSystem> systems = new();
            
            foreach (var res in sorted)
            {
                var system = Activator.CreateInstance(res) as RegSystem;
                systems.Add(system);
            }
            
            return systems;
        }

        private static void RegisterUpdateSystems()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == typeof(UnityEngine.PlayerLoop.Update))
                {
                    var list = new System.Collections.Generic.List<PlayerLoopSystem>(playerLoop.subSystemList[i].subSystemList);

                    list.Add(new PlayerLoopSystem
                    {
                        type = typeof(SystemRegisterer),
                        updateDelegate = UpdateSystems
                    });

                    playerLoop.subSystemList[i].subSystemList = list.ToArray();
                    break;
                }
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void UpdateSystems()
        {
            foreach (var system in RegisteredSystems)
            {
                if (system is IUpdatable updatable)
                    updatable.Update();
            }
        }
    }
}