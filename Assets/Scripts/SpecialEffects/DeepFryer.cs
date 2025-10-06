// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public class DeepFryer : MonoBehaviour
{
    /// <summary>
    /// A special effect for the deep fryer. Moves the basket in and out of the fryer.
    /// Uses the CT_AutomaticPrep component delegate to know when to move the basket.
    /// </summary>

    [SerializeField]
    private Transform basket;

    private CT_AutomaticPrep autoPrepStation;

    private float basketInPosition;
    private float basketOutPosition;

    private void Start()
    {
        autoPrepStation = GetComponent<CT_AutomaticPrep>();
        autoPrepStation.OnPrepStart += HandlePrepStart;
        autoPrepStation.OnPrepEnd += HandlePrepEnd;

        basketOutPosition = basket.localPosition.z;
        basketInPosition = basketOutPosition - 0.00824307f;

        SetBasketOut();
    }

    private void HandlePrepStart()
    {
        SetBasketIn();
    }

    private void HandlePrepEnd()
    {
        SetBasketOut();
    }

    private void SetBasketIn()
    {
        basket.localPosition = new Vector3(basket.localPosition.x, basket.localPosition.y, basketInPosition);
    }
    private void SetBasketOut()
    {
        basket.localPosition = new Vector3(basket.localPosition.x, basket.localPosition.y, basketOutPosition);
    }
}
