using System;
using UnityEngine;

namespace ShitSystem
{
    public abstract class RegBehaviour : MonoBehaviour
    {
        private bool _registered;
        private bool _initialized;

        internal void OnInitialize()
        {
            if (_initialized)
                return;
            _initialized = true;
            Injector.Register(this);
            _registered = true;
            Initialize();
        }
        
        private void Awake()
        {
            OnInitialize();
        }

        protected virtual void OnDestroy()
        {
            Injector.Unregister(this);
            _registered = false;
        }

        protected virtual void OnEnable()
        {
            if (_registered)
                return;
            Injector.Register(this);
            _registered = true;
        }

        protected virtual void OnDisable()
        {
            if (!_registered)
                return;
            Injector.Unregister(this);
            _registered = false;
        }

        protected virtual void Initialize(){}
    }
}