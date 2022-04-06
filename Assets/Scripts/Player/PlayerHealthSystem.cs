using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealthSystem : MonoBehaviour
{
    PlayerController player;

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
    public bool canMoveHead = true;
    public bool awake = true;

    Coroutine waking = null;
    public Animator wakeUpAnimator;

    public Text heartRateText;

    // Start is called before the first frame update
    void Awake()
    {

        player = GetComponent<PlayerController>();

        if (SceneManager.GetActiveScene().name == "RoomHomeScreen")

            WakeUp();

        else
        {
            player.wakeUpCamera.gameObject.SetActive(false);
            player.playerCamera.gameObject.SetActive(true);

        }
        

        StartCoroutine(UpdateHealth());


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WakeUp()
    {
        awake = false;
        waking = StartCoroutine(WakeUpSequence());
    }

    IEnumerator WakeUpSequence()
    {
        wakeUpAnimator.SetBool("isWaking", true);

        canMoveHead = false;

        player.wakeUpCamera.gameObject.SetActive(true);
        player.playerCamera.gameObject.SetActive(false);

        yield return new WaitForSeconds(4.417f);

        player.gameObject.transform.position = new Vector3(2.668f, 2.997f, 1.12f);
        player.wakeUpCamera.gameObject.SetActive(false);
        player.playerCamera.gameObject.SetActive(true);

        canMoveHead = true;

        awake = true;


    }

    public void Sleep()
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
