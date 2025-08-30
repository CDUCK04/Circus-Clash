using UnityEngine;
using CircusClash.Troops.Movement;

[RequireComponent(typeof(UnitMover2D))]
public class UnitCommandReceiver : MonoBehaviour
{
    [Header("Side")]
    public bool isPlayerUnit = true;

    [Header("Retreat")]
    public float retreatStopDistance = 1.2f;

    UnitMover2D mover;
    BattleDirector dir;
    Transform playerTent;

    void Awake()
    {
        mover = GetComponent<UnitMover2D>();
    }

    void OnEnable()
    {
        dir = BattleDirector.Instance;
        if (dir != null)
        {
            dir.OnStanceChanged += HandleStance;
            playerTent = dir.playerTent;
            HandleStance(dir.CurrentStance);
        }
    }

    void OnDisable()
    {
        if (dir != null) dir.OnStanceChanged -= HandleStance;
    }

    void HandleStance(ArmyStance s)
    {
        if (mover == null) return;

        switch (s)
        {
            case ArmyStance.Attack:
                mover.SetSide(isPlayerUnit); 
                mover.Resume();              
                break;

            case ArmyStance.Defend:
                mover.Stop();                
                break;

            case ArmyStance.Retreat:
                mover.SetSide(!isPlayerUnit);
                mover.Resume();
                break;
        }
    }

    void Update()
    {
        if (dir == null || mover == null) return;

        if (dir.CurrentStance == ArmyStance.Retreat && playerTent != null)
        {
            float dx = Mathf.Abs(transform.position.x - playerTent.position.x);
            if (dx <= Mathf.Max(retreatStopDistance, dir.tentStopRadius))
            {
                mover.Stop();
            }
        }

    }
}
