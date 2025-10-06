// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public class CT_Spawner : CounterTop
{
    [SerializeField]
    private FoodObject spawnerObject;

    [SerializeField]
    private SpriteRenderer imageSprite;

    [SerializeField]
    private Animator spawnerAnimator;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private Sprite plateSprite;

    // Initializes the counter top with a new object
    public override void InitCounterTop(KitchenObject inKitchenObject)
    {
        CreateNewObject();
    }

    // Handles the interaction with the counter top
    public override InteractionResponse Interact(KitchenObject inKitchenObject = null)
    {
        InteractionResponse interactionResponse = new InteractionResponse();
        interactionResponse.Result = InteractionResult.None;

        // If there is no kitchen object interacting with the counter top
        if (inKitchenObject == null)
        {
            interactionResponse = base.Interact(inKitchenObject);

            // Create a new object on the counter top
            CreateNewObject();

            // Trigger the "Interact" animation if available
            spawnerAnimator?.SetTrigger("Interact");

            // Play the audio source if available
            audioSource?.Play();

            return interactionResponse;
        }

        return interactionResponse;
    }

    // Creates a new object on the counter top
    private void CreateNewObject()
    {
        CounterObject = Instantiate(defaultObject, FoodLocationTransform);

        // If there is a spawner object, initialize the counter object with it
        if (spawnerObject != null)
        {
            CounterObject.Init(spawnerObject, false);
            imageSprite.sprite = spawnerObject.foodSprite;
        }
        else
        {
            // If there is no spawner object, initialize the counter object as an empty plate
            CounterObject.Init(null, true);
            imageSprite.sprite = plateSprite;
        }
    }
}
