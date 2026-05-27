using UnityEngine;
using System.Collections;

public class LineAttack : MonoBehaviour
{
    [Header("Line Settings")]
    public float lineThickness = 3f;
    public float mapWidth = 24f;
    public float mapHeight = 11.5f;
    public float coverPercent = 0.65f;
    public float warningDuration = 1f;
    public float holdDuration = 1.5f;

    [Header("Audio")]
    public AudioClip warningSound;
    public AudioClip attackSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public IEnumerator DoLineAttack()
    {
        float coverHeight = mapHeight * coverPercent;

        float topStartY = mapHeight / 2f + lineThickness + 5f;
        float botStartY = -(mapHeight / 2f + lineThickness + 5f);

        float topEndY = mapHeight / 2f - coverHeight / 2f;
        float botEndY = -(mapHeight / 2f - coverHeight / 2f);

        GameObject topLine = CreateLine(new Vector3(0, topStartY, 0));
        GameObject botLine = CreateLine(new Vector3(0, botStartY, 0));

        // preview atakow
        SetAlpha(topLine, 0.3f);
        SetAlpha(botLine, 0.3f);

        if (warningSound != null && audioSource != null)
            audioSource.PlayOneShot(warningSound);

    
        float t = 0f;
        Vector3 topStart = topLine.transform.position;
        Vector3 botStart = botLine.transform.position;
        Vector3 topEnd = new Vector3(0, topEndY, 0);
        Vector3 botEnd = new Vector3(0, botEndY, 0);

        while (t < warningDuration)
        {
            t += Time.deltaTime;
            float progress = t / warningDuration;
            topLine.transform.position = Vector3.Lerp(topStart, topEnd, progress);
            botLine.transform.position = Vector3.Lerp(botStart, botEnd, progress);
            yield return null;
        }

        topLine.transform.position = topEnd;
        botLine.transform.position = botEnd;

        // pelem atak (bez preview)
        SetAlpha(topLine, 1f);
        SetAlpha(botLine, 1f);

        // kolizje
        topLine.GetComponent<BoxCollider2D>().enabled = true;
        botLine.GetComponent<BoxCollider2D>().enabled = true;

        if (attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound);

        yield return new WaitForSeconds(holdDuration);

        Destroy(topLine);
        Destroy(botLine);
    }

    GameObject CreateLine(Vector3 pos)
    {
        GameObject obj = new GameObject("Line");
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(mapWidth, lineThickness, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = new Color(1f, 0.2f, 0.2f, 1f);

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.enabled = false; // wylaczona podczas preview

        KillPlayer kp = obj.AddComponent<KillPlayer>();

        return obj;
    }

    void SetAlpha(GameObject obj, float alpha)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
}