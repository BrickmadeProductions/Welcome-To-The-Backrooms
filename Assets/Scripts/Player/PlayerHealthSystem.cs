using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class PlayerHealthSystem : MonoBehaviour
{
    PlayerController player;

    public float health = 100.0f;
    public TextMeshProUGUI healthText;

    public float hunger = 100.0f;
    public TextMeshProUGUI hungerText;

    public float thirst = 100.0f;
    public TextMeshProUGUI thirstText;

    public float stamina = 100.0f;

    public float sanity = 100.0f;
    public TextMeshProUGUI sanityText;

    public int heartRate = 90;
    public TextMeshProUGUI heartRateText;

    public float bodyTemperature = 98.6f;
    public TextMeshProUGUI bodyTemperatureText;

    public bool adrenalineActive = false;
    public bool canUseAdrenaline = true;


    public bool canRun = true;
    public bool canWalk = true;
    public bool canJump = true;
    public bool canMoveHead = true;
    public bool awake = true;

    public Coroutine waking = null;
    public Coroutine sleeping = null;

    public Animator animator;
    
    //audio
    public AudioSource heartBeatSource;
    public AudioMixerGroup heartBeatMixer;

    public AudioSource adrenalineAudio;

    // Start is called before the first frame update
    void Awake()
    {

        player = GetComponent<PlayerController>();

        if (SceneManager.GetActiveScene().name == "RoomHomeScreen")

            WakeUp();

        else
        {
            player.animatorCamera.gameObject.SetActive(false);
            player.playerCamera.gameObject.SetActive(true);

        }
        

        StartCoroutine(UpdateHealth());

        //StartCoroutine(GetComponent<Blinking>().RandomBlinking());

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Blink"))
        {
            GetComponent<Blinking>().eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
        }
        else if (Input.GetButtonUp("Blink"))
        {
            GetComponent<Blinking>().eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
        }

        healthText.text = (int)health + " HP";
        //hungerText.text = (int)hunger + " FP";
        heartRateText.text = (int)heartRate + " BPM";
        thirstText.text = (int)thirst + " TP";

        heartBeatMixer.audioMixer.SetFloat("pitchBend", 1f / (heartRate / 90f));
        heartBeatSource.pitch = heartRate / 90f;
        heartBeatSource.volume = (heartRate / 100f) - 1f;
    }

    public void WakeUp()
    {
        if (!awake)
        {
            StartCoroutine(GetComponent<Blinking>().WakeUp());
            waking = StartCoroutine(WakeUpSequence());
        }
            
    }
    public void Sleep()
    {
        if (awake)
            sleeping = StartCoroutine(SleepSequence());
    }

    IEnumerator WakeUpSequence()
    {

        animator.speed = 0.3f;
        player.arms.SetActive(false);

        animator.SetBool("isSleeping", false);
        animator.SetBool("isWaking", true);

        canMoveHead = false;

        player.animatorCamera.gameObject.SetActive(true);
        player.playerCamera.gameObject.SetActive(false);

        yield return new WaitForSeconds(4.417f * 1.95f);

        animator.speed = 1;

        yield return new WaitForSeconds(1.1f);

        player.gameObject.transform.position = new Vector3(2.668f, 2.997f, 1.12f);
        player.animatorCamera.gameObject.SetActive(false);
        player.playerCamera.gameObject.SetActive(true);

        canMoveHead = true;

        awake = true;

        player.arms.SetActive(true);

    }

    IEnumerator SleepSequence()
    {

        animator.SetBool("isSleeping", true);
        animator.SetBool("isWaking", false);

        canMoveHead = false;

        player.animatorCamera.gameObject.SetActive(true);
        player.playerCamera.gameObject.SetActive(false);

        yield return new WaitForSeconds(4.417f);

        player.gameObject.transform.position = new Vector3(2.668f, 2.997f, 1.12f);
        player.animatorCamera.gameObject.SetActive(false);
        player.playerCamera.gameObject.SetActive(true);

        canMoveHead = true;

        awake = false;


    }

    public IEnumerator UpdateHealth()
    {
        while (true)
        {
            hunger -= 1;
            sanity -= 1;

            yield return new WaitForSeconds(5f);
        }
    }

    public IEnumerator Run()
    {
        while (true)
        {
            ChangeHeartRate(1);

            yield return new WaitForSeconds(Random.Range(1, 3));
        }
    }

    public IEnumerator ActivateAdrenaline()
    {
        adrenalineAudio.Play();

        adrenalineActive = true;
        player.adrenalineSpeedMultiplier = 1.35f;

        while (adrenalineActive)
        {
            
            ChangeHeartRate(Random.Range(2f, 4f));
            health -= 0.5f;
            yield return new WaitForSeconds(1f);
            if (heartRate >= 140)
            {
                player.adrenalineSpeedMultiplier = 1f;
                StartCoroutine(AdrenalineCooldown());
                adrenalineActive = false;
                
            }
            
        }
    }
    public IEnumerator AdrenalineCooldown()
    {
        adrenalineAudio.Stop();

        canUseAdrenaline = false;
        yield return new WaitForSeconds(300f);
        canUseAdrenaline = true;
    }


    public IEnumerator Walk()
    {
        while (true)
        {
            ChangeHeartRate(1);

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
            ChangeHeartRate(Random.Range(-2, 2));
            yield return new WaitForSeconds(5f);
            
        }
       
    }
    public IEnumerator ReviveHeartRate()
    {
        
        while (true)
        {
            ChangeHeartRate(Random.Range(-3, -4));
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
        heartRate += (int)amount;
        
        //heartRateText.text = "HeartRate: " + heartRate + " BPM";

    }

}
