using UnityEngine;
using CircusClash.Troops.Movement;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance;

    [Header("Spawn Settings")]
    public Transform playerSpawn;     
    public Transform enemySpawn;     
    public bool playerSideMovesRight = true;

    [Header("Prefabs")]
    public GameObject clownPrefab;
    public GameObject magicianPrefab;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GameObject SpawnPlayerUnit(GameObject prefab)
    {
        if (!prefab || !playerSpawn) return null;
        var go = Instantiate(prefab, playerSpawn.position, Quaternion.identity);

        var mover = go.GetComponent<UnitMover2D>();
        if (mover) mover.SetSide(playerSideMovesRight); 

        var cmd = go.GetComponent<UnitCommandReceiver>();
        if (cmd) cmd.isPlayerUnit = true;

        var dir = BattleDirector.Instance;
        if (dir != null) cmd?.SendMessage("HandleStance", dir.CurrentStance, SendMessageOptions.DontRequireReceiver);

        return go;
    }

    public void BtnSpawnClown() => SpawnPlayerUnit(clownPrefab);
    public void BtnSpawnMagician() => SpawnPlayerUnit(magicianPrefab);
}
