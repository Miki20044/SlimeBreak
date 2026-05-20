using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 1.5f;
    public LayerMask bossLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, bossLayer);

        if (hit == null) return;

        BossController boss = hit.GetComponent<BossController>();

        if (boss == null) return;

        if (boss.CanBeDamaged())
        {
            boss.TakeDamage(boss.maxHealth * 0.25f);

            if (CameraShake.instance != null)
                CameraShake.instance.Shake(0.12f, 0.08f);

            if (SlowMotion.instance != null)
                SlowMotion.instance.Play(0.08f, 0.35f);

            PlayerMovement pm = GetComponent<PlayerMovement>();

            if (pm != null)
            {
                Vector2 dir = (transform.position - hit.transform.position).normalized;

                pm.Knockback(dir, 1.5f);
                pm.PullToPoint();
            }
        }
    }
}