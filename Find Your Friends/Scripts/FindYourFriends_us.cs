
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FindYourFriends_us : UdonSharpBehaviour
{
    public Text tipText;

    private int scrollDir = 0;
    private int scrollPos = 696969;

    private VRCPlayerApi[] allPlayers = new VRCPlayerApi[80];
    private VRCPlayerApi[] visiblePlayers = new VRCPlayerApi[80];
    [UdonSynced]
    private int[] hiddenPlayers = new int[80];
    [UdonSynced]
    private int hiddenLength = 0;

    void Start()
    {
        
    }

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

    public void _Scroll()
    {
        scrollPos = scrollPos + scrollDir;
        int visiblePlayersSize = 0;
        int playerCount = VRCPlayerApi.GetPlayerCount();
        for (int iAllPlayers = 0; iAllPlayers < playerCount; iAllPlayers++)
        {
            VRCPlayerApi player = allPlayers[iAllPlayers];
            if (Utilities.IsValid(player))
            {
                int playerId = player.playerId;
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

                // add final part of this block
            }

        }

        // later parts of this block
    }
}
