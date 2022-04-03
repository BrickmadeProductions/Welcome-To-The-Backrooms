using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSystem : MonoBehaviour
{
    public float health = 100.0f;
    public float hunger = 100.0f;
    public float thirst = 100.0f;
    public float stamina = 100.0f;
    public float sanity = 100.0f;

    public int heartRate = 90;

    public float bodyTemperature = 98.6f;

    public bool canRun = true;
    public bool canWalk = true;
    public bool canJump = true;

    public Text heartRateText;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateHealth());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator UpdateHealth()
    {
        while (true)
        {
            hunger *= 0.99f;
            sanity *= 0.99f;

            yield return new WaitForSeconds(60f);
        }
    }

    public IEnumerator Run()
    {
        while (true)
        {
            ChangeHeartRate(heartRate + 2);

            yield return new WaitForSeconds(Random.Range(1, 3));
        }
    }

    public IEnumerator Walk()
    {
        while (true)
        {
            ChangeHeartRate(heartRate + 1);

            yield return new WaitForSeconds(10f);
        }
    }

    public void Spook()
    {
        sanity -= 2f;
        
    }

    public IEnumerator RandomHeartRate()
    {
        while (true)
        {
            ChangeHeartRate(heartRate + Random.Range(-2, 2));
            yield return new WaitForSeconds(5f);
            
        }
       
    }
    public IEnumerator ReviveHeartRate()
    {
        
        while (true)
        {
            ChangeHeartRate(heartRate - Random.Range(2, 3));
            yield return new WaitForSeconds(2f);
            
        }
        
        
        
    }
    public IEnumerator ChangeStamina(float amount)
    {

        while (true)
        {
            stamina += amount;
            yield return new WaitForSeconds(0.5f);
        }

 
    }

    public void ChangeHeartRate(float amount)
    {
        heartRate = (int)amount;
        
        heartRateText.text = "HeartRate: " + heartRate + " BPM";

    }

}
