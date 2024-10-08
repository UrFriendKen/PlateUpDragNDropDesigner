﻿using Controllers;
using Kitchen;
using Kitchen.NetworkSupport;
using KitchenDragNDropDesigner.Helpers;
using KitchenMods;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KitchenDragNDropDesigner.Views
{
    public class MouseInputView : UpdatableObjectView<MouseInputView.ViewData>, ISpecificViewResponse
    {
        public class UpdateView : ResponsiveViewSystemBase<ViewData, ResponseData>, IModSystem
        {
            EntityQuery Players;

            List<int> InputSources = new List<int>();
            
            protected override void Initialise()
            {
                base.Initialise();
                Players = GetEntityQuery(typeof(CLinkedView), typeof(CPlayer), typeof(CMouseData));
            }

            List<(int id, ConnectionType connection, int index)> _playerConnectionTypes = new List<(int id, ConnectionType connection, int index)>();
            private IEnumerable<int> GetOrderedPlayersIndex()
            {
                using NativeArray<CPlayer> players = Players.ToComponentDataArray<CPlayer>(Allocator.Temp);
                _playerConnectionTypes.Clear();
                for (int i = 0; i < players.Length; i++)
                {
                    CPlayer cPlayer = players[i];

                    bool hasNetworkPeerInfo = Session.PeerInformation.TryGetValue(cPlayer.InputSource, out NetworkPeerInformation networkPeerInfo);
                    ConnectionType connection = (hasNetworkPeerInfo ? networkPeerInfo.Connection : ConnectionType.Unknown);
                    _playerConnectionTypes.Add((cPlayer.ID, connection, i));
                }
                return _playerConnectionTypes
                    .OrderBy(item => item.id == 0 ? 1 : 0)
                    .ThenBy(item => item.connection)
                    .ThenBy(item => item.id)
                    .Select(item => item.index);
            }

            protected override void OnUpdate()
            {
                using NativeArray<Entity> entities = Players.ToEntityArray(Allocator.Temp);
                using NativeArray<CLinkedView> views = Players.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                using NativeArray<CPlayer> players = Players.ToComponentDataArray<CPlayer>(Allocator.Temp);
                using NativeArray<CMouseData> mouseDatas = Players.ToComponentDataArray<CMouseData>(Allocator.Temp);

                InputSources.Clear();
                foreach (int i in GetOrderedPlayersIndex())
                {
                    Entity entity = entities[i];
                    CLinkedView view = views[i];
                    CPlayer player = players[i];

                    SendUpdate(view, new ViewData()
                    {
                        PlayerID = player.ID,
                        InputSource = player.InputSource,
                        IsFirstLocalPlayer = !InputSources.Contains(player.InputSource)
                    });

                    InputSources.Add(player.InputSource);
                    bool isHostPlayer = player.InputSource == InputSourceIdentifier.Identifier.Value;

                    CMouseData mouseData = default;
                    
                    ButtonState GetModifiedCachedButtonState(ButtonState oldButtonState)
                    {
                        switch (oldButtonState)
                        {
                            case ButtonState.Held:
                                return oldButtonState;
                            default:
                                return ButtonState.Up;
                        }
                    }

                    CMouseData oldMouseData = mouseDatas[i];
                    mouseData.Active = oldMouseData.Active;
                    mouseData.Position = oldMouseData.Position;
                    mouseData.Grab = GetModifiedCachedButtonState(oldMouseData.Grab);
                    mouseData.Act = GetModifiedCachedButtonState(oldMouseData.Act);
                    mouseData.Ping = GetModifiedCachedButtonState(oldMouseData.Ping);
                    mouseData.StoreRetrieveBlueprint = GetModifiedCachedButtonState(oldMouseData.StoreRetrieveBlueprint);
                    mouseData.Miscellaneous = GetModifiedCachedButtonState(oldMouseData.Miscellaneous);

                    ApplyUpdates(view, HandleResponse);
                    Set(entity, mouseData);


                    void HandleResponse(ResponseData responseData)
                    {
                        if (mouseData.Grab == ButtonState.Pressed ||
                            mouseData.Act == ButtonState.Pressed ||
                            mouseData.Ping == ButtonState.Pressed ||
                            mouseData.StoreRetrieveBlueprint == ButtonState.Pressed ||
                            mouseData.Miscellaneous == ButtonState.Pressed)
                            return;

                        mouseData = new CMouseData()
                        {
                            Active = responseData.Active,
                            Position = responseData.Position,
                            Grab = responseData.Grab,
                            Act = responseData.Act,
                            Ping = responseData.Ping,
                            StoreRetrieveBlueprint = responseData.StoreRetrieveBlueprint,
                            Miscellaneous = responseData.Miscellaneous
                        };
                    }
                }
            }
        }

        [MessagePackObject(false)]
        public class ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public int PlayerID;
            [Key(1)] public int InputSource;
            [Key(3)] public bool IsFirstLocalPlayer;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<MouseInputView>();

            public bool IsChangedFrom(ViewData check)
            {
                return IsFirstLocalPlayer != check.IsFirstLocalPlayer ||
                    PlayerID != check.PlayerID ||
                    InputSource != check.InputSource;
            }
        }

        [MessagePackObject(false)]
        public class ResponseData : IResponseData, IViewResponseData
        {
            [Key(0)] public bool Active;
            [Key(1)] public Vector3 Position;
            [Key(2)] public ButtonState Grab;
            [Key(3)] public ButtonState Act;
            [Key(4)] public ButtonState Ping;
            [Key(5)] public ButtonState StoreRetrieveBlueprint;
            [Key(6)] public ButtonState Miscellaneous;
        }

        int PlayerID = 0;
        bool IsMyPlayer = false;
        bool IsFirstLocalPlayer = false;

        protected override void UpdateData(ViewData data)
        {
            IsMyPlayer = data.InputSource == InputSourceIdentifier.Identifier.Value;
            PlayerID = data.PlayerID;
            IsFirstLocalPlayer = data.IsFirstLocalPlayer;
        }

        Action<IResponseData, Type> Callback;
        public void SetCallback(Action<IResponseData, Type> callback) => Callback = callback;

        private Dictionary<Action, ButtonState> _cache = new Dictionary<Action, ButtonState>();

        private static readonly Dictionary<Action, MouseApplianceInteractionSystem.MouseButton> _mapping = new Dictionary<Action, MouseApplianceInteractionSystem.MouseButton>()
        {
            { Action.Grab, MouseApplianceInteractionSystem.MouseButton.Left },
            { Action.Act, MouseApplianceInteractionSystem.MouseButton.Right },
            { Action.Ping, MouseApplianceInteractionSystem.MouseButton.Middle },
            { Action.StoreRetrieveBlueprint, MouseApplianceInteractionSystem.MouseButton.Middle },
            { Action.Miscellaneous, MouseApplianceInteractionSystem.MouseButton.Middle },
        };

        private enum Action
        {
            Grab,
            Act,
            Ping,
            StoreRetrieveBlueprint,
            Miscellaneous
        }

        float LastUpdateTime = 0f;

        void Update()
        {
            if (!IsMyPlayer)
                return;

            ControllerType activeControllers = ControllerType.None;
            foreach (PlayerInfo playerInfo in Players.Main.All())
            {
                if (playerInfo.Identifier != InputSourceIdentifier.Identifier.Value)
                    continue;
                activeControllers |= InputSourceIdentifier.DefaultInputSource.GetCurrentController(playerInfo.ID);
            }
            bool hasKeyboardPlayer = activeControllers.HasFlag(ControllerType.Keyboard);

            bool isKeyboardPlayer = InputSourceIdentifier.DefaultInputSource.GetCurrentController(PlayerID) == ControllerType.Keyboard;

            if (Callback == null)
                return;

            bool isMousePlayer = !(
                (!hasKeyboardPlayer && !IsFirstLocalPlayer) ||
                (hasKeyboardPlayer && !isKeyboardPlayer));

            if (isMousePlayer && !InputSourceIdentifier.DefaultInputSource.GlobalLock.IsLocked)
            {
                Callback(new ResponseData()
                {
                    Active = true,
                    Position = MouseHelpers.MousePlanePos(),
                    Grab = GetButtonState(Action.Grab),
                    Act = GetButtonState(Action.Act),
                    Ping = GetButtonState(Action.Ping),
                    StoreRetrieveBlueprint = GetButtonState(Action.StoreRetrieveBlueprint),
                    Miscellaneous = GetButtonState(Action.Miscellaneous)
                }, typeof(ResponseData));
                return;
            }

            float time = Time.realtimeSinceStartup;
            if (time - LastUpdateTime > 3f)
            {
                Callback(default(ResponseData), typeof(ResponseData));
                LastUpdateTime = time;
            }
        }

        ButtonState GetButtonState(Action action)
        {
            bool isPressed = MouseHelpers.IsMouseButtonPressed(_mapping[action]);
            ButtonState old = _cache.TryGetValue(action, out ButtonState cachedState) ? cachedState : ButtonState.Up;
            ButtonState buttonState = ResolveButtonState(isPressed, old);
            _cache[action] = buttonState;
            return buttonState;
        }

        ButtonState ResolveButtonState(bool isPressed, ButtonState old)
        {
            if (old == ButtonState.Consumed)
            {
                if (isPressed)
                    return ButtonState.Consumed;
                return ButtonState.Up;
            }

            if (isPressed)
            {
                if (old != ButtonState.Held && old != ButtonState.Pressed)
                    return ButtonState.Pressed;
                return ButtonState.Held;
            }
            
            if (old != ButtonState.Up && old != ButtonState.Released)
                return ButtonState.Released;

            return ButtonState.Up;
        }
    }
}
