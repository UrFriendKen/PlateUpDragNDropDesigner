using HarmonyLib;
using KitchenMods;
using System.Collections.Generic;
using System.Reflection;

namespace KitchenDragNDropDesigner
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = "IcedMilo.PlateUp.DragNDropDesigner";
        public const string MOD_NAME = "Drag N' Drop Designer";
        public const string MOD_VERSION = "1.2.0";

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
        }

        public void PreInject() { }

        public void PostInject() { }

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
