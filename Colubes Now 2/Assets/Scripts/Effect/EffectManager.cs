using System.Collections;
using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    public GameObject[] effects;
    Pooler[] effectPools = new Pooler[7];

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < effects.Length; i++)
        {
            effectPools[i] = new Pooler(effects[i], 5);
        }
    }

    public void Play(int i, Vector3 pos)
    {
        GameObject tmpGameObject = effectPools[i].Get(pos, Quaternion.identity);
        ParticleSystem particleSystem = tmpGameObject.GetComponent<ParticleSystem>();
        particleSystem.Play();
        StartCoroutine(Disable(i, particleSystem.main.duration, tmpGameObject));
    }

    IEnumerator Disable(int i, float time, GameObject go)
    {
        yield return new WaitForSeconds(time);
        go.GetComponent<ParticleSystem>().Stop();
        effectPools[i].Free(go);
    }

}
