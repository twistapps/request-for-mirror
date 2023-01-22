#if MODULA
using System;
using System.Linq;
using Mirror;
using Modula;
using UnityEngine;

namespace RequestForMirror
{
    //todo: rename to RequestGroup?
    public abstract class RequestManagerBase : ModularBehaviour
    {
        private static RequestManagerBase _globalInstance;
        public static RequestManagerBase Global => _globalInstance ??= FindGlobalInstance();

        public bool IsGlobal => GetComponent<NetworkIdentity>() == null;

        protected override void Awake()
        {
            base.Awake();
        }

        private static RequestManagerBase FindGlobalInstance()
        {
            var requestManagers = FindObjectsOfType<RequestManagerBase>();
            Debug.Log("Found request managers in scene: " + requestManagers.Length);
            return requestManagers.FirstOrDefault(manager => manager.GetComponent<NetworkIdentity>() == null);
        }

        public void Dispatch(Type requestType, object[] args)
        {
            //todo: notify if module is null
            if (!(GetModule(requestType) is IRequest request)) return;
            request.HandleRequest(args);
        }
    }
}
#endif