using UnityEngine;
using System.Collections;
using TMPro;

public class BossDeathMessage : MonoBehaviour
{
    public static BossDeathMessage instance;

    [Header("UI")]
    public TMP_Text messageText;
    public float displayDuration = 3f;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.15f;

    void Awake()
    {
        instance = this;

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    public void ShowMessage(string msg)
    {
        StartCoroutine(MessageRoutine(msg));
    }

    IEnumerator MessageRoutine(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
        }

        if (CameraShake.instance != null)
            CameraShake.instance.Shake(shakeDuration, shakeMagnitude);

        yield return new WaitForSeconds(displayDuration);

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }
}