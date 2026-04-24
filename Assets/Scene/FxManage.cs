using System.Collections;
using UnityEngine;

public class FxManage : MonoBehaviour
{
    public static FxManage Instance { get; private set; }

    public GameObject fxSlashPrefab;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnSlash(Transform spawnPoint)
    {
        GameObject fx = Instantiate(fxSlashPrefab, spawnPoint.position, fxSlashPrefab.transform.rotation);
        fx.transform.localScale = fxSlashPrefab.transform.localScale;
        StartCoroutine(DestroyAfterEffect(fx));
    }

    private IEnumerator DestroyAfterEffect(GameObject fx)
    {
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            yield return new WaitUntil(() => !ps.IsAlive(true));
        }

        Destroy(fx);
    }
}
