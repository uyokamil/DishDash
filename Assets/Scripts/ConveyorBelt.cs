// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using UnityEngine;

public enum ConveyorBeltDirection
{
    SendOffPlate,
    ReturnPlate
}

public class ConveyorBelt : MonoBehaviour, IInteractable
{
    [SerializeField]
    private ConveyorBeltDirection direction;
    [SerializeField]
    private float plateSpeed = 1.5f;

    [SerializeField]
    private Transform plateCoverTransform; // the transform of plates spawning
    [SerializeField]
    private Transform plateRoomTransform; // the end transform of plates exiting, OR the set transform of plates returning

    private ConveyorBeltManager conveyorBeltManager;
    
    public ConveyorBeltDirection GetConveyorBeltDirection() { return direction; }

    [SerializeField]
    private Transform[] conveyorBeltRolls;

    private KitchenObject kitchenObjectOnBelt;
    private bool bIsRolling = false;

    private AudioSource audioSource;
    [SerializeField]
    private AudioClip plateReturnSound;
    [SerializeField]
    private AudioClip plateSendOffSound;

    private void Start()
    {
        conveyorBeltManager = FindObjectOfType<ConveyorBeltManager>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Rotate conveyor belt rolls based on direction
        float rotationDirection = (direction == ConveyorBeltDirection.ReturnPlate) ? 1f : -1f;
        foreach (Transform roll in conveyorBeltRolls)
        {
            roll.Rotate(Vector3.forward, rotationDirection * 50f * Time.deltaTime);
        }
    }

    public bool CanAcceptPlate()
    {
        return kitchenObjectOnBelt == null;
    }

    public void ReturnPlate(bool returnDirty = false)
    {
        if (direction == ConveyorBeltDirection.ReturnPlate)
        {
            if (kitchenObjectOnBelt == null)
            {
                bIsRolling = true;
                if(conveyorBeltManager == null)
                {
                    conveyorBeltManager = FindObjectOfType<ConveyorBeltManager>();
                }

                if(returnDirty)
                {
                    kitchenObjectOnBelt = Instantiate(conveyorBeltManager.dirtyPlateKitchenObject, plateCoverTransform.position, Quaternion.identity);
                }
                else
                {
                    kitchenObjectOnBelt = Instantiate(conveyorBeltManager.baseKitchenObject, plateCoverTransform.position, Quaternion.identity);
                }
                kitchenObjectOnBelt.Init(null, true);

                // Move the kitchen object on belt from plateCoverTransform to plateRoomTransform over time
                StartCoroutine(MoveBeltObject(ConveyorBeltDirection.ReturnPlate, plateSpeed));

                if (audioSource)
                {
                    audioSource.PlayOneShot(plateReturnSound);
                }
            }
        }
    }

    /// <summary>
    /// Moves the kitchen object on the conveyor belt from one position to another over a specified duration.
    /// </summary>
    /// <param name="direction">The direction in which the kitchen object should move.</param>
    /// <param name="duration">The duration of the movement.</param>
    private IEnumerator MoveBeltObject(ConveyorBeltDirection direction, float duration)
    {
        // Initialize variables
        float elapsedTime = 0f;
        Vector3 startPosition = direction == ConveyorBeltDirection.ReturnPlate ? plateCoverTransform.position : plateRoomTransform.position;
        Vector3 endPosition = direction == ConveyorBeltDirection.ReturnPlate ? plateRoomTransform.position : plateCoverTransform.position;

        // Move the kitchen object on the conveyor belt
        while (elapsedTime < duration)
        {
            if (kitchenObjectOnBelt != null) // Just in case the player grabs the plate before the coroutine finishes.
            {
                kitchenObjectOnBelt.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            else
            {
                bIsRolling = false;
                yield break;
            }
        }

        // Set the final position of the kitchen object
        if (kitchenObjectOnBelt != null)
        {
            kitchenObjectOnBelt.gameObject.transform.position = endPosition;
        }

        bIsRolling = false;

        // Perform additional actions based on the direction
        if (direction == ConveyorBeltDirection.SendOffPlate)
        {
            conveyorBeltManager.SendOffPlate(kitchenObjectOnBelt);
            Destroy(kitchenObjectOnBelt.gameObject);
            kitchenObjectOnBelt = null;
        }
    }

    public void SendOffPlate(KitchenObject inKitchenObject)
    {
        if(inKitchenObject != null)
        {
            kitchenObjectOnBelt = inKitchenObject;

            if (direction == ConveyorBeltDirection.SendOffPlate)
            {
                bIsRolling = true;
                // Move the kitchen object on belt from plateCoverTransform to plateRoomTransform over time
                StartCoroutine(MoveBeltObject(ConveyorBeltDirection.SendOffPlate, plateSpeed));

                audioSource?.PlayOneShot(plateSendOffSound);
            }
        }
    }

    /// <summary>
    /// Handles the interaction between the player and the conveyor belt.
    /// </summary>
    /// <param name="inKitchenObject">The kitchen object that the player interacts with.</param>
    /// <returns>An InteractionResponse object containing the result of the interaction.</returns>
    public InteractionResponse Interact(KitchenObject inKitchenObject = null)
    {
        InteractionResponse interactionResponse = new InteractionResponse();

        if (kitchenObjectOnBelt != null && direction == ConveyorBeltDirection.ReturnPlate && inKitchenObject == null)
        {
            // Take plate from conveyor belt
            interactionResponse.KitchenObject = kitchenObjectOnBelt;
            kitchenObjectOnBelt = null;

            if (bIsRolling)
            {
                bIsRolling = false;
                StopCoroutine(MoveBeltObject(ConveyorBeltDirection.ReturnPlate, plateSpeed));
            }

            interactionResponse.Result = InteractionResult.TakenByPlayer;
            return interactionResponse;
        }
        else if (inKitchenObject != null && direction == ConveyorBeltDirection.SendOffPlate && kitchenObjectOnBelt == null)
        {
            // Send off food/plate on conveyor belt

            // place player's plate on the conveyor belt
            inKitchenObject.PlaceObjectInParent(plateRoomTransform);

            SendOffPlate(inKitchenObject);

            interactionResponse.Result = InteractionResult.PlacedByPlayer;
            return interactionResponse;
        }

        interactionResponse.Result = InteractionResult.None;
        return interactionResponse;
    }

    public bool InteractWithTask()
    {
        return false;
    }
}
