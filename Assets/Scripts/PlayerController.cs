// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public enum InteractionType
{
    Take,
    Use
}

public class PlayerController : MonoBehaviour
{
    // Character Controller
    private CharacterController characterController;

    // Animator
    [SerializeField]
    private Animator animator;

    // Main Camera
    private Camera mainCamera;

    // Kitchen Object Transform
    public Transform kitchenObjectTransform;

    // Held Kitchen Object
    private KitchenObject heldKitchenObject;

    // Movement Speed
    public float movementSpeed = 5f;

    // Rotation Speed
    public float rotationSpeed = 10f;

    // Player Objects
    [SerializeField] private GameObject playerKnifeObject;
    [SerializeField] private GameObject playerSpongeObject;

    // Audio Source
    private AudioSource audioSource;

    // Player Sounds
    [Header("Player Sounds")]
    [SerializeField] private AudioClip knifeChopSound;
    [SerializeField] private AudioClip takeSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip[] plateSounds;
    [SerializeField] private AudioClip[] stepSounds;

    // Can Move
    public bool canMove = false;

    // Total Distance Moved
    private float totalDistanceMoved = 0f;
    public float TotalDistanceMoved { get { return totalDistanceMoved; } }

    // Dash Variables
    private bool isDashing = false;
    private float dashDistance = 2f;
    private float dashDuration = 0.15f;
    private float dashTimer = 0f;
    private float dashCooldown = 0.5f;
    private float dashCooldownTimer = 0f;
    [SerializeField]
    private GameObject dashParticle;

    // Settings
    private Settings settings;

    // Extra effects
    [SerializeField] private GameObject playerKnife;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;
        settings = FindObjectOfType<Settings>();

        settings?.CloseSettings();
    }

    private void Update()
    {
        if (!canMove)
        {
            animator.SetBool("IsWalking", false);

            if (settings)
            {
                settings.CloseSettings();
            }

            return;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (settings)
            {
                settings.OpenSettings();
            }
        }

        // Check if the player is currently dashing
        if (isDashing)
        {
            // Update the dash timer
            dashTimer += Time.deltaTime;

            // Check if the dash duration has elapsed
            if (dashTimer >= dashDuration)
            {
                // Stop dashing
                isDashing = false;
                dashTimer = 0f;
            }
            else
            {
                // Move the character using CharacterController
                float dashDistanceMoved = dashDistance * Time.deltaTime / dashDuration;
                characterController.Move(transform.forward * dashDistanceMoved);
                totalDistanceMoved += dashDistanceMoved;

            }
        }
        else
        {
            // Get the input for movement
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // Convert input to world space based on camera's rotation
            Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
            movement = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f) * movement;

            movement.Normalize();

            // Rotate the player to face the movement direction
            if (movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Move the character using CharacterController
            float movementDistanceMoved = movementSpeed * Time.deltaTime;
            characterController.Move(movement * movementDistanceMoved);
            totalDistanceMoved += movementDistanceMoved;

            // Set the animator parameter for walking
            bool isWalking = Mathf.Abs(characterController.velocity.magnitude) > 2f;
            animator.SetBool("IsWalking", isWalking);

            animator.SetBool("IsHoldingItem", heldKitchenObject != null);

            // Check if spacebar is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnInteract(InteractionType.Take);
            }

            if (Input.GetKey(KeyCode.E))
            {
                if (heldKitchenObject != null)
                {
                    return;
                }

                OnInteract(InteractionType.Use);
            }

            // Check if the dash is on cooldown
            if (dashCooldownTimer < dashCooldown)
            {
                // Update the dash cooldown timer
                dashCooldownTimer += Time.deltaTime;
            }
            // Check if the dash key is pressed
            if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer > dashCooldown)
            {
                // Start dashing
                dashCooldownTimer = 0f;
                isDashing = true;
                PlaySound(dashSound, 1f, Random.Range(0.8f, 1.2f));
                Instantiate(dashParticle, transform.position, transform.rotation);
            }
        }
    }

    public void OnInteract(InteractionType interactionType)
    {
        // Check if the player is within an interactable object
        RaycastHit hit;
        Vector3 raySource = transform.position + Vector3.up * 1f;
        Vector3[] rayOffsets = new Vector3[7] { raySource, raySource - transform.right * 0.15f, raySource + transform.right * 0.15f, raySource - transform.right * 0.25f, raySource + transform.right * 0.25f, raySource - transform.right * 0.5f, raySource + transform.right * 0.5f };
        foreach (Vector3 rayPos in rayOffsets)
        {
            Debug.DrawRay(rayPos, transform.forward * 1.5f, Color.red, 10f);
            if (Physics.Raycast(rayPos, transform.forward, out hit, 1.5f))
            {
                Collider collider = hit.collider;
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (interactionType == InteractionType.Take)
                    {
                        if (InteractionTrace(interactable))
                        {
                            break;
                        }
                    }
                    else if (interactionType == InteractionType.Use)
                    {
                        interactable.InteractWithTask();
                        break;
                    }
                }
            }
        }
    }

    public bool InteractionTrace(IInteractable interactable)
    {
        if (interactable != null)
        {
            InteractionResponse interactionResponse = new InteractionResponse();
            InteractionResult interaction;
            if (heldKitchenObject != null)
            {
                interactionResponse = interactable.Interact(heldKitchenObject);
                interaction = interactionResponse.Result;
            }
            else
            {
                interactionResponse = interactable.Interact();
                interaction = interactionResponse.Result;
            }

            // Handle the interaction result
            switch (interaction)
            {
                case InteractionResult.PlacedByPlayer:
                    Sound_Place();
                    LeaveKitchenObject();
                    return true;
                case InteractionResult.TakenByPlayer:
                    TakeKitchenObject(interactionResponse.KitchenObject);
                    Sound_Take();
                    return true;
                case InteractionResult.CombinedByPlayer:
                    Sound_Place();
                    CombineKitchenObject(interactionResponse.KitchenObject);
                    return true;
                case InteractionResult.PlacedObjectByPlayer:
                    Sound_Place();
                    LeaveKitchenObjectFood();
                    return true;
                default:
                    break;
            }
        }

        return false;
    }

    public void LeaveKitchenObject()
    {
        heldKitchenObject = null;
    }
    public void LeaveKitchenObjectFood()
    {
        if (heldKitchenObject.bPlate)
        {
            // Create a copy of the object
            KitchenObject copyObject = Instantiate(heldKitchenObject, kitchenObjectTransform);
            
            // Reinitialize the original object with original food, without a plate
            heldKitchenObject.Init(heldKitchenObject.GetFoodObject(), false);
            
            // Initialize the copy object with no food and a plate
            copyObject.Init(null, true);
            heldKitchenObject = copyObject;
        }
    }

    public void CombineKitchenObject(KitchenObject kitchenObject)
    {
        if (kitchenObject != null)
        {
            if (kitchenObject.bPlate && heldKitchenObject.bPlate)
            {
                heldKitchenObject.Init(null, true);
                return;
            }
        }

        if(heldKitchenObject != null && heldKitchenObject.transform.parent == kitchenObjectTransform)
        {
            Destroy(heldKitchenObject.gameObject);
        }

        heldKitchenObject = null;
    }

    public void TakeKitchenObject(KitchenObject kitchenObject)
    {
        kitchenObject.PlaceObjectInParent(kitchenObjectTransform);
        heldKitchenObject = kitchenObject;
    }

    public void StartTask(PreparationMethod method)
    {
        if (method == PreparationMethod.Wash)
        {
            animator.SetBool("IsWashing", true);
            animator.SetTrigger("StartWashing");
        }
        else if (method == PreparationMethod.Chop)
        {
            animator.SetBool("IsChopping", true);
            playerKnife.SetActive(true);
            animator.SetTrigger("StartChopping");
        }
    }

    public void StopTask(PreparationMethod method)
    {
        if (method == PreparationMethod.Wash)
        {
            animator.SetBool("IsWashing", false);
        }
        else if (method == PreparationMethod.Chop)
        {
            animator.SetBool("IsChopping", false);
            playerKnife.SetActive(false);
        }
    }

    /// SOUND FUNCTIONS ///

    public void PlaySound(AudioClip audioClip, float volume, float pitch)
    {
        // set new values & play
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioClip);
    }

    public void Sound_KnifeChop()
    {
        PlaySound(knifeChopSound, 1.5f, Random.Range(1f, 1.3f));
    }
    public void Sound_Take()
    {
        audioSource.PlayOneShot(takeSound);
        if (heldKitchenObject != null)
        {
            if (heldKitchenObject.bPlate)
            {
                Sound_Plate();
            }
        }
    }
    public void Sound_Place()
    {
        audioSource.PlayOneShot(placeSound);
        if (heldKitchenObject != null)
        {
            if (heldKitchenObject.bPlate)
            {
                Sound_Plate();
            }
        }
    }
    public void Sound_Plate()
    {
        PlaySound(plateSounds[Random.Range(0, plateSounds.Length)], 0.7f, 0.5f);
    }
    public void Sound_Step()
    {
        PlaySound(stepSounds[Random.Range(0, stepSounds.Length)], 1f, 1.2f);
    }
}
