using Kitchen;
using Kitchen.Modules;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.Preferences;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Markup;
using Unity.Entities;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    public class Main : BaseMod
    {
        public const string MOD_GUID = "IcedMilo.PlateUp.DragNDropDesigner";
        public const string MOD_NAME = "Drag N' Drop Designer";
        public const string MOD_VERSION = "0.2.5";
        public const string MOD_AUTHOR = "IcedMilo";
        public const string MOD_GAMEVERSION = ">=1.1.4";

        private readonly HarmonyLib.Harmony m_harmony = new HarmonyLib.Harmony("IcedMilo.PlateUp.Harmony.DragNDropDesigner");

        internal static string GRAB_BUTTON_PREF_ID = "grabButton";
        internal static string ACT_BUTTON_PREF_ID = "actButton";
        internal static string PING_BUTTON_PREF_ID = "pingButton";

        internal static PreferenceManager Manager;

        internal static PreferenceMouseButton GrabButtonPreference;
        internal static PreferenceMouseButton ActButtonPreference;
        internal static PreferenceMouseButton PingButtonPreference;


        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        internal static EntityQuery Players;

        internal static bool IsPauseMenuOpen = false;

        protected override void OnInitialise()
        {
            // For log file output so the official plateup support staff can identify if/which a mod is being used
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            instance = this;

            if (Session.CurrentGameNetworkMode == GameNetworkMode.Host)
            {
                m_harmony.PatchAll(Assembly.GetExecutingAssembly());
                try
                {
                    Players = GetEntityQuery(typeof(CPlayer));
                    World.GetExistingSystem<PickUpAndDropAppliance>().Enabled = false;
                    World.GetExistingSystem<RotateAppliances>().Enabled = false;
                    World.GetExistingSystem<RotateChairs>().Enabled = false;
                    LogWarning("Disabled Vanilla Systems");
                }
                catch (NullReferenceException)
                {
                    LogWarning("Could not disable system! Are in you multiplayer as a non-host?");
                }
            }
        }
        
        protected override void OnUpdate()
        {
            Players = GetEntityQuery(typeof(CPlayer));

            GameObject pauseMenuPopup = GameObject.Find("Player Pause Popup");
            if (pauseMenuPopup != null)
            {
                Transform container = pauseMenuPopup.transform.Find("Container");
                IsPauseMenuOpen = container.gameObject.activeSelf;
            }
        }

        protected override void OnPostActivate(Mod mod)
        {
            RegisterPreferences();
            RegisterMenu();
        }

        protected void RegisterPreferences()
        {
            Manager = new PreferenceManager(MOD_GUID);
            GrabButtonPreference = Manager.RegisterPreference<PreferenceMouseButton>(new PreferenceMouseButton(GRAB_BUTTON_PREF_ID, MouseApplianceInteractionSystem.MouseButton.Left));
            ActButtonPreference = Manager.RegisterPreference<PreferenceMouseButton>(new PreferenceMouseButton(ACT_BUTTON_PREF_ID, MouseApplianceInteractionSystem.MouseButton.Right));
            PingButtonPreference = Manager.RegisterPreference<PreferenceMouseButton>(new PreferenceMouseButton(PING_BUTTON_PREF_ID, MouseApplianceInteractionSystem.MouseButton.Middle));
            Manager.Load();
        }

        protected void RegisterMenu()
        {
            Events.PreferenceMenu_PauseMenu_CreateSubmenusEvent += (s, args) =>
            {
                args.Menus.Add(typeof(DragNDropMenu<PauseMenuAction>), new DragNDropMenu<PauseMenuAction>(args.Container, args.Module_list));
            };
            ModsPreferencesMenu<PauseMenuAction>.RegisterMenu(MOD_NAME, typeof(DragNDropMenu<PauseMenuAction>), typeof(PauseMenuAction));
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

    internal class PreferenceMouseButton : PreferenceBase<MouseApplianceInteractionSystem.MouseButton>
    {
        
        public PreferenceMouseButton(string key, MouseApplianceInteractionSystem.MouseButton defaultValue = MouseApplianceInteractionSystem.MouseButton.Left)
            : base(key, defaultValue)
        {
        }

        public override void Deserialize(string json)
        {
            base.Value = (MouseApplianceInteractionSystem.MouseButton)int.Parse(json);
        }

        public override string Serialize()
        {
             return base.Value.ToString();
        }
    }

    public class DragNDropMenu<T> : KLMenu<T>
    {
        Option<MouseApplianceInteractionSystem.MouseButton> GrabOption;
        Option<MouseApplianceInteractionSystem.MouseButton> ActOption;
        Option<MouseApplianceInteractionSystem.MouseButton> PingOption;

        readonly List<MouseApplianceInteractionSystem.MouseButton> values = new List<MouseApplianceInteractionSystem.MouseButton>
        {
            MouseApplianceInteractionSystem.MouseButton.Left,
            MouseApplianceInteractionSystem.MouseButton.Right,
            MouseApplianceInteractionSystem.MouseButton.Middle,
            MouseApplianceInteractionSystem.MouseButton.Forward,
            MouseApplianceInteractionSystem.MouseButton.Back
        };

        readonly List<string> texts = new List<string>
        {
            "Left Click",
            "Right Click",
            "Middle Click",
            "Forward Button",
            "Back Button"
        };

        public DragNDropMenu(Transform container, ModuleList module_list) : base(container, module_list)
        {
        }

        public override void Setup(int player_id)
        {
            AddLabel("Drag N' Drop Controls");
            New<SpacerElement>();

            AddLabel("Grab");
            GrabOption = new Option<MouseApplianceInteractionSystem.MouseButton>(
                values, Main.GrabButtonPreference.Get(), texts);
            Add<MouseApplianceInteractionSystem.MouseButton>(GrabOption).OnChanged += delegate (object _, MouseApplianceInteractionSystem.MouseButton value)
            {
                Main.GrabButtonPreference.Set(value);
                Main.Manager.Save();
            };

            AddLabel("Act");
            ActOption = new Option<MouseApplianceInteractionSystem.MouseButton>(
                values, Main.ActButtonPreference.Get(), texts);
            Add<MouseApplianceInteractionSystem.MouseButton>(ActOption).OnChanged += delegate (object _, MouseApplianceInteractionSystem.MouseButton value)
            {
                Main.ActButtonPreference.Set(value);
                Main.Manager.Save();
            };

            AddLabel("Ping");
            PingOption = new Option<MouseApplianceInteractionSystem.MouseButton>(
                values, Main.PingButtonPreference.Get(), texts);
            Add<MouseApplianceInteractionSystem.MouseButton>(PingOption).OnChanged += delegate (object _, MouseApplianceInteractionSystem.MouseButton value)
            {
                Main.PingButtonPreference.Set(value);
                Main.Manager.Save();
            };

            New<SpacerElement>();
            New<SpacerElement>();

            AddButton(base.Localisation["MENU_BACK_SETTINGS"], delegate
            {
                RequestPreviousMenu();
            });
        }
    }
}
