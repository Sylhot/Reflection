using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    public float bulletSpeed = 10f; // Mermi hızı
    public float lifetime = 10f; // Mermi yaşam süresi
    
    private Vector3 direction;
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Direction Start'ta belirlenmez, SetTarget ile belirlenir
        // Eğer SetTarget çağrılmazsa transform.up kullan
        if (direction == Vector3.zero)
        {
            direction = transform.up; // Varsayılan yön
        }
        
        // Mermiyi belirtilen süre sonra yok et
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Rigidbody2D ile hareket ettir
        if (rb != null && direction != Vector3.zero)
        {
            rb.velocity = direction * bulletSpeed;
        }
    }  
    public void SetTarget(Transform target, Transform firePoint)
    {
        if (target != null && firePoint != null)
        {
            direction = (target.position - firePoint.position).normalized;
            
            // Bullet'ı hareket yönüne döndür
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward); // -90f çünkü sprite yukarı bakıyor
            
            Debug.Log("Bullet direction set to: " + direction + " towards " + target.name + " angle: " + angle);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Çarpışma durumunda mermiyi yok et (opsiyonel)
        if (other.CompareTag("Wall") || other.CompareTag("Player")|| other.CompareTag("Mirror") )
        {
            if(other.CompareTag("Player"))
            {
                // Player'a hasar verme işlemi burada yapılabilir
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.ReduceLife();
                }
            }
            Destroy(gameObject);
        }
    }
}
