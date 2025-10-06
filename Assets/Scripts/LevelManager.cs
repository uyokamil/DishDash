// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private int score;
    public int Score { set { score = value; } get { return score; } }
    public void AddScore(int value) { score += value; RefreshScoreText(); }

    [Header("Scene Objects")]
    [SerializeField]
    private OrderManager orderManager;
    [SerializeField]
    private ConveyorBeltManager conveyorBeltManager;
    [SerializeField]
    private AudioSource musicPlayer;
    [SerializeField]
    private PlayerController player;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI[] scoreTexts;
    [SerializeField] private TextMeshProUGUI timeTextMinutes;
    [SerializeField] private TextMeshProUGUI timeTextSeconds;
    [SerializeField] private TextMeshProUGUI timeTextDivider;
    [SerializeField] private Image timeTextBackground;
    [SerializeField] private AudioSource timerAudio;

    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameOverText;

    [SerializeField] private FinalScorePanel finalScorePanel;

    [Header("Trackers")]
    private int ordersCompleted;
    public int OrdersCompleted { get { return ordersCompleted; } set { ordersCompleted = value; } }

    private int ordersMissed;
    public int OrdersMissed { get { return ordersMissed; } set { ordersMissed = value; } }

    private int foodBurnt;
    public int FoodBurnt { get { return foodBurnt; } }

    private int distanceMoved;
    public int DistanceMoved { get { return distanceMoved; } }

    public float timer = 240f; // 4 minutes

    private void Awake()
    {
        UpdateTimerUI();

        gameOverPanel.SetActive(false);

        RefreshScoreText();
    }

    private void Start()
    {
        if (!orderManager)
            orderManager = FindObjectOfType<OrderManager>();
        orderManager.levelManager = this;

        if (!conveyorBeltManager)
            conveyorBeltManager = FindObjectOfType<ConveyorBeltManager>();

        if (!musicPlayer)
            musicPlayer = FindObjectOfType<AudioSource>();

        if (!player)
            player = FindObjectOfType<PlayerController>();

        if (!finalScorePanel)
            finalScorePanel = FindObjectOfType<FinalScorePanel>();

        StartCoroutine(LevelCountdown());

        // Subscribe to the OnFoodBurnt event from all applicable objects in the scene
        CT_AutomaticPrep[] automaticPreps = FindObjectsOfType<CT_AutomaticPrep>();
        foreach (CT_AutomaticPrep automaticPrep in automaticPreps)
        {
            automaticPrep.OnFoodBurnt += HandleFoodBurnt;
        }
    }

    private void OnDestroy()
    {
        // Subscribe to the OnFoodBurnt event from all applicable objects in the scene
        CT_AutomaticPrep[] automaticPreps = FindObjectsOfType<CT_AutomaticPrep>();
        foreach (CT_AutomaticPrep automaticPrep in automaticPreps)
        {
            automaticPrep.OnFoodBurnt -= HandleFoodBurnt;
        }
    }

    // Implement the countdown function
    private IEnumerator LevelCountdown()
    {
        int count = 4;
        while (count > 0)
        {
            count--;

            if (count == 0)
            {
                countdownText.text = "COOK!";

                countdownText.GetComponent<Animator>().SetTrigger("Countdown");
                countdownPanel.GetComponent<Animator>().SetTrigger("CountdownEnd");

                musicPlayer.Play();

                yield return new WaitForSeconds(.5f);
            }
            else
            {
                countdownText.text = count.ToString();

                countdownText.GetComponent<Animator>().SetTrigger("Countdown");

                yield return new WaitForSeconds(1f);
            }
        }

        countdownPanel.SetActive(false);

        player.canMove = true;
        orderManager.StartOrders();

        StartLevelTimer();
    }

    public void StartLevelTimer()
    {
        StartCoroutine(LevelTimer());
    }

    private IEnumerator LevelTimer()
    {
        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer--;
            UpdateTimerUI();
        }

        // Game over: timer ran out
        HandleGameOver();
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timeTextMinutes.text = minutes.ToString("00");
        timeTextSeconds.text = seconds.ToString("00");

        if(timer == 30)
        {
            timeTextMinutes.color = Color.white;
            timeTextSeconds.color = Color.white;
            timeTextDivider.color = Color.white;
            timeTextBackground.color = Color.red;

            timerAudio.Play();
        }
    }

    private void HandleGameOver()
    {
        player.canMove = false;
        timerAudio.Stop();

        orderManager.StopOrders();

        distanceMoved = (int)Mathf.Ceil(player.TotalDistanceMoved);

        gameOverPanel.SetActive(true);
        gameOverText.GetComponent<Animator>().SetTrigger("Countdown");

        finalScorePanel.DisplayFinalScores();
    }

    private void RefreshScoreText()
    {
        int hundreds = score / 100;
        int tens = (score / 10) % 10;
        int ones = score % 10;

        StartCoroutine(MoveScoreText(scoreTexts[2], hundreds));
        StartCoroutine(MoveScoreText(scoreTexts[1], tens));
        StartCoroutine(MoveScoreText(scoreTexts[0], ones));
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

    private void HandleFoodBurnt()
    {
        // Increase the foodBurnt counter when any food gets burnt (delegate handler)
        foodBurnt++;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
