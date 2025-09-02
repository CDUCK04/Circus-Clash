using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitFacing2D : MonoBehaviour
{
    [Header("Sources")]
    public CircusClash.Troops.AI.UnitSensor2D sensor;
    public CircusClash.Troops.Movement.UnitMover2D mover;
    public CircusClash.Troops.Combat.UnitHealth health;

    [Header("Behaviour")]
    public bool preferTargetFacing = true;

    void Awake()
    {
        if (!sensor) sensor = GetComponent<CircusClash.Troops.AI.UnitSensor2D>();
        if (!mover) mover = GetComponent<CircusClash.Troops.Movement.UnitMover2D>();
        if (!health) health = GetComponent<CircusClash.Troops.Combat.UnitHealth>();
    }

    void LateUpdate()
    {
        if (health != null && health.IsDead) return;

        float dir = 0;

        Transform target = sensor ? sensor.FindClosestEnemy() : null;
        if (preferTargetFacing && target != null)
        {
            dir = Mathf.Sign(target.position.x - transform.position.x);
        }

        if (dir == 0 && mover != null)
        {
            dir = mover.isPlayerSide ? +1f : -1f;
        }

        if (dir != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir > 0 ? +1 : -1);
            transform.localScale = s;
        }
    }
}