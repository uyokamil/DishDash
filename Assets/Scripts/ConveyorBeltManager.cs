// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using UnityEngine;

public class ConveyorBeltManager : MonoBehaviour
{
    public ConveyorBelt[] conveyorBelts;
    private int platesSentOffCount = 0;

    public KitchenObject baseKitchenObject;
    public KitchenObject dirtyPlateKitchenObject;

    [SerializeField]
    bool shouldReturnPlates = true;
    [SerializeField]
    bool shouldReturnDirtyPlates = false;

    private OrderManager orderManager;

    private void Start()
    {
        orderManager = FindObjectOfType<OrderManager>();

        if(shouldReturnPlates)
        {
            FindReturnBelt().ReturnPlate(shouldReturnDirtyPlates);
        }
    }

    public void SendOffPlate(KitchenObject inKitchenObject)
    /// <summary>
    /// The SendOffPlate function is called when a plate is sent off from a ConveyorBelt. It takes a KitchenObject parameter representing the plate that is being sent off.
    /// First, it checks if the KitchenObject has a food object associated with it using the GetFoodObject method.
    /// If there is a food object, it calls the CompleteOrder method of the orderManager object, passing the food object and the bPlate property of the KitchenObject.
    /// The CompleteOrder method is responsible for handling the completion of an order, but this function doesn't care about the return value of CompleteOrder.
    /// Next, it checks if the bPlate property of the KitchenObject is true. If it is, it checks if the shouldReturnPlates flag is also true.
    /// If both conditions are met, it increments the platesSentOffCount variable and starts a coroutine called ReturnPlateDelay.
    /// The coroutine introduces a delay before returning the plate to the conveyor belt.
    /// In summary, the SendOffPlate function is responsible for processing a plate that is sent off from a conveyor belt.
    /// It checks if the plate can be used to complete an order and handles the logic for returning the plate if necessary.
    /// </summary>
    /// <param name="inKitchenObject">The KitchenObject representing the plate being sent off</param>
    {
        if (inKitchenObject.GetFoodObject() != null)
        {
            orderManager.CompleteOrder(inKitchenObject.GetFoodObject(), inKitchenObject.bPlate);
        }

        if (inKitchenObject.bPlate)
        {
            if (shouldReturnPlates)
            {
                platesSentOffCount++;
                StartCoroutine(ReturnPlateDelay());
            }
        }
    }

    private IEnumerator ReturnPlateDelay()
    {
        // delay for 5 seconds before returning the dirty plate
        yield return new WaitForSeconds(5f);


        if (platesSentOffCount <= 0)
        {
            yield break;
        }
        else
        {
            while (FindReturnBelt() == null)
            {
                yield return new WaitForSeconds(1f);
            }

            platesSentOffCount--;
            ConveyorBelt returnBelt = FindReturnBelt();

            if (returnBelt != null)
            {
                returnBelt.ReturnPlate(shouldReturnDirtyPlates);
            }
        }
    }

    private ConveyorBelt FindReturnBelt()
    {
        foreach (ConveyorBelt belt in conveyorBelts)
        {
            if (belt.GetConveyorBeltDirection() == ConveyorBeltDirection.ReturnPlate)
            {
                if(belt.CanAcceptPlate())
                {
                    return belt;
                }
            }
        }
        return null;
    }
}
