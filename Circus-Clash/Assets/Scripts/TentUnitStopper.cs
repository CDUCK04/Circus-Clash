using UnityEngine;
using CircusClash.Troops.Movement;

public class TentUnitStopper : MonoBehaviour
{
    void Awake()
    {
        var mover = GetComponent<UnitMover2D>();
        if (mover) mover.Stop(); // tents do not move
    }
}
