// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public enum InteractionResult
{
    None,
    PlacedByPlayer,
    PlacedObjectByPlayer,
    TakenByPlayer,
    CombinedByPlayer
}

public struct InteractionResponse
{
    public InteractionResult Result { get; set; }
    public KitchenObject KitchenObject { get; set; }
}

public interface IInteractable
/// <summary>
/// This interface represents an object (in our case, a countertop or child thereof) that can be interacted with.
/// It defines two methods: Interact and InteractWithTask.
/// The Interact method takes an optional KitchenObject as an argument and returns an InteractionResponse.
/// The InteractWithTask method returns a boolean value.
/// Classes that implement this interface must provide implementations for these methods.
/// </summary>
{
    InteractionResponse Interact(KitchenObject inKitchenObject = null);
    bool InteractWithTask();
}

public class CounterTop : MonoBehaviour, IInteractable
{
    [SerializeField]
    protected KitchenObject defaultObject; // Variable to store the default KitchenObject that is on the CounterTop

    [SerializeField]
    protected Transform FoodLocationTransform; // Variable to store the spawn location for FoodObjects

    protected KitchenObject CounterObject; // Variable to store the KitchenObject that is currently on the CounterTop

    private void Awake()
    {
        InitCounterTop(defaultObject);
    }

    public virtual void InitCounterTop(KitchenObject inKitchenObject)
    /// <summary>
    /// This method is used to initialize the CounterTop with a KitchenObject.
    /// It first checks if there is already a CounterObject on the CounterTop and destroys it if it exists.
    /// Then, it checks if an inKitchenObject is provided.
    /// If an inKitchenObject is provided, it sets the CounterObject to the inKitchenObject and places it on the CounterTop.
    /// If no inKitchenObject is provided, it checks if there is a defaultObject set and sets the CounterObject to the defaultObject and places it on the CounterTop.
    /// </summary>
    /// <param name="inKitchenObject"></param>
    {
        if (CounterObject != null)
            Destroy(CounterObject.gameObject);

        if (inKitchenObject)
        {
            CounterObject = inKitchenObject;
            inKitchenObject.PlaceObjectInParent(FoodLocationTransform);
        }
        else if (defaultObject != null)
        {
            CounterObject = defaultObject;
            defaultObject.PlaceObjectInParent(FoodLocationTransform);
        }
    }

    public virtual InteractionResponse Interact(KitchenObject inKitchenObject = null)
    /// <summary>
    /// This method represents the interaction between the player and the CounterTop.
    /// It takes an optional KitchenObject as an argument, which represents the object the player wants to interact with.
    /// The method returns an InteractionResponse struct, which contains the result of the interaction and the KitchenObject involved in the interaction.
    /// The method first checks if an object is provided for interaction.
    /// If an object is provided, it checks if the CounterTop already has an object.
    /// If the CounterTop has an object, it checks various conditions to determine the type of interaction and performs the necessary actions accordingly.
    /// If the CounterTop does not have an object, it places the provided object on the CounterTop.
    /// If no object is provided, it checks if the CounterTop has an object and removes it from the CounterTop.
    /// Finally, it returns the InteractionResponse struct with the appropriate result and KitchenObject.
    /// </summary>
    /// <returns>An InteractionResponse struct relating to what happened during the interaction.</returns>
    {
        InteractionResponse interactionResponse = new InteractionResponse();
        interactionResponse.Result = InteractionResult.None;

        // If an object is provided for interaction
        if (inKitchenObject != null)
        {
            // If the CounterTop already has an object
            if (CounterObject != null)
            {
                // Interaction: Counter has a Plate, Counter has no FoodObject
                if (CounterObject.bPlate && CounterObject.GetFoodObject() == null)
                {
                    // Interaction: Counter has a Plate and no FoodObject, inKitchenObject has no Plate and a FoodObject
                    if (!inKitchenObject.bPlate && inKitchenObject.GetFoodObject() != null)
                    {
                        // Initialize the CounterObject with the FoodObject from inKitchenObject and set the Plate flag to true
                        CounterObject.Init(inKitchenObject.GetFoodObject(), true);
                        interactionResponse.Result = InteractionResult.CombinedByPlayer;
                        return interactionResponse;
                    }
                }
                // Interaction: Player has no FoodObject, Player has a Plate, Counter has no Plate, Counter's FoodObject has no PreparationMethod
                else if (inKitchenObject.GetFoodObject() == null && inKitchenObject.bPlate && !CounterObject.bPlate && CounterObject.GetFoodObject().preparationMethod == PreparationMethod.None)
                {
                    // Initialize the CounterObject with its own FoodObject and set the Plate flag to true
                    CounterObject.Init(CounterObject.GetFoodObject(), true);
                    interactionResponse.Result = InteractionResult.CombinedByPlayer;
                    return interactionResponse;
                }
                // Interaction: Counter's FoodObject can be combined with Player's FoodObject
                else if (CounterObject.GetFoodObject().combineWith.Length > 0)
                {
                    if (CounterObject.bPlate && inKitchenObject.bPlate)
                        interactionResponse.KitchenObject = CounterObject;

                    // Combine the FoodObjects and return the result
                    if (CombineFoodObjects(inKitchenObject))
                    {
                        interactionResponse.Result = InteractionResult.CombinedByPlayer;
                        return interactionResponse;
                    }
                    interactionResponse.KitchenObject = null;
                }
                // Interaction: Player has no FoodObject
                else if (inKitchenObject.GetFoodObject() == null)
                {
                    return interactionResponse;
                }
                // Interaction: Player's FoodObject can be combined with Counter's FoodObject
                else if (inKitchenObject.GetFoodObject().combineWith.Length > 0)
                {
                    if (CounterObject.bPlate && inKitchenObject.bPlate)
                        interactionResponse.KitchenObject = CounterObject;

                    // Combine the FoodObjects and return the result
                    if (CombineFoodObjects(inKitchenObject))
                    {
                        interactionResponse.Result = InteractionResult.CombinedByPlayer;
                        return interactionResponse;
                    }
                    interactionResponse.KitchenObject = null;
                }
            }
            else
            {
                // Place the inKitchenObject on the CounterTop
                inKitchenObject.PlaceObjectInParent(FoodLocationTransform);
                CounterObject = inKitchenObject;

                interactionResponse.Result = InteractionResult.PlacedByPlayer;
                return interactionResponse;
            }
        }
        else
        {
            if (CounterObject != null)
            {
                // Take the CounterObject from the CounterTop
                interactionResponse.KitchenObject = CounterObject;
                CounterObject = null;

                interactionResponse.Result = InteractionResult.TakenByPlayer;
                return interactionResponse;
            }
        }

        return interactionResponse;
    }

    public virtual bool InteractWithTask()
    /// <summary>
    /// This method is used to interact with the task associated with the CounterTop.
    /// It is a virtual method that returns a boolean value.
    /// The default implementation returns false, and it can be overridden in derived classes to implement specific task interactions.
    /// </summary>
    /// <returns>A bool for if the task associated with the countertop can be/was interacted with.</returns>
    {
        return false;
    }


    private bool CombineFoodObjects(KitchenObject inKitchenObject)
    /// <summary>
    /// This method combines the food objects on the counter with the food object passed as an argument.
    /// It checks if both the counter food object and the kitchen food object are not null.
    /// Then, it iterates through the possible combinations of the counter food object.
    /// If it finds a match with the kitchen food object, it calls the Combine method of the counter object and returns true.
    /// If no match is found, it iterates through the possible combinations of the kitchen food object.
    /// If it finds a match with the counter food object, it calls the Combine method of the counter object and returns true.
    /// If no match is found in either case, it returns false.
    /// </summary>
    /// <param name="inKitchenObject"></param>
    /// <returns>Bool for if the combination was successful.</returns>
    {
        if (CounterObject.GetFoodObject() != null && inKitchenObject != null)
        {
            FoodObject counterFoodObject = CounterObject.GetFoodObject();
            FoodObject kitchenFoodObject = inKitchenObject.GetFoodObject();

            int i = 0;
            foreach (FoodObject possibleCombination in counterFoodObject.combineWith)
            {
                if (possibleCombination.foodIdentifier == kitchenFoodObject.foodIdentifier)
                {
                    return CounterObject.Combine(CounterObject, i, this);
                }
                i++;
            }

            i = 0;
            foreach (FoodObject possibleCombination in kitchenFoodObject.combineWith)
            {
                if (possibleCombination.foodIdentifier == counterFoodObject.foodIdentifier)
                {
                    return CounterObject.Combine(inKitchenObject, i, this);
                }
                i++;
            }
        }
        return false;
    }
}
