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
    public float laserStartRotateSpeed = 30f;
    public float laserRotateSpeed = 90f;
    public float laserAccelerationTime = 1.5f;
    public float laserRotationCount = 2f;
    public float laserDirectionPause = 1f;
    public AudioClip laserSound;

    [Header("Attack 5 - Falling Rain")]
    public int fallingTotalProjectiles = 45;
    public float fallingSpawnInterval = 0.1f;
    public float fallingBaseSpeed = 3.5f;
    public float fallingMaxSpeedMultiplier = 1.8f;
    public float fallingProjectileLifetime = 5.5f;
    public float fallingProjectileScale = 0.25f;
    public AudioClip fallingRainSound;

    [Header("Attack 6 - Bouncing Bullets")]
    public int bounceBulletCount = 12;
    public float bounceBulletSpeed = 5f;
    public int bounceMaxBounces = 3;
    public float bounceAttackDuration = 5f;

    [Header("Attack 7 - Expanding Rings")]
    public int ringCount = 4;
    public int bulletsPerRing = 12;
    public float ringStagger = 0.7f;
    public float ringBulletSpeed = 3.5f;
    public float ringBulletLifetime = 3f;

    [Header("Attack 8 - Bomb Drop")]
    public int bombCount = 6;
    public float bombWarningTime = 1.2f;
    public float bombRadius = 1.5f;
    public int bombShrapnelCount = 8;
    public float bombShrapnelSpeed = 4f;

    [Header("Attack 9 - Walls + Dash Line")]
    public float wallThickness = 4f;
    public float wallGapWidth = 2.2f;
    public float wallCloseDuration = 2f;
    public float wallWarningDuration = 0.6f;
    public float dashLineThickness = 1.2f;
    public float dashLineWarningDuration = 0.5f;
    public float dashLineSweepDuration = 1.5f;

    [Header("Attack 10 - Dash Gauntlet")]
    public int gauntletLineCount = 5;
    public float gauntletLineThickness = 1.5f;
    public float gauntletSpawnInterval = 0.6f;
    public float gauntletLineWarning = 0.5f;
    public float gauntletLineDuration = 0.7f;

    [Header("Attack 11 - Death Curtain")]
    public int curtainBulletCount = 16;
    public int curtainGapSize = 3;
    public float curtainSpeed = 5f;
    public float curtainBulletScale = 0.35f;

    [Header("Attack 12 - Implosion")]
    public int implosionBulletCount = 18;
    public float implosionWarningTime = 1.2f;
    public float implosionSpeed = 4f;

    [Header("Attack 13 - Forced Corridor")]
    public float corridorHeight = 2.2f;
    public float corridorWarning = 0.7f;
    public int corridorBulletCount = 18;
    public float corridorBulletInterval = 0.18f;
    public float corridorBulletSpeed = 6f;

    [Header("Stamina / Sleep")]
    public float maxStamina = 100f;
    public float staminaPerAttack = 20f;
    public float restDuration = 2f;

    private float currentStamina;
    private bool isResting = false;
    private float phase2SpeedBoost = 1f;

    // 0=Lines, 1=ReturnBullets, 2=Lasers, 3=FallingRain, 4=Bouncing, 5=Rings, 6=Bombs, 7=WallsDash
    // 8=DashGauntlet, 9=DeathCurtain, 10=Implosion, 11=ForcedCorridor, 12=ReverseRain
    private int[] attackOrder = { 1, 6, 8, 3, 10, 0, 5, 11, 2, 9, 4, 7, 12 };
    private int attackIndex = 0;

    private AudioSource audioSource;
    private BossController boss;
    private bool isAttacking = false;

    void Awake()
    {
        instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();

        // przypisz do SFX grupy mixera jesli AudioSettings istnieje
        if (AudioSettings.instance != null && AudioSettings.instance.mixer != null)
        {
            var groups = AudioSettings.instance.mixer.FindMatchingGroups("Sfx");
            if (groups.Length > 0)
                audioSource.outputAudioMixerGroup = groups[0];
        }
    }

    void Start()
    {
        boss = GetComponent<BossController>();

        // skaluj staminy przez difficulty
        if (DifficultyManager.instance != null)
            maxStamina *= DifficultyManager.instance.GetStaminaMultiplier();

        currentStamina = maxStamina;
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }

    float ProjMult()
    {
        float baseMult = DifficultyManager.instance != null
            ? DifficultyManager.instance.GetProjectileMultiplier()
            : 1f;
        return baseMult * phase2SpeedBoost;
    }

    float ReactionMult()
    {
        return DifficultyManager.instance != null
            ? DifficultyManager.instance.GetReactionTimeMultiplier()
            : 1f;
    }

    public void ApplyPhase2(float staminaMult, float speedBoost)
    {
        maxStamina *= staminaMult;
        currentStamina = maxStamina;
        phase2SpeedBoost = speedBoost;
    }

    public void StartAttacking()
    {
        StartCoroutine(PatternLoop());
    }

    IEnumerator PatternLoop()
    {
        while (true)
        {
            if (isAttacking || isResting)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(0.3f);

            int pattern = attackOrder[attackIndex];
            attackIndex = (attackIndex + 1) % attackOrder.Length;

            isAttacking = true;

            switch (pattern)
            {
                case 0: yield return StartCoroutine(Attack1_Lines()); break;
                case 1: yield return StartCoroutine(Attack2_ReturnBullets()); break;
                case 2: yield return StartCoroutine(Attack4_Lasers()); break;
                case 3: yield return StartCoroutine(Attack5_FallingRain()); break;
                case 4: yield return StartCoroutine(Attack6_BouncingBullets()); break;
                case 5: yield return StartCoroutine(Attack7_ExpandingRings()); break;
                case 6: yield return StartCoroutine(Attack8_BombDrop()); break;
                case 7: yield return StartCoroutine(Attack9_WallsDashLine()); break;
                case 8: yield return StartCoroutine(Attack10_DashGauntlet()); break;
                case 9: yield return StartCoroutine(Attack11_DeathCurtain()); break;
                case 10: yield return StartCoroutine(Attack12_Implosion()); break;
                case 11: yield return StartCoroutine(Attack13_ForcedCorridor()); break;
                case 12: yield return StartCoroutine(Attack14_ReverseRain()); break;
            }

            isAttacking = false;

            // drain stamina po ataku
            currentStamina -= staminaPerAttack;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                yield return StartCoroutine(RestCycle());
            }
        }
    }

    IEnumerator RestCycle()
    {
        isResting = true;

        Debug.Log("[Boss] Going to sleep - VULNERABLE NOW. Press E to hit!");
        if (boss != null) boss.OnSleepStart();

        yield return new WaitForSeconds(restDuration);

        Debug.Log("[Boss] Waking up. HP=" + (boss != null ? boss.GetHealthPercent().ToString("F2") : "?"));
        if (boss != null) boss.WakeUp();

        currentStamina = maxStamina;
        isResting = false;
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

        // czekaj az kazda kulka wroci do bossa (z safety timeoutem)
        float safetyTimeout = (bulletOutwardTime + pauseBeforeReturn) * 1.5f + 1f;
        float elapsed = 0f;
        int remaining = bullets.Count;

        while (remaining > 0 && elapsed < safetyTimeout)
        {
            elapsed += Time.deltaTime;
            remaining = 0;

            for (int i = 0; i < bullets.Count; i++)
            {
                if (bullets[i] == null) continue;

                float dist = Vector2.Distance(bullets[i].transform.position, transform.position);
                if (dist < 0.5f)
                {
                    Destroy(bullets[i]);
                    bullets[i] = null;
                }
                else
                {
                    remaining++;
                }
            }

            yield return null;
        }

        // sprzatnij ewentualne resztki
        for (int i = 0; i < bullets.Count; i++)
        {
            if (bullets[i] != null) Destroy(bullets[i]);
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
        float startSpeed = laserStartRotateSpeed * ProjMult();
        float maxSpeed = laserRotateSpeed * ProjMult();

        float rotated = 0f;
        float elapsed = 0f;

        while (rotated < totalDegrees)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / laserAccelerationTime);
            float currentSpeed = Mathf.Lerp(startSpeed, maxSpeed, t);

            float step = currentSpeed * Time.deltaTime;
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

    // ==================== ATAK 5 - DESZCZ Z GORY (sekwencyjny spam) ====================
    IEnumerator Attack5_FallingRain()
    {
        if (fallingRainSound != null)
            audioSource.PlayOneShot(fallingRainSound);

        for (int i = 0; i < fallingTotalProjectiles; i++)
        {
            SpawnFallingProjectile();

            if (i < fallingTotalProjectiles - 1)
                yield return new WaitForSeconds(fallingSpawnInterval);
        }

        // odczekaj az ostatni dolaci
        yield return new WaitForSeconds(fallingProjectileLifetime);
    }

    void SpawnFallingProjectile()
    {
        float x = Random.Range(-mapWidth / 2f, mapWidth / 2f);
        float topY = mapHeight / 2f + 1f;
        Vector3 pos = new Vector3(x, topY, 0);

        float initialSpeed = fallingBaseSpeed * ProjMult();
        GameObject b = SpawnBullet(pos, Vector2.down * initialSpeed);
        b.transform.localScale = Vector3.one * fallingProjectileScale;

        StartCoroutine(AccelerateFalling(b));
    }

    IEnumerator AccelerateFalling(GameObject p)
    {
        float elapsed = 0f;
        while (p != null && elapsed < fallingProjectileLifetime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallingProjectileLifetime);
            float speedMult = Mathf.Lerp(1f, fallingMaxSpeedMultiplier, t);
            float currentSpeed = fallingBaseSpeed * speedMult * ProjMult();

            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = Vector2.down * currentSpeed;

            yield return null;
        }

        if (p != null) Destroy(p);
    }

    // ==================== ATAK 6 - ODBIJAJACE KULKI ====================
    IEnumerator Attack6_BouncingBullets()
    {
        for (int i = 0; i < bounceBulletCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GameObject b = SpawnBullet(transform.position, dir * bounceBulletSpeed * ProjMult());
            b.transform.localScale = Vector3.one * 0.2f;
            StartCoroutine(BounceBullet(b));
        }

        yield return new WaitForSeconds(bounceAttackDuration);
    }

    IEnumerator BounceBullet(GameObject b)
    {
        int bounces = 0;
        float halfW = mapWidth / 2f;
        float halfH = mapHeight / 2f;

        while (b != null && bounces < bounceMaxBounces)
        {
            Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
            if (rb == null) break;

            Vector3 pos = b.transform.position;
            Vector2 vel = rb.velocity;
            bool bounced = false;

            if (pos.x > halfW && vel.x > 0) { vel.x = -vel.x; bounced = true; }
            else if (pos.x < -halfW && vel.x < 0) { vel.x = -vel.x; bounced = true; }
            if (pos.y > halfH && vel.y > 0) { vel.y = -vel.y; bounced = true; }
            else if (pos.y < -halfH && vel.y < 0) { vel.y = -vel.y; bounced = true; }

            if (bounced)
            {
                rb.velocity = vel;
                bounces++;
            }

            yield return null;
        }

        if (b != null) Destroy(b);
    }

    // ==================== ATAK 7 - ROZSZERZAJACE SIE PIERSCIENIE ====================
    IEnumerator Attack7_ExpandingRings()
    {
        for (int ring = 0; ring < ringCount; ring++)
        {
            float angleOffset = ring * (360f / bulletsPerRing / 2f);

            for (int i = 0; i < bulletsPerRing; i++)
            {
                float angle = (i * (360f / bulletsPerRing) + angleOffset) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                GameObject b = SpawnBullet(transform.position, dir * ringBulletSpeed * ProjMult());
                b.transform.localScale = Vector3.one * 0.2f;
                Destroy(b, ringBulletLifetime);
            }

            if (ring < ringCount - 1)
                yield return new WaitForSeconds(ringStagger);
        }

        yield return new WaitForSeconds(ringBulletLifetime);
    }

    // ==================== ATAK 8 - BOMBY ====================
    IEnumerator Attack8_BombDrop()
    {
        System.Collections.Generic.List<GameObject> warnings = new System.Collections.Generic.List<GameObject>();
        System.Collections.Generic.List<Vector3> positions = new System.Collections.Generic.List<Vector3>();

        for (int i = 0; i < bombCount; i++)
        {
            float x = Random.Range(-mapWidth / 2f + bombRadius, mapWidth / 2f - bombRadius);
            float y = Random.Range(-mapHeight / 2f + bombRadius, mapHeight / 2f - bombRadius);
            Vector3 pos = new Vector3(x, y, 0);
            positions.Add(pos);

            GameObject warn = CreateBombWarning(pos);
            warnings.Add(warn);
        }

        yield return new WaitForSeconds(bombWarningTime);

        // eksplozja
        if (CameraShake.instance != null)
            CameraShake.instance.Shake(0.3f, 0.1f);

        for (int i = 0; i < positions.Count; i++)
        {
            if (warnings[i] != null) Destroy(warnings[i]);

            for (int j = 0; j < bombShrapnelCount; j++)
            {
                float angle = j * (360f / bombShrapnelCount) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                GameObject b = SpawnBullet(positions[i], dir * bombShrapnelSpeed * ProjMult());
                b.transform.localScale = Vector3.one * 0.2f;
                Destroy(b, 3f);
            }
        }

        yield return new WaitForSeconds(2f);
    }

    GameObject CreateBombWarning(Vector3 pos)
    {
        GameObject obj = new GameObject("BombWarning");
        obj.transform.position = pos;
        obj.transform.localScale = Vector3.one * bombRadius * 2f;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = new Color(1f, 0.3f, 0.3f, 0.45f);

        return obj;
    }

    // ==================== ATAK 9 - SCIANY + DASH LINIA ====================
    IEnumerator Attack9_WallsDashLine()
    {
        // dwie sciany z lewej i prawej
        float halfMapH = mapHeight / 2f;

        GameObject leftWall = CreateWall(new Vector3(-mapWidth / 2f - wallThickness, 0, 0));
        GameObject rightWall = CreateWall(new Vector3(mapWidth / 2f + wallThickness, 0, 0));

        SetAlpha(leftWall, 0.4f);
        SetAlpha(rightWall, 0.4f);
        leftWall.GetComponent<BoxCollider2D>().enabled = false;
        rightWall.GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSeconds(wallWarningDuration);

        SetAlpha(leftWall, 1f);
        SetAlpha(rightWall, 1f);
        leftWall.GetComponent<BoxCollider2D>().enabled = true;
        rightWall.GetComponent<BoxCollider2D>().enabled = true;

        // close in
        Vector3 leftStart = leftWall.transform.position;
        Vector3 rightStart = rightWall.transform.position;
        Vector3 leftEnd = new Vector3(-wallGapWidth / 2f - wallThickness / 2f, 0, 0);
        Vector3 rightEnd = new Vector3(wallGapWidth / 2f + wallThickness / 2f, 0, 0);

        float t = 0f;
        while (t < wallCloseDuration)
        {
            t += Time.deltaTime;
            float p = t / wallCloseDuration;
            leftWall.transform.position = Vector3.Lerp(leftStart, leftEnd, p);
            rightWall.transform.position = Vector3.Lerp(rightStart, rightEnd, p);
            yield return null;
        }

        // poziomy laser sweepuje przez srodek (gracz musi dashnoc)
        GameObject dashLine = CreateHorizontalDashLine();
        SetAlpha(dashLine, 0.4f);
        dashLine.GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSeconds(dashLineWarningDuration);

        SetAlpha(dashLine, 1f);
        dashLine.GetComponent<BoxCollider2D>().enabled = true;

        Vector3 sweepStart = new Vector3(0, halfMapH, 0);
        Vector3 sweepEnd = new Vector3(0, -halfMapH, 0);

        t = 0f;
        while (t < dashLineSweepDuration)
        {
            t += Time.deltaTime;
            float p = t / dashLineSweepDuration;
            dashLine.transform.position = Vector3.Lerp(sweepStart, sweepEnd, p);
            yield return null;
        }

        Destroy(dashLine);
        Destroy(leftWall);
        Destroy(rightWall);
    }

    GameObject CreateWall(Vector3 pos)
    {
        GameObject obj = new GameObject("CloseInWall");
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(wallThickness, mapHeight, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = new Color(1f, 0.2f, 0.2f, 1f);

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.enabled = false;

        obj.AddComponent<KillPlayer>();

        return obj;
    }

    GameObject CreateHorizontalDashLine()
    {
        GameObject obj = new GameObject("DashSweepLine");
        obj.transform.localScale = new Vector3(wallGapWidth, dashLineThickness, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = new Color(1f, 0.4f, 0.1f, 1f);

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.enabled = false;

        obj.AddComponent<KillPlayer>();

        return obj;
    }

    // ==================== ATAK 10 - DASH GAUNTLET ====================
    IEnumerator Attack10_DashGauntlet()
    {
        for (int i = 0; i < gauntletLineCount; i++)
        {
            float y = Random.Range(-mapHeight / 2f + 1f, mapHeight / 2f - 1f);
            StartCoroutine(SpawnGauntletLine(y));

            if (i < gauntletLineCount - 1)
                yield return new WaitForSeconds(gauntletSpawnInterval);
        }

        yield return new WaitForSeconds(gauntletLineWarning + gauntletLineDuration);
    }

    IEnumerator SpawnGauntletLine(float y)
    {
        GameObject line = new GameObject("GauntletLine");
        line.transform.position = new Vector3(0, y, 0);
        line.transform.localScale = new Vector3(mapWidth, gauntletLineThickness, 1f);

        SpriteRenderer sr = line.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = new Color(1f, 0.3f, 0.3f, 0.4f);

        BoxCollider2D col = line.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.enabled = false;

        line.AddComponent<KillPlayer>();

        yield return new WaitForSeconds(gauntletLineWarning);

        sr.color = new Color(1f, 0.2f, 0.2f, 1f);
        col.enabled = true;

        yield return new WaitForSeconds(gauntletLineDuration);

        Destroy(line);
    }

    // ==================== ATAK 11 - DEATH CURTAIN ====================
    IEnumerator Attack11_DeathCurtain()
    {
        bool fromLeft = Random.value > 0.5f;
        float spacing = mapHeight / (curtainBulletCount + 1);

        // wybierz pozycje gap (gap to ciagly zakres ktorego nie spawnujemy)
        int gapStart = Random.Range(1, curtainBulletCount - curtainGapSize);

        float startX = fromLeft ? -mapWidth / 2f - 1f : mapWidth / 2f + 1f;
        Vector2 dir = fromLeft ? Vector2.right : Vector2.left;

        System.Collections.Generic.List<GameObject> bullets = new System.Collections.Generic.List<GameObject>();

        for (int i = 0; i < curtainBulletCount; i++)
        {
            // pomijamy gap
            if (i >= gapStart && i < gapStart + curtainGapSize) continue;

            float y = -mapHeight / 2f + (i + 1) * spacing;
            GameObject b = SpawnBullet(new Vector3(startX, y, 0), dir * curtainSpeed * ProjMult());
            b.transform.localScale = Vector3.one * curtainBulletScale;
            bullets.Add(b);
        }

        // czekaj az przejda przez mape
        float travelTime = (mapWidth + 3f) / (curtainSpeed * ProjMult());
        yield return new WaitForSeconds(travelTime);

        foreach (var b in bullets) if (b != null) Destroy(b);
    }

    // ==================== ATAK 12 - IMPLOSION ====================
    IEnumerator Attack12_Implosion()
    {
        System.Collections.Generic.List<GameObject> bullets = new System.Collections.Generic.List<GameObject>();
        float spawnRadius = Mathf.Min(mapWidth, mapHeight) / 2f - 0.5f;

        for (int i = 0; i < implosionBulletCount; i++)
        {
            float angle = i * (360f / implosionBulletCount) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spawnRadius;
            Vector3 pos = transform.position + offset;

            GameObject b = SpawnBullet(pos, Vector2.zero);
            b.transform.localScale = Vector3.one * 0.25f;
            SetAlpha(b, 0.4f);
            BoxCollider2D col = b.GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = false;
            bullets.Add(b);
        }

        yield return new WaitForSeconds(implosionWarningTime);

        // wszystkie ledo do bossa
        foreach (var b in bullets)
        {
            if (b == null) continue;
            SetAlpha(b, 1f);
            BoxCollider2D col = b.GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = true;

            Vector3 toBoss = (transform.position - b.transform.position).normalized;
            Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = (Vector2)toBoss * implosionSpeed * ProjMult();
        }

        // czekaj az przelecio przez srodek
        float travelTime = spawnRadius / (implosionSpeed * ProjMult()) + 1f;
        yield return new WaitForSeconds(travelTime);

        foreach (var b in bullets) if (b != null) Destroy(b);
    }

    // ==================== ATAK 13 - FORCED CORRIDOR ====================
    IEnumerator Attack13_ForcedCorridor()
    {
        GameObject topWall = CreateCorridorWall(true);
        GameObject botWall = CreateCorridorWall(false);

        SetAlpha(topWall, 0.4f); SetAlpha(botWall, 0.4f);
        topWall.GetComponent<BoxCollider2D>().enabled = false;
        botWall.GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSeconds(corridorWarning);

        SetAlpha(topWall, 1f); SetAlpha(botWall, 1f);
        topWall.GetComponent<BoxCollider2D>().enabled = true;
        botWall.GetComponent<BoxCollider2D>().enabled = true;

        // spam kulek w korytarzu
        for (int i = 0; i < corridorBulletCount; i++)
        {
            bool fromLeft = Random.value > 0.5f;
            float y = Random.Range(-corridorHeight / 2f + 0.3f, corridorHeight / 2f - 0.3f);
            float x = fromLeft ? -mapWidth / 2f - 0.5f : mapWidth / 2f + 0.5f;
            Vector2 dir = fromLeft ? Vector2.right : Vector2.left;

            GameObject b = SpawnBullet(new Vector3(x, y, 0), dir * corridorBulletSpeed * ProjMult());
            b.transform.localScale = Vector3.one * 0.22f;
            Destroy(b, 3f);

            yield return new WaitForSeconds(corridorBulletInterval);
        }

        yield return new WaitForSeconds(0.8f);

        Destroy(topWall);
        Destroy(botWall);
    }

    GameObject CreateCorridorWall(bool isTop)
    {
        float wallHeight = mapHeight / 2f - corridorHeight / 2f;
        float wallCenterY = isTop
            ? corridorHeight / 2f + wallHeight / 2f
            : -(corridorHeight / 2f + wallHeight / 2f);

        GameObject obj = new GameObject("CorridorWall");
        obj.transform.position = new Vector3(0, wallCenterY, 0);
        obj.transform.localScale = new Vector3(mapWidth, wallHeight, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = new Color(1f, 0.2f, 0.2f, 1f);

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.enabled = false;

        obj.AddComponent<KillPlayer>();

        return obj;
    }

    // ==================== ATAK 14 - REVERSE RAIN (z dolu do gory) ====================
    IEnumerator Attack14_ReverseRain()
    {
        for (int i = 0; i < fallingTotalProjectiles; i++)
        {
            SpawnRisingProjectile();
            if (i < fallingTotalProjectiles - 1)
                yield return new WaitForSeconds(fallingSpawnInterval);
        }
        yield return new WaitForSeconds(fallingProjectileLifetime);
    }

    void SpawnRisingProjectile()
    {
        float x = Random.Range(-mapWidth / 2f, mapWidth / 2f);
        float bottomY = -mapHeight / 2f - 1f;
        Vector3 pos = new Vector3(x, bottomY, 0);

        float initialSpeed = fallingBaseSpeed * ProjMult();
        GameObject b = SpawnBullet(pos, Vector2.up * initialSpeed);
        b.transform.localScale = Vector3.one * fallingProjectileScale;

        StartCoroutine(AccelerateRising(b));
    }

    IEnumerator AccelerateRising(GameObject p)
    {
        float elapsed = 0f;
        while (p != null && elapsed < fallingProjectileLifetime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallingProjectileLifetime);
            float speedMult = Mathf.Lerp(1f, fallingMaxSpeedMultiplier, t);
            float currentSpeed = fallingBaseSpeed * speedMult * ProjMult();

            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = Vector2.up * currentSpeed;

            yield return null;
        }

        if (p != null) Destroy(p);
    }
}