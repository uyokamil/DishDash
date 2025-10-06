// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum LevelName
{
    PattyPalace,
    PauliesPizzeria,
    RockNRollDiner
}

public enum LevelDifficulty
{
    Easy,
    Balanced,
    Difficult
}

public class LevelSelect : MonoBehaviour
{
    /// <summary>
    /// Level Select UI for the main menu.
    /// Lets the player choose a level and difficulty to play.
    /// </summary>

    [Header("Images of Levels")]
    public Sprite pattyPalace;
    public Sprite pauliesPizzeria;
    public Sprite rocknrollDiner;
    public Sprite wingWonders;

    [Header("Level Info")]
    public string[] levelNames;
    public string[] levelDescriptions;

    public string[] levelPlateWashing_Easy;
    public string[] levelPlateWashing_Balanced;
    public string[] levelPlateWashing_Difficult;

    [Header("UI")]
    public Image levelPreviewImage;

    [Header("UI/LevelInfo")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI levelDescriptionText;

    [Header("UI/Difficulty")]
    public TextMeshProUGUI difficultyText;

    public TextMeshProUGUI orderSpeedText;
    public TextMeshProUGUI taskSpeedText;
    public TextMeshProUGUI plateWashingText;

    private int selectedLevel = 0;
    private int selectedDifficulty = 1;

    private void Start()
    {
        UpdateLevelInfo();
    }

    public void NextLevel()
    {
        if (selectedLevel < System.Enum.GetValues(typeof(LevelName)).Length - 1)
        {
            selectedLevel++;
        }
        else
        {
            selectedLevel = 0;
        }

        UpdateLevelInfo();
    }
    public void PrevLevel()
    {
        if (selectedLevel > 0)
        {
            selectedLevel--;
        }
        else
        {
            selectedLevel = System.Enum.GetValues(typeof(LevelName)).Length - 1;
        }

        UpdateLevelInfo();
    }

    public void NextDifficulty()
    {
        if (selectedDifficulty < System.Enum.GetValues(typeof(LevelDifficulty)).Length - 1)
        {
            selectedDifficulty++;
        }
        else
        {
            selectedDifficulty = 0;
        }

        UpdateLevelInfo(true);
    }
    public void PrevDifficulty()
    {
        if (selectedDifficulty > 0)
        {
            selectedDifficulty--;
        }
        else
        {
            selectedDifficulty = System.Enum.GetValues(typeof(LevelDifficulty)).Length - 1;
        }

        UpdateLevelInfo(true);
    }

    public void UpdateLevelInfo(bool difficulty = false)
    {
        if (!difficulty)
            selectedDifficulty = 1;

        levelNameText.text = levelNames[selectedLevel];
        levelDescriptionText.text = levelDescriptions[selectedLevel];

        switch (selectedLevel)
        {
            case 0:
                levelPreviewImage.sprite = pattyPalace;
                break;
            case 1:
                levelPreviewImage.sprite = pauliesPizzeria;
                break;
            case 2:
                levelPreviewImage.sprite = rocknrollDiner;
                break;
            case 3:
                levelPreviewImage.sprite = wingWonders;
                break;
        }

        switch (selectedDifficulty)
        {
            case 0:
                difficultyText.text = "Easy";
                orderSpeedText.text = "Order Speed: Slow";
                taskSpeedText.text = "Task Speed: Fast";
                plateWashingText.text = levelPlateWashing_Easy[selectedLevel];
                break;
            case 1:
                difficultyText.text = "Balanced";
                orderSpeedText.text = "Order Speed: Normal";
                taskSpeedText.text = "Task Speed: Normal";
                plateWashingText.text = levelPlateWashing_Balanced[selectedLevel];
                break;
            case 2:
                difficultyText.text = "Difficult";
                orderSpeedText.text = "Order Speed: Fast";
                taskSpeedText.text = "Task Speed: Slow";
                plateWashingText.text = levelPlateWashing_Difficult[selectedLevel];
                break;
        }
    }

    public void PlayLevel()
    {
        if (selectedLevel == 0)
        {
            if (selectedDifficulty == 0)
            {
                SceneManager.LoadScene("Burger_Easy");
            }
            else if (selectedDifficulty == 1)
            {
                SceneManager.LoadScene("Burger_Balanced");
            }
            else if (selectedDifficulty == 2)
            {
                SceneManager.LoadScene("Burger_Difficult");
            }
        }
        else if (selectedLevel == 1)
        {
            if (selectedDifficulty == 0)
            {
                SceneManager.LoadScene("Pizzeria_Easy");
            }
            else if (selectedDifficulty == 1)
            {
                SceneManager.LoadScene("Pizzeria_Balanced");
            }
            else if (selectedDifficulty == 2)
            {
                SceneManager.LoadScene("Pizzeria_Difficult");
            }
        }
        else if (selectedLevel == 2)
        {
            if (selectedDifficulty == 0)
            {
                SceneManager.LoadScene("Diner_Easy");
            }
            else if (selectedDifficulty == 1)
            {
                SceneManager.LoadScene("Diner_Balanced");
            }
            else if (selectedDifficulty == 2)
            {
                SceneManager.LoadScene("Diner_Difficult");
            }
        }
    }
}