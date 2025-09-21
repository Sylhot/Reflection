using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    [Header("Çıkış Ayarları")]
    [SerializeField] private bool requireBothToWin = true; // Kazanmak için hem oyuncu hem de gölge çıkışa ulaşmalı
    [SerializeField] private bool showDebugMessages = true;
    
    [Header("Tetikleme Durumu")]
    [SerializeField] private bool playerInExit = false;
    [SerializeField] private bool shadowInExit = false;
    
    // Görsel geri bildirim (opsiyonel)
    [Header("Görsel Geri Bildirim")]
    [SerializeField] private GameObject playerExitIndicator;
    [SerializeField] private GameObject shadowExitIndicator;
    
    void Start()
    {
        // Görsel göstergeleri başlat
        if (playerExitIndicator != null)
            playerExitIndicator.SetActive(false);
            
        if (shadowExitIndicator != null)
            shadowExitIndicator.SetActive(false);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Oyuncunun girip girmediğini kontrol et
        if (other.CompareTag("Player"))
        {
            playerInExit = true;
            
            if (showDebugMessages)
                Debug.Log("Oyuncu çıkışa ulaştı!");
                
            if (playerExitIndicator != null)
                playerExitIndicator.SetActive(true);
                
            CheckWinCondition();
        }
        
        // Gölgenin girip girmediğini kontrol et
        else if (other.CompareTag("Shadow"))
        {
            shadowInExit = true;
            
            if (showDebugMessages)
                Debug.Log("Gölge çıkışa ulaştı!");
                
            if (shadowExitIndicator != null)
                shadowExitIndicator.SetActive(true);
                
            CheckWinCondition();
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // Oyuncunun ayrılıp ayrılmadığını kontrol et
        if (other.CompareTag("Player"))
        {
            playerInExit = false;
            
            if (showDebugMessages)
                Debug.Log("Oyuncu çıkış alanından ayrıldı.");
                
            if (playerExitIndicator != null)
                playerExitIndicator.SetActive(false);
        }
        
        // Gölgenin ayrılıp ayrılmadığını kontrol et
        else if (other.CompareTag("Shadow"))
        {
            shadowInExit = false;
            
            if (showDebugMessages)
                Debug.Log("Gölge çıkış alanından ayrıldı.");
                
            if (shadowExitIndicator != null)
                shadowExitIndicator.SetActive(false);
        }
    }
    
    private void CheckWinCondition()
    {
        // Kazanma koşulunun karşılanıp karşılanmadığını kontrol et
        bool canWin = false;
        
        if (requireBothToWin)
        {
            // Her ikisi de çıkış alanında olmalı
            canWin = playerInExit && shadowInExit;
        }
        else
        {
            // Oyuncu veya gölgenin çıkışa ulaşması yeterli
            canWin = playerInExit || shadowInExit;
        }
        
        if (canWin)
        {
            // Ek kontrol: her ikisinin de hala hayatta olduğundan emin ol
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.AreBothAlive())
            {
                TriggerLevelComplete();
            }
            else if (showDebugMessages)
            {
                Debug.Log("Seviye tamamlanamıyor - karakterlerden biri ölü!");
            }
        }
    }
    
    private void TriggerLevelComplete()
    {
        if (showDebugMessages)
        {
            Debug.Log("Seviye tamamlandı! Hem oyuncu hem de gölge güvende!");
        }
        
        // GameManager'ı bilgilendir
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnLevelComplete();
        }
    }
    
    // Mevcut çıkış durumunu kontrol etmek için genel metod
    public bool IsPlayerInExit() => playerInExit;
    public bool IsShadowInExit() => shadowInExit;
    public bool AreBothInExit() => playerInExit && shadowInExit;
    
    // Seviye tamamlamayı manuel olarak tetiklemek için metod (test için faydalı)
    [ContextMenu("Seviye Tamamlamayı Zorla")]
    public void ForceLevelComplete()
    {
        TriggerLevelComplete();
    }
    
    // Çıkış gereksinimlerini ayarlamak için metod (farklı seviye türleri için faydalı)
    public void SetRequireBothToWin(bool requireBoth)
    {
        requireBothToWin = requireBoth;
    }
}