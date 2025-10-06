// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CT_ManualPrep : CounterTop
{
    // Progress variables
    private int nProgress = 0;
    private int nMaxProgress = 100;

    // Interaction variables
    private bool isInteracting = false;
    private int interactionTimer = 0;
    private int interactionThreshold = 5;

    // Scale variables
    private float minScale;
    private float maxScale;
    private float scaleSpeed = 2.0f;

    // Progress speed
    [SerializeField] private float progressSpeed = 1.0f;
    private Coroutine progressCoroutine;

    // UI elements
    [Header("UI Elements")]
    [SerializeField] private Slider progressSlider;

    [SerializeField] private Image progressReminderImage;

    // Preparation method
    [Header("Preparation Method")]
    [SerializeField] private PreparationMethod preparationMethod;

    // Visual effects
    [Header("Visual Effects")]
    [SerializeField] private GameObject vfxObject;

    // Audio sources and clips
    [Header("Audio")]
    [SerializeField] private AudioSource loopingAudioSource;

    [SerializeField] private AudioSource oneShotAudioSource;

    [SerializeField] private AudioClip loopingPreparationSound;

    [SerializeField] private AudioClip progressCompleteSound;

    // Audio volume
    [SerializeField]
    private float audioVolume;

    private void Awake()
    {
        progressSlider.gameObject.SetActive(false);
        progressReminderImage.gameObject.SetActive(false);

        if(loopingAudioSource != null)
        {
            loopingAudioSource.loop = true;
            loopingAudioSource.Stop();
            loopingAudioSource.clip = loopingPreparationSound;
        }

        minScale = progressReminderImage.transform.localScale.x * 0.9f;
        maxScale = progressReminderImage.transform.localScale.x * 1.1f;

        if (vfxObject != null)
        {
            vfxObject.SetActive(false);
        }
    }

    public override InteractionResponse Interact(KitchenObject inKitchenObject = null)
    /// <summary>
    /// Handles the interaction between the player and the counter top.
    /// Determines the result of the interaction based on the provided kitchen object.
    /// </summary>
    /// <param name="inKitchenObject">The kitchen object to interact with.</param>
    /// <returns>An InteractionResponse object indicating the result of the interaction.</returns>
    {
        InteractionResponse placeResponse = new InteractionResponse();

        if (inKitchenObject == null || inKitchenObject.GetFoodObject() == null)
        {
            if (CounterObject != null)
            {
                placeResponse = base.Interact(inKitchenObject);
                if (placeResponse.Result == InteractionResult.TakenByPlayer)
                {
                    OnItemRemoved(inKitchenObject);
                }
                else
                {
                    placeResponse.Result = InteractionResult.None;
                }
            }
            else
            {
                placeResponse.Result = InteractionResult.None;
            }
        }
        else if (inKitchenObject.bPlate && inKitchenObject.GetFoodObject().preparationMethod == PreparationMethod.Wash && preparationMethod == PreparationMethod.Wash)
        {
            placeResponse = base.Interact(inKitchenObject);
            if (placeResponse.Result == InteractionResult.PlacedByPlayer)
            {
                OnItemPlaced(inKitchenObject);
            }
            else if (placeResponse.Result == InteractionResult.TakenByPlayer)
            {
                OnItemRemoved(inKitchenObject);
            }
        }
        else if (inKitchenObject.GetFoodObject().preparationMethod == preparationMethod)
        {
            placeResponse = base.Interact(inKitchenObject);
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
        }
        else
        {
            placeResponse.Result = InteractionResult.None;
        }

        return placeResponse;
    }

    public override bool InteractWithTask()
    /// <summary>
    /// Handles the interaction between the player and the counter top for performing a task.
    /// Checks if the counter top has a food object and if it matches the preparation method.
    /// Starts the progress and sets the interaction state to true.
    /// </summary>
    /// <returns>True if the interaction with the task is successful, false otherwise.</returns>
    {
        if (CounterObject == null || CounterObject.GetFoodObject() == null || CounterObject.GetFoodObject().preparationMethod != preparationMethod)
        {
            return false;
        }

        interactionTimer = -1;

        isInteracting = true;
        StartProgress();

        return true;
    }

    private void FixedUpdate()
    {
        if(CounterObject != null)
        {
            if(CounterObject.GetFoodObject() == null)
            {
                return;
            }
            if (CounterObject.GetFoodObject().preparationMethod != preparationMethod)
            {
                return;
            }

            if (isInteracting)
            {
                interactionTimer++;
                if (interactionTimer > interactionThreshold)
                {
                    isInteracting = false;
                    PauseProgress();
                }
            }

            UpdateProgressReminder(); // Call the UpdateProgressReminder function
        }
    }

    public void OnItemPlaced(KitchenObject inKitchenObject)
    {
        nProgress = 0;
    }

    public void OnItemRemoved(KitchenObject inKitchenObject)
    {
        nProgress = 0;
        StopProgress();
        StopCoroutine(ScaleProgressReminderCoroutine());

        progressReminderImage.gameObject.SetActive(false);
        progressReminderImage.transform.localScale = Vector3.one;
    }

    private void StartProgress()
    {
        if (progressCoroutine == null)
        {
            if(progressSlider != null)
                progressSlider.gameObject.SetActive(true);

            progressCoroutine = StartCoroutine(IncrementProgressCoroutine());

            if (vfxObject != null)
            {
                vfxObject.SetActive(true);
            }

            // Find the player object
            PlayerController player = GameObject.FindObjectOfType<PlayerController>();

            // Check if the player object is found
            if (player != null)
            {
                // Call the StartTask method on the PlayerController component
                player.StartTask(preparationMethod);
            }

            if (loopingAudioSource != null)
            {
                StartCoroutine(FadeSound(loopingAudioSource, 0.15f, audioVolume));
            }
        }
    }

    private void PauseProgress()
    {
        if (progressCoroutine != null && interactionTimer > interactionThreshold)
        {
            if (progressSlider != null)
                progressSlider.gameObject.SetActive(false);

            StopCoroutine(progressCoroutine);
            progressCoroutine = null;

            if (vfxObject != null)
            {
                vfxObject.SetActive(false);
            }

            // Find the player object
            PlayerController player = GameObject.FindObjectOfType<PlayerController>();

            // Check if the player object is found
            if (player != null)
            {
                // Call the StartTask method on the PlayerController component
                player.StopTask(preparationMethod);
            }

            if (loopingAudioSource != null)
            {
                StartCoroutine(FadeSound(loopingAudioSource, 0.15f, 0));
            }
        }
    }

    private void StopProgress()
    {
        nProgress = 0;
        if(progressSlider != null)
            progressSlider.gameObject.SetActive(false);

        if(progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }
            
        if (vfxObject != null)
        {
            vfxObject.SetActive(false);
        }

        // Find the player object
        PlayerController player = GameObject.FindObjectOfType<PlayerController>();

        // Check if the player object is found
        if (player != null)
        {
            // Call the StartTask method on the PlayerController component
            player.StopTask(preparationMethod);
        }

        if (oneShotAudioSource != null)
        {
            StartCoroutine(FadeSound(loopingAudioSource, 0.15f, 0));
            oneShotAudioSource.PlayOneShot(progressCompleteSound);
        }
    }

    private IEnumerator IncrementProgressCoroutine()
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

        if(nProgress >= nMaxProgress)
        {
            StopProgress();
            // Stove progress is complete
            OnProgressComplete();
        }
    }

    private void OnProgressComplete()
    {
        // check if CounterObject's foodobject has a PreparationResult
        if (CounterObject.GetFoodObject().preparationResult != null)
        {
            // if it does, replace the CounterObject's foodobject with the preparationResult
            CounterObject.Init(CounterObject.GetFoodObject().preparationResult, CounterObject.bPlate);
        }
        else if (CounterObject.GetFoodObject().foodIdentifier.Contains("plate"))
        {
            CounterObject.Init(null, true);
        }
    }

    private void UpdateProgressReminder()
    {
        if (CounterObject != null && !isInteracting)
        {
            if (progressReminderImage != null)
            {
                progressReminderImage.gameObject.SetActive(true);
                StartCoroutine(ScaleProgressReminderCoroutine());
            }
        }
        else
        {
            if (progressReminderImage != null)
            {
                progressReminderImage.gameObject.SetActive(false);
                progressReminderImage.transform.localScale = Vector3.one;
                StopCoroutine(ScaleProgressReminderCoroutine());
            }
        }
    }

    private IEnumerator ScaleProgressReminderCoroutine()
    {
        float t = Mathf.PingPong(Time.time * scaleSpeed, 1.0f);
        float scale = Mathf.Lerp(minScale, maxScale, t);
        progressReminderImage.transform.localScale = new Vector3(scale, scale, scale);
        yield return null;
    }

    public static IEnumerator FadeSound(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;

        if(!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        
        if(targetVolume == 0)
        {
            audioSource.Stop();
        }

        yield break;
    }
}