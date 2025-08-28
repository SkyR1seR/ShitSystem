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
        private static readonly Dictionary<Type, HashSet<object>> RegisteredObjects = new ();
        private static readonly Dictionary<Type, HashSet<object>> RegisteredListeners = new();

        public static void Register<T>(T registeringObject)
        {
            RegisterObject(registeringObject);
            RegisterListener(registeringObject);
            RegisterInjections(registeringObject);
        }

        public static void Unregister<T>(T unregisteringObject)
        {
            UnregisterObject(unregisteringObject);
            UnregisterListener(unregisteringObject);
        }

        public static HashSet<T> GetListeners<T>()
        {
            if (!RegisteredListeners.TryGetValue(typeof(T), out var registeredListeners))
                return new HashSet<T>();
            
            HashSet<T> listeners = new();
            
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

        public static void RegisterInjections<T>(T registeringObject)
        {
            var fieldsInClass = registeringObject.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var field in fieldsInClass)
            {
                if (field.GetCustomAttribute<Inject>() == null)
                    continue;
                if (!RegisteredObjects.TryGetValue(field.FieldType, out HashSet<object> registeredObjects))
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
            
            if (RegisteredObjects.TryGetValue(type, out HashSet<object> registeredObjects))
                registeredObjects.Add(registeringObject);
            else
                RegisteredObjects.Add(type, new HashSet<object> { registeringObject });
        }
        
        private static void UnregisterObject<T>(T unregisteringObject)
        {
            var type = unregisteringObject.GetType();
            
            if (RegisteredObjects.TryGetValue(type, out HashSet<object> registeredObjects))
                registeredObjects.Remove(unregisteringObject);
        }

        private static void RegisterListener<T>(T registeringObject)
        {
            foreach (var part in registeringObject.GetType().GetInterfaces())
            {
                if (!part.IsDefined(typeof(Listener), false))
                    continue;
                if (RegisteredListeners.TryGetValue(part, out HashSet<object> registeredListeners))
                    registeredListeners.Add(registeringObject);
                else
                    RegisteredListeners.Add(part, new HashSet<object> { registeringObject });
            }
        }

        private static void UnregisterListener<T>(T unregisteringObject)
        {
            foreach (var part in unregisteringObject.GetType().GetInterfaces())
            {
                if (!part.IsDefined(typeof(Listener), false))
                    continue;
                if (RegisteredListeners.TryGetValue(part, out HashSet<object> registeredListeners))
                    registeredListeners.Remove(unregisteringObject);
            }
        }
    }
}
