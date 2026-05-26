using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    [Header("Dash")]
    public float dashSpeed = 25f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDir;
    private bool dashing;

    public bool isInvincible;

    [Header("Pull")]
    public Transform pullTarget;
    public float pullSpeed = 3f;
    public bool isBeingPulled = false;

    float GetCurrentSpeed()
    {
        float difficultyMult = DifficultyManager.instance != null
            ? DifficultyManager.instance.GetPlayerSpeedMultiplier()
            : 1f;

        if (PlayerHealth.instance == null) return speed * difficultyMult;

        float hp = PlayerHealth.instance.currentHealth / PlayerHealth.instance.maxHealth;

        // 100% hp = speed, 25% hp = speed * 0.5
        float speedMultiplier = Mathf.Lerp(0.5f, 1f, (hp - 0.25f) / 0.75f);
        speedMultiplier = Mathf.Clamp(speedMultiplier, 0.5f, 1f);

        return speed * speedMultiplier * difficultyMult;
    }

    void Update()
    {
        if (isBeingPulled) return;

        if (dashing)
        {
            transform.position += dashDir * dashSpeed * Time.deltaTime;
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
                dashing = false;

            return;
        }

        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(x, y, 0);
        transform.position += move * GetCurrentSpeed() * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0)
        {
            float dashLengthMult = DifficultyManager.instance != null
                ? DifficultyManager.instance.GetDashLengthMultiplier()
                : 1f;
            float dashCooldownMult = DifficultyManager.instance != null
                ? DifficultyManager.instance.GetDashCooldownMultiplier()
                : 1f;

            float effectiveDashTime = dashTime * dashLengthMult;

            dashDir = (move != Vector3.zero) ? move.normalized : transform.right;

            StartCoroutine(IFrames(effectiveDashTime));

            dashing = true;
            dashTimer = effectiveDashTime;
            cooldownTimer = dashCooldown * dashCooldownMult;
        }
    }

    public void PullToPoint()
    {
        if (pullTarget == null) return;

        StopCoroutine("PullRoutine");
        StartCoroutine(PullRoutine());
    }

    IEnumerator PullRoutine()
    {
        isBeingPulled = true;

        while (Vector3.Distance(transform.position, pullTarget.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                pullTarget.position,
                pullSpeed * Time.deltaTime
            );

            yield return null;
        }

        isBeingPulled = false;
    }

    public void Knockback(Vector2 dir, float force)
    {
        StartCoroutine(KnockRoutine(dir, force));
    }

    IEnumerator KnockRoutine(Vector2 dir, float force)
    {
        float t = 0f;
        float duration = 0.12f;

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(dir * force);

        while (t < duration)
        {
            transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
    }

    IEnumerator IFrames(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration + 0.1f);
        isInvincible = false;
    }
}