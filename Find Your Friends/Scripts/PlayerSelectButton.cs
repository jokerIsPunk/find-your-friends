
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace jokerispunk
{
    // written by github.com/jokerispunk
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerSelectButton : UdonSharpBehaviour
    {
        public Text label;
        public Image arrow;

        [Header("UI arrows color range")]
        public Color color1 = Color.cyan;
        public Color color2 = Color.red;
        [HideInInspector] public VRCPlayerApi vrcPlayer;

        public void _ConfigureButton(VRCPlayerApi playerInput)
        {
            vrcPlayer = playerInput;
            label.text = playerInput.displayName;

            if (arrow)
            {
                // random hue; uses color2's saturation and value
                float hue1, hue2;
                float s, v;
                Color.RGBToHSV(color1, out hue1, out s, out v);
                Color.RGBToHSV(color2, out hue2, out s, out v);
                float hueRandom = Random.Range(hue1, hue2);
                Color col = Color.HSVToRGB(hueRandom, s, v);
                arrow.color = col;
            }
        }

        public void _OnClick()
        {
            // teleport!
            if (Utilities.IsValid(vrcPlayer))
            {
                VRCPlayerApi lp = Networking.LocalPlayer;
                lp.TeleportTo(vrcPlayer.GetPosition(), vrcPlayer.GetRotation());
            }
            else
            {
                // invalid player means the player list data is old, so refresh
                PlayerSelectionUI buttonManager = GetComponentInParent<PlayerSelectionUI>();
                buttonManager._Refresh();
            }
        }
    }
}
