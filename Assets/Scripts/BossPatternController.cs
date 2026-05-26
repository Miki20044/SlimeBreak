using UnityEngine;
using System.Collections;

public class BossPatternController : MonoBehaviour
{
    public static BossPatternController instance;

    [Header("General")]
    public float mapWidth = 12f;
    public float mapHeight = 11.5f;

    [Header("Projectile")]
    public GameObject bulletPrefab;

    [Header("Attack 1 - Sweeping Lines")]
    public float lineThickness = 0.5f;
    public float lineCoveragePercent = 0.6f;
    public float lineWarningDuration = 0.4f;
    public float lineMoveDuration = 1.3f;
    public int lineCount = 6;
    public float lineSpawnDelay = 0.9f;
    public AudioClip lineWarningSound;
    public AudioClip lineAttackSound;

    [Header("Attack 1 - Visual Ring")]
    public int ringProjectileCount = 12;
    public float ringRadius = 1.5f;
    public float ringRotateSpeed = 120f;
    public float ringProjectileScale = 0.15f;

    [Header("Attack 2 - Return Bullets")]
    public int spamBurstCount = 6;
    public int bulletsPerBurst = 12;
    public float burstInterval = 0.2f;
    public float spamBulletSpeed = 4f;
    public float spamBulletScale = 0.2f;
    public float bulletOutwardTime = 2f;
    public float pauseBeforeReturn = 1f;
    public AudioClip returnBulletSound;

    [Header("Attack 4 - Laser")]
    public float laserWarningDuration = 1.3f;
    public float laserPreviewSpeed = 30f;
    public float laserRotateSpeed = 90f;
    public float laserRotationCount = 2f;
    public float laserDirectionPause = 1f;
    public AudioClip laserSound;

    private AudioSource audioSource;
    private BossController boss;
    private bool isAttacking = false;

    void Awake()
    {
        instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        boss = GetComponent<BossController>();
    }

    float ProjMult()
    {
        return DifficultyManager.instance != null
            ? DifficultyManager.instance.GetProjectileMultiplier()
            : 1f;
    }

    float ReactionMult()
    {
        return DifficultyManager.instance != null
            ? DifficultyManager.instance.GetReactionTimeMultiplier()
            : 1f;
    }

    public void StartAttacking()
    {
        Debug.Log("StartAttacking called!");
        StartCoroutine(PatternLoop());
    }

    IEnumerator PatternLoop()
    {
        while (true)
        {
            if (isAttacking)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(0.3f);

            int pattern = Random.Range(0, 3);

            isAttacking = true;

            switch (pattern)
            {
                case 0: yield return StartCoroutine(Attack1_Lines()); break;
                case 1: yield return StartCoroutine(Attack2_ReturnBullets()); break;
                case 2: yield return StartCoroutine(Attack4_Lasers()); break;
            }

            isAttacking = false;
        }
    }

    // ==================== ATAK 1 - PRZEJEZDZAJACE LINIE ====================
    IEnumerator Attack1_Lines()
    {
        // wizualne kolo projectili wokol bossa (tylko dekoracja, bez kolizji)
        System.Collections.Generic.List<GameObject> ring = SpawnVisualRing();
        Coroutine ringRotation = StartCoroutine(RotateRing(ring));

        for (int i = 0; i < lineCount; i++)
        {
            bool fromLeft = (i % 2 == 0);
            bool isTop = (i % 2 == 0);
            StartCoroutine(SpawnSingleLine(fromLeft, isTop));

            if (i < lineCount - 1)
                yield return new WaitForSeconds(lineSpawnDelay);
        }

        // odczekaj na zakonczenie ostatniego paska
        yield return new WaitForSeconds(lineWarningDuration * ReactionMult() + lineMoveDuration / ProjMult());

        // sprzatanie ringa
        if (ringRotation != null) StopCoroutine(ringRotation);
        foreach (GameObject p in ring)
        {
            if (p != null) Destroy(p);
        }
    }

    System.Collections.Generic.List<GameObject> SpawnVisualRing()
    {
        var list = new System.Collections.Generic.List<GameObject>();

        for (int i = 0; i < ringProjectileCount; i++)
        {
            float rad = i * (360f / ringProjectileCount) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * ringRadius;
            Vector3 pos = transform.position + offset;

            GameObject p = SpawnBullet(pos, Vector2.zero);
            p.transform.localScale = Vector3.one * ringProjectileScale;

            // wylacz kolizje - to tylko dekoracja
            BoxCollider2D col = p.GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = false;

            // usun KillPlayer zeby na pewno nie zabil
            KillPlayer kp = p.GetComponent<KillPlayer>();
            if (kp != null) Destroy(kp);

            // wylacz fizyke (kinematic) zeby nie spadl
            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            list.Add(p);
        }

        return list;
    }

    IEnumerator RotateRing(System.Collections.Generic.List<GameObject> ring)
    {
        while (true)
        {
            float rot = ringRotateSpeed * Time.deltaTime;
            foreach (GameObject p in ring)
            {
                if (p == null) continue;
                p.transform.RotateAround(transform.position, Vector3.forward, rot);
            }
            yield return null;
        }
    }

    IEnumerator SpawnSingleLine(bool fromLeft, bool isTop)
    {
        float coverage = mapHeight * lineCoveragePercent;
        float centerY = isTop
            ? mapHeight / 2f - coverage / 2f
            : -mapHeight / 2f + coverage / 2f;

        // linie startuja i koncza przy scianach
        float leftX = -mapWidth / 2f + lineThickness / 2f;
        float rightX = mapWidth / 2f - lineThickness / 2f;

        Vector3 startPos = new Vector3(fromLeft ? leftX : rightX, centerY, 0);
        Vector3 endPos = new Vector3(fromLeft ? rightX : leftX, centerY, 0);

        GameObject line = CreateVerticalLine(startPos, coverage);
        SetAlpha(line, 0.4f);

        if (lineWarningSound != null)
            audioSource.PlayOneShot(lineWarningSound);

        yield return new WaitForSeconds(lineWarningDuration * ReactionMult());

        SetAlpha(line, 1f);
        line.GetComponent<BoxCollider2D>().enabled = true;

        if (lineAttackSound != null)
            audioSource.PlayOneShot(lineAttackSound);

        float effectiveMoveDuration = lineMoveDuration / ProjMult();
        float t = 0f;
        while (t < effectiveMoveDuration)
        {
            t += Time.deltaTime;
            float p = t / effectiveMoveDuration;
            line.transform.position = Vector3.Lerp(startPos, endPos, p);
            yield return null;
        }

        Destroy(line);
    }

    GameObject CreateVerticalLine(Vector3 pos, float height)
    {
        GameObject obj = new GameObject("VerticalLine");
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(lineThickness, height, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = new Color(1f, 0.2f, 0.2f, 1f);

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.enabled = false;

        obj.AddComponent<KillPlayer>();

        return obj;
    }

    // ==================== ATAK 2 - SPAM I POWROT KULEK ====================
    IEnumerator Attack2_ReturnBullets()
    {
        if (returnBulletSound != null)
            audioSource.PlayOneShot(returnBulletSound);

        System.Collections.Generic.List<GameObject> bullets = new System.Collections.Generic.List<GameObject>();

        for (int burst = 0; burst < spamBurstCount; burst++)
        {
            // przesuniecie katu co fale zeby nie strzelaly w te same kierunki
            float angleOffset = burst * (360f / (bulletsPerBurst * 2));

            float currentSpeed = spamBulletSpeed * ProjMult();
            for (int i = 0; i < bulletsPerBurst; i++)
            {
                float angle = i * (360f / bulletsPerBurst) + angleOffset;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                GameObject b = SpawnBullet(transform.position, dir * currentSpeed);
                b.transform.localScale = Vector3.one * spamBulletScale;
                bullets.Add(b);
            }

            if (burst < spamBurstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        // kulki leco na zewnatrz
        yield return new WaitForSeconds(bulletOutwardTime);

        // pauza po wyjsciu z mapy
        yield return new WaitForSeconds(pauseBeforeReturn);

        if (CameraShake.instance != null)
            CameraShake.instance.Shake(0.5f, 0.05f);

        foreach (GameObject b in bullets)
        {
            if (b == null) continue;
            Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = -rb.velocity;
        }

        // czas powrotu = czas wylotu (zeby zdazyly wrocic do bossa)
        yield return new WaitForSeconds(bulletOutwardTime + pauseBeforeReturn + 0.5f);

        foreach (GameObject b in bullets)
        {
            if (b != null)
                Destroy(b);
        }
    }

    // atk 4 (lasery)
    IEnumerator Attack4_Lasers()
    {
        if (laserSound != null)
            audioSource.PlayOneShot(laserSound);

        float[] startAngles = { 0f, 90f, 180f, 270f };
        GameObject[] lasers = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            lasers[i] = CreateLaser(startAngles[i]);
            SetAlpha(lasers[i], 0.3f);
            lasers[i].GetComponent<BoxCollider2D>().enabled = false;
        }

        
        float rotDir = Random.value > 0.5f ? 1f : -1f;

        // previwe
        float effectiveWarning = laserWarningDuration * ReactionMult();
        float currentPreviewSpeed = laserPreviewSpeed * ProjMult();
        float elapsedPreview = 0f;
        while (elapsedPreview < effectiveWarning)
        {
            elapsedPreview += Time.deltaTime;
            float rotAmount = currentPreviewSpeed * rotDir * Time.deltaTime;
            foreach (GameObject laser in lasers)
            {
                if (laser == null) continue;
                laser.transform.RotateAround(transform.position, Vector3.forward, rotAmount);
            }
            yield return null;
        }

        foreach (GameObject laser in lasers)
        {
            if (laser != null)
            {
                SetAlpha(laser, 1f);
                laser.GetComponent<BoxCollider2D>().enabled = true;
            }
        }

        float totalDegrees = 360f * laserRotationCount;

        yield return StartCoroutine(RotateLasers(lasers, rotDir, totalDegrees));

        yield return new WaitForSeconds(laserDirectionPause);

        yield return StartCoroutine(RotateLasers(lasers, -rotDir, totalDegrees));

        foreach (GameObject laser in lasers)
        {
            if (laser != null)
                Destroy(laser);
        }
    }

    IEnumerator RotateLasers(GameObject[] lasers, float dir, float totalDegrees)
    {
        float effectiveSpeed = laserRotateSpeed * ProjMult();
        float rotated = 0f;
        while (rotated < totalDegrees)
        {
            float step = effectiveSpeed * Time.deltaTime;
            if (rotated + step > totalDegrees)
                step = totalDegrees - rotated;

            float rotAmount = step * dir;

            foreach (GameObject laser in lasers)
            {
                if (laser == null) continue;
                laser.transform.RotateAround(transform.position, Vector3.forward, rotAmount);
            }

            rotated += step;
            yield return null;
        }
    }

    GameObject CreateLaser(float angleDeg)
    {
        float laserLength = mapWidth * 1.5f;
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

        GameObject obj = new GameObject("Laser");
        obj.transform.position = transform.position + dir * (laserLength / 2f);
        obj.transform.rotation = Quaternion.Euler(0, 0, angleDeg);
        obj.transform.localScale = new Vector3(laserLength, 0.5f, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = new Color(1f, 0.2f, 0.2f, 1f);

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        obj.AddComponent<KillPlayer>();

        return obj;
    }

    // helppr
    GameObject SpawnBullet(Vector3 pos, Vector2 velocity)
    {
        GameObject obj;

        if (bulletPrefab != null)
        {
            obj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        }
        else
        {
            obj = new GameObject("Bullet");
            obj.transform.position = pos;
            obj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSprite();
            sr.color = new Color(1f, 0.2f, 0.2f, 1f);

            obj.AddComponent<BoxCollider2D>().isTrigger = true;
            obj.AddComponent<Rigidbody2D>().gravityScale = 0f;
            obj.AddComponent<KillPlayer>();
        }

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = velocity;

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
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}