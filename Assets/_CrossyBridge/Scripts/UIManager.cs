using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;
using SgLib;
using UnityEngine.Localization;
using TMPro;

#if EASY_MOBILE
//using EasyMobile;
#endif

public class UIManager : MonoBehaviour
{
    public static bool firstLoad = true;
    private bool isPaused = false;

    [Header("Object References")]
    public GameManager gameManager;
    public CameraController camController;
    public DailyRewardController dailyRewardController;

    [Header("Canvases")]
    
    // был mainCanvas
    public GameObject startGameCanvas;
    public GameObject gameplayCanvas;
    public GameObject respawnCanvas;
    public GameObject gameOverCanvas;
    public GameObject levelsCanvas;
    public GameObject settingsCanvas;
    public GameObject storeCanvas;
    public GameObject rewardCanvas;
    public GameObject pauseCanvas;

    [Header("UIElements")]
    public GameObject blackPanel;
    public GameObject header;
    public Text score;
    public TextMeshProUGUI bestScore;
    public TextMeshProUGUI gold;
    //public Text title;
    public TextMeshProUGUI levelNumber;
    public GameObject currentLevel;
    public GameObject tapToStart;
    public GameObject characterSelectBtn;
    public GameObject menuButtons;
    public GameObject restartGameButton;
    public GameObject nextLevelButton;
    public TextMeshProUGUI dailyRewardBtnText;
    public GameObject levelCompleted;
    public GameObject gameOver;
    public GameObject gameCoins;
    public TextMeshProUGUI gameCoinsCount;
    public GameObject dailyRewardBtn;
    public GameObject rewardUI;
    public GameObject soundOffBtn;
    public GameObject soundOnBtn;
    public GameObject musicOnBtn;
    public GameObject musicOffBtn;
    public GameObject pauseButton;
    public GameObject gameplayManagementCanvas;
    public GameObject respawnUI;
    public GameObject countdownUI;
    public GameObject timerSlider;

    public GameObject levelSelect;
    public GameObject levelTimer;

    [Header("Premium Features Only")]
    public GameObject watchForCoinsBtn;
    public GameObject leaderboardBtn;
    public GameObject iapPurchaseBtn;
    public GameObject removeAdsBtn;
    public GameObject restorePurchaseBtn;
    public int rewardedCoins = 35;

    [Header("Sharing-Specific")]
    public GameObject shareUI;
    //public ShareUIController shareUIController;

    // Animator scoreAnimator;
    // Animator dailyRewardAnimator;
    bool isWatchAdsForCoinBtnActive;
    
    bool _isGameOver;

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
        ScoreManager.ScoreUpdated += OnScoreUpdated;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
        ScoreManager.ScoreUpdated -= OnScoreUpdated;
        
    }

    // Use this for initialization
    void Start()
    {
        // scoreAnimator = score.GetComponent<Animator>();
        // dailyRewardAnimator = dailyRewardBtn.GetComponent<Animator>();

        // TODO раскомментировать для игры
        // Reset();
        ShowStartUI();
    }

    // Update is called once per frame
    void Update()
    {
        //if (startGameCanvas.activeSelf)
        //{
        //    // score.text = ScoreManager.Instance.Score.ToString();
        //    bestScore.text = ScoreManager.Instance.HighScore.ToString();
        //    gold.text = CoinManager.Instance.Coins.ToString();

        //    if (!DailyRewardController.Instance.disable && dailyRewardBtn.gameObject.activeSelf)
        //    {
        //        TimeSpan timeToReward = DailyRewardController.Instance.TimeUntilReward;

        //        if (timeToReward <= TimeSpan.Zero)
        //        {
        //            dailyRewardBtnText.text = "grab your reward!";
        //            // dailyRewardAnimator.SetTrigger("activate");
        //        }
        //        else
        //        {
        //            dailyRewardBtnText.text = string.Format("{0:00}:{1:00}:{2:00}", timeToReward.Hours, timeToReward.Minutes, timeToReward.Seconds);
        //            // dailyRewardAnimator.SetTrigger("deactivate");
        //        }
        //    }
        //}

        if (settingsCanvas.activeSelf)
        {
            UpdateMuteButtons();
            UpdateMusicButtons();
        }
    }

    void GameManager_GameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {              
            ShowGameUI();
        }
        else if (newState == GameState.PreGameOver)
        {
            ShowRespawnUI();
        }
        else if (newState == GameState.Recovering)
        {
            // play recovering player animation
        }
        else if (newState == GameState.GameOver)
        {
            Invoke("ShowGameOverUI", 0.5f);
        }
        else if (newState == GameState.LevelPassed)
        {
            Invoke("ShowGameOverUI", 0.5f);
        }
    }

    void OnScoreUpdated(int newScore)
    {
        // scoreAnimator.Play("NewScore");

        PlayerPrefs.SetInt("CurrentGameScore", newScore);
    }

    void Reset()
    {
        EventManager.OnNewCanvasOpening.Invoke(startGameCanvas);

        //startGameCanvas.SetActive(true);
        //settingsCanvas.SetActive(false);

        //blackPanel.SetActive(false);
        //header.SetActive(false);
        //score.gameObject.SetActive(false);
        //currentLevel.SetActive(false);
        //tapToStart.SetActive(false);
        //characterSelectBtn.SetActive(false);
        //menuButtons.SetActive(false);
        //dailyRewardBtn.SetActive(false);
        //settingsCanvas.SetActive(false);
        //pauseButton.SetActive(false);
        //gameplayManagementCanvas.SetActive(false);
        //timerSlider.SetActive(false);
        //levelTimer.SetActive(false);
        //gameOver.SetActive(false);
        //levelCompleted.SetActive(false);

        //shareUI.SetActive(false);

        //watchForCoinsBtn.SetActive(false);
    }

    public void ShowStartUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(startGameCanvas);

        //startGameCanvas.SetActive(true);
        //settingsCanvas.SetActive(false);

        //if (PlayerPrefs.GetInt("MovingPlanesInLevel", -1) == -1)
        //    currentLevel.SetActive(false);
        //else
        //{
        //    levelNumber.text = (PlayerPrefs.GetInt("MovingPlanesInLevel", -1) - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2)).ToString();
        //    currentLevel.SetActive(true);
        //}

        //header.SetActive(true);
        //tapToStart.SetActive(true);
        //characterSelectBtn.SetActive(true);
        //pauseButton.SetActive(false);
        //gameOver.SetActive(false);
        //levelCompleted.SetActive(false);
        //gameplayManagementCanvas.SetActive(false);
        //timerSlider.SetActive(false);
        //levelTimer.SetActive(false);
    }

    public void ShowGameUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(gameplayCanvas);

        //header.SetActive(true);
        //score.gameObject.SetActive(true);
        //tapToStart.SetActive(false);
        //characterSelectBtn.SetActive(false);
        //pauseButton.SetActive(true);
        //gameOver.SetActive(false);
        //levelCompleted.SetActive(false);
        //gameplayManagementCanvas.SetActive(true);

        // TODO перенести логику на канвас игры
        //if (PlayerPrefs.GetInt("MovingPlanesInLevel", -1) == -1)
        //    timerSlider.SetActive(false);
        //else
        //    timerSlider.SetActive(true);
        //levelTimer.SetActive(true);
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            Time.timeScale = 0f;
            isPaused = true;
            // pauseButton.SetActive(false);

            // Показываем меню паузы
            ShowPauseMenu();
        }

    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseButton.SetActive(true);

        // Скрываем меню паузы
        HidePauseMenu();
    }

    public void OpenSettingsFromPause()
    {
        EventManager.OnNewCanvasOpening.Invoke(settingsCanvas);

        //pauseMenuCanvas.SetActive(false);
        //startGameCanvas.SetActive(true); // Опционально, если нужно скрыть основной интерфейс
        //settingsCanvas.SetActive(true);
        //gameplayManagementCanvas.SetActive(false);
    }


    void ShowPauseMenu()
    {
        EventManager.OnNewCanvasOpening.Invoke(pauseCanvas);

        //pauseMenuCanvas.SetActive(true);
        //gameplayManagementCanvas.SetActive(false);
    }

    void HidePauseMenu()
    {
        EventManager.OnNewCanvasOpening.Invoke(gameplayCanvas);

        //pauseMenuCanvas.SetActive(false);
        //gameplayManagementCanvas.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main");
    }


    public void ShowGameOverUI()
    {
        _isGameOver = true;

        EventManager.OnNewCanvasOpening.Invoke(gameOverCanvas);

        //blackPanel.SetActive(true);
        //header.SetActive(true);
        //score.gameObject.SetActive(true);
        //gameOver.SetActive(true);
        //levelCompleted.SetActive(false);
        //currentLevel.SetActive(false);
        //tapToStart.SetActive(false);
        //menuButtons.SetActive(true);
        //restartGameButton.SetActive(true);
        //nextLevelButton.SetActive(false);
        //pauseButton.SetActive(false);
        //gameplayManagementCanvas.SetActive(false);
        //timerSlider.SetActive(false);
        //levelTimer.SetActive(false);

        //gameCoinsCount.text = PlayerPrefs.GetInt("CurrentGameCoins", 0).ToString();
        //gameCoins.SetActive(true);

        ////
        //watchForCoinsBtn.gameObject.SetActive(true);
        ////
        //settingsCanvas.SetActive(false);

        // Only show "watch for coins button" if a rewarded ad is loaded and premium features are enabled
        // #if EASY_MOBILE
        // if (gameManager.enablePremiumFeatures && AdDisplayer.Instance.CanShowRewardedAd() && AdDisplayer.Instance.watchAdToEarnCoins)
        // {
        //     watchForCoinsBtn.SetActive(true);
        //     watchForCoinsBtn.GetComponent<Animator>().SetTrigger("activate");
        // }
        // else
        // {
        //     watchForCoinsBtn.SetActive(false);
        // }

        //watchForCoinsBtn.SetActive(true);
        //watchForCoinsBtn.GetComponent<Animator>().SetTrigger("activate");
        // #endif

        // Not showing the daily reward button if the feature is disabled
        // TODO использовать в Canvas Manager для UI
        //if (!DailyRewardController.Instance.disable)
        //{
        //    dailyRewardBtn.SetActive(true);
        //}
        // TODO использовать в Canvas Manager для UI

        //if (IsPremiumFeaturesEnabled())
        //    ShowShareUI();


        // Blur the background
    }

    public void ShowLevelPassedUI()
    {
        //_isGameOver = false;

        //blackPanel.SetActive(true);
        //header.SetActive(true);
        //score.gameObject.SetActive(false);
        //levelCompleted.SetActive(true);
        //gameOver.SetActive(false);
        //currentLevel.SetActive(false);
        //tapToStart.SetActive(false);
        //menuButtons.SetActive(true);
        //restartGameButton.SetActive(false);
        //nextLevelButton.SetActive(true);
        //pauseButton.SetActive(false);
        //gameplayManagementCanvas.SetActive(false);
        //timerSlider.SetActive(false);
        //levelTimer.SetActive(false);

        //gameCoinsCount.text = PlayerPrefs.GetInt("CurrentGameCoins", 0).ToString();
        //gameCoins.SetActive(true);

        ////
        //watchForCoinsBtn.gameObject.SetActive(true);
        ////
        //settingsCanvas.SetActive(false);

        // Only show "watch for coins button" if a rewarded ad is loaded and premium features are enabled
// #if EASY_MOBILE
        // if (gameManager.enablePremiumFeatures && AdDisplayer.Instance.CanShowRewardedAd() && AdDisplayer.Instance.watchAdToEarnCoins)
        // {
        //     watchForCoinsBtn.SetActive(true);
        //     watchForCoinsBtn.GetComponent<Animator>().SetTrigger("activate");
        // }
        // else
        // {
        //     watchForCoinsBtn.SetActive(false);
        // }

        //watchForCoinsBtn.SetActive(true);
        //watchForCoinsBtn.GetComponent<Animator>().SetTrigger("activate");
// #endif

        // Not showing the daily reward button if the feature is disabled
        //if (!DailyRewardController.Instance.disable)
        //{
        //    dailyRewardBtn.SetActive(true);
        //}

        //if (IsPremiumFeaturesEnabled())
        //    ShowShareUI();


        // Blur the background
    }

    public void ShowSettingsUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(settingsCanvas);

        //startGameCanvas.SetActive(false);
        //settingsCanvas.SetActive(true);
        //gameplayManagementCanvas.SetActive(false);
    }

    public void HideSettingsUI()
    {
        if (gameManager.GameState == GameState.GameOver || gameManager.GameState == GameState.LevelPassed)
        {
            EventManager.OnNewCanvasOpening.Invoke(gameOverCanvas);
        }
        else if (gameManager.GameState == GameState.Paused)
        {
            EventManager.OnNewCanvasOpening.Invoke(pauseCanvas);
        }

        //startGameCanvas.SetActive(true);
        //// pauseMenuCanvas.SetActive(true);
        //settingsCanvas.SetActive(false);
        //gameplayManagementCanvas.SetActive(true);
    }

    public void ShowStoreUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(storeCanvas);

        //startGameCanvas.SetActive(false);
        //storeCanvas.SetActive(true);
        //gameplayManagementCanvas.SetActive(false);
    }

    public void HideStoreUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(gameOverCanvas);

        //startGameCanvas.SetActive(true);
        //storeCanvas.SetActive(false);
        //gameplayManagementCanvas.SetActive(false);
    }

    public void StartGame()
    {
        gameManager.StartGame();
    }

    public void EndGame()
    {
        gameManager.GameOver();
    }

    //public void RestartGame()
    //{
    //    gameManager.RestartGame(0.2f);
    //}

    //public void PlayNextLevel()
    //{
    //    int movingPlanesInLevel = PlayerPrefs.GetInt("MovingPlanesInLevel", -1);
    //    int newLevel = movingPlanesInLevel == -1 ? -1 : movingPlanesInLevel + 1 - PlayerPrefs.GetInt("DeltaPlatesForLevel", 2);
    //    PlayerPrefs.SetInt("MovingPlanesInLevel", movingPlanesInLevel == -1 ? -1 : movingPlanesInLevel + 1);
    //    string sceneName = "Level" + newLevel.ToString();
    //    gameManager.SelectLevel(newLevel, true);
    //    gameManager.RestartGame(0f);
    //    levelSelect.SetActive(false);
    //}

    public void WatchRewardedAdForCoins()
    {
        watchForCoinsBtn.SetActive(false);
    }

    void OnCompleteRewardedAdToEarnCoins(int id = 1) => ShowRewardUI(rewardedCoins);

    public void GrabDailyReward()
    {
        if (DailyRewardController.Instance.TimeUntilReward <= TimeSpan.Zero)
        {
            float reward = UnityEngine.Random.Range(dailyRewardController.minRewardValue, dailyRewardController.maxRewardValue);

            // Round the number and make it mutiplies of 5 only.
            int roundedReward = (Mathf.RoundToInt(reward) / 5) * 5;

            // Show the reward UI
            ShowRewardUI(roundedReward);

            // Update next time for the reward
            DailyRewardController.Instance.SetNextRewardTime(dailyRewardController.rewardIntervalHours, dailyRewardController.rewardIntervalMinutes, dailyRewardController.rewardIntervalSeconds);
        }
    }

    public void ShowRewardUI(int reward)
    {
        PlayerPrefs.SetInt("Reward", reward);
        EventManager.OnNewCanvasOpening.Invoke(rewardCanvas);

        //rewardUI.SetActive(true);
        //rewardUI.GetComponent<RewardUIController>().Reward(reward);
    }

    public void HideRewardUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(gameOverCanvas);

        // rewardUI.SetActive(false);
    }

    public void ShowLevelsUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(levelsCanvas);

        //levelSelect.SetActive(true);
        //if (_isGameOver)
        //    gameOver.SetActive(false);
        //else
        //    levelCompleted.SetActive(false);
    }

    public void HideLevelsUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(gameOverCanvas);

        //levelSelect.SetActive(false);
        //if (_isGameOver)
        //    gameOver.SetActive(true);
        //else
        //    levelCompleted.SetActive(true);
    }

    public void ShowLeaderboardUI()
    {
        //#if EASY_MOBILE
        //if (GameServices.IsInitialized())
        //{
        //    GameServices.ShowLeaderboardUI();
        //}
        //else
        //{
        //    #if UNITY_IOS
        //    NativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
        //    #elif UNITY_ANDROID
        //    GameServices.Init();
        //    #endif
        //}
        //#endif
    }

    //public void PurchaseRemoveAds()
    //{
    //    #if EASY_MOBILE
    //    InAppPurchaser.Instance.Purchase(InAppPurchaser.Instance.removeAds);
    //    #endif
    //}

    //public void RestorePurchase()
    //{
    //    #if EASY_MOBILE
    //    InAppPurchaser.Instance.RestorePurchase();
    //    #endif
    //}

    //public void ShowShareUI()
    //{
    //    StartCoroutine(SetUSphareUI());
    //}
//    IEnumerator SetUSphareUI()
//    {
//        yield return new WaitForSeconds(0.4F);
//        if (!ScreenshotSharer.Instance.disableSharing)
//        {
//            Texture2D texture = ScreenshotSharer.Instance.CapturedScreenshot;
//            shareUIController.ImgTex = texture;

//#if EASY_MOBILE
//            AnimatedClip clip = ScreenshotSharer.Instance.RecordedClip;
//            shareUIController.AnimClip = clip;
//#endif
//            shareUI.SetActive(true);

//        }
//    }

    public void HideShareUI()
    {
        shareUI.SetActive(false);
    }

    public void ShowRespawnUI()
    {
        EventManager.OnNewCanvasOpening.Invoke(respawnCanvas);

        // gameplayManagementCanvas.SetActive(false);

        //blackPanel.SetActive(true);
        //respawnUI.SetActive(true);
    }

    //public void CallRespawn()
    //{
    //    EventManager.OnNewCanvasOpening.Invoke(respawnCanvas);
    //}

    //public void HideRespawnUI()
    //{
    //    EventManager.OnNewCanvasOpening.Invoke(gameOverCanvas);

    //    // gameplayManagementCanvas.SetActive(true);
    //    //blackPanel.SetActive(false);
    //    //respawnUI.SetActive(false);
    //}

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
    }

    public void ToggleMusic()
    {
        SoundManager.Instance.ToggleMusic();
    }

    public void SelectCharacter()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void RateApp()
    {
        Utilities.RateApp();
    }

    public void OpenXPage()
    {
        Utilities.OpenXPage();
    }

    public void OpenGmailPage()
    {
        Utilities.OpenGmailPage();
    }

    public void OpenTelegramPage()
    {
        Utilities.OpenTelegramPage();
    }

    public void OpenFacebookPage()
    {
        Utilities.OpenFacebookPage();
    }

    public void ButtonClickSound()
    {
        Utilities.ButtonClickSound();
    }

    void UpdateMuteButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            soundOnBtn.gameObject.SetActive(false);
            soundOffBtn.gameObject.SetActive(true);
        }
        else
        {
            soundOnBtn.gameObject.SetActive(true);
            soundOffBtn.gameObject.SetActive(false);
        }
    }

    void UpdateMusicButtons()
    {
        if (SoundManager.Instance.IsMusicOff())
        {
            musicOffBtn.gameObject.SetActive(true);
            musicOnBtn.gameObject.SetActive(false);
        }
        else
        {
            musicOffBtn.gameObject.SetActive(false);
            musicOnBtn.gameObject.SetActive(true);
        }
    }

    //bool IsPremiumFeaturesEnabled()
    //{
    //    return PremiumFeaturesManager.Instance != null && PremiumFeaturesManager.Instance.enablePremiumFeatures;
    //}

    //public void HideCountdown()
    //{
    //    EventManager.OnNewCanvasOpening.Invoke(gameplayCanvas);
    //}

    public void RemoveAdds()
    {
        EventManager.OnNewCanvasOpening.Invoke(storeCanvas);
    }
}
