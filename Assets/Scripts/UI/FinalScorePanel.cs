// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class FinalScorePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] finalScoreTexts;
    [SerializeField] private GameObject finalScoreObject;

    [Header("Values")]
    [SerializeField] private TextMeshProUGUI ordersCompletedValue;
    [SerializeField] private TextMeshProUGUI pointsCollectedValue;
    [SerializeField] private TextMeshProUGUI ordersMissedValue;
    [SerializeField] private TextMeshProUGUI foodBurntValue;
    [SerializeField] private TextMeshProUGUI distanceMovedValue;
    [SerializeField] private TextMeshProUGUI wildcardValue;

    [Header("Containers")]
    [SerializeField] private GameObject ordersCompletedContainer;
    [SerializeField] private GameObject pointsCollectedContainer;
    [SerializeField] private GameObject ordersMissedContainer;
    [SerializeField] private GameObject foodBurntContainer;
    [SerializeField] private GameObject distanceMovedContainer;
    [SerializeField] private GameObject wildcardContainer;

    [Header("Wildcard")]
    [SerializeField] private TextMeshProUGUI wildcardText;

    [Header("Button")]
    [SerializeField] private GameObject exitButton;

    [SerializeField] private AudioClip[] penScratches;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip timeOutWhistle;

    public LevelManager levelManager;

    private int finalScore = 0;

    private IEnumerator ShowContainersInOrder()
    /// <summary>
    /// Display the containers in order with a delay between each one.
    /// A little bit of a hacky way to do it, but it works.
    /// </summary>
    {
        float waitTime = 0.5f;
        finalScoreObject.SetActive(false);

        GetComponent<AudioSource>().PlayOneShot(timeOutWhistle);
        yield return new WaitForSeconds(waitTime * 2);
        GetComponent<Animator>().SetTrigger("OpenPanel");
        yield return new WaitForSeconds(waitTime * 2);

        // Orders Completed
        ordersCompletedValue.text = "+ " + levelManager.OrdersCompleted;
        ordersCompletedContainer.SetActive(true);
        finalScore += levelManager.OrdersCompleted;

        GetComponent<AudioSource>().PlayOneShot(penScratches[Random.Range(0, penScratches.Length)]);

        yield return new WaitForSeconds(waitTime);

        // Points Collected
        pointsCollectedValue.text = "+ " + levelManager.Score;
        pointsCollectedContainer.SetActive(true);
        finalScore += levelManager.Score;

        GetComponent<AudioSource>().PlayOneShot(penScratches[Random.Range(0, penScratches.Length)]);

        yield return new WaitForSeconds(waitTime);

        // Orders Missed
        ordersMissedValue.text = "- " + levelManager.OrdersMissed;
        ordersMissedContainer.SetActive(true);
        finalScore -= levelManager.OrdersMissed;

        GetComponent<AudioSource>().PlayOneShot(penScratches[Random.Range(0, penScratches.Length)]);

        yield return new WaitForSeconds(waitTime);

        // Food Burnt
        foodBurntValue.text = "- " + levelManager.FoodBurnt;
        foodBurntContainer.SetActive(true);
        finalScore -= levelManager.FoodBurnt;

        GetComponent<AudioSource>().PlayOneShot(penScratches[Random.Range(0, penScratches.Length)]);

        yield return new WaitForSeconds(waitTime);

        // Distance Moved
        distanceMovedValue.text = "+ " + (levelManager.DistanceMoved / 100.0f) + "m";
        distanceMovedContainer.SetActive(true);
        finalScore += (int)Mathf.Ceil(levelManager.DistanceMoved / 100.0f);

        GetComponent<AudioSource>().PlayOneShot(penScratches[Random.Range(0, penScratches.Length)]);

        yield return new WaitForSeconds(waitTime);

        // Wildcard
        string[] wildcardTexts = new string[] { "Attempted Robberies", "Cats Scared", "Windows Broken", "Car Accidents", "Existential Dread", "That Dream Again", "Parents Dissapointed", "Black Parades" };
        int randomWildcardScore = Random.Range(1, 30);

        wildcardText.text = wildcardTexts[Random.Range(0, wildcardTexts.Length)];
        wildcardValue.text = randomWildcardScore.ToString();

        finalScore += randomWildcardScore;

        wildcardContainer.SetActive(true);

        finalScoreObject.SetActive(true);
        RefreshScoreText();

        yield return new WaitForSeconds(1f);

        GetComponent<AudioSource>().PlayOneShot(victorySound);

        yield return new WaitForSeconds(waitTime);

        // Button
        exitButton.SetActive(true);
    }

    // Call this function to start showing the containers in order
    public void DisplayFinalScores()
    {
        if (!levelManager)
            levelManager = FindObjectOfType<LevelManager>();

        StartCoroutine(ShowContainersInOrder());
    }

    private void RefreshScoreText()
    {
        if (finalScore > 999)
            finalScore = 999; // Cap the final score at 999 (3 digits)
        if (finalScore < 0)
            finalScore = 0; // Make sure the final score is not negative

        int hundreds = finalScore / 100;
        int tens = (finalScore / 10) % 10;
        int ones = finalScore % 10;

        StartCoroutine(MoveScoreText(finalScoreTexts[2], hundreds));
        StartCoroutine(MoveScoreText(finalScoreTexts[1], tens));
        StartCoroutine(MoveScoreText(finalScoreTexts[0], ones));
    }

    private IEnumerator MoveScoreText(TextMeshProUGUI scoreText, int targetY)
    {
        float duration = 1f; // Duration of the movement
        float elapsedTime = 0f; // Elapsed time since the movement started
        float startY = scoreText.rectTransform.anchoredPosition.y; // Starting Y position
        float[] yPoints = new float[] { -430, -335, -238, -140, -42, 50, 148, 242, 340, 435 }; // Y positions for each digit, 0 to 9 inclusive
        float endY = yPoints[targetY]; // Ending Y position

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration); // Normalized time

            // Calculate the new Y position using easing function
            float newY = Mathf.Lerp(startY, endY, EasingFunction(t));

            // Update the Y position of the score text
            scoreText.rectTransform.anchoredPosition = new Vector2(scoreText.rectTransform.anchoredPosition.x, newY);

            yield return null;
        }

        // Set the final Y position of the score text
        scoreText.rectTransform.anchoredPosition = new Vector2(scoreText.rectTransform.anchoredPosition.x, endY);
    }

    private float EasingFunction(float t)
    {
        // Use easing function to control the movement speed
        return t * t * (3f - 2f * t);
    }
}
