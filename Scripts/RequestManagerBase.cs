#if MODULA
using System;
using System.Linq;
using Mirror;
using Modula;
using Modula.Common;
using UnityEngine;

namespace RequestForMirror
{
    public abstract class RequestManagerBase : ModularBehaviour
    {
        private static RequestManagerBase _globalInstance;
        public static RequestManagerBase Global => _globalInstance ??= FindGlobalInstance();

        private static RequestManagerBase FindGlobalInstance()
        {
            var requestManagers = FindObjectsOfType<RequestManagerBase>();
            Debug.Log($"Found request managers in scene: " + requestManagers.Length);
            return requestManagers.FirstOrDefault(manager => manager.GetComponent<NetworkIdentity>() == null);
        }

        public void Dispatch(Type requestType, object[] args)
        {
            //todo: notify if module is null
            if (!(GetModule(requestType) is IRequest request)) return;
            request.HandleRequest(args);
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
#endif