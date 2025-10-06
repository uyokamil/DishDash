// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public class PlayerAnimationAudio : MonoBehaviour
{
    /// <summary>
    /// A class that is responsible for playing audio clips called from the player's animation.
    /// </summary>

    private PlayerController playerController;

    private void Awake()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
    }

    public void Sound_KnifeChop()
    {
        playerController.Sound_KnifeChop();
    }
    public void Sound_Footstep()
    {
        playerController.Sound_Step();
    }
}
