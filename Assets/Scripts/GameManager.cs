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

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text finalScoreText;

    public GameState CurrentState { get; private set; }
    public int Score { get; private set; }

    public bool IsPlaying =>
        CurrentState == GameState.Playing;

    public bool IsPaused =>
        CurrentState == GameState.Paused;

    public bool HasGameEnded =>
        CurrentState ==
            GameState.GameOver ||
        CurrentState ==
            GameState.Completed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Important when returning from a paused scene reload.
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
        if (CurrentState != GameState.Ready)
        {
            return;
        }

        CurrentState = GameState.Playing;
        Score = 0;

        Time.timeScale = 1f;

        UpdateScoreUI();

        SetPanelActive(startPanel, false);
        SetPanelActive(hudPanel, true);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);

        if (bird != null)
        {
            bird.BeginGame();
        }

        if (backgroundStageManager != null)
        {
            backgroundStageManager
                .BeginProgression();
        }

        StartCoroutine(
            BeginEnemySpawningAfterIntro()
        );

    }

    public void AddScore(int amount)
    {
        if (CurrentState != GameState.Playing)
        {
            return;
        }

        Score += amount;

        UpdateScoreUI();
        
    }

    public void PauseGame()
    {
        if (!IsPlaying)
        {
            return;
        }

        CurrentState = GameState.Paused;

        // Disable the Gameplay/Flap action before freezing time.
        if (bird != null)
        {
            bird.PauseInput();
        }

        SetPanelActive(hudPanel, false);
        SetPanelActive(pausePanel, true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!IsPaused)
        {
            return;
        }

        // Restore gameplay time before accepting input.
        Time.timeScale = 1f;
        CurrentState = GameState.Playing;

        if (bird != null)
        {
            bird.ResumeInput();
        }

        SetPanelActive(pausePanel, false);
        SetPanelActive(hudPanel, true);
    }

    public void GameOver()
    {
        if (!IsPlaying)
        {
            return;
        }

        CurrentState = GameState.GameOver;

        if (backgroundStageManager != null)
        {
            backgroundStageManager
                .StopProgression();
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

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Score: {Score}";
        }

        Time.timeScale = 0f;
    }

    public void RetryGame()
    {
        // Always restore normal time before loading the scene.
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
        if (
            CurrentState !=
            GameState.Playing
        )
        {
            return;
        }

        CurrentState = GameState.Completed;

        if (backgroundStageManager != null)
        {
            backgroundStageManager
                .StopProgression();
        }

        if (crowSpawner != null)
        {
            crowSpawner.StopSpawning();
        }

        if (bird != null)
        {
            bird.StopBird();
        }

        SetPanelActive(
            hudPanel,
            false
        );

        SetPanelActive(
            pausePanel,
            false
        );

        SetPanelActive(
            gameOverPanel,
            true
        );

        if (finalScoreText != null)
        {
            finalScoreText.text =
                $"FLIGHT COMPLETE\nScore: {Score}";
        }

        Debug.Log(
            $"GAME COMPLETED AT SCORE {Score}"
        );

        Time.timeScale = 0f;
    }

    private void OnApplicationPause(bool applicationPaused)
    {
        if (applicationPaused && IsPlaying)
        {
            PauseGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && IsPlaying)
        {
            PauseGame();
        }
    }

    private static void SetPanelActive(
        GameObject panel,
        bool active
    )
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}