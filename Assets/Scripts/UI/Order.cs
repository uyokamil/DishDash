// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Order : MonoBehaviour
{
    [SerializeField] private FoodObject foodObject;
    public FoodObject FoodObject { get { return foodObject; } }

    private float orderTime;
    public float OrderTime { get { return orderTime; } }

    private float maxWaitDuration;
    public float MaxWaitDuration { get { return maxWaitDuration; } }

    private Transform targetPosition;
    private float movementSpeed = 2000f;
    private float rotationSpeed = 150f;

    [SerializeField] private Image foodReferenceImage;
    [SerializeField] private Image plateSpriteImage;

    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image timerSliderFill;
    [SerializeField] private Image stopwatch;

    [SerializeField] private Color timerColorNormal;
    [SerializeField] private Color timerColorLow;

    private OrderManager orderManager;

    [SerializeField] private Image completionImageObject;
    [SerializeField] private Sprite successImage;
    [SerializeField] private Sprite failImage;

    public void SetTargetTransform(Transform target)
    {
        targetPosition = target;
    }

    public void Init(FoodObject inFoodObject, float inOrderTime, float inMaxWaitDuration, Transform inTargetPositionObject, OrderManager inOrderManager)
    {
        orderManager = inOrderManager;
        foodObject = inFoodObject;
        orderTime = inOrderTime;
        maxWaitDuration = inMaxWaitDuration;
        targetPosition = inTargetPositionObject;

        foodReferenceImage.sprite = foodObject.foodSprite;

        timerSliderFill.color = timerColorNormal;

        if(foodObject.foodType == FoodType.Drink || foodObject.foodType == FoodType.Pizza)
        {
            plateSpriteImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    /// <summary>
    /// Smoothly move the order towards the target position.
    /// Enhances game feel by adding easing in/out based on distance.
    /// Also updates the timer slider value and color based on time remaining.
    /// </summary>
    {
        if (targetPosition != null)
        {
            Vector3 direction = targetPosition.position - transform.position;
            direction.z = 0f;
            direction.Normalize();

            float distance = Vector3.Distance(transform.position, targetPosition.position);

            if (distance > 5f)
            {
                // Ease in/out based on distance
                if (distance < 50f)
                {
                    float easeFactor = 1000f - (distance / 50f);
                    transform.position = Vector3.Lerp(transform.position, targetPosition.position, easeFactor);
                }
                else
                {
                    transform.position += direction * movementSpeed * Time.deltaTime;
                }

                // Calculate the angle to rotate towards the movement direction
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
                Quaternion clampedRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(targetRotation.eulerAngles.z, -25f, 25f));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, clampedRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = targetPosition.position;
                Quaternion rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * rotationSpeed);
                transform.rotation = rotation;
            }
        }

        // Update the timer slider value
        float elapsedTime = Time.time - orderTime;
        float timeRemaining = maxWaitDuration - elapsedTime;
        timerSlider.value = timeRemaining / maxWaitDuration;

        float threshold = 0.3f * maxWaitDuration;
        if (timeRemaining <= threshold)
        {
            // timeRemaining is 30% away from 0
            timerSliderFill.color = timerColorLow;

            // Rotate the stopwatch image on Z +- 5 from its current rotation repeatedly
            float rotationAmount = Mathf.Sin(Time.time * 50f) * 5f;
            stopwatch.transform.rotation = Quaternion.Euler(0f, 0f, -15f + rotationAmount);
        }

        if (timeRemaining <= 0)
        {
            // Order has expired
            orderManager.ExpireOrder(this);
        }
    }

    public void Complete()
    {
        completionImageObject.sprite = successImage;

        GetComponent<Animator>().SetTrigger("Complete");
    }

    public void Expire()
    {
        completionImageObject.sprite = failImage;

        GetComponent<Animator>().SetTrigger("Complete");
    }

    public void NoPlateWarning()
    {
        StartCoroutine(NoPlateAnimation());
    }

    private IEnumerator NoPlateAnimation()
    {
        // Smoothly change plateSpriteImage color to red
        float duration = 0.25f;
        Color originalColor = Color.white;
        Color targetColor = Color.red;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            plateSpriteImage.color = Color.Lerp(originalColor, targetColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            // Rotate the object's transform on the Z axis
            float rotationAmount = Mathf.Sin(Time.time * 20f) * 8f;
            transform.rotation = Quaternion.Euler(0f, 0f, rotationAmount);

            yield return null;
        }

        // Play sound
        if (orderManager != null)
        {
            orderManager.PlaySound(orderManager.angrySound);
        }

        // Wait for a little bit
        float waitDuration = 1f;
        yield return new WaitForSeconds(waitDuration);

        // Smoothly change plateSpriteImage color back to original (white)
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            plateSpriteImage.color = Color.Lerp(targetColor, originalColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void Animation_RemoveTarget()
    {
        if (targetPosition != null)
        {
            Destroy(targetPosition.gameObject);
        }
    }

    public void Animation_Completed()
    {
        if (gameObject)
        {
            Destroy(gameObject);
        }
    }
}
