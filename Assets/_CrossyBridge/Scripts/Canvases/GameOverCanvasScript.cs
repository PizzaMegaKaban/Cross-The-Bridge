using SgLib;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverCanvasScript : MonoBehaviour
{
    [Header("Rates indicators")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI totalCoinsText;

    [Header("Round indicators")]
    public TextMeshProUGUI roundScoreText;
    public TextMeshProUGUI roundCoinsText;

    [Header("Level passed objects")]
    public GameObject levelCompletedText;
    public GameObject nextLevelButton;

    [Header("Game over objects")]
    public GameObject gameOverText;
    public GameObject restartButton;
    public GameObject dailyRewardText;
    public GameObject dailyRewardTimeRemainingText;
    public TextMeshProUGUI dailyRewardTimeRemainingTextMeshPro;
    public Button dailyRewardBtn;

    [Header("Helper objects")]
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        totalScoreText.text = ScoreManager.Instance.HighScore.ToString();
        totalCoinsText.text = CoinManager.Instance.Coins.ToString();

        roundScoreText.text = PlayerPrefs.GetInt("CurrentGameScore", 0).ToString();
        roundCoinsText.text = PlayerPrefs.GetInt("CurrentGameCoins", 0).ToString();

        if (gameManager.GameState == GameState.LevelPassed)
        {
            levelCompletedText.SetActive(true);
            nextLevelButton.SetActive(true);

            gameOverText.SetActive(false);
            restartButton.SetActive(false);
        }
        else if (gameManager.GameState == GameState.GameOver)
        {
            gameOverText.SetActive(true);
            restartButton.SetActive(true);

            levelCompletedText.SetActive(false);
            nextLevelButton.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!DailyRewardController.Instance.disable && dailyRewardBtn.gameObject.activeSelf)
        {
            TimeSpan timeToReward = DailyRewardController.Instance.TimeUntilReward;

            if (timeToReward <= TimeSpan.Zero)
            {
                dailyRewardBtn.interactable = true;
                dailyRewardText.SetActive(true);
                dailyRewardTimeRemainingText.SetActive(false);
            }
            else
            {
                dailyRewardBtn.interactable = false;
                dailyRewardText.SetActive(false);
                dailyRewardTimeRemainingText.SetActive(true);
                dailyRewardTimeRemainingTextMeshPro.text = string.Format("{0:00}:{1:00}:{2:00}", timeToReward.Hours, timeToReward.Minutes, timeToReward.Seconds);
            }
        }
    }

    public void RestartGame()
    {
        gameManager.RestartGame(0.2f);
    }

    public void PlayNextLevel()
    {
        int movingPlanesInLevel = PlayerPrefs.GetInt("MovingPlanesInLevel", -1);
        int newLevel = movingPlanesInLevel == -1 ? -1 : movingPlanesInLevel + 1 - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2);
        PlayerPrefs.SetInt("MovingPlanesInLevel", movingPlanesInLevel == -1 ? -1 : movingPlanesInLevel + 1);
        string sceneName = "Level" + newLevel.ToString();
        gameManager.SelectLevel(newLevel, true);
        gameManager.RestartGame(0f);
    }
}
