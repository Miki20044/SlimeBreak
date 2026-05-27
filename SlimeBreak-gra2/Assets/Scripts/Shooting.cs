using UnityEngine;
using System.Collections;

public class RandomShooter : MonoBehaviour
{
    public GameObject projectilePrefab;

    [Header("Global")]
    public float maxStamina = 50f;
    public float staminaDrain = 1f;
    public float staminaRegen = 5f;
    public float restTime = 2f;

    private float currentStamina;
    private float restTimer;
    private bool isResting;

    private BossController boss;

    private int patternIndex;

    private float patternTimer;
    private float attackTimer;
    private float currentAttackInterval;

    private float spiralAngle = 0f;

    private bool isWaveActive = false;
    private float waveCooldownTimer = 0f;

    [Header("Circle Burst")]
    public float circleInterval = 0.8f;
    public float circleForce = 5f;

    [Header("Spiral")]
    public float spiralInterval = 0.15f;
    public float spiralForce = 4f;

    [Header("Aimed Burst")]
    public float aimedInterval = 1.2f;
    public float aimedForce = 6f;

    [Header("Ring Wave")]
    public float waveInterval = 3f;
    public float waveForce = 5f;

    [Header("Phase Multipliers")]
    public float phase2Multiplier = 1.25f; // 75% hp
    public float phase3Multiplier = 1.5f;  // 50% hp
    public float phase4Multiplier = 2f;    // 25% hp

    void Start()
    {
      currentStamina = maxStamina;
      boss = GetComponent<BossController>();

      if (DifficultyManager.instance != null)
      {
          float m = DifficultyManager.instance.GetStaminaMultiplier();
          maxStamina *= m;
          currentStamina = maxStamina;
          staminaDrain /= m;
      }

      patternTimer = 0f;
      attackTimer = 0f;
      patternIndex = 0;
    }
    float GetPhaseMultiplier()
    {
      float hp = boss.GetHealthPercent();
      float phase = 1f;

      if (hp <= 0.25f) phase = phase4Multiplier;
      else if (hp <= 0.5f) phase = phase3Multiplier;
      else if (hp <= 0.75f) phase = phase2Multiplier;

      float difficulty = DifficultyManager.instance != null
        ? DifficultyManager.instance.GetProjectileMultiplier()
        : 1f;

      return phase * difficulty;
    }

    void Update()
    {
        if (isResting)
        {
            restTimer -= Time.deltaTime;

            currentStamina += staminaRegen * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            if (restTimer <= 0)
            {
                isResting = false;
                boss.WakeUp();
            }

            return;
        }

        patternTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        waveCooldownTimer -= Time.deltaTime;

        if (patternTimer <= 0)
        {
            patternIndex = Random.Range(0, 4);

            patternTimer = 3f;
            currentAttackInterval = GetAttackInterval(patternIndex);

            attackTimer = 0f;
        }

        if (currentStamina <= 0) return;

        bool canShoot = !(isWaveActive || waveCooldownTimer > 0f);

        if (canShoot && attackTimer <= 0f)
        {
            ShootPattern();
            attackTimer = currentAttackInterval;
        }

        currentStamina -= staminaDrain * Time.deltaTime;

        if (currentStamina <= 0)
        {
            currentStamina = 0;
            isResting = true;
            restTimer = restTime;

            boss.OnSleepStart();
        }
    }

    float GetAttackInterval(int index)
    {
        switch (index)
        {
            case 0: return circleInterval;
            case 1: return spiralInterval;
            case 2: return aimedInterval;
            case 3: return waveInterval;
        }

        return 1f;
    }

    void ShootPattern()
    {
        switch (patternIndex)
        {
            case 0: CircleBurst(); break;
            case 1: Spiral(); break;
            case 2: AimedBurst(); break;
            case 3:
                if (!isWaveActive)
                    StartCoroutine(CircleWave());
                break;
        }
    }

    void CircleBurst()
    {
        int bullets = 10;

        for (int i = 0; i < bullets; i++)
        {
            float angle = i * (360f / bullets);
            ShootAngle(angle, circleForce * GetPhaseMultiplier());
        }
    }

    void Spiral()
    {
        ShootAngle(spiralAngle, spiralForce * GetPhaseMultiplier());
        spiralAngle += 25f;
    }

    void AimedBurst()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        Shoot(dir, aimedForce * GetPhaseMultiplier());
    }

    IEnumerator CircleWave()
    {
        isWaveActive = true;

        int rings = 4;
        int bulletsPerRing = 36;
        float ringDelay = 1.7f;

        for (int r = 0; r < rings; r++)
        {
            for (int i = 0; i < bulletsPerRing; i++)
            {
                float angle = i * (360f / bulletsPerRing);
                ShootAngle(angle, waveForce * GetPhaseMultiplier());
            }

            yield return new WaitForSeconds(ringDelay);
        }

        isWaveActive = false;
        waveCooldownTimer = waveInterval;
    }

    void ShootAngle(float angle, float force)
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Shoot(dir, force);
    }

    void Shoot(Vector2 dir, float force)
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        rb.velocity = dir.normalized * force;
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }
}