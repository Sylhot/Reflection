using UnityEngine;

public class TriggerLineController : MonoBehaviour
{
    private GuardAI associatedGuard; // Bu trigger line hangi guard'a ait
    
    public void SetGuardReference(GuardAI guard)
    {
        associatedGuard = guard;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Player trigger line'a çarptığında guard'ı aktifleştir
        if (other.CompareTag("Player") && associatedGuard != null)
        {
            associatedGuard.ActivateGuard();
            Debug.Log("Player triggered guard activation!");
        }
    }
}