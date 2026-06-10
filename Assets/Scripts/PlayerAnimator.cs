using UnityEngine;

// Animacja gracza: 4 kierunki kardynalne (gora/dol/lewo/prawo), kazdy ma 2 fazy.
// Diagonalny ruch (np. up-right) uzywa najblizszego cardinal sprite'a + obraca transform.
public class PlayerAnimator : MonoBehaviour
{
    [Header("Idle")]
    public Sprite idleSprite;

    [Header("Up (gora)")]
    public Sprite upSprite1;
    public Sprite upSprite2;

    [Header("Down (dol)")]
    public Sprite downSprite1;
    public Sprite downSprite2;

    [Header("Left (lewo)")]
    public Sprite leftSprite1;
    public Sprite leftSprite2;

    [Header("Right (prawo)")]
    public Sprite rightSprite1;
    public Sprite rightSprite2;

    [Header("Timing")]
    public float accelerationTime = 0.25f;
    public float inputDeadzone = 0.15f;

    [Header("Rotation")]
    public bool enableDiagonalRotation = true;

    private SpriteRenderer sr;
    private float movementTimer;
    private Vector2 lastCardinalDir = Vector2.zero;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // idle
        if (Mathf.Abs(x) < inputDeadzone && Mathf.Abs(y) < inputDeadzone)
        {
            movementTimer = 0f;
            lastCardinalDir = Vector2.zero;
            transform.rotation = Quaternion.identity;
            if (idleSprite != null) sr.sprite = idleSprite;
            return;
        }

        Vector2 input = new Vector2(x, y).normalized;
        float moveAngle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg; // -180..180

        // wybierz cardinal sprite na podstawie kwadrantu (90deg sektorow)
        Sprite cardinalSprite1 = null;
        Sprite cardinalSprite2 = null;
        float spriteFacing = 0f;
        Vector2 cardinalDir = Vector2.zero;

        if (moveAngle >= -45f && moveAngle < 45f)
        {
            cardinalSprite1 = rightSprite1; cardinalSprite2 = rightSprite2;
            spriteFacing = 0f;
            cardinalDir = Vector2.right;
        }
        else if (moveAngle >= 45f && moveAngle < 135f)
        {
            cardinalSprite1 = upSprite1; cardinalSprite2 = upSprite2;
            spriteFacing = 90f;
            cardinalDir = Vector2.up;
        }
        else if (moveAngle >= -135f && moveAngle < -45f)
        {
            cardinalSprite1 = downSprite1; cardinalSprite2 = downSprite2;
            spriteFacing = -90f;
            cardinalDir = Vector2.down;
        }
        else
        {
            cardinalSprite1 = leftSprite1; cardinalSprite2 = leftSprite2;
            spriteFacing = 180f;
            cardinalDir = Vector2.left;
        }

        // reset timera przy zmianie kierunku kardynalnego
        if (cardinalDir != lastCardinalDir)
        {
            movementTimer = 0f;
            lastCardinalDir = cardinalDir;
        }

        movementTimer += Time.deltaTime;
        bool useSprite2 = movementTimer >= accelerationTime;

        Sprite target = useSprite2 ? cardinalSprite2 : cardinalSprite1;
        if (target != null) sr.sprite = target;

        // rotacja do dokladnego kierunku ruchu
        if (enableDiagonalRotation)
        {
            float rotZ = Mathf.DeltaAngle(spriteFacing, moveAngle);
            transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
