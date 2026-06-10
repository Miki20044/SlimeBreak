using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    [Header("Dash")]
    public float dashSpeed = 25f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;

    [Header("Dash UI")]
    public Image dashCooldownBar;
    public TMP_Text dashCooldownText;
    public string readyText = "Dash Ready";
    public Color dashReadyColor = Color.white;
    public Color dashCooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Dash Afterimage")]
    public float afterimageInterval = 0.04f;
    public float afterimageFadeDuration = 0.4f;
    public float afterimageStartAlpha = 0.5f;
    public Color afterimageTint = new Color(0.6f, 0.8f, 1f, 1f);

    [Header("SFX")]
    public AudioClip dashSound;
    public float dashVolume = 1f;

    private AudioSource sfxSource;

    private float dashTimer;
    private float cooldownTimer;
    private float lastCooldownDuration = 1f;
    private Vector3 dashDir;
    private bool dashing;

    public bool isInvincible;

    void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();

        if (AudioSettings.instance != null && AudioSettings.instance.mixer != null)
        {
            var groups = AudioSettings.instance.mixer.FindMatchingGroups("Sfx");
            if (groups.Length > 0)
                sfxSource.outputAudioMixerGroup = groups[0];
        }
    }

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
        UpdateDashBar();

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
            lastCooldownDuration = cooldownTimer;

            if (dashSound != null && sfxSource != null)
                sfxSource.PlayOneShot(dashSound, dashVolume);

            StartCoroutine(AfterimageRoutine(effectiveDashTime));
        }
    }

    IEnumerator AfterimageRoutine(float dashDuration)
    {
        SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
        if (playerSR == null) yield break;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            SpawnAfterimage(playerSR);
            yield return new WaitForSeconds(afterimageInterval);
            elapsed += afterimageInterval;
        }
    }

    void SpawnAfterimage(SpriteRenderer playerSR)
    {
        GameObject ghost = new GameObject("DashAfterimage");
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = transform.lossyScale;

        SpriteRenderer ghostSR = ghost.AddComponent<SpriteRenderer>();
        ghostSR.sprite = playerSR.sprite;
        ghostSR.flipX = playerSR.flipX;
        ghostSR.flipY = playerSR.flipY;
        ghostSR.sortingLayerID = playerSR.sortingLayerID;
        ghostSR.sortingOrder = playerSR.sortingOrder - 1;

        Color c = afterimageTint;
        c.a = afterimageStartAlpha;
        ghostSR.color = c;

        StartCoroutine(FadeAfterimage(ghost, ghostSR));
    }

    IEnumerator FadeAfterimage(GameObject ghost, SpriteRenderer ghostSR)
    {
        float t = 0f;
        Color startColor = ghostSR.color;

        while (t < afterimageFadeDuration && ghost != null)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(afterimageStartAlpha, 0f, t / afterimageFadeDuration);
            Color c = startColor;
            c.a = alpha;
            if (ghostSR != null) ghostSR.color = c;
            yield return null;
        }

        if (ghost != null) Destroy(ghost);
    }

    void UpdateDashBar()
    {
        bool ready = cooldownTimer <= 0f;

        if (dashCooldownBar != null)
        {
            if (ready)
            {
                dashCooldownBar.fillAmount = 1f;
                dashCooldownBar.color = dashReadyColor;
            }
            else
            {
                // bar wypelnia sie od 0 do 1 w miare ladowania
                float fill = 1f - (cooldownTimer / lastCooldownDuration);
                dashCooldownBar.fillAmount = Mathf.Clamp01(fill);
                dashCooldownBar.color = dashCooldownColor;
            }
        }

        if (dashCooldownText != null)
        {
            if (ready)
                dashCooldownText.text = readyText;
            else
                dashCooldownText.text = cooldownTimer.ToString("F1") + "s";
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
        yield return new WaitForSeconds(duration + 0.3f);
        isInvincible = false;
    }
}