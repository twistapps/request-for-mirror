#if MODULA && REQUESTIFY_ENABLED
using System;
using System.Linq;
using Modula;
using Modula.Common;
using Unity.Netcode;
using UnityEngine;
#if MIRROR
using Mirror;
#endif

namespace RequestForMirror
{
    //todo: rename to RequestGroup?
    public abstract class RequestManagerBase : ModularBehaviour
    {
        private static RequestManagerBase _globalInstance;
        public static RequestManagerBase Global => _globalInstance ??= FindGlobalInstance();

        #if MIRROR
        public bool IsGlobal => GetComponent<NetworkIdentity>() == null;
        #elif UNITY_NETCODE
        public bool IsGlobal => GetComponent<NetworkObject>() == null;
        #endif

        protected override void Awake()
        {
            base.Awake();
        }

        private static RequestManagerBase FindGlobalInstance()
        {
            var requestManagers = FindObjectsOfType<RequestManagerBase>();
            Debug.Log("Found request managers in scene: " + requestManagers.Length);
            return requestManagers.FirstOrDefault(manager => manager.IsGlobal);
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