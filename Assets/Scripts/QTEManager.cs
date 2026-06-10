using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QTEManager : MonoBehaviour
{
    public static QTEManager instance;

    [Header("UI")]
    public RectTransform bar;
    public RectTransform successZone;
    public RectTransform movingLine;
    public Text keyText;
    public CanvasGroup screenFade;

    [Header("Settings")]
    public float interval = 15f;
    public float lineSpeed = 500f;
    public float successWidth = 60f;

    [Header("Music")]
    public float qtePitch = 0.75f;

    [Header("SFX")]
    public AudioClip qteStartSound;
    public float qteStartVolume = 1f;

    private AudioSource sfxSource;

    float timer;
    float startDelay = 10f;
    bool active;
    bool startedMoving;

    float barWidth;
    float successX;
    KeyCode requiredKey;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        barWidth = bar.sizeDelta.x;
        timer = interval;
        startDelay = 10f;

        bar.gameObject.SetActive(false);

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
        if (!active)
        {
            if (startDelay > 0)
            {
                startDelay -= Time.deltaTime;
                return;
            }

            timer -= Time.deltaTime;

            if (timer <= 0 && BossIntro.instance != null && BossIntro.instance.CanAttack)
                StartCoroutine(StartQTE());

            return;
        }

        if (!startedMoving) return;

        movingLine.anchoredPosition += Vector2.right * lineSpeed * Time.deltaTime;

        if (movingLine.anchoredPosition.x > barWidth)
        {
            Fail();
        }

        if (Input.GetKeyDown(requiredKey))
        {
            Check();
        }
    }

    IEnumerator StartQTE()
    {
        active = true;
        startedMoving = false;

        if (qteStartSound != null && sfxSource != null)
            sfxSource.PlayOneShot(qteStartSound, qteStartVolume);

        bar.gameObject.SetActive(true);

        bar.anchoredPosition = new Vector2(
            Random.Range(-300f, 300f),
            Random.Range(-150f, 150f)
        );

        movingLine.anchoredPosition = Vector2.zero;

        successX = Random.Range(80f, barWidth - 80f);

        successZone.anchoredPosition = new Vector2(successX, 0);
        successZone.sizeDelta = new Vector2(successWidth, successZone.sizeDelta.y);

        int r = Random.Range(0, 3);

        requiredKey = r == 0 ? KeyCode.Q :
                      r == 1 ? KeyCode.R :
                               KeyCode.T;

        keyText.text = requiredKey.ToString();

        if (BossIntro.instance != null && BossIntro.instance.musicSource != null)
            BossIntro.instance.musicSource.pitch = qtePitch;

        yield return new WaitForSeconds(1f);

        startedMoving = true;
    }

    void Check()
    {
        float lineX = movingLine.anchoredPosition.x;

        if (Mathf.Abs(lineX - successX) <= successWidth * 0.5f)
        {
            Success();
        }
        else
        {
            Fail();
        }
    }

    void Success()
    {
        EndQTE();
    }

    void Fail()
    {
        if (PlayerHealth.instance != null)
            PlayerHealth.instance.TakeDamage(PlayerHealth.instance.maxHealth * 0.25f, true);

        if (CameraShake.instance != null)
            CameraShake.instance.Shake(0.2f, 0.1f);

        if (SlowMotion.instance != null)
            SlowMotion.instance.Play(0.1f, 0.5f);

        if (screenFade != null)
            StartCoroutine(FadeFlash());

        EndQTE();
    }

    IEnumerator FadeFlash()
    {
        screenFade.alpha = 0.4f;
        yield return new WaitForSecondsRealtime(0.2f);
        screenFade.alpha = 0f;
    }

    void EndQTE()
    {
        active = false;
        startedMoving = false;
        timer = interval;

        bar.gameObject.SetActive(false);

        if (BossIntro.instance != null && BossIntro.instance.musicSource != null)
            BossIntro.instance.musicSource.pitch = 1f;
    }
}