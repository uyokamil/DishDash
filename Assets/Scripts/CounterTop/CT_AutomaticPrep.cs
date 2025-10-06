// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CT_AutomaticPrep : CounterTop
{
    // Events
    public event Action OnPrepStart;
    public event Action OnPrepEnd;
    public event Action OnFoodBurnt;

    // Progress variables
    private int nProgress = 0;
    private int nMaxProgress = 100;

    // Progress speed
    [Header("Progress Speed")]
    [SerializeField] private float progressSpeed = 1.0f;

    // Preparation method
    [Header("Preparation")]
    [SerializeField] private PreparationMethod preparationMethod;

    // Progress coroutine
    private Coroutine progressCoroutine;

    // Progress UI
    [Header("Progress UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image progressSliderFill;

    // Warning image
    [Header("Warning Image")]
    [SerializeField] private Image warningImage;
    private float minScale;
    private float maxScale;
    private float scaleSpeed = 5.0f;

    // Visual effects
    [Header("Progress VFX")]
    [SerializeField] private GameObject vfxObject;

    // Audio
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip loopingPreparationSound;
    [SerializeField] private AudioClip progressCompleteSound;

    private void Awake()
    {
        progressSlider.gameObject.SetActive(false);
        progressSliderFill.color = Color.green;

        if(audioSource != null)
        {
            audioSource.loop = true;
            audioSource.Stop();
        }

        if (warningImage != null)
        {
            warningImage.gameObject.SetActive(false);
            float scale = warningImage.transform.localScale.x;
            minScale = scale * 0.95f;
            maxScale = scale * 1.05f;
        }

        if(vfxObject != null)
        {
            vfxObject.SetActive(false);
        }
    }

    public override InteractionResponse Interact(KitchenObject inKitchenObject = null)
    /// <summary>
    /// Overrides the Interact method from the base class CounterTop.
    /// Handles the interaction between the player and the CT_AutomaticPrep object.
    /// Checks if the object being interacted with is a plate and if it has the correct food preparation method.
    /// Calls the base class' Interact method to handle placing or removing the object from the counter.
    /// Starts or stops the progress of the food preparation based on the interaction.
    /// </summary>
    /// <param name="inKitchenObject"></param>
    /// <returns></returns>
    {
        // Check if the object is/has a plate. should not be able to place a plate on the stove.
        if (inKitchenObject == null)
        {
            InteractionResponse placeResponse = base.Interact(inKitchenObject);
            if (placeResponse.Result == InteractionResult.TakenByPlayer)
            {
                OnItemRemoved(inKitchenObject);
            }
            return placeResponse;
        }

        if (inKitchenObject.GetFoodObject() == null)
        {
            InteractionResponse intResponse = new InteractionResponse();
            intResponse.Result = InteractionResult.None;
            return intResponse;
        }

        if (inKitchenObject.GetFoodObject().preparationMethod == preparationMethod)
        {
            // Call the parent class' Interact() method
            InteractionResponse placeResponse = base.Interact(inKitchenObject);
            if (placeResponse.Result == InteractionResult.PlacedByPlayer)
            {
                OnItemPlaced(inKitchenObject);
            }
            else if (placeResponse.Result == InteractionResult.TakenByPlayer)
            {
                OnItemRemoved(inKitchenObject);
            }
            if (inKitchenObject.bPlate)
            {
                placeResponse.Result = InteractionResult.PlacedObjectByPlayer;
            }
            return placeResponse;
        }
        else
        {
            InteractionResponse intResponse = new InteractionResponse();
            intResponse.Result = InteractionResult.None;
            return intResponse;
        }
    }

    public void OnItemPlaced(KitchenObject inKitchenObject)
    {
        if(inKitchenObject.GetFoodObject().preparationSpeed == PreparationSpeed.Instant)
        {
            // instant actions to change item, for example cracking an egg
            OnProgressComplete();
        }
        else
        {
            StartProgress();
        }
    }

    public void OnItemRemoved(KitchenObject inKitchenObject)
    {
        // Code to execute when an item is removed from the stove
        StopProgress();
    }

    private void StartProgress()
    {
        var foodObject = CounterObject.GetFoodObject();

        if (foodObject == null || foodObject.preparationMethod != preparationMethod)
        {
            return;
        }

        if (progressCoroutine != null)
        {
            return;
        }

        if (progressSlider != null)
        {
            progressSliderFill.color = Color.green;

            var preparationResult = foodObject.preparationResult;
            if (preparationResult != null && preparationResult.foodIdentifier.Contains("burn"))
            {
                progressSliderFill.color = Color.red;
                if (warningImage)
                {
                    warningImage.gameObject.SetActive(true);
                    StartCoroutine(FlashingWarningImage());
                }
            }

            progressSlider.gameObject.SetActive(true);
        }

        progressCoroutine = StartCoroutine(IncrementStoveProgressCoroutine());

        if (vfxObject != null)
        {
            vfxObject.SetActive(true);
        }

        if (audioSource != null && loopingPreparationSound != null)
        {
            audioSource.clip = loopingPreparationSound;
            audioSource.Play();
        }

        OnPrepStart?.Invoke();
    }

    private void StopProgress()
    {
        nProgress = 0;
        if (progressCoroutine != null)
        {
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(false);
                if (warningImage)
                {
                    StopCoroutine(FlashingWarningImage());
                    warningImage.gameObject.SetActive(false);
                }
            }

            StopCoroutine(progressCoroutine);
            progressCoroutine = null;

            if (vfxObject != null)
            {
                vfxObject.SetActive(false);
            }

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(progressCompleteSound);
            }

            OnPrepEnd?.Invoke();
        }
    }

    private IEnumerator IncrementStoveProgressCoroutine()
    {
        float timeToWait = 1.0f / progressSpeed; // Calculate the time to wait based on progressSpeed
        while (nProgress < nMaxProgress)
        {
            nProgress++;

            // Update the progress slider
            if(progressSlider != null)
                progressSlider.value = (float)nProgress / nMaxProgress;

            yield return new WaitForSeconds(timeToWait);
        }

        StopProgress();

        // Progress is complete
        OnProgressComplete();
    }

    private void OnProgressComplete()
    {
        // check if CounterObject's foodobject has a PreparationResult
        if (CounterObject.GetFoodObject().preparationResult != null)
        {
            // if it does, replace the CounterObject's foodobject with the preparationResult
            CounterObject.Init(CounterObject.GetFoodObject().preparationResult, false);

            if(CounterObject.GetFoodObject().foodIdentifier.Contains("burn"))
            {
                OnFoodBurnt?.Invoke();

                if(CounterObject.GetFoodObject() is PizzaObject pizzaObject)
                {
                    pizzaObject.toppings.Clear();
                    CounterObject.ClearToppings();
                }
            }

            if(CounterObject.GetFoodObject().preparationMethod == preparationMethod)
            {
                // Start the progress again
                StartProgress();
            }
        }
    }

    private IEnumerator FlashingWarningImage()
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * scaleSpeed, 1.0f);
            float scale = Mathf.Lerp(minScale, maxScale, t);
            warningImage.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }
}
