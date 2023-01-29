using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

namespace RequestForMirror
{
    public static class NetworkEvents
    {
        // /// <summary>
        // ///     Runs on both Server and Client
        // /// </summary>
        // public override void LateUpdate()
        // {
        //     base.LateUpdate();
        //
        //     if (!_didCallEventsThisFrame) return;
        //     _hasBeenCalledThisFrame = new Dictionary<ServerCallbackActionType, bool>();
        //     _didCallEventsThisFrame = false;
        // }

        [RuntimeInitializeOnLoadMethod]
        private static void InitEvents()
        {
            var nm = NetworkManager.Singleton;
            nm.OnServerStarted += () =>
            {
                if (nm.IsHost) CallEvent(ServerCallbackActionType.StartHost);
                CallEvent(ServerCallbackActionType.StartServer);
                _hasBeenCalledThisFrame.Clear();
            };

            nm.OnClientConnectedCallback += clientId =>
            {
                if (clientId == nm.LocalClientId) CallEvent(ServerCallbackActionType.StartClient);
                _hasBeenCalledThisFrame.Clear();
            };

            nm.OnClientDisconnectCallback += clientId =>
            {
                CallEvent(ServerCallbackActionType.StopClient);
                _hasBeenCalledThisFrame.Clear();
            };
        }


        #region Events

        private enum ServerCallbackActionType
        {
            StartHost,
            StartServer,
            StartClient,
            StopHost,
            StopServer,
            StopClient
        }

        private static readonly Dictionary<ServerCallbackActionType, bool> _hasBeenCalledThisFrame =
            new Dictionary<ServerCallbackActionType, bool>();

        [UsedImplicitly] public static Action StartHostEvent;
        [UsedImplicitly] public static Action StartServerEvent;
        [UsedImplicitly] public static Action StartClientEvent;
        [UsedImplicitly] public static Action StopHostEvent;
        [UsedImplicitly] public static Action StopServerEvent;
        [UsedImplicitly] public static Action StopClientEvent;

        private static bool _didCallEventsThisFrame;

        /// <summary>
        ///     Call event by its type, restricted to one call per frame
        /// </summary>
        private static void CallEvent(ServerCallbackActionType eventType)
        {
            if (_hasBeenCalledThisFrame.ContainsKey(eventType) && _hasBeenCalledThisFrame[eventType]) return;
            var action = eventType switch
            {
                ServerCallbackActionType.StartHost => StartHostEvent,
                ServerCallbackActionType.StartServer => StartServerEvent,
                ServerCallbackActionType.StartClient => StartClientEvent,
                ServerCallbackActionType.StopHost => StopHostEvent,
                ServerCallbackActionType.StopServer => StopServerEvent,
                ServerCallbackActionType.StopClient => StopClientEvent,
                _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
            };

            _didCallEventsThisFrame = true;
            _hasBeenCalledThisFrame[eventType] = true;
            action?.Invoke();
        }

        public static void ExecuteWhenStartedHost(Action action)
        {
            StartHostEvent += action;
            #if MIRROR
            if (NetworkServer.activeHost)
                action();
            #elif UNITY_NETCODE
            if (NetworkManager.Singleton.IsHost)
                action();
            #endif
        }

        public static void ExecuteWhenStartedServer(Action action)
        {
            StartServerEvent += action;
            #if MIRROR
            if (NetworkServer.active)
                action();
            #elif UNITY_NETCODE
            if (NetworkManager.Singleton.IsServer)
                action();
            #endif
        }

        public static void ExecuteWhenStartedClient(Action action)
        {
            StartClientEvent += action;
            #if MIRROR
            if (NetworkClient.active)
                action();
            #elif UNITY_NETCODE
            if (NetworkManager.Singleton.IsClient)
                action();
            #endif
        }

        public static void ExecuteWhenStoppedHost(Action action)
        {
            StopHostEvent += action;
            #if MIRROR
            if (!NetworkServer.activeHost)
                action();
            #elif UNITY_NETCODE
            if (!NetworkManager.Singleton.IsHost)
                action();
            #endif
        }

        public static void ExecuteWhenStoppedServer(Action action)
        {
            StopServerEvent += action;
            #if MIRROR
        if (!NetworkServer.active)
            action();
            #elif UNITY_NETCODE
            if (!NetworkManager.Singleton.IsServer)
                action();
            #endif
        }

        public static void ExecuteWhenStoppedClient(Action action)
        {
            StopClientEvent += action;
            #if MIRROR
            if (!NetworkClient.active)
                action();
            #elif UNITY_NETCODE
            if (!NetworkManager.Singleton.IsClient)
                action();
            #endif
        }

        #endregion
    }
}