using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Image healthBar;
    public Image staminaBar;

    [Header("SFX")]
    public AudioClip deathSound;
    public float deathVolume = 1f;

    private RandomShooter shooter;
    private AudioSource sfxSource;

    [Header("State")]
    public bool isVulnerable = false;
    public bool wasHitThisSleep = false;

    void Start()
    {
        if (DifficultyManager.instance != null)
            maxHealth *= DifficultyManager.instance.GetBossHPMultiplier();

        currentHealth = maxHealth;
        shooter = GetComponent<RandomShooter>();

        sfxSource = gameObject.AddComponent<AudioSource>();
        if (AudioSettings.instance != null && AudioSettings.instance.mixer != null)
        {
            var groups = AudioSettings.instance.mixer.FindMatchingGroups("Sfx");
            if (groups.Length > 0)
                sfxSource.outputAudioMixerGroup = groups[0];
        }
    }

    void Update()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;

        if (staminaBar != null)
        {
            if (BossPatternController.instance != null)
                staminaBar.fillAmount = BossPatternController.instance.GetStaminaPercent();
            else if (shooter != null)
                staminaBar.fillAmount = shooter.GetStaminaPercent();
        }
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
        {
            currentHealth = 0;
            if (healthBar != null)
                healthBar.fillAmount = 0f;

            BossPhaseManager pm = GetComponent<BossPhaseManager>();

            if (pm != null && pm.currentPhase == 1)
            {
                pm.TriggerPhase2();
            }
            else
            {
                StartCoroutine(DeathRoutine());
            }
        }
    }

    public void ResetForPhase2(float hpPercent, float staminaMult, float speedMult)
    {
        currentHealth = maxHealth * hpPercent;
        isVulnerable = false;
        wasHitThisSleep = false;

        if (BossPatternController.instance != null)
            BossPatternController.instance.ApplyPhase2(staminaMult, speedMult);

        // fallback dla starej RandomShooter mechaniki (jesli kiedys wroci)
        RandomShooter rs = GetComponent<RandomShooter>();
        if (rs != null)
        {
            rs.maxStamina *= staminaMult;
            rs.circleForce *= speedMult;
            rs.spiralForce *= speedMult;
            rs.aimedForce *= speedMult;
            rs.waveForce *= speedMult;
        }
    }

    IEnumerator DeathRoutine()
    {
        if (deathSound != null && sfxSource != null)
            sfxSource.PlayOneShot(deathSound, deathVolume);

        if (BossDeathMessage.instance != null)
            BossDeathMessage.instance.ShowMessage("You've defeated me...");

        yield return new WaitForSeconds(3.5f);

        if (GameManager.instance != null)
            GameManager.instance.WinGame();

        Destroy(gameObject);
    }

    public bool CanBeDamaged()
    {
        return isVulnerable && !wasHitThisSleep;
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}