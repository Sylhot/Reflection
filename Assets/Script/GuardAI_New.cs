using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAI : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public GameObject bulletPrefab; // Mermi prefabı
    public Transform firePoint; // Ateş noktası
    public Transform target; // Hedef (Player)
    public float fireRate = 1f; // Ateş hızı (saniye)
    private float nextFireTime = 0f;
    
    [Header("Trigger Line Sistemi")]
    public GameObject triggerLine; // Trigger line objesi (Inspector'dan atanacak)
    
    private bool isActivated = false; // Guard aktif mi?
    
    void Start()
    {
        // Player'ı otomatik bul
        if (target == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
                target = player.transform;
        }
        
        // Başlangıçta guard'ı gizle
        
        // Trigger line'a bu guard'ı referans olarak ver
        if (triggerLine != null)
        {
            TriggerLine triggerComponent = triggerLine.GetComponent<TriggerLine>();
            if (triggerComponent == null)
            {
                triggerComponent = triggerLine.AddComponent<TriggerLine>();
            }
            triggerComponent.guardToActivate = this;
        }
    }
    
    public void ActivateGuard()
    {
        isActivated = true;
        
        // Guard'ı görünür yap
        gameObject.SetActive(true);
        
        Debug.Log(gameObject.name + " Guard activated!");
    }
    
    void Update()
    {
        // Sadece aktifleştirilmiş guard'lar ateş edebilir
        if ( target != null && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        // Bullet sesi çal
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBulletSound();
        }

        // Mermi oluştur
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // BulletMovement scriptine hedef ver
        BulletMovement bulletMovement = bullet.GetComponent<BulletMovement>();
        if (bulletMovement != null)
        {
            bulletMovement.SetTarget(target, firePoint);
        }
        
        // Ayna yansıması için mermiyi ekle
        MirrorReflection mirrorReflection = FindObjectOfType<MirrorReflection>();
        if (mirrorReflection != null)
        {
            mirrorReflection.ObjectAddList(bullet);
        }
    }
}