using UnityEngine;
using CircusClash.Troops.Movement;   
using CircusClash.Troops.Combat;    
using CircusClash.Troops.AI;        

[RequireComponent(typeof(Animator))]
public class UnitAnimationDriver : MonoBehaviour
{
    [Header("Param Names")]
    public string isMovingParam = "IsMoving";
    public string isDeadParam = "IsDead";
    public string attackParam = "Attack";

    Animator anim;
    UnitMover2D mover;
    UnitHealth health;

    void Awake()
    {
        anim = GetComponent<Animator>();
        mover = GetComponent<UnitMover2D>();
        health = GetComponent<UnitHealth>();
    }

    void OnEnable()
    {
        if (health != null) health.onDied.AddListener(OnDied);
    }
    void OnDisable()
    {
        if (health != null) health.onDied.RemoveListener(OnDied);
    }

    void Update()
    {
        bool isDead = (health != null && health.IsDead);
        bool isMoving = !isDead && (mover != null && !mover.IsStopped);
        if (anim)
        {
            anim.SetBool(isDeadParam, isDead);
            anim.SetBool(isMovingParam, isMoving);
        }
    }

    void OnDied()
    {
        if (anim) anim.SetBool(isDeadParam, true);
    }

    public void PlayAttack()
    {
        if (anim) anim.SetTrigger(attackParam);
    }
}