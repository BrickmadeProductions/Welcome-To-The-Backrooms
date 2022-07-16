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
        Debug.Log("BloodFLoor");
        int collCount = bloodShoot.GetSafeCollisionEventSize();

        if (collCount > CollisionEvents.Count)
            CollisionEvents = new List<ParticleCollisionEvent>(collCount);

        int eventCount = bloodShoot.GetCollisionEvents(other, CollisionEvents);

        for (int i = 0; i < eventCount; i++)
        {
            GameObject splat = Instantiate(bloodSurfaceSplats[Random.Range(0, bloodSurfaceSplats.Length)], CollisionEvents[i].intersection, Quaternion.LookRotation(CollisionEvents[i].normal));
            splat.transform.localScale = (Vector3.one * 0.25f) * Random.Range(0.7f, 1.3f);
            GameSettings.Instance.worldInstance.globalBloodAndGoreObjects.Add(splat);
            if (GameSettings.Instance.BloodAndGore == false)
            {
                gameObject.SetActive(false);
            }

        }

    }
}
