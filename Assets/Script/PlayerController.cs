using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Hareket Sınırları")]
    [SerializeField] private float minXX; // Minimum X pozisyonu (oyuncu X=1'in altına gidemez)
    [SerializeField] private float maxXX; // Maksimum X pozisyonu
    [SerializeField] private float minYY; // Minimum Y pozisyonu (aşağı sınır)
    [SerializeField] private float maxYY; // Maksimum Y pozisyonu (yukarı sınır)
    
    [Header("Bileşenler")]
    private Rigidbody2D rb;
    public ShadowController shadowController;
    private AnimationChar animationChar;
    
    [Header("Oyun Durumu")]
    [SerializeField] private int lifes = 3;
    [SerializeField] private bool isAlive = true;
    
    // Başlangıç can değeri (reset için)
    private int maxLifes;
    
    [Header("Hasar Efekti")]
    [SerializeField] private Color damageColor = Color.red; // Hasar aldığında renk
    [SerializeField] private float colorChangeDuration = 0.3f; // Renk değişim süresi
    [SerializeField] private float damageCooldown = 1f; // Can kaybetme arasındaki gecikme (saniye)
    
    // Efekt değişkenleri
    private Vector3 originalPosition;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private bool isShaking = false;
    private bool isColorChanging = false;
    private float colorTimer = 0f;
    private float lastDamageTime = 0f; // Son hasar alınan zaman
    
    private Vector2 moveInput;
    private string idleInfo;

    void Start()
    {
        // Gerekli bileşenleri al
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController bir Rigidbody2D bileşeni gerektiriyor!");
        }

        // Sahnedeki gölge kontrolcüsünü bul
        shadowController = FindObjectOfType<ShadowController>();
        if (shadowController == null)
        {
            Debug.LogWarning("Sahnede ShadowController bulunamadı!");
        }
        else
        {
            // Gölgeye hareket sınırlarını gönder
            shadowController.SetMovementBoundsFromPlayer(minXX, maxXX, minYY, maxYY);
        }
        animationChar = GetComponent<AnimationChar>();
        if (animationChar == null)
        {
            Debug.LogWarning("Sahnede AnimationChar bulunamadı!");
        }
        
        // Hasar efekti için bileşenleri al
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        originalPosition = transform.position;
        
        // Başlangıç can değerini kaydet
        maxLifes = lifes;
    }
    
    void Update()
    {
        // Efektleri güncelle
        UpdateDamageEffects();
        
        // Sadece oyuncu hayattaysa hareket et
        if (!isAlive) return;
        
        HandleInput();
        SetAnimationInfo();
        HandleMovement();
    }
    
    private void HandleInput()
    {
        // WASD veya ok tuşlarından giriş al
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        
        // Çapraz hareketi normalize et (daha hızlı hareket etmeyi önlemek için)
        moveInput = moveInput.normalized;
    }
    private void SetAnimationInfo()
    {
        if (animationChar != null && animationChar.animator != null)
        {
            if (moveInput.x != 0 || moveInput.y != 0)
            {
                if (Math.Abs(moveInput.x) > Math.Abs(moveInput.y))
                {
                    if (moveInput.x > 0)
                    {
                        animationChar.CharAnimationWalkSide();
                        gameObject.transform.localScale = new Vector3(1, 1, 1);
                        idleInfo = "Right";
                        shadowController.SetAnimationInfoShadow("Right");
                    }
                    else if (moveInput.x < 0)
                    {
                        animationChar.CharAnimationWalkSide();
                        gameObject.transform.localScale = new Vector3(-1, 1, 1);
                        idleInfo = "Left";
                        shadowController.SetAnimationInfoShadow("Left");
                    }
                }
                else
                {
                    if (moveInput.y > 0)
                    {
                        animationChar.CharAnimationWalk();
                        gameObject.transform.localScale = new Vector3(1, 1, 1);
                        idleInfo = "Up";
                        shadowController.SetAnimationInfoShadow("Up");
                    }
                    else if (moveInput.y < 0)
                    {
                        animationChar.CharAnimationWalkFront();
                        gameObject.transform.localScale = new Vector3(1, 1, 1);
                        idleInfo = "Down";
                        shadowController.SetAnimationInfoShadow("Down");
                    }
                }
            }
            else
            {
                if (idleInfo == "Right")
                {
                    animationChar.CharAnimationIdleSide();
                    gameObject.transform.localScale = new Vector3(1, 1, 1);
                    shadowController.SetAnimationInfoShadow("IdleRight");
                }
                else if (idleInfo == "Left")
                {
                    animationChar.CharAnimationIdleSide();
                    gameObject.transform.localScale = new Vector3(-1, 1, 1);
                    shadowController.SetAnimationInfoShadow("IdleLeft");
                }
                else if (idleInfo == "Up")
                {
                    animationChar.CharAnimationIdle();
                    gameObject.transform.localScale = new Vector3(1, 1, 1);
                    shadowController.SetAnimationInfoShadow("IdleUp");
                }
                else if (idleInfo == "Down")
                {
                    animationChar.CharAnimationIdleFront();
                    gameObject.transform.localScale = new Vector3(1, 1, 1);
                    shadowController.SetAnimationInfoShadow("IdleDown");
                }
            }
            
        }
    }
    private void HandleMovement()
    {
        // Hareket vektörünü hesapla
        Vector2 velocity = moveInput * moveSpeed;
        
        // Velocity'yi uygula
        rb.velocity = velocity;
        
        // Basit pozisyon kısıtlaması (her frame sonrasında)
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minXX, maxXX);
        pos.y = Mathf.Clamp(pos.y, minYY, maxYY);
        transform.position = pos;
        
        // Gölgeyi güncellenmiş pozisyonla haberdar et (gecikme olmadan)
        if (shadowController != null)
        {
            shadowController.UpdateShadowPosition(pos.x, pos.y);
        }
    }
    
    //Oyuncunun canını azaltmak için metod
    public void ReduceLife()
    {
        if (!isAlive) return;
        
        // Damage cooldown kontrolü
        if (Time.time - lastDamageTime < damageCooldown)
        {
            Debug.Log("Hasar cooldown aktif - can azalmadı");
            return; // Henüz gecikme süresi geçmemiş
        }
        
        lifes--;
        lastDamageTime = Time.time; // Son hasar zamanını güncelle
        Debug.Log($"Oyuncu canı azaldı! Kalan can: {lifes}");
        
        // Hasar efektini başlat
        StartDamageEffect();
        
        if (lifes <= 0)
        {
            Die();
        }
    }
    
    // Oyuncuyu öldürmek için genel metod
    public void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        rb.velocity = Vector2.zero; // Hareketi durdur

        Debug.Log("Oyuncu öldü!");

        // GameManager'ı bilgilendir
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnPlayerDeath();
        }
    }
    
    // Oyuncu durumunu sıfırlamak için genel metod
    public void ResetPlayer()
    {
        isAlive = true;
        rb.velocity = Vector2.zero;
        moveInput = Vector2.zero;
        
        // Can değerini başlangıç değerine sıfırla
        lifes = maxLifes; // Başlangıçta ayarlanan can değeri
        lastDamageTime = 0f; // Damage cooldown'u sıfırla
        
        // Efektleri durdur
        StopAllDamageEffects();
        
        Debug.Log($"Oyuncu resetlendi! Can: {lifes}");
    }
    
    // Canlı durumu için getter
    public bool IsAlive => isAlive;
    
    // Hasar efektini başlat
    private void StartDamageEffect()
    {
        // Titreme efektini başlat
        isShaking = true;
        originalPosition = transform.position;
        
        // Renk değişimi efektini başlat
        if (spriteRenderer != null)
        {
            isColorChanging = true;
            colorTimer = colorChangeDuration;
        }
    }
    
    // Hasar efektlerini güncelle
    private void UpdateDamageEffects()
    {
        UpdateColorEffect();
    }
    
    // Titreme efektini güncelle

    
    // Renk efektini güncelle
    private void UpdateColorEffect()
    {
        if (!isColorChanging || spriteRenderer == null) return;
        
        colorTimer -= Time.deltaTime;
        
        if (colorTimer > 0)
        {
            // Orijinal renk ile hasar rengi arasında geçiş
            float t = 1f - (colorTimer / colorChangeDuration);
            Color lerpedColor = Color.Lerp(damageColor, originalColor, t);
            spriteRenderer.color = lerpedColor;
        }
        else
        {
            // Renk efekti bitti, orijinal renge dön
            spriteRenderer.color = originalColor;
            isColorChanging = false;
        }
    }
    
    // Tüm hasar efektlerini durdur (reset için)
    private void StopAllDamageEffects()
    {
        // Titreme efektini durdur
        if (isShaking)
        {
            isShaking = false;
            transform.position = originalPosition;
        }
        
        // Renk efektini durdur
        if (isColorChanging && spriteRenderer != null)
        {
            isColorChanging = false;
            spriteRenderer.color = originalColor;
        }
        
        // Timer'ları sıfırla
        colorTimer = 0f;
    }
    
}