
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace jokerispunk
{
    // written by github.com/jokerispunk
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class HiddenPlayerManager : UdonSharpBehaviour
    {
        [UdonSynced] private int[] hiddenPlayers = { 0 };
        public TextMeshProUGUI hideTipText;
        public Image hideCheckmark;
        private VRCPlayerApi lp;
        private float lastToggledTime = -100f;

        public void _ToggleHideState()
        {
            lp = Networking.LocalPlayer;
            Networking.SetOwner(lp, gameObject);

            if (hideCheckmark.enabled)
            {
                _RemoveLPFromHideArray();
                hideCheckmark.enabled = false;

                if (hideTipText)
                    hideTipText.text = "Others may teleport to you";
            }
            else
            {
                _AddLPToHideArray();
                hideCheckmark.enabled = true;

                if (hideTipText)
                    hideTipText.text = "Others cannot teleport to you";
            }

            lastToggledTime = Time.time;
            RequestSerialization();
        }

        private void _AddLPToHideArray()
        {
            int lpId = lp.playerId;

            // find the first array element which does not contain a valid playerId
            int len = hiddenPlayers.Length;
            bool foundSlot = false;
            for (int i = 0; i < len; i++)
            {
                int playerId = hiddenPlayers[i];
                VRCPlayerApi pl = VRCPlayerApi.GetPlayerById(playerId);
                foundSlot = !Utilities.IsValid(pl);    // "it's free real estate!"

                if (foundSlot)
                {
                    hiddenPlayers[i] = lpId;
                    break;
                }
            }

            // if the array is full of valid ids, expand the array and append the id
            if (!foundSlot)
            {
                int[] hiddenPlayerArrayCopied = hiddenPlayers;
                hiddenPlayers = new int[len + 1];
                for (int i = 0; i < len; i++)
                    hiddenPlayers[i] = hiddenPlayerArrayCopied[i];

                hiddenPlayers[len] = lpId;
            }
        }

        private void _RemoveLPFromHideArray()
        {
            int lpId = lp.playerId;
            int len = hiddenPlayers.Length;

            // find any instance of this player's id and erase it with 0
            for (int i = 0; i < len; i++)
            {
                if (hiddenPlayers[i] == lpId)
                    hiddenPlayers[i] = 0;
            }
        }

        public override void OnDeserialization()
        {
            // check for a data race by checking if the hidden player array is being updated shortly after local player tried to update it
            // expect data race when two players try to toggle state at the same time
                // one player would fail to take network ownership and thus fail to modify the synced data
            float recentThreshold = 10f;
            float timeSinceToggle = Time.time - lastToggledTime;
            if (timeSinceToggle < recentThreshold)
            {
                // if the updates are close in time, verify that the state of the synced data matches the local player's expected state
                bool lpHiddenPerData = _IsPlayerIdHidden(lp.playerId);
                if (lpHiddenPerData != hideCheckmark.enabled)
                {
                    if (hideTipText)
                        hideTipText.text = "SYNC ERROR! Please retry.";
                    hideCheckmark.enabled = lpHiddenPerData;
                }
            }
        }

        public bool _IsPlayerIdHidden(int playerId)
        {
            int index = System.Array.IndexOf(hiddenPlayers, playerId);

            return (index > -1);
        }
    }
}
