using System;
using UnityEngine;

public class ShadowController : MonoBehaviour
{
    [Header("Gölge Ayarları")]
    [SerializeField] private float shadowOffsetY = 0f; // Gölge konumlandırması için Y ofseti
    [SerializeField] private float xInversionScale = 1f; // X ekseninde ters çevirme oranı (1 = tam ters)
    [SerializeField] private float xMovementRatio; // Player hareket oranı (0.5 = player 5 hareket ederse shadow 2.5 hareket eder)
    [SerializeField] private float mirrorPositionX; // Aynanın X pozisyonu (yansıma merkezi)

    [Header("Ayna Scale Ayarları")]
    [SerializeField] private float minScale = 0.2f; // En küçük scale oranı (uzakken)
    [SerializeField] private float maxScale = 1f; // En büyük scale oranı (yakınken)
    [SerializeField] private float scaleTransitionSpeed = 2f; // Scale değişim hızı (smooth geçiş için)
    [SerializeField, Range(0f, 1f)] private float currentDistanceRatio = 0f; // Mevcut mesafe oranı (sadece gösterim)
    [SerializeField] private float currentDistanceValue = 0f; // Gerçek mesafe değeri (sadece gösterim)    

    [Header("Player Scale Referansı")]
    [SerializeField] private float playerMinXForScale = 1f;  // Scale hesabı için player min X
    [SerializeField] private float playerMaxXForScale = 10f; // Scale hesabı için player max X
    [SerializeField] private float minX = -10f; // Gölge minimum X pozisyonu (manuel ayar)
    [SerializeField] private float maxX = 10f;  // Gölge maksimum X pozisyonu (manuel ayar)
    [SerializeField] private float minY = -5f;  // Gölge minimum Y pozisyonu (manuel ayar)
    [SerializeField] private float maxY = 5f;   // Gölge maksimum Y pozisyonu (manuel ayar)

    [Header("Bileşenler")]
    private Rigidbody2D rb;
    // Yumuşak hareket için hedef pozisyon
    private float targetX;
    private float targetY;
    private Vector2 startPosition;
    private float playerStartX; // Oyuncunun başlangıç X pozisyonu
    private AnimationChar animationChar;
    private PlayerController playerController; // Player referansı
    public float valueYOffset;
    public float currentScaleX;
    private float currentScale; // Holds the current scale value for smooth scaling
    public GameObject colliderObject;
    [Header("Oyun Durumu")]
    [SerializeField] private bool isAlive = true;

    void Start()
    {
        colliderObject = GameObject.FindGameObjectWithTag("ColliderObject");
        animationChar = GetComponent<AnimationChar>();
        if (animationChar == null)
        {
            Debug.LogWarning("Sahnede AnimationChar bulunamadı!");
        }
        // Gerekli bileşenleri al
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("ShadowController bir Rigidbody2D bileşeni gerektiriyor!");
        }

        // Oyuncunun başlangıç X pozisyonunu al
        PlayerController player = FindObjectOfType<PlayerController>();
        playerController = player; // Referansı sakla
        if (player != null)
        {
            playerStartX = player.transform.position.x;

            // Shadow'un GERÇEK yansıma pozisyonunu hesapla
            float playerDistanceFromMirror = playerStartX - mirrorPositionX;
            float correctShadowX = mirrorPositionX - (playerDistanceFromMirror * xInversionScale * xMovementRatio);

            // Shadow'u tamamen doğru pozisyona taşı
            Vector3 correctPosition = new Vector3(correctShadowX, player.transform.position.y, transform.position.z);
            transform.position = correctPosition;

            // Yeni pozisyonu başlangıç pozisyonu olarak kaydet
            startPosition = correctPosition;
            targetX = correctShadowX;
            targetY = player.transform.position.y;
        }
        else
        {
            playerStartX = 0f; // Varsayılan değer
            Debug.LogWarning("PlayerController bulunamadı, playerStartX varsayılan değere ayarlandı!");
        }

        // Gölge davranışı için Rigidbody2D'yi ayarla
        if (rb != null)
        {
            rb.gravityScale = 0f; // Gölge yerçekimi nedeniyle düşmemeli
            rb.freezeRotation = true; // Gölgeyi dik tut
            rb.bodyType = RigidbodyType2D.Kinematic; // Trigger collision için kinematic olmalı
        }

        // Collider2D'nin trigger olduğunu kontrol et
        Collider2D shadowCollider = GetComponent<Collider2D>();
        if (shadowCollider != null)
        {
            shadowCollider.isTrigger = true; // Trigger olduğundan emin ol
            Debug.Log("Shadow Collider Trigger: " + shadowCollider.isTrigger);
        }
        else
        {
            Debug.LogError("Shadow'da Collider2D bulunamadı!");
        }

        // Başlangıç scale'i ayarla (değiştirme, sadece kaydet)
        Vector3 initialScale = transform.localScale;
        currentScale = Mathf.Abs(initialScale.x);
        maxScale = currentScale;
    }

    // Başlangıçta doğru scale'i ayarla


    // PlayerController tarafından gölge pozisyonunu güncellemek için çağrılır
    public void UpdateShadowPosition(float playerX, float playerY)
    {
        if (!isAlive) return; // Ölüyse hareket etme

        // X pozisyonunu ayna pozisyonuna göre yansıt: oyuncu aynaya göre hareket eder
        float playerDistanceFromMirror = playerX - mirrorPositionX; // Oyuncunun aynaya olan uzaklığı
        float shadowX = mirrorPositionX - (playerDistanceFromMirror * xInversionScale * xMovementRatio); // Aynanın diğer tarafına yansıt

        // Y pozisyonunu aynı şekilde yansıt (ters çevirme yok)
        targetX = shadowX;
        targetY = playerY;

        // Pozisyonu güncelle - collision kontrolü ile
        Vector2 newPosition = new Vector2(targetX, targetY);

        // Pozisyonu clamp et
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY) + valueYOffset;

        // Rigidbody ile güvenli hareket (collision'lar dikkate alınır)
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }

        UpdateShadowRotation();
        CurrentScale();
        UpdateScaleBasedOnDistanceToMirror();

    }
    private void CurrentScale()
    {
        Vector3 scale = transform.localScale;
        currentScaleX = Math.Abs(scale.x);
        maxScale = currentScaleX;
    }

    // Ayna mesafesine göre scale değiştirme fonksiyonu
    private void UpdateScaleBasedOnDistanceToMirror()
    {
        // Player pozisyonuna dayalı scale hesapla (daha mantıklı)
        // Player aynaya ne kadar yakınsa shadow da o kadar büyük olmalı
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            float playerX = player.transform.position.x;
            float playerDistanceFromMirror = Mathf.Abs(playerX - mirrorPositionX);

            // Player'ın aynaya olan maksimum mesafesini hesapla - inspector'dan ayarlanabilir
            float playerMaxDistance = Mathf.Max(
                Mathf.Abs(playerMinXForScale - mirrorPositionX),
                Mathf.Abs(playerMaxXForScale - mirrorPositionX)
            );

            // Player aynaya yakınken = 0, uzakken = 1
            float normalizedDistance = playerDistanceFromMirror / playerMaxDistance;

            // Inspector'da göster
            currentDistanceValue = playerDistanceFromMirror;
            currentDistanceRatio = normalizedDistance;

            // Hedef scale hesapla (player aynaya yakın = maxScale, uzak = minScale)
            float targetScale = Mathf.Lerp(maxScale, minScale, normalizedDistance);

            // Smooth geçiş
            currentScale = Mathf.Lerp(currentScale, targetScale, scaleTransitionSpeed * Time.deltaTime);
        }
        else
        {
            // Fallback: Shadow pozisyonuna dayalı hesaplama
            float shadowX = transform.position.x;
            float currentDistance = Mathf.Abs(shadowX - mirrorPositionX);
            float maxDistanceFromMirror = Mathf.Max(
                Mathf.Abs(maxX - mirrorPositionX),
                Mathf.Abs(minX - mirrorPositionX)
            );
            float normalizedDistance = currentDistance / maxDistanceFromMirror;

            currentDistanceValue = currentDistance;
            currentDistanceRatio = normalizedDistance;

            float targetScale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
            currentScale = Mathf.Lerp(currentScale, targetScale, scaleTransitionSpeed * Time.deltaTime);
        }

        // Mevcut scale'in işaretini koru
        Vector3 currentLocalScale = transform.localScale;
        float scaleSignX = currentLocalScale.x >= 0 ? 1f : -1f; // X işaretini koru
        float scaleSignY = currentLocalScale.y >= 0 ? 1f : -1f; // Y işaretini koru

        // Scale uygula (işaretleri koruyarak)
        transform.localScale = new Vector3(currentScale * scaleSignX, currentScale * scaleSignY, 1f);
    }

    private void UpdateShadowRotation()
    {
        transform.rotation = Quaternion.Euler(30, 20, 65);
    }

    // Gölgeyi öldürmek için genel metod
    public void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        rb.velocity = Vector2.zero; // Hareketi durdur

        Debug.Log("Gölge öldü!");

        // GameManager'ı bilgilendir
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnShadowDeath();
        }
    }

    // Gölge durumunu sıfırlamak için genel metod
    public void ResetShadow()
    {
        isAlive = true;
        rb.velocity = Vector2.zero;
        transform.position = startPosition;
        targetX = startPosition.x;
        targetY = startPosition.y;
    }

    // Canlı durumu için getter
    public bool IsAlive => isAlive;

    // Gölge ofsetini ayarlamak için metod (farklı ışık senaryoları için faydalı)
    public void SetShadowOffset(float offsetY)
    {
        shadowOffsetY = offsetY;
    }

    // X ters çevirme oranını ayarlamak için metod
    public void SetXInversionScale(float scale)
    {
        xInversionScale = scale;
    }

    // PlayerController'dan hareket sınırlarını almak için metod
    public void SetMovementBoundsFromPlayer(float playerMinX, float playerMaxX, float playerMinY, float playerMaxY)
    {
        // Scale hesabı için player sınırlarını da güncelle
        playerMinXForScale = playerMinX;
        playerMaxXForScale = playerMaxX;

        // Ayna pozisyonuna göre shadow sınırlarını hesapla
        // Player'ın aynaya olan maksimum mesafesi kadar shadow da hareket edebilir
        float playerMaxDistanceFromMirror = Mathf.Max(
            Mathf.Abs(playerMaxX - mirrorPositionX),
            Mathf.Abs(playerMinX - mirrorPositionX)
        );

        // Shadow'un sınırları ayna pozisyonundan eşit mesafede olmalı
        minX = mirrorPositionX - playerMaxDistanceFromMirror;
        maxX = mirrorPositionX + playerMaxDistanceFromMirror;

        // Y sınırları aynı
        minY = playerMinY;
        maxY = playerMaxY;
    }
    public void SetAnimationInfoShadow(String moveInfo)
    {
        if (animationChar != null && animationChar.animator != null)
        {
            switch (moveInfo)
            {
                case "Right":
                    animationChar.CharAnimationWalk();
                    gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                    break;
                case "Left":
                    animationChar.CharAnimationWalkFront();
                    gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                    break;
                case "Up":
                    animationChar.CharAnimationWalkSide();
                    gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1);
                    break;
                case "Down":
                    animationChar.CharAnimationWalkSide();
                    gameObject.transform.localScale = new Vector3(-1.25f, 1.25f, 1);
                    break;
                case "IdleRight":
                    animationChar.CharAnimationIdle();
                    gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                    break;
                case "IdleLeft":
                    animationChar.CharAnimationIdleFront();
                    gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1);
                    break;
                case "IdleUp":
                    animationChar.CharAnimationIdleSide();
                    gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1);
                    break;
                case "IdleDown":
                    animationChar.CharAnimationIdleSide();
                    gameObject.transform.localScale = new Vector3(-1.25f, 1.25f, 1);
                    break;
                default:
                    Debug.LogWarning("Bilinmeyen hareket bilgisi: " + moveInfo);
                    break;
            }
        }
    }
    // Dinamik engel sistemi için
    private GameObject currentInvisibleObstacle = null;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle") && currentInvisibleObstacle == null)
        {
            Vector3 copyPosition = collision.transform.position;
            Collider2D copyCollider = collision;
            
            // Sadece aynanın solundaki objeler için çalış
            if (copyPosition.x < mirrorPositionX)
            {
                float normaldistance = mirrorPositionX - copyPosition.x;  // Aynaya olan mesafe
                float moveX = normaldistance / xMovementRatio;            // Oranla böl
                copyPosition = new Vector3(mirrorPositionX + moveX, copyPosition.y, 0f); // Aynadan sağa ekle
                
                Vector3 scale = collision.transform.localScale;
                Quaternion originalRotation = collision.transform.rotation;
                
                // Oluşturulan objeyi currentInvisibleObstacle'a ata
                currentInvisibleObstacle = Instantiate(copyCollider.gameObject, copyPosition, originalRotation);
                
                // Shadow ile collision olmasın diye "PlayerObstacle" tag ver
                currentInvisibleObstacle.tag = "PlayerObstacle"; // Shadow bunu görmezden gelir
                
                // Ayna yansıması için X scale'ini ters çevir
                Vector3 mirrorScale = scale;
                mirrorScale.x = -(scale.x / 0.7f); // Mevcut scale'i 0.7'ye böl ve ters çevir
                mirrorScale.y = scale.y / 0.7f;    // Mevcut scale'i 0.7'ye böl
                
                // Rotation'ı da ayna yansıması için ters çevir (Y ekseninde 180 derece döndür)
                Vector3 eulerAngles = originalRotation.eulerAngles;
                eulerAngles.y += 180f; // Y ekseninde 180 derece döndür
                currentInvisibleObstacle.transform.rotation = Quaternion.Euler(eulerAngles);
                
                currentInvisibleObstacle.transform.localScale = mirrorScale;
                
                // Rigidbody2D'yi Static yap - sarsılmayı önler
                Rigidbody2D obstacleRb = currentInvisibleObstacle.GetComponent<Rigidbody2D>();
                if (obstacleRb != null)
                {
                    obstacleRb.bodyType = RigidbodyType2D.Static; // Sabit engel - hareket etmez
                }
                
                Destroy(currentInvisibleObstacle.GetComponent<SpriteRenderer>());
                Debug.Log("Engel oluşturuldu (Static): " + currentInvisibleObstacle.name + " Tag: PlayerObstacle");
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle") && currentInvisibleObstacle != null)
        {
            Destroy(currentInvisibleObstacle,0.2f);
            currentInvisibleObstacle = null;
            Debug.Log("Engel kaldırıldı (Trigger)");
        }
    }

}