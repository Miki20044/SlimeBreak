using UnityEngine;
using System.Collections;

public class BossIntro : MonoBehaviour
{
    public static BossIntro instance;

    [Header("Intro")]
    public float delayBeforeAppear = 3f;
    public float fadeInDuration = 5f;
    public float attackDelay = 9f;

    [Header("Audio")]
    public AudioSource musicSource;

    public bool CanAttack = false;

    private SpriteRenderer sr;
    private RandomShooter shooter;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        shooter = GetComponent<RandomShooter>();

        if (shooter != null)
            shooter.enabled = false;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        StartCoroutine(IntroRoutine());
        StartCoroutine(AttackDelayRoutine());
    }

    IEnumerator IntroRoutine()
    {
        yield return new WaitForSeconds(delayBeforeAppear);

        if (musicSource != null)
            musicSource.Play();

        float t = 0f;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Clamp01(t / fadeInDuration);
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
    }

    IEnumerator AttackDelayRoutine()
    {
        yield return new WaitForSeconds(attackDelay);

        CanAttack = true;

        BossPatternController bpc = GetComponent<BossPatternController>();
        if (bpc != null)
            bpc.StartAttacking();
    }
}