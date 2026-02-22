using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private float healAmount = 20f;

    public void OnPickup(Health playerHealth)
    {
        // This method is required by the Pickup interface but is not used in this context.
        if(playerHealth != null)
        {
            playerHealth.Refill(healAmount);
            Destroy(gameObject); // Destroy the pickup after use
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"HealthPickup collided with {other.name}");
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.transform.parent.GetComponent<Health>();
            if (playerHealth != null)
            {
                Debug.Log($"HealthPickup picked up by {other.transform.parent.name}");
                OnPickup(playerHealth);
            }
        }
    }
}
