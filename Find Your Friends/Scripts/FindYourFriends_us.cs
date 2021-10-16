
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FindYourFriends_us : UdonSharpBehaviour
{
    private VRCPlayerApi selectedPlayer;            // store and display a player
    public TMPro.TextMeshProUGUI displayText;

    public Toggle hideMeToggle;                     // hide UI
    public TMPro.TextMeshProUGUI tipText;

    private int scrollDir = 0;                      // private and synced vars for managing synced data about players
    private int scrollPos = 696969;
    private VRCPlayerApi[] allPlayers = new VRCPlayerApi[80];
    private VRCPlayerApi[] visiblePlayers = new VRCPlayerApi[80];
    private int visiblePlayersSize = 0;
    [UdonSynced]
    private int[] hiddenPlayers = new int[80];
    [UdonSynced]
    private int hiddenLength = 0;
    private int hiddenLengthLast = 0;

    public bool verboseLogging = true;             // logging

    #region Events

    public void _Left()
    {
        scrollDir = -1;
        _Scroll();
    }

    public void _Right()
    {
        scrollDir = 1;
        _Scroll();
    }

    public void _HideToggle()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        _UpdateHideState();
    }

    public override void OnDeserialization()
    {
        if (verboseLogging && hiddenLength != hiddenLengthLast)
        {
            hiddenLengthLast = hiddenLength;
            Debug.Log("[FindFriends] hiddenArray updated, length " + hiddenLength.ToString());
        }
    }

    #endregion Events

    #region Functions

    public void _Scroll()
    {
        scrollPos = scrollPos + scrollDir;                  // move scroll position

        // Build the instantaneous list of visible players.

        // Initialize variables for the outer for loop, which iterates through all players
        visiblePlayersSize = 0;
        int playerId = 0;
        VRCPlayerApi player;
        allPlayers = VRCPlayerApi.GetPlayers(allPlayers);   // array of all players, plus garbage
        int playerCount = VRCPlayerApi.GetPlayerCount();    // length of player array minus garbage

        for (int iAllPlayers = 0; iAllPlayers < playerCount; iAllPlayers++)
        {
            player = allPlayers[iAllPlayers];
            if (Utilities.IsValid(player))                  // IsValid check is redundant
            {
                // Initialize for the inner for loop, which checks each player for membership in the hidden array
                playerId = player.playerId;
                bool foundHidden = false;
                int hiddenId = 0;

                for (int iHidden = 0; iHidden < hiddenLength; iHidden++)    // not using foreach because that would do the whole array; only want list length
                {
                    hiddenId = hiddenPlayers[iHidden];      // for each index, retrieve the playerId stored there
                    if (hiddenId == playerId)
                    {
                        foundHidden = true;
                        break;
                    }
                }
                
                if (!foundHidden)                           // for loop proves the negative
                {
                    visiblePlayers[visiblePlayersSize] = VRCPlayerApi.GetPlayerById(playerId);      // store visible player at end of list and increase length
                    visiblePlayersSize++;
                }
            }

        }
        
        // Now we're ready to actually scroll the UI and select a player
        if (visiblePlayersSize > 0)         // avoid div by 0
        {
            scrollPos = Mathf.Abs(scrollPos);                       //
            int iVisiblePlayers = scrollPos % visiblePlayersSize;   // force scroll into a valid range

            if (Utilities.IsValid(visiblePlayers[iVisiblePlayers]))
            {
                selectedPlayer = visiblePlayers[iVisiblePlayers];   // store for use with Teleport()
                displayText.text = selectedPlayer.displayName;      // update interface
            }
        }
        else { Debug.LogWarning("[FindFriends] Scroll trying to divide by 0! Are there 0 visible players?"); }

        Debug.Log("[FindFriends] Rebuilt visible list. PlayerCount " + playerCount.ToString() + ", hidden " + hiddenLength.ToString() + ", visible " + visiblePlayersSize.ToString());
    }

    public void _Teleport()
    {
        if (Utilities.IsValid(selectedPlayer))
        {
            Networking.LocalPlayer.TeleportTo(selectedPlayer.GetPosition(), selectedPlayer.GetRotation());
        }
        // adding an offset would be easy enough, but it's impossible to guarantee that forward or backward from the target is a valid location for
        // the local player. Could be dropping people off cliffs or shoving them into walls. Would need a bunch of complicated raycasts, and how does
        // that even work with occlusion culling??
    }

    public void _UpdateHideState()
    {
        Debug.Log("[FindFriends] Calling UpdateHideState()..");
        if (hideMeToggle.isOn) { _AddToArray(); } else { _RemoveFromArray(); }
        // TODO just collapse this into HideToggle(), don't remember why I chopped them up
    }

    public void _AddToArray()
    {
        Debug.Log("[FindFriends] Calling AddToArray()...");

        // initialize variables
        int playerId = 0;
        VRCPlayerApi player;

        // search for the first empty slot in the list
        for (int iHidden = 0; iHidden < hiddenPlayers.Length; iHidden++)
        {
            playerId = hiddenPlayers[iHidden];                          //
            player = VRCPlayerApi.GetPlayerById(playerId);              // get player from array of IDs

            if (!Utilities.IsValid(player))                             // invalid means empty slot; "It's free real estate!"
            {
                hiddenPlayers[iHidden] = Networking.LocalPlayer.playerId;                           // store local's id there
                Debug.Log("[FindFriends] Added player to hidden, iHidden " + iHidden.ToString());

                if (iHidden >= hiddenLength)                                                        // if replacing a disconnected player, no need to increase length
                {
                    hiddenLength = Mathf.Clamp(hiddenLength + 1, 0, hiddenPlayers.Length);          // TODO feels like there might be an off-by-one error lurking here..
                    Debug.Log("[FindFriends] hiddenArrayLength increased to " + hiddenLength.ToString());
                }

                tipText.text = "Others cannot teleport to you";

                break;                                                                              // avoid filling more than one slot
            }
        }
        
        RequestSerialization();
    }

    public void _RemoveFromArray()
    {
        Debug.Log("[FindFriends] Calling RemoveFromArray()...");
        int localPlayerId = Networking.LocalPlayer.playerId;                        // caching this early since it's referenced repeatedly
        int playerId = 0;

        // seek local id in the list and replace with 0s
        for (int iHidden = 0; iHidden < hiddenPlayers.Length; iHidden++)            // I don't remember why I'm going over the full array rather than just the list length.. but it's safe and maybe I had a good reason
        {
            playerId = hiddenPlayers[iHidden];
            if (playerId == localPlayerId)
            {
                hiddenPlayers[iHidden] = 0;
                Debug.Log("[FindFriends] Removed local player, iHidden " + iHidden.ToString());

                // could arguably break here; don't expect more than 1, but going over the whole thing for robustness
            }
        }

        RequestSerialization();
        tipText.text = "Others may teleport to you";
    }

    #endregion Functions
}
