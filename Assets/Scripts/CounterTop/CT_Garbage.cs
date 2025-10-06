// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public class CT_Garbage : CounterTop
{
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private bool bShouldDestroyPlate = false;

    public override InteractionResponse Interact(KitchenObject inKitchenObject = null)
    /// <summary>
    /// Overrides the Interact method from the CounterTop base class.
    /// Handles the interaction with a KitchenObject.
    /// </summary>
    /// <param name="inKitchenObject">The KitchenObject to interact with.</param>
    /// <returns>An InteractionResponse object.</returns>
    {
        InteractionResponse interactionResponse = new InteractionResponse();
        interactionResponse.Result = InteractionResult.None;

        if (inKitchenObject != null && inKitchenObject.GetFoodObject() != null)
        {
            if (inKitchenObject.GetFoodObject().foodIdentifier.Contains("dirty") && inKitchenObject.GetFoodObject().foodIdentifier.Contains("plate"))
            {
                // This condition checks if the KitchenObject is a dirty plate.
                // If it is, it returns the interactionResponse object without performing any further actions.
                // This could potentially be a funny future achievement: "nice try!"
                return interactionResponse;
            }

            if (inKitchenObject.bPlate && !bShouldDestroyPlate)
            {
                // This condition checks if the KitchenObject is a plate and the bShouldDestroyPlate flag is false.
                // If both conditions are true, it calls the Init method of the KitchenObject with null parameters and sets the second parameter to true.
                // This will reset the KitchenObject to its initial state only an empty plate.
                inKitchenObject.Init(null, true);
            }
            else
            {
                // If the above conditions are not met, it means that the object is either food or a plate that should be destroyed.
                // In this case, it destroys the gameObject of the KitchenObject using the Destroy method.
                // It also sets the Result property of the interactionResponse object to InteractionResult.PlacedByPlayer to indicate that the object was placed by the player.
                Destroy(inKitchenObject.gameObject);
                interactionResponse.Result = InteractionResult.PlacedByPlayer;
            }

            // Plays the audio source (if it exists).
            audioSource?.Play();
            return interactionResponse;
        }

        return interactionResponse;
    }
}
