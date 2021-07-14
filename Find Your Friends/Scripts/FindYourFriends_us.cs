
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class FindYourFriends_us : UdonSharpBehaviour
{
    public Text tipText;

    private int scrollDir = 0;

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

    }
}
