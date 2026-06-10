using UnityEngine;
using System.Collections;

public class BossPhaseManager : MonoBehaviour
{
    public static BossPhaseManager instance;

    [Header("Phase 2")]
    public float phase2StaminaMultiplier = 0.5f;
    public float phase2SpeedMultiplier = 1.5f;
    public float phase2HPPercent = 0.5f;

    [Header("Phase 2 Visual (opcjonalne)")]
    public Sprite phase2Sprite;
    public Color phase2Color = Color.white;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip phase2Music;

    private SpriteRenderer sr;
    private BossController boss;
    private RandomShooter shooter;

    public int currentPhase = 1;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        boss = GetComponent<BossController>();
        shooter = GetComponent<RandomShooter>();
    }

    public void TriggerPhase2()
    {
        StartCoroutine(Phase2Transition());
    }

    IEnumerator Phase2Transition()
    {
        currentPhase = 2;

        // wyłącz strzelanie
        if (shooter != null)
            shooter.enabled = false;

        // shake bossa
        float shakeDuration = 1.5f;
        float shakeMagnitude = 0.1f;
        float t = 0f;
        Vector3 originalPos = transform.position;

        while (t < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = originalPos + new Vector3(x, y, 0);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;

        // flash efekt (blink)
        for (int i = 0; i < 6; i++)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;
            }

            yield return new WaitForSeconds(0.15f);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }

            yield return new WaitForSeconds(0.15f);
        }

        // screen shake
        if (CameraShake.instance != null)
            CameraShake.instance.Shake(0.8f, 0.2f);

        // zmiana muzyki
        if (musicSource != null && phase2Music != null)
        {
            musicSource.Stop();
            musicSource.clip = phase2Music;
            musicSource.Play();
        }

        // fade out
        t = 0f;
        float fadeDuration = 0.5f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - (t / fadeDuration);
                sr.color = c;
            }
            yield return null;
        }

        // reset pozycji i statystyk bossa
        transform.position = originalPos;
        boss.ResetForPhase2(phase2HPPercent, phase2StaminaMultiplier, phase2SpeedMultiplier);

        // zmiana wygladu na faze 2
        if (sr != null)
        {
            if (phase2Sprite != null)
                sr.sprite = phase2Sprite;

            // color zachowany ale z alpha = 0 (bedzie fade in)
            Color phase2BaseColor = phase2Color;
            phase2BaseColor.a = 0f;
            sr.color = phase2BaseColor;
        }

        // fade in
        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = t / fadeDuration;
                sr.color = c;
            }
            yield return null;
        }

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        // krótkie odczekanie i włącz strzelanie
        yield return new WaitForSeconds(1f);

        if (shooter != null)
            shooter.enabled = true;
    }
}