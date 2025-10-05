using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using ShitSystem.Attributes;

namespace ShitSystem
{
    [UsedImplicitly]
    public static class Injector
    {
        private static readonly Dictionary<Type, List<object>> RegisteredObjects = new ();
        private static readonly Dictionary<Type, List<object>> RegisteredListeners = new();

        public static void Register<T>([NotNull]T registeringObject)
        {
            RegisterObject(registeringObject);
            RegisterListener(registeringObject);
            RegisterInjections(registeringObject);
        }

        public static void Unregister<T>([NotNull]T unregisteringObject)
        {
            UnregisterObject(unregisteringObject);
            UnregisterListener(unregisteringObject);
        }

        public static List<T> GetListeners<T>()
        {
            if (!RegisteredListeners.TryGetValue(typeof(T), out var registeredListeners))
                return new List<T>();
            
            List<T> listeners = new();
            
            foreach (var obj in registeredListeners)
                listeners.Add((T)obj);
            
            return listeners;
        }
        
        public static void GetListeners<T>(Action<T> action)
        {
            if (!RegisteredListeners.TryGetValue(typeof(T), out var registeredListeners))
                return;
            
            if (action == null) 
                throw new ArgumentNullException(nameof(action));
            
            HashSet<T> listeners = new();
            
            foreach (var obj in registeredListeners)
                listeners.Add((T)obj);
            
            foreach (var item in listeners)
            {
                action(item);
            }
        }
        
        public static bool TryGetObject<T>(out T type)
        {
            if (!RegisteredObjects.TryGetValue(typeof(T), out List<object> registeredObjects))
            {
                type = default(T);
                return false;
            }
            
            object first = null;
                
            foreach (var item in registeredObjects)
            {
                first = item;
                break;
            }
            
            type = (T)first;
            return true;
        }

        public static void RegisterInjections<T>(T registeringObject)
        {
            var fieldsInClass = registeringObject.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var field in fieldsInClass)
            {
                if (field.GetCustomAttribute<Inject>() == null)
                    continue;
                if (!RegisteredObjects.TryGetValue(field.FieldType, out List<object> registeredObjects))
                    continue;
                
                object first = null;
                
                foreach (var item in registeredObjects)
                {
                    first = item;
                    break;
                }
                
                field.SetValue(registeringObject, first);
            }
        }

        private static void RegisterObject<T>(T registeringObject)
        {
            var type = registeringObject.GetType();
            
            if (RegisteredObjects.TryGetValue(type, out List<object> registeredObjects))
                registeredObjects.Add(registeringObject);
            else
                RegisteredObjects.Add(type, new List<object> { registeringObject });
        }
        
        private static void UnregisterObject<T>(T unregisteringObject)
        {
            var type = unregisteringObject.GetType();
            
            if (RegisteredObjects.TryGetValue(type, out List<object> registeredObjects))
                registeredObjects.Remove(unregisteringObject);
        }

        private static void RegisterListener<T>(T registeringObject)
        {
            foreach (var part in registeringObject.GetType().GetInterfaces())
            {
                if (!part.IsDefined(typeof(Listener), false))
                    continue;
                if (RegisteredListeners.TryGetValue(part, out List<object> registeredListeners))
                    registeredListeners.Add(registeringObject);
                else
                    RegisteredListeners.Add(part, new List<object> { registeringObject });
            }
        }

        private static void UnregisterListener<T>(T unregisteringObject)
        {
            foreach (var part in unregisteringObject.GetType().GetInterfaces())
            {
                if (!part.IsDefined(typeof(Listener), false))
                    continue;
                if (RegisteredListeners.TryGetValue(part, out List<object> registeredListeners))
                    registeredListeners.Remove(unregisteringObject);
            }
        }
    }
}
