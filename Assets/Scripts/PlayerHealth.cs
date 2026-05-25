using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Image healthBar;

    private bool diedFromQTE = false;
    private int qteFailCount = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float dmg, bool fromQTE = false)
    {
        if (fromQTE)
        {
            qteFailCount++;

            int limit = DifficultyManager.instance != null
                ? DifficultyManager.instance.GetQTEFailLimit()
                : 4;

            if (qteFailCount >= limit)
            {
                currentHealth = 0;
                UpdateUI();
                diedFromQTE = true;
                Die();
                return;
            }
        }

        currentHealth -= dmg;

        if (currentHealth < 0)
            currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            if (fromQTE)
                diedFromQTE = true;

            Die();
        }
    }

    void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;
    }

    void Die()
    {
        currentHealth = 0;
        if (healthBar != null)
            healthBar.fillAmount = 0f;

        if (diedFromQTE)
        {
            if (GameManager.instance != null)
                GameManager.instance.SleepGameOver();
        }
        else
        {
            if (GameManager.instance != null)
                GameManager.instance.GameOver();
        }

        gameObject.SetActive(false);
    }
}