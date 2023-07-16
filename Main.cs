using HarmonyLib;
using KitchenMods;
using PreferenceSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = "IcedMilo.PlateUp.DragNDropDesigner";
        public const string MOD_NAME = "Drag N' Drop Designer";
        public const string MOD_VERSION = "1.0.1";

        internal static string GRAB_BUTTON_PREF_ID = "grabButton";
        internal static string ACT_BUTTON_PREF_ID = "actButton";
        internal static string PING_BUTTON_PREF_ID = "pingButton";
        internal static string BLUEPRINT_BUTTON_PREF_ID = "blueprintButton";
        internal static string MISCELLANEOUS_BUTTON_PREF_ID = "miscButton";

        internal static PreferenceSystemManager PrefManager;

        internal static MouseApplianceInteractionSystem.MouseButton GrabButton;
        internal static MouseApplianceInteractionSystem.MouseButton ActButton;
        internal static MouseApplianceInteractionSystem.MouseButton PingButton;
        internal static MouseApplianceInteractionSystem.MouseButton BlueprintButton;
        internal static MouseApplianceInteractionSystem.MouseButton MiscellaneousButton;

        private readonly Harmony _harmony;
        private static List<Assembly> PatchedAssemblies = new List<Assembly>();

        public Main()
        {
            if (_harmony == null)
            {
                _harmony = new Harmony(MOD_GUID);
            }
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null && !PatchedAssemblies.Contains(assembly))
            {
                _harmony.PatchAll(assembly);
                PatchedAssemblies.Add(assembly);
            }
        }

        public void PostActivate(Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            RegisterPreferences();
            InitButtons();
        }

        public void PreInject() { }

        public void PostInject() { }

        protected void RegisterPreferences()
        {
            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);

            string[] mouseButtons = new string[]
            {
                MouseApplianceInteractionSystem.MouseButton.Left.ToString(),
                MouseApplianceInteractionSystem.MouseButton.Right.ToString(),
                MouseApplianceInteractionSystem.MouseButton.Middle.ToString(),
                MouseApplianceInteractionSystem.MouseButton.Forward.ToString(),
                MouseApplianceInteractionSystem.MouseButton.Back.ToString()
            };

            string[] mouseButtonTexts = new string[]
            {
                "Left Click",
                "Right Click",
                "Middle Click",
                "Forward Button",
                "Back Button"
            };

            PrefManager
                .AddLabel("Drag N' Drop Controls")
                .AddSpacer()
                .AddLabel("Grab")
                .AddOption<string>(
                    GRAB_BUTTON_PREF_ID,
                    MouseApplianceInteractionSystem.MouseButton.Left.ToString(),
                    mouseButtons,
                    mouseButtonTexts,
                    delegate (string value)
                    {
                        UpdateButton(ref GrabButton, value);
                    })
                .AddLabel("Act")
                .AddOption<string>(
                    ACT_BUTTON_PREF_ID,
                    MouseApplianceInteractionSystem.MouseButton.Right.ToString(),
                    mouseButtons,
                    mouseButtonTexts,
                    delegate (string value)
                    {
                        UpdateButton(ref ActButton, value);
                    })
                .AddLabel("Ping")
                .AddOption<string>(
                    PING_BUTTON_PREF_ID,
                    MouseApplianceInteractionSystem.MouseButton.Middle.ToString(),
                    mouseButtons,
                    mouseButtonTexts,
                    delegate (string value)
                    {
                        UpdateButton(ref PingButton, value);
                    })
                .AddLabel("Store/Retrieve Blueprint")
                .AddOption<string>(
                    BLUEPRINT_BUTTON_PREF_ID,
                    MouseApplianceInteractionSystem.MouseButton.Middle.ToString(),
                    mouseButtons,
                    mouseButtonTexts,
                    delegate (string value)
                    {
                        UpdateButton(ref BlueprintButton, value);
                    })
                .AddLabel("Miscellaneous Action")
                .AddOption<string>(
                    MISCELLANEOUS_BUTTON_PREF_ID,
                    MouseApplianceInteractionSystem.MouseButton.Middle.ToString(),
                    mouseButtons,
                    mouseButtonTexts,
                    delegate (string value)
                    {
                        UpdateButton(ref MiscellaneousButton, value);
                    })
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

        private void UpdateButton(ref MouseApplianceInteractionSystem.MouseButton button, string value)
        {
            if (!Enum.TryParse(value, true, out button))
            {
                LogError($"Failed to parse {value} to {typeof(MouseApplianceInteractionSystem.MouseButton)}.");
                button = default;
            }
        }

        private void InitButtons()
        {
            UpdateButton(ref GrabButton, PrefManager.Get<string>(GRAB_BUTTON_PREF_ID));
            UpdateButton(ref ActButton, PrefManager.Get<string>(ACT_BUTTON_PREF_ID));
            UpdateButton(ref PingButton, PrefManager.Get<string>(PING_BUTTON_PREF_ID));
            UpdateButton(ref BlueprintButton, PrefManager.Get<string>(BLUEPRINT_BUTTON_PREF_ID));
            UpdateButton(ref MiscellaneousButton, PrefManager.Get<string>(MISCELLANEOUS_BUTTON_PREF_ID));
        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
