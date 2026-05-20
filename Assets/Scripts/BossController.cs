using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Image healthBar;
    public Image staminaBar;

    private RandomShooter shooter;

    [Header("State")]
    public bool isVulnerable = false;
    public bool wasHitThisSleep = false;

    void Start()
    {
        currentHealth = maxHealth;
        shooter = GetComponent<RandomShooter>();
    }

    void Update()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;

        if (staminaBar != null && shooter != null)
            staminaBar.fillAmount = shooter.GetStaminaPercent();
    }

    public void OnSleepStart()
    {
        isVulnerable = true;
        wasHitThisSleep = false;
    }

    public void WakeUp()
    {
        isVulnerable = false;
        wasHitThisSleep = false;
    }

    public void TakeDamage(float amount)
    {
        if (!isVulnerable) return;
        if (wasHitThisSleep) return;

        wasHitThisSleep = true;
        currentHealth -= amount;

        WakeUp();

        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    public bool CanBeDamaged()
    {
        return isVulnerable && !wasHitThisSleep;
    }
}