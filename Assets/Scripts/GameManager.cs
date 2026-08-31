using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Ready,
        Playing,
        Paused,
        WaitingForRevive,
        GameOver,
        Completed
    }

    [Header("Game Start Sequence")]
    [SerializeField] private float enemyStartDelay = 1.8f;

    [Header("Gameplay")]
    [SerializeField] private BirdController bird;
    [SerializeField] private CrowSpawner crowSpawner;
    [SerializeField] private BackgroundStageManager backgroundStageManager;

    [Header("UI Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject watchAdButton;
    [SerializeField] private GameObject storePanle;
    [SerializeField] private GameObject exitConfirmationModal;

    [Header("Managers")]
    [SerializeField] private StoreManager storeManager;

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text finalScoreText;

    public GameState CurrentState { get; private set; }
    public int Score { get; private set; }

    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool HasGameEnded => CurrentState == GameState.GameOver || CurrentState == GameState.Completed;
    public bool IsWaitingForRevive =>
    CurrentState == GameState.WaitingForRevive;

    // Track the coroutine reference so we can stop it safely on clean up
    private Coroutine introSpawnCoroutine;
    private const int MaxRevivesPerRun = 3;
    private int revivesUsed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        EnterReadyState();
    }

    private void EnterReadyState()
    {
        CurrentState = GameState.Ready;
        Score = 0;
        Time.timeScale = 1f;
        UpdateScoreUI();

        SetPanelActive(startPanel, true);
        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(helpPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(storePanle, false);
        SetPanelActive(exitConfirmationModal, false);
    }

    private IEnumerator BeginEnemySpawningAfterIntro()
    {
        yield return new WaitForSeconds(enemyStartDelay);

        if (CurrentState != GameState.Playing)
        {
            yield break;
        }

        if (crowSpawner != null)
        {
            crowSpawner.BeginSpawning();
        }
    }

    public void StartGame()
    {
        revivesUsed = 0;

        if (CurrentState != GameState.Ready) return;

        CurrentState = GameState.Playing;
        Score = 0;
        Time.timeScale = 1f;
        UpdateScoreUI();

        SetPanelActive(startPanel, false);
        SetPanelActive(hudPanel, true);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(helpPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(storePanle, false);
        SetPanelActive(exitConfirmationModal, false);

        if (bird != null) bird.BeginGame();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartMusic();
        }

        if (backgroundStageManager != null)
        {
            backgroundStageManager.BeginProgression();
        }

        // Store reference to clean it up safely later
        introSpawnCoroutine = StartCoroutine(BeginEnemySpawningAfterIntro());
    }

    public void OpenHelpPanel() => SetPanelActive(helpPanel, true);
    public void CloseHelpPanel() => SetPanelActive(helpPanel, false);
    public void OpenSettingsPanel() => SetPanelActive(settingsPanel, true);
    public void CloseHSettingsPanel() => SetPanelActive(settingsPanel, false);

    public void OpenStorePanel()
    {
        if (storeManager != null)
        {
            storeManager.Refresh();
        }

        SetPanelActive(storePanle, true);
        SetPanelActive(startPanel, false);
    }
    
    public void CloseStorePanel() 
    {
        SetPanelActive(startPanel, true);
        SetPanelActive(storePanle, false);
    }

    public void ExitConfirmedYes()
    {
        // If running inside the Unity Editor, stop play mode
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If running as a built standalone game, close the application
            Application.Quit();
        #endif
    }

    public void ExitConfirmedNo()
    {
        SetPanelActive(exitConfirmationModal, false);
    }

    public void ExitGame()
    {
        SetPanelActive(exitConfirmationModal, true);
    }

    public void HandleBirdOutOfLives()
    {
        if (!IsPlaying)
        {
            return;
        }

        if (revivesUsed < MaxRevivesPerRun)
        {
            ShowReviveOpportunity();
            return;
        }

        GameOver();
    }

    private void UpdateAdWatchButton()
    {
        if ( watchAdButton != null )
        {
            bool canRevive = revivesUsed < MaxRevivesPerRun;

            watchAdButton.SetActive(canRevive);
        }
    }

    private void SetCurrentRunScore()
    {
        if (finalScoreText != null) finalScoreText.text = $"Score: {Score}";
    }

    private void ShowReviveOpportunity()
    {
        CurrentState = GameState.WaitingForRevive;

        if (backgroundStageManager != null)
        {
            backgroundStageManager.StopProgression();
        }

        if (crowSpawner != null)
        {
            crowSpawner.StopSpawning();
        }

        if (bird != null)
        {
            bird.StopBird();
        }

        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, true);

        SetCurrentRunScore();

        UpdateAdWatchButton();

        Time.timeScale = 0f;
    }

    public void ReviveAfterRewardedAd()
    {
        if (CurrentState != GameState.WaitingForRevive)
        {
            return;
        }

        if (revivesUsed >= MaxRevivesPerRun)
        {
            return;
        }

        revivesUsed++;

        if (bird != null)
        {
            bird.AddLife();
            bird.ResumeAfterRevive();
        }

        CurrentState = GameState.Playing;

        Time.timeScale = 1f;

        if (backgroundStageManager != null)
        {
            backgroundStageManager.BeginProgression();
        }

        if (crowSpawner != null)
        {
            crowSpawner.BeginSpawning();
        }

        SetPanelActive(gameOverPanel, false);
        SetPanelActive(hudPanel, true);
    }

        public void AddScore(int amount)
        {
            if (CurrentState != GameState.Playing) return;

            Score += amount;
            UpdateScoreUI();
        }

    public void PauseGame()
    {
        if (!IsPlaying) return;

        CurrentState = GameState.Paused;

        if (bird != null) bird.PauseInput();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
        }

        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;

        Time.timeScale = 1f;
        CurrentState = GameState.Playing;

        if (bird != null) bird.ResumeInput();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        SetPanelActive(pausePanel, false);
        SetPanelActive(hudPanel, true);
    }

    public void GameOver()
    {
        if (!IsPlaying) return;

        CurrentState = GameState.GameOver;

        // Clean up active coroutines to prevent background spawn leak issues
        if (introSpawnCoroutine != null) StopCoroutine(introSpawnCoroutine);

        if (backgroundStageManager != null) backgroundStageManager.StopProgression();
        if (crowSpawner != null) crowSpawner.StopSpawning();
        if (bird != null) bird.StopBird();

        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, true);

        UpdateAdWatchButton();

        SetCurrentRunScore();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveBestScore(Score);
            
            int newTotalScoreAmount = SaveManager.Instance.TotalScoreAmount + Score;

            SaveManager.Instance.SetTotalScoreAmount(newTotalScoreAmount);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        Time.timeScale = 0f;
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = Score.ToString();
        }
    }

    public void CompleteGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Completed;

        // Clean up active coroutines
        if (introSpawnCoroutine != null) StopCoroutine(introSpawnCoroutine);

        if (backgroundStageManager != null) backgroundStageManager.StopProgression();
        if (crowSpawner != null) crowSpawner.StopSpawning();
        if (bird != null) bird.StopBird();

        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, true);

        if (finalScoreText != null)
        {
            finalScoreText.text = $"FLIGHT COMPLETE\nScore: {Score}";
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveBestScore(Score);

            int newTotalScoreAmount =
                SaveManager.Instance.TotalScoreAmount + Score;

            SaveManager.Instance.SetTotalScoreAmount(newTotalScoreAmount);
        }

        Time.timeScale = 0f;
    }

    private void OnApplicationPause(bool applicationPaused)
    {
        if (applicationPaused && IsPlaying) PauseGame();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && IsPlaying) PauseGame();
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
