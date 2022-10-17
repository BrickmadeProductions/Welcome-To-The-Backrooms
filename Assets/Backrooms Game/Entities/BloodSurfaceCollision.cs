using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSurfaceCollision : MonoBehaviour
{
    ParticleSystem bloodShoot;
    public GameObject[] bloodSurfaceSplats;
    private List<ParticleCollisionEvent> CollisionEvents;

    private void Awake()
    {
        CollisionEvents = new List<ParticleCollisionEvent>();
        bloodShoot = GetComponent<ParticleSystem>();
    }

    // Start is called before the first frame update
    public void OnParticleCollision(GameObject other)
    {
        //Debug.Log("BloodFLoor");
        int collCount = bloodShoot.GetSafeCollisionEventSize();

        if (collCount > CollisionEvents.Count)
            CollisionEvents = new List<ParticleCollisionEvent>(collCount);

        int eventCount = bloodShoot.GetCollisionEvents(other, CollisionEvents);

        for (int i = 0; i < eventCount; i++)
        {
            GameObject splat = Instantiate(bloodSurfaceSplats[Random.Range(0, bloodSurfaceSplats.Length)], CollisionEvents[i].intersection, Quaternion.LookRotation(CollisionEvents[i].normal));
            splat.transform.localScale = (Vector3.one * 0.25f) * Random.Range(0.7f, 1.3f);

            if (!other.transform.parent.gameObject.GetComponent<DeferredDecalRenderer>() 
                && other.gameObject.layer != 11
                && other.gameObject.layer != 12
                && other.gameObject.layer != 22
                && other.gameObject.layer != 21
                && other.gameObject.layer != 9
                && other.gameObject.layer != 10)
            {
                other.transform.parent.gameObject.AddComponent<DeferredDecalRenderer>();
                splat.transform.parent = other.transform;
            }
                

            GameSettings.Instance.worldInstance.globalBloodAndGoreObjects.Add(splat);

        }

    }
}
