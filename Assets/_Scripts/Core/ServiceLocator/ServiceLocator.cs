using System;
using System.Collections.Generic;

namespace Game.Core.ServiceLocator
{
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        public static ServiceLocator Instance => _instance ?? (_instance = new ServiceLocator());

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            Instance.RegisterService(service);
        }

        public static T Get<T>() where T : class
        {
            return Instance.GetService<T>();
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            return Instance.TryGetService(out service);
        }

        public static void Clear()
        {
            Instance.ClearServices();
        }

        private void RegisterService<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
            }
            _services[type] = service;
        }

        private T GetService<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            throw new Exception($"Service {type.Name} not registered.");
        }

        private bool TryGetService<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return true;
            }
            service = null;
            return false;
        }

        private void ClearServices()
        {
            _services.Clear();
        }
    }
}
