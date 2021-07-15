
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FindYourFriends_us : UdonSharpBehaviour
{
    public Text tipText;
    public Text displayText;
    public Toggle hideMeToggle;
    public bool verboseLogging = true;

    private int scrollDir = 0;
    private int scrollPos = 696969;

    private VRCPlayerApi[] allPlayers = new VRCPlayerApi[80];
    private VRCPlayerApi[] visiblePlayers = new VRCPlayerApi[80];
    private int visiblePlayersSize = 0;
    private VRCPlayerApi selectedPlayer;
    [UdonSynced]
    private int[] hiddenPlayers = new int[80];
    [UdonSynced]
    private int hiddenLength = 0;
    private int hiddenLengthLast = 0;

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

    public void _Scroll()
    {
        scrollPos = scrollPos + scrollDir;
        allPlayers = VRCPlayerApi.GetPlayers(allPlayers);
        int playerCount = VRCPlayerApi.GetPlayerCount();    // focus the loop on contiguous, valid players
        visiblePlayersSize = 0;                             // only increments up
        int playerId = 0;
        VRCPlayerApi player;
        for (int iAllPlayers = 0; iAllPlayers < playerCount; iAllPlayers++)
        {
            player = allPlayers[iAllPlayers];
            if (Utilities.IsValid(player))                  // IsValid check should be redundant
            {
                playerId = player.playerId;
                bool foundHidden = false;
                int hiddenId = 0;
                for (int iHidden = 0; iHidden < hiddenLength; iHidden++)
                {
                    hiddenId = hiddenPlayers[iHidden];
                    if (hiddenId == playerId)
                    {
                        foundHidden = true;
                        break;
                    }
                }
                
                if (!foundHidden)                           // for loop proves the negative
                {
                    visiblePlayers[visiblePlayersSize] = VRCPlayerApi.GetPlayerById(playerId);
                    visiblePlayersSize++;
                }
            }

        }
        
        if (visiblePlayersSize > 0)         // avoid div by 0
        {
            scrollPos = Mathf.Abs(scrollPos);
            int iVisiblePlayers = scrollPos % visiblePlayersSize;   // force scroll into a valid range
            if (Utilities.IsValid(visiblePlayers[iVisiblePlayers]))
            {
                selectedPlayer = visiblePlayers[iVisiblePlayers];
                displayText.text = selectedPlayer.displayName;
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
    }

    public void _UpdateHideState()
    {
        Debug.Log("[FindFriends] Calling UpdateHideState()..");
        if (hideMeToggle.isOn) { _AddToArray(); } else { _RemoveFromArray(); }
    }

    public void _AddToArray()
    {
        Debug.Log("[FindFriends] Calling AddToArray()...");
        int playerId = 0;
        VRCPlayerApi player;
        for (int iHidden = 0; iHidden < hiddenPlayers.Length; iHidden++)
        {
            playerId = hiddenPlayers[iHidden];
            player = VRCPlayerApi.GetPlayerById(playerId);
            if (!Utilities.IsValid(player))                 // looking for the first empty slot
            {
                hiddenPlayers[iHidden] = Networking.LocalPlayer.playerId;                           // store local's id there
                Debug.Log("[FindFriends] Added player to hidden, iHidden " + iHidden.ToString());

                if (iHidden >= hiddenLength)
                {
                    hiddenLength = Mathf.Clamp(hiddenLength + 1, 0, hiddenPlayers.Length);          // TODO feels like there might be an off-by-one error lurking here..
                    Debug.Log("[FindFriends] hiddenArrayLength increased to " + hiddenLength.ToString());
                }

                tipText.text = "Others cannot teleport to you";

                break;                                                                              // avoid more than once
            }
        }
        
        RequestSerialization();
    }

    public void _RemoveFromArray()
    {
        Debug.Log("[FindFriends] Calling RemoveFromArray()...");
        int localPlayerId = Networking.LocalPlayer.playerId;
        int playerId = 0;
        for (int iHidden = 0; iHidden < hiddenPlayers.Length; iHidden++)
        {
            playerId = hiddenPlayers[iHidden];
            if (playerId == localPlayerId)
            {
                hiddenPlayers[iHidden] = 0;
                Debug.Log("[FindFriends] Removed local player, iHidden " + iHidden.ToString());
            }
        }

        RequestSerialization();
        tipText.text = "Others may teleport to you";
    }

    public override void OnDeserialization()
    {
        if (verboseLogging && hiddenLength != hiddenLengthLast)
        {
            hiddenLengthLast = hiddenLength;
            Debug.Log("[FindFriends] hiddenArray updated, length " + hiddenLength.ToString());
        }
    }
}
