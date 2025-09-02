using UnityEngine;

[RequireComponent(typeof(CircusClash.Troops.Movement.UnitMover2D))]
public class UnitYSteering2D : MonoBehaviour
{
    public CircusClash.Troops.AI.UnitSensor2D sensor; 
    public CircusClash.Troops.Movement.UnitMover2D mover; 
    public CircusClash.Troops.Combat.UnitHealth health;   

    [Header("Y Movement")]
    public float verticalSpeed = 1.5f;    
    public float laneY = 0f;               
    public float laneJitter = 0.3f;       
    public float engageYOffset = 0.0f;     

    [Header("When to seek target Y")]
    public float seekSightMultiplier = 0.9f;  
    public bool onlyWhenAdvancing = true;   

    float _myLaneY;

    void Awake()
    {
        if (!sensor) sensor = GetComponent<CircusClash.Troops.AI.UnitSensor2D>();
        if (!mover) mover = GetComponent<CircusClash.Troops.Movement.UnitMover2D>();
        if (!health) health = GetComponent<CircusClash.Troops.Combat.UnitHealth>();
        _myLaneY = laneY + Random.Range(-laneJitter, laneJitter);
    }

    void Update()
    {
        if (health != null && health.IsDead) return;
        if (mover == null) return;


        if (onlyWhenAdvancing && mover.IsStopped) return;

        float targetY = _myLaneY;

        if (sensor != null)
        {
            Transform t = sensor.FindClosestEnemy(); 
            if (t != null)
            {
                float sqr = (t.position - transform.position).sqrMagnitude;
                float sight = sensor.sightRange * seekSightMultiplier;
                if (sqr <= sight * sight)
                {
                    targetY = t.position.y + engageYOffset;
                }
            }
        }

        Vector3 pos = transform.position;
        float newY = Mathf.MoveTowards(pos.y, targetY, verticalSpeed * Time.deltaTime);
        transform.position = new Vector3(pos.x, newY, pos.z);
    }
}