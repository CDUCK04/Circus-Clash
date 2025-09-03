using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public Transform playerSpawn;
    public GameObject clownPrefab;
    public GameObject magicianPrefab;
    public FormationManager playerFormation;
    public bool enforceMax20 = true;

    public GameObject SpawnClown() => SpawnPlayerUnit(clownPrefab);
    public GameObject SpawnMagician() => SpawnPlayerUnit(magicianPrefab);

    public void BtnSpawnClown()
    {
        SpawnClown();
    }

    public void BtnSpawnMagician()
    {
        SpawnMagician();
    }

    GameObject SpawnPlayerUnit(GameObject prefab)
    {
        if (!prefab || !playerSpawn) return null;

        if (!TroopCounter.Instance || !TroopCounter.Instance.CanSpawnPlayer())
        {
            Debug.Log("Player troop limit reached.");
            return null;
        }

        var go = Instantiate(prefab, playerSpawn.position, Quaternion.identity);
        var brain = go.GetComponent<UnitBrain>();
        if (brain)
        {
            brain.isPlayerUnit = true;
            brain.startStopped = false;  
        }

        if (playerFormation != null)
            playerFormation.Register(brain);

        if (!TroopCounter.Instance.Register(brain))
        {
            Debug.Log("Could not register unit in TroopCounter; destroying.");
            Destroy(go);
            return null;
        }

        return go;
    }


    bool playerFormationCapacityFull()
    {
        // simple check: compare child count or track internally; here quick heuristic:
        // Ideally FormationManager exposes a "IsFull" but to keep it minimal, you can inspect in FormMgr.
        return false; // replace with real check if you want a hard cap
    }
}