// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] protected FoodObject defaultFoodObject = null;

    [Header("Food Object")]
    [SerializeField] protected FoodObject foodObject;
    [SerializeField] protected GameObject foodGameObject;
    public bool bPlate;

    [Header("Base Object Inits")]
    [SerializeField] protected GameObject plateGameObject;
    [SerializeField] protected Transform foodObjectTransform;

    [Header("Pizza")]
    protected List<GameObject> toppingObjects;

    // Get the current food object
    public FoodObject GetFoodObject()
    {
        return foodObject;
    }

    // Start is called before the first frame update
    protected void Start()
    {
        // Initialize with default food object if available
        if (defaultFoodObject != null)
        {
            Init(defaultFoodObject, bPlate);

            defaultFoodObject = null;
        }
    }

    /// <summary>
    /// Initializes the kitchen object with a new food object and plate status.
    /// </summary>
    public void Init(FoodObject food = null, bool plate = false)
    {
        // Set the plate game object active/inactive based on the plate status
        plateGameObject.SetActive(plate);
        bPlate = plate;

        // Destroy the existing food game object
        Destroy(foodGameObject);
        foodGameObject = null;
        foodObject = null;

        if (food != null)
        {
            // Get the saved toppings if the current food object is a pizza
            List<FoodObject> savedToppings = foodObject is PizzaObject pizzaObject ? pizzaObject.toppings : new List<FoodObject>();

            // Instantiate a new food object
            FoodObject newFoodObject = Instantiate(food);

            // Assign the saved toppings to the new food object if it is a pizza
            if (savedToppings.Count > 0 && newFoodObject.foodType == FoodType.Pizza)
            {
                (newFoodObject as PizzaObject).toppings = savedToppings;
            }

            // Assign the new food object to the kitchen object
            foodObject = newFoodObject;
            foodGameObject = Instantiate(foodObject.foodPrefab, foodObjectTransform);
        }
    }

    /// <summary>
    /// Place the kitchen object in a new parent transform.
    /// This is used for transferring the kitchen object to a counter space or prep station.
    /// </summary>
    /// <param name="newParent"></param>
    public void PlaceObjectInParent(Transform newParent)
    {
        transform.SetParent(newParent);
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Produce the result of this kitchen object's combination via the given index.
    /// </summary>
    /// <param name="kitchenObject"></param>
    /// <param name="combinationIndex"></param>
    /// <param name="_refCounter"></param>
    /// <returns>Returns if the object was successfully combined.</returns>
    public virtual bool Combine(KitchenObject kitchenObject, int combinationIndex, CounterTop _refCounter = null)
    {
        if (foodObject.foodIdentifier.Contains("margherita") && foodObject.foodIdentifier.Contains("raw"))
        {
            AddTopping(this, kitchenObject.GetFoodObject().combineResult[combinationIndex]);
            return true;
        }
        else if (kitchenObject.GetFoodObject().foodIdentifier.Contains("margherita") && kitchenObject.GetFoodObject().foodIdentifier.Contains("raw"))
        {
            if (AddTopping(kitchenObject, kitchenObject.GetFoodObject().combineResult[combinationIndex]))
            {
                if (_refCounter != null)
                {
                    _refCounter.InitCounterTop(kitchenObject);
                }

                return true;
            }
        }
        else
        {
            Init(kitchenObject.GetFoodObject().combineResult[combinationIndex], (kitchenObject.bPlate || bPlate));
            return true;
        }
        return false;
    }

    // Add a topping to the pizza
    public bool AddTopping(KitchenObject kitchenObject, FoodObject topping)
    {
        if (kitchenObject.GetFoodObject().foodIdentifier.Contains("margherita") && kitchenObject.GetFoodObject().foodIdentifier.Contains("raw") && kitchenObject.GetFoodObject().foodType == FoodType.Pizza) // Pizza needs to be "base pizza" before toppings can be added
        {
            if (foodObject is PizzaObject pizzaObject)
            {
                bool toppingExists = false;
                foreach (FoodObject existingTopping in pizzaObject.toppings)
                {
                    if (existingTopping.foodIdentifier == topping.foodIdentifier)
                    {
                        toppingExists = true;
                        break;
                    }
                }

                if (pizzaObject != null && topping != null && kitchenObject != null && !toppingExists)
                {
                    pizzaObject.toppings.Add(topping);
                    GameObject toppingObject = Instantiate(topping.foodPrefab, kitchenObject.foodObjectTransform);

                    if (toppingObjects == null)
                    {
                        toppingObjects = new List<GameObject>();
                    }

                    kitchenObject.toppingObjects.Add(toppingObject);

                    return true;
                }
            }
        }

        return false;
    }

    // Clear all toppings from the pizza
    public void ClearToppings()
    {
        if (foodObject is PizzaObject pizzaObject)
        {
            pizzaObject.toppings.Clear();

            if (toppingObjects != null)
            {
                foreach (GameObject toppingObject in toppingObjects)
                {
                    Destroy(toppingObject);
                }

                toppingObjects.Clear();
            }
        }
    }
}
