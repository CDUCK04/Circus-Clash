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

        var jitter = new Vector3(Random.Range(-0.20f, 0.20f), Random.Range(-0.15f, 0.15f), 0f);

        var go = Instantiate(prefab, enemySpawn.position + jitter, Quaternion.identity);

        var brain = go.GetComponent<UnitBrain>();
        if (brain)
        {
            brain.isPlayerUnit = false;
            brain.startStopped = false;

            go.transform.position += new Vector3(-0.35f, 0f, 0f);
        }

        if (enemyFormation != null && brain != null)
        {
            enemyFormation.Register(brain);
        }

        return go;
    }
}