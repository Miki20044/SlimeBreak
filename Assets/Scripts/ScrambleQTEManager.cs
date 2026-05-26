using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScrambleQTEManager : MonoBehaviour
{
    public static ScrambleQTEManager instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text sequenceText;
    public Image timerBar;
    public CanvasGroup darknessOverlay;

    [Header("QTE")]
    public float interval = 15f;
    public float qteDuration = 5f;
    public int sequenceLength = 6;

    [Header("Time Slow")]
    public float slowMotionScale = 0.35f;

    [Header("Darkness")]
    public float darknessIncrease = 0.25f;
    public float maxDarkness = 1f;

    [Header("Music")]
    public float qtePitch = 0.75f;

    float startDelay = 10f;

    KeyCode[] possibleKeys =
    {
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow
    };

    List<KeyCode> currentSequence = new List<KeyCode>();

    int currentIndex;

    float timer;
    float currentTime;

    bool active;

    float darkness;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        timer = interval;
        startDelay = 10f;
        panel.SetActive(false);

        if (darknessOverlay != null)
            darknessOverlay.alpha = 0f;
    }

    void Update()
    {
        if (!active)
        {
            if (startDelay > 0)
            {
                startDelay -= Time.unscaledDeltaTime;
                return;
            }

            timer -= Time.unscaledDeltaTime;

            if (timer <= 0 && BossIntro.instance != null && BossIntro.instance.CanAttack)
                StartQTE();

            return;
        }

        currentTime -= Time.unscaledDeltaTime;

        timerBar.fillAmount = currentTime / qteDuration;

        if (currentTime <= 0)
        {
            Fail();
            return;
        }

        CheckInputs();
    }

    void StartQTE()
    {
        active = true;

        currentTime = qteDuration;
        currentIndex = 0;

        GenerateSequence();
        UpdateText();

        panel.SetActive(true);

        if (BossIntro.instance != null && BossIntro.instance.musicSource != null)
            BossIntro.instance.musicSource.pitch = qtePitch;

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * slowMotionScale;
    }

    void EndTimeEffect()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void GenerateSequence()
    {
        currentSequence.Clear();

        for (int i = 0; i < sequenceLength; i++)
        {
            currentSequence.Add(
                possibleKeys[Random.Range(0, possibleKeys.Length)]
            );
        }
    }

    void UpdateText()
    {
        string txt = "";

        for (int i = 0; i < currentSequence.Count; i++)
        {
            if (i < currentIndex)
                txt += "<color=green>";
            else
                txt += "<color=white>";

            txt += GetArrow(currentSequence[i]) + " ";
        }

        sequenceText.text = txt;
    }

    string GetArrow(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
        }

        return "?";
    }

    void CheckInputs()
    {
        foreach (KeyCode key in possibleKeys)
        {
            if (Input.GetKeyDown(key))
            {
                if (key == currentSequence[currentIndex])
                {
                    currentIndex++;
                    UpdateText();

                    if (currentIndex >= currentSequence.Count)
                        Success();
                }
                else
                {
                    Fail();
                }

                break;
            }
        }
    }

    void Success()
    {
        darkness = 0f;

        if (darknessOverlay != null)
            darknessOverlay.alpha = 0f;

        EndQTE();
    }

    void Fail()
    {
        darkness += darknessIncrease;
        darkness = Mathf.Clamp01(darkness);

        if (darknessOverlay != null)
            darknessOverlay.alpha = darkness;

        if (CameraShake.instance != null)
            CameraShake.instance.Shake(0.2f, 0.1f);

        if (SlowMotion.instance != null)
            SlowMotion.instance.Play(0.1f, 0.5f);

        if (PlayerHealth.instance != null)
            PlayerHealth.instance.TakeDamage(PlayerHealth.instance.maxHealth * 0.25f, true);

        EndQTE();
    }

    void EndQTE()
    {
        active = false;
        timer = interval;

        panel.SetActive(false);

        if (BossIntro.instance != null && BossIntro.instance.musicSource != null)
            BossIntro.instance.musicSource.pitch = 1f;

        EndTimeEffect();
    }
}