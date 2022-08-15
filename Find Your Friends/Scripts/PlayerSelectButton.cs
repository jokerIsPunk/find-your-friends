
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
        [HideInInspector] public VRCPlayerApi vrcPlayer;

        public void _ConfigureButton(VRCPlayerApi playerInput)
        {
            vrcPlayer = playerInput;
            label.text = playerInput.displayName;

            // random color arrow between cyan and red
            float hue = Random.Range(.5f, 1f);
            Color col = Color.HSVToRGB(hue, 0.8f, 1f);
            arrow.color = col;
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
