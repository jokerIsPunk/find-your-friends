
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    // written by github.com/jokerispunk
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerSelectionUI : UdonSharpBehaviour
    {
        public HiddenPlayerManager hiddenPlayers;
        public RectTransform buttonParent;
        public RectTransform buttonTemplate;
        public float playerListTimeout = 15f;
        public GameObject[] showUI, hideUI;

        public void _Refresh()
        {
            // destroy any pre-existing buttons
            for (int i = 0; i < buttonParent.childCount; i++)
            {
                RectTransform tf = (RectTransform)buttonParent.GetChild(i);
                if (tf != buttonTemplate)
                    Destroy(tf.gameObject);
            }

            // generate the list of players to be used
            VRCPlayerApi[] nonHiddenPlayers = _GetNonHiddenPlayers();

            // measure the dimensions of the UI
            int len = nonHiddenPlayers.Length;
            float buttonHeight = buttonTemplate.sizeDelta.y;
            buttonParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonHeight * len);

            // instantiate buttons
            for (int i = 0; i < len; i++)
            {
                // instantiate and transform the button
                GameObject newButton = VRCInstantiate(buttonTemplate.gameObject);
                RectTransform newButtonTf = newButton.GetComponent<RectTransform>();
                newButtonTf.SetParent(buttonParent);
                newButtonTf.localScale = Vector3.one;
                newButtonTf.localRotation = Quaternion.identity;
                newButtonTf.anchoredPosition3D = buttonHeight * i * Vector3.down;
                newButton.SetActive(true);

                // modify the button's data
                PlayerSelectButton buttonProgram = newButton.GetComponent<PlayerSelectButton>();
                buttonProgram._ConfigureButton(nonHiddenPlayers[i]);
            }
        }

        private VRCPlayerApi[] _GetNonHiddenPlayers()
        {
            // generate full player list
            int playerCount = VRCPlayerApi.GetPlayerCount();
            VRCPlayerApi[] allPlayers = new VRCPlayerApi[playerCount];
            VRCPlayerApi.GetPlayers(allPlayers);

            // filter out players who are hidden
            VRCPlayerApi[] nonHiddenPlayersLong = new VRCPlayerApi[playerCount];
            int filteredCount = 0;
            for (int i = 0; i < playerCount; i++)
            {
                int plId = allPlayers[i].playerId;
                bool playerHidden = hiddenPlayers._IsPlayerIdHidden(plId);
                if (!playerHidden)
                {
                    nonHiddenPlayersLong[filteredCount] = allPlayers[i];
                    filteredCount++;
                }
            }

            // shorten the array to its valid contents
            VRCPlayerApi[] nonHiddenPlayers = new VRCPlayerApi[filteredCount];
            for (int i = 0; i < filteredCount; i++)
                nonHiddenPlayers[i] = nonHiddenPlayersLong[i];

            return nonHiddenPlayers;
        }

        private void _SetUIState(bool state)
        {
            foreach (GameObject go in showUI)
                go.SetActive(state);

            foreach (GameObject go in hideUI)
                go.SetActive(!state);
        }

        public void _DelayedHideUI()
        {
            SendCustomEventDelayedSeconds(nameof(_HideUI), playerListTimeout);
        }

        public void _HideUI()
        {
            _SetUIState(false);
        }

        public void _ShowUI()
        {
            _SetUIState(true);
        }
    }
}
