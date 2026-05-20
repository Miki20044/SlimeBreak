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
    public CanvasGroup screenFade; // do przyciemnienia

    [Header("Settings")]
    public float interval = 10f;
    public float lineSpeed = 500f;
    public float successWidth = 60f;

    float timer;
    bool active;
    bool startedMoving;

    float barWidth;
    float successX;
    KeyCode requiredKey;

    int failCount = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        barWidth = bar.sizeDelta.x;
        timer = interval;

        bar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!active)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
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

        bar.gameObject.SetActive(true);

        // LOSOWA POZYCJA NA EKRANIE
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

        // ? 1 SEKUNDA NIC NIE ROBI
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
        failCount++;

        if (PlayerHealth.instance != null)
            PlayerHealth.instance.TakeDamage(25);

        // efekt
        if (CameraShake.instance != null)
            CameraShake.instance.Shake(0.2f, 0.1f);

        if (SlowMotion.instance != null)
            SlowMotion.instance.Play(0.1f, 0.5f);

        if (screenFade != null)
            StartCoroutine(FadeFlash());

        if (failCount >= 4)
        {
            if (PlayerHealth.instance != null)
                PlayerHealth.instance.TakeDamage(999);
        }

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
    }
}