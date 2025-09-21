using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Kamera Ayarları")]
    [SerializeField] private Camera mainCamera;

    [Header("Telefon Ekranı Ayarları")]
    [SerializeField] private float targetAspectRatio; // Telefon ekranı oranı (manuel ayar - örn: 0.75 = 9:12, 0.5625 = 9:16)
    [SerializeField] private bool forceAspectRatio = true;
    [SerializeField] private float cameraSize = 5f; // Kamera boyutu (kendin ayarlayabilirsin)
    
    [Header("Takip Ayarları")]
    [SerializeField] private Transform playerTarget; // Takip edilecek oyuncu
    [SerializeField] private float followSpeed = 5f; // Kamera takip hızı
    
    [Header("Y Ekseni Hareket Sınırları")]
    [SerializeField] private float minY = -5f; // Kameranın en aşağı gidebileceği Y pozisyonu
    [SerializeField] private float maxY = 5f;  // Kameranın en yukarı gidebileceği Y pozisyonu
    [SerializeField] private float fixedX = 0f; // X pozisyonu sabit kalacak
    [SerializeField] private float fixedZ = -10f; // Z pozisyonu sabit kalacak
    
    private Vector3 velocity = Vector3.zero; // SmoothDamp için
    
    void Start()
    {
        // Ana kamerayı al
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = GetComponent<Camera>();
            }
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: Kamera bulunamadı!");
            return;
        }
        
        // Oyuncuyu otomatik bul
        if (playerTarget == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }
        
        // Kamera boyutunu ayarla
        mainCamera.orthographicSize = cameraSize;
        
        // Telefon ekranı oranını ayarla
        SetupPhoneAspectRatio();
    }
    
    void FixedUpdate()
    {
        if (mainCamera == null || playerTarget == null) return;
        
        UpdateCameraPosition();
    }
    
    private void SetupPhoneAspectRatio()
    {
        if (!forceAspectRatio) return;
        
        // Mevcut ekran oranını al
        float currentAspectRatio = (float)Screen.width / Screen.height;
        
        // Hedef oran ile karşılaştır
        float scaleHeight = currentAspectRatio / targetAspectRatio;
        
        if (scaleHeight < 1.0f)
        {
            // Ekran daha dar - yan taraflardan kırp
            Rect rect = mainCamera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            mainCamera.rect = rect;
        }
        else
        {
            // Ekran daha geniş - üst/alt'tan kırp
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = mainCamera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            mainCamera.rect = rect;
        }
    }
    
    private void UpdateCameraPosition()
    {
        // Hedef Y pozisyonu (oyuncunun Y pozisyonu)
        float targetY = playerTarget.position.y;
        
        // Y pozisyonunu sınırlar içinde tut
        targetY = Mathf.Clamp(targetY, minY, maxY);
        
        // Hedef pozisyon (sadece Y değişir, X ve Z sabit)
        Vector3 targetPosition = new Vector3(fixedX, targetY, fixedZ);
        
        // SmoothDamp ile yumuşak takip (titreme önleme)
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);
        
        transform.position = newPosition;
    }
    
    // Kamera boyutunu değiştirme
    public void SetCameraSize(float newSize)
    {
        cameraSize = newSize;
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = cameraSize;
        }
    }
    
    // Y sınırlarını değiştirme
    public void SetYBounds(float min, float max)
    {
        minY = min;
        maxY = max;
    }
    
    // Takip hızını değiştirme
    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
    }
}