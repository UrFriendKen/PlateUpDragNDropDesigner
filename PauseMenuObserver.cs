using Kitchen;
using KitchenMods;
using UnityEngine;

namespace KitchenDragNDropDesigner
{
    public class PauseMenuObserver : GenericSystemBase, IModSystem
    {
        public static bool IsPauseMenuOpen { get; private set; } = false;

        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            GameObject pauseMenuPopup = GameObject.Find("Player Pause Popup");
            if (pauseMenuPopup != null)
            {
                Transform container = pauseMenuPopup.transform.Find("Container");
                IsPauseMenuOpen = container.gameObject.activeSelf;
            }
        }
    }
}
