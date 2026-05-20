using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMovement p = other.GetComponent<PlayerMovement>();

        if (p != null && p.isInvincible)
            return;

        gameManager.GameOver();
        Destroy(other.gameObject);
    }
}