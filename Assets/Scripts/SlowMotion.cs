using UnityEngine;
using System.Collections;

public class SlowMotion : MonoBehaviour
{
    public static SlowMotion instance;

    void Awake()
    {
        instance = this;
    }

    public void Play(float duration, float scale)
    {
        StopAllCoroutines();
        StartCoroutine(SlowRoutine(duration, scale));
    }

    IEnumerator SlowRoutine(float duration, float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * scale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}