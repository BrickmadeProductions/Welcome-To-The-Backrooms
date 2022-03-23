using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SmilerAI : Entity
{
    GameObject player;
    Collider col;
    Rigidbody body;
    Renderer renderer;

    public Texture2D normal;
    public Texture2D attacking;

    Vector3 nextMovePosition = Vector3.zero;
    Coroutine turnAround = null;
    bool turningAround;
    // Start is called before the first frame update
    void Start()
    {
        renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        col = GetComponent<BoxCollider>();
        body = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectsWithTag("Player")[0];

        StartCoroutine(RandomWandering());
        turningAround = false;
    }

    // Update is called once per frame
    void Update()
    {
        

        float distance = Vector3.Distance(player.transform.position, transform.position);
        float move = speed * Time.deltaTime;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (distance < 30)
        {
            
            if (distance < 10)
            {
                renderer.material.SetTexture("_MainTex", attacking);
            }
            if (distance < 2)
            {
                GameSettings.Instance.LoadScene("HomeScreen");
            }

            RenderSettings.ambientIntensity = 1 + (distance / 30);
            RenderSettings.reflectionIntensity = (distance / 30);

            GameSettings.Instance.Chrom.intensity.value = 1 - (distance / 30);
            GameSettings.Instance.Grain.intensity.value = 1 - (distance / 30);

            /*GameObject[] allLights = GameObject.FindGameObjectsWithTag("Light");

            foreach (GameObject i in allLights)
            {
                if (i.GetComponent<Light>() != null)
                {
                    //i.GetComponent<Light>().intensity = (distance / 30);
                }
                    
                else
                {
                    //i.GetComponent<Renderer>().material.SetVector("_EmissionColor", new Vector4(0.8196f, 0.783f, 0) * ((distance / 30) - 4f));
                }
            }*/

            if (Vector3.Distance(player.transform.position, transform.position) > 5)
            {
                col.enabled = false;
            }
            else
            {
                col.enabled = true;

            }
            transform.LookAt(player.transform);

            
            nextMovePosition = player.transform.position;

            
        }

        else
        {
            renderer.material.SetTexture("_MainTex", normal);

            if (distance >= 30) {


                Mathf.Lerp(GameSettings.Instance.Chrom.intensity.value, 0.081f, Time.deltaTime * 5);
                Mathf.Lerp(GameSettings.Instance.Grain.intensity.value, 0f, Time.deltaTime * 5);

                col.enabled = false;

                if (Physics.Raycast(transform.position, fwd, 1.4f) && !turningAround)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-transform.forward, Vector3.up), Time.deltaTime * 5f);

                    nextMovePosition = transform.position - fwd * 10f;

                    if (turnAround == null)
                        turnAround = StartCoroutine(TurnAround());

                }

                if (distance > 80)
                {
                    Destroy(gameObject);
                }

            }
        }

        transform.position = Vector3.MoveTowards(transform.position, nextMovePosition, move);
        transform.LookAt(nextMovePosition);

        Debug.DrawRay(transform.position + fwd, fwd * 1.4f, Color.green);

        
    }
    IEnumerator TurnAround()
    {
        turningAround = true;
        yield return new WaitForSeconds(0.1f);
        turningAround = false;

    }
    IEnumerator RandomWandering()
    {
        while (true)
        {
            nextMovePosition = new Vector3(transform.position.x + Random.Range(-10, 10), transform.position.y, transform.position.y + Random.Range(-10, 10));

            yield return new WaitForSeconds(5);
        }
    }
}
