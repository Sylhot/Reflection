using UnityEngine;

public class TriggerLine : MonoBehaviour
{
    public GuardAI guardToActivate; // Hangi guard'ı aktifleştireceği
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Player trigger line'a çarptığında guard'ı aktifleştir
        if (other.CompareTag("Player") && guardToActivate != null)
        {
            guardToActivate.ActivateGuard();
            Debug.Log("Player triggered guard activation!");
        }
    }
}