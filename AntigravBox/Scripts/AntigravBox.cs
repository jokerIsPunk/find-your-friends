
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AntigravBox : UdonSharpBehaviour
{
    // this whole thing might be a bad idea. this trigger would need to be on MirrorReflection layer...

    public float objectsGravityPercent = 33f;
    private float objectsAccel = -3.27f;
    public float playerGravityPercent = 33f;
    private float playerGravStrength = .33f;
    private bool skipObjectCalc = false;

    void Start()
    {
        skipObjectCalc = objectsGravityPercent == 0f;
        objectsAccel = objectsGravityPercent / -100f;
        playerGravStrength = playerGravityPercent / 100f;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            player.SetGravityStrength(playerGravStrength);
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            player.SetGravityStrength(1);
        }
    }

    private void OnTriggerEnter(Collider other)             // what if this is not a synced object? does IsOwner just always return true?
    {
        if (Networking.IsOwner(other.gameObject))
        {
            Rigidbody otherRb = other.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                otherRb.useGravity = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!skipObjectCalc && Networking.IsOwner(other.gameObject))
        {
            Rigidbody otherRb = other.GetComponent<Rigidbody>();            // can I improve perf here by using a tag, or otherwise something that writes boolean data to the object on first impression?
            if (otherRb != null)
            {
                // stuff
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Networking.IsOwner(other.gameObject))
        {
            Rigidbody otherRb = other.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                otherRb.useGravity = true;
            }
        }
    }
}
