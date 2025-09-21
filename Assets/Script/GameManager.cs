using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Yeniden Doğum Ayarları")]
    [SerializeField] private bool respawnAtStartPoint = true; // true = startPoint'e git, false = scene restart
    [SerializeField] private float respawnDelay = 1f; // Yeniden doğum gecikmesi
    [SerializeField] private float restartDelay = 2f; // Seviyeyi yeniden başlatmadan önceki gecikme
    [SerializeField] private bool showDebugMessages = true;
    
    [Header("Oyuncu Referansları")]
    private PlayerController player;
    private ShadowController shadow;
    
    [Header("Oyun Durumu")]
    [SerializeField] private bool gameOver = false;
    [SerializeField] private bool levelComplete = false;
    [SerializeField] private Transform playerStartPoint;
    
    // Kolay erişim için Singleton deseni
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton kurulumu
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
        InitializeGame();
        
        // PlayerStartPoint'i bul
        GameObject startPointObj = GameObject.Find("PlayerStartPoint");
        if (startPointObj != null)
        {
            playerStartPoint = startPointObj.transform;
            if (showDebugMessages)
            {
                Debug.Log("PlayerStartPoint bulundu: " + playerStartPoint.position);
            }
        }
        else
        {
            // PlayerStartPoint bulunamazsa player'ın mevcut pozisyonunu kullan
            if (player != null)
            {
                playerStartPoint = player.transform;
                if (showDebugMessages)
                {
                    Debug.LogWarning("PlayerStartPoint bulunamadı! Player'ın mevcut pozisyonu start point olarak ayarlandı.");
                }
            }
            else
            {
                Debug.LogError("Ne PlayerStartPoint ne de Player bulunamadı!");
            }
        }
    }
    
    void Update()
    {
        // Oyun bittiğinde yeniden başlatma girişini kontrol et
        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
        
        // Çıkış girişini kontrol et
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }
    
    private void InitializeGame()
    {
        // Oyuncu ve gölge kontrolcülerini bul
        player = FindObjectOfType<PlayerController>();
        shadow = FindObjectOfType<ShadowController>();
        
        if (player == null)
        {
            Debug.LogError("GameManager: Sahnede PlayerController bulunamadı!");
        }
        
        if (shadow == null)
        {
            Debug.LogError("GameManager: Sahnede ShadowController bulunamadı!");
        }
        
        // Oyun durumunu sıfırla
        gameOver = false;
        levelComplete = false;
        
        if (showDebugMessages)
        {
            Debug.Log("Oyun başlatıldı. WASD/Ok tuşlarını kullanarak hareket et. Hem oyuncu hem de gölge hayatta kalmalı!");
        }
    }
    
    // Oyuncu öldüğünde çağrılır
    public void OnPlayerDeath()
    {
        if (gameOver || levelComplete) return;
        
        // Oyunu 3 saniye durdur ve restart yap
        StartCoroutine(PlayerDeathSequence());
    }
    
    private System.Collections.IEnumerator PlayerDeathSequence()
    {
        if (showDebugMessages)
        {
            Debug.Log("Player öldü! Oyun 3 saniye duruyor...");
        }
        
        // Oyunu durdur
        Time.timeScale = 0f;
        
        // Player hit sesi çal
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerHitSound();
        }
        
        // 3 saniye bekle (scaled time kullanmadan)
        yield return new WaitForSecondsRealtime(3f);
        
        // Oyunu tekrar başlat
        Time.timeScale = 1f;
        
        if (showDebugMessages)
        {
            Debug.Log("Oyun yeniden başlatılıyor...");
        }
        
        // UIManager üzerinden restart yap
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RestartGame();
        }
        else
        {
            // Fallback: Eski sistem
            RestartGameCompletely();
        }
    }
    
    private void RestartGameCompletely()
    {
        // Tüm mermileri yok et
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }
        
        // Tüm dinamik engelleri yok et (ShadowController'ın oluşturduğu)
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("PlayerObstacle");
        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
        
        // Player'ı başlangıç pozisyonuna koy
        if (player != null && playerStartPoint != null)
        {
            player.transform.position = playerStartPoint.position;
            
            // Player'ın canını yenile
            if (player.GetComponent<PlayerController>() != null)
            {
                // Player'ın can sistemini reset et (eğer varsa)
                player.GetComponent<PlayerController>().ResetPlayer();
            }
        }
        
        // Shadow'u da reset et
        if (shadow != null)
        {
            shadow.ResetShadow();
        }
        
        // Oyun durumunu sıfırla
        gameOver = false;
        levelComplete = false;
        
        if (showDebugMessages)
        {
            Debug.Log("Oyun tamamen yeniden başlatıldı!");
        }
    }
    
    // Gölge öldüğünde çağrılır
    public void OnShadowDeath()
    {
        if (gameOver || levelComplete) return;
        
        TriggerGameOver("Gölge yok edildi!");
    }
    
    // Hem oyuncu hem de gölge çıkışa ulaştığında çağrılır
    public void OnLevelComplete()
    {
        if (gameOver || levelComplete) return;
        
        levelComplete = true;
        
        if (showDebugMessages)
        {
            Debug.Log("Seviye Tamamlandı! Hem oyuncu hem de gölge çıkışa ulaştı!");
        }
        
        // Burada bir sonraki seviyeyi yükleyebilir, zafer ekranı gösterebilirsin, vs.
        // Şimdilik gecikme sonrasında yeniden başlatacağız
        Invoke(nameof(RestartLevel), restartDelay);
    }
    
    private void TriggerGameOver(string reason)
    {
        gameOver = true;
        
        if (showDebugMessages)
        {
            Debug.Log($"Oyun Bitti: {reason}");
            Debug.Log("Yeniden başlatmak için R'ye, çıkmak için ESC'ye bas.");
        }
        
        // Gecikme sonrasında otomatik yeniden başlat
        Invoke(nameof(RestartLevel), restartDelay);
    }
    
    public void RestartLevel()
    {
        // Bekleyen invoke'ları iptal et
        CancelInvoke();
        
        if (showDebugMessages)
        {
            Debug.Log("Seviye yeniden başlatılıyor...");
        }
        
        // Mevcut sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        if (showDebugMessages)
        {
            Debug.Log("Oyundan çıkılıyor...");
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Oyuncuyu startPoint'e yeniden doğur
    private void RespawnPlayer()
    {
        if (showDebugMessages)
        {
            Debug.Log($"Oyuncu {respawnDelay} saniye sonra yeniden doğuyor...");
        }
        
        // Gecikme ile yeniden doğur
        Invoke(nameof(ExecuteRespawn), respawnDelay);
    }
    
    // Sadece player'ı start pozisyonuna geri döndüren restart fonksiyonu
    public void Restart()
    {
        if (showDebugMessages)
        {
            Debug.Log("Player start pozisyonuna geri dönüyor...");
        }
        
        // Player'ı start pozisyonuna geri döndür
        if (player != null)
        {
            if (playerStartPoint != null)
            {
                player.transform.position = playerStartPoint.position;
                
                if (showDebugMessages)
                {
                    Debug.Log("Player start pozisyonuna taşındı: " + playerStartPoint.position);
                }
            }
            else
            {
                Debug.LogWarning("PlayerStartPoint bulunamadı! Player pozisyonu değiştirilmedi.");
            }
        }
        else
        {
            Debug.LogError("Player bulunamadı!");
        }
    }
    
    private void ExecuteRespawn()
    {
        if (player != null && playerStartPoint != null)
        {
            // Oyuncuyu startPoint pozisyonuna taşı
            player.transform.position = playerStartPoint.position;
            
            // Oyuncuyu resetle
            player.ResetPlayer();
            
            // Gölgeyi de resetle
            if (shadow != null)
            {
                shadow.ResetShadow();
            }
            
            if (showDebugMessages)
            {
                Debug.Log("Oyuncu yeniden doğdu!");
            }
        }
        else
        {
            Debug.LogError("Player veya PlayerStartPoint bulunamadı! Scene restart yapılıyor...");
            RestartLevel();
        }
    }
    
    // Oyun durumu için genel getter'lar
    public bool IsGameOver => gameOver;
    public bool IsLevelComplete => levelComplete;
    
    // Hem oyuncu hem de gölgenin hayatta olup olmadığını kontrol etmek için metod
    public bool AreBothAlive()
    {
        if (player == null || shadow == null) return false;
        return player.IsAlive && shadow.IsAlive;
    }
    
    // Hem oyuncu hem de gölgeyi sıfırlamak için metod (kontrol noktaları için faydalı)
    public void ResetPlayersToCheckpoint()
    {
        if (player != null) player.ResetPlayer();
        if (shadow != null) shadow.ResetShadow();
        
        gameOver = false;
        levelComplete = false;
    }
}
