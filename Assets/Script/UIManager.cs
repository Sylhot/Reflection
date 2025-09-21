using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Game Objects to Control")]
    [SerializeField] private GameObject[] gameObjects; // Player, Shadow, Environment vs.
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private GameObject[] environmentObjects;
    [SerializeField] private GameObject[] guardObjects;
    
    [Header("Game State")]
    private bool gameStarted = false;
    private bool gamePaused = false;
    
    // Singleton pattern
    public static UIManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        ShowMainMenu();
    }
    
    void Update()
    {
        // ESC tuşu ile pause/resume
        if (gameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    private void InitializeUI()
    {
        // Başlangıçta oyun objelerini gizle
        SetGameObjectsActive(false);
        
        // Panel durumlarını ayarla
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
    
    private void SetupButtonListeners()
    {
        // Button listener'ları ekle
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(BackToMainMenu);
    }
    
    #region Game Control Methods
    public void StartGame()
    {
        gameStarted = true;
        mainMenuPanel.SetActive(false);
        
        // Oyun objelerini aktif et
        SetGameObjectsActive(true);
        
        // Oyunu başlat
        Time.timeScale = 1f;
        
        // Background müzik başlat
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBackgroundMusic();
        }
        
        Debug.Log("Oyun başlatıldı!");
    }
    
    public void PauseGame()
    {
        if (!gameStarted) return;
        
        gamePaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null) pausePanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        
        // Müziği duraklat
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PauseBackgroundMusic();
        }
        
        Debug.Log("Oyun duraklatıldı!");
    }
    
    public void ResumeGame()
    {
        if (!gameStarted) return;
        
        gamePaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        
        // Müziği devam ettir
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ResumeBackgroundMusic();
        }
        
        Debug.Log("Oyun devam ettiriliyor!");
    }
    
    public void RestartGame()
    {
        // Oyunu sıfırla
        Time.timeScale = 1f;
        gamePaused = false;
        
        // Scene'i yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void BackToMainMenu()
    {
        gameStarted = false;
        gamePaused = false;
        Time.timeScale = 1f;
        
        // Oyun objelerini gizle
        SetGameObjectsActive(false);
        
        // Ana menüyü göster
        ShowMainMenu();
        
        // Müziği durdur
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBackgroundMusic();
        }
        
        Debug.Log("Ana menüye dönüldü!");
    }
    
    public void QuitGame()
    {
        Debug.Log("Oyundan çıkılıyor...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion
    
    #region UI Panel Management
    private void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
    
    private void ShowGameUI()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
    
    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        
        gameStarted = false;
        
        // Game over sesi çal
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayGameOverSound();
        }
    }
    #endregion
    
    #region Game Objects Management
    private void SetGameObjectsActive(bool active)
    {
        // Player'ı aktif/deaktif et
        if (playerObject != null)
        {
            playerObject.SetActive(active);
        }
        
        // Shadow'u aktif/deaktif et
        if (shadowObject != null)
        {
            shadowObject.SetActive(active);
        }
        
        // Environment objelerini aktif/deaktif et
        if (environmentObjects != null)
        {
            foreach (GameObject env in environmentObjects)
            {
                if (env != null) env.SetActive(active);
            }
        }
        
        // Guard objelerini aktif/deaktif et
        if (guardObjects != null)
        {
            foreach (GameObject guard in guardObjects)
            {
                if (guard != null) guard.SetActive(active);
            }
        }
        
        // Genel game objects listesi
        if (gameObjects != null)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (obj != null) obj.SetActive(active);
            }
        }
    }
    #endregion
    
    #region Getters
    public bool IsGameStarted() => gameStarted;
    public bool IsGamePaused() => gamePaused;
    #endregion
}
