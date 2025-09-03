using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform enemySpawn;
    public GameObject clownEnemyPrefab;
    public GameObject magicianEnemyPrefab;
    public FormationManager enemyFormation;   
    public bool enforceMax20 = true;

    public GameObject SpawnClownEnemy() => SpawnEnemy(clownEnemyPrefab);
    public GameObject SpawnMagicianEnemy() => SpawnEnemy(magicianEnemyPrefab);

    GameObject SpawnEnemy(GameObject prefab)
    {
        if (!prefab || !enemySpawn) return null;
        var go = Instantiate(prefab, enemySpawn.position, Quaternion.identity);

        var brain = go.GetComponent<UnitBrain>();
        if (brain)
        {
            brain.isPlayerUnit = false;     
            brain.startStopped = false;    
        }

        if (enemyFormation != null)
        {
            enemyFormation.Register(brain);
        }

        return go;
    }
}