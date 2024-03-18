using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CardGame.Managers;

namespace CardGame
{
    public class Placeholder : MonoBehaviour
    {
        Renderer r = null;
        // Material material = null;

        void Awake()
        {
            r = gameObject.GetComponent<Renderer>();
        }

        void OnEnable() {
        }

        void OnDisable() {
        }

        public void set_valid()
        {
            // material.SetColor("_line_color", 2 * GameConfig.placeholder.line_color);
            r.sharedMaterial = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, "card_border_on");
        }

        public void set_invalid()
        {
            // material.SetColor("_line_color", GameConfig.placeholder.line_color_invalid);
            r.sharedMaterial = GameSystems.asset_manager.aquire_material(BundleEnum.MATERIALS, "card_border_off");
        }
    }
}
