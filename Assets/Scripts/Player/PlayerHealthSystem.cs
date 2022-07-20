// PlayerHealthSystem
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PlayerHealthSystem : MonoBehaviour
{
	
	private PlayerController player;

	public float health = 100f;

	public TextMeshProUGUI healthText;

	public float hunger = 100f;

	public TextMeshProUGUI hungerText;

	public float thirst = 100f;

	public TextMeshProUGUI thirstText;

	public float stamina = 100f;

	public float sanity = 100f;

	public TextMeshProUGUI sanityText;

	public int heartRate = 90;

	public TextMeshProUGUI heartRateText;

	public float bodyTemperature = 98.6f;

	public TextMeshProUGUI bodyTemperatureText;

	public bool adrenalineActive;

	public bool canUseAdrenaline = true;

	public bool adrenalineCoolDownActive;

	public bool calmingActive;

	public bool canRun = true;

	public bool canWalk = true;

	public bool canJump = true;

	public bool canCrouch = true;

	public bool canMoveHead = true;

	public bool awake;

	public Coroutine waking;

	public Coroutine sleeping;

	public Coroutine calmingDown;

	public Animator animator;

	public AudioSource heartBeatSource;

	public AudioMixerGroup heartBeatMixer;

	public AudioSource adrenalineAudio;

	private void Awake()
	{
		player = GetComponent<PlayerController>();
		StartCoroutine(UpdateHealth());
	}

	private void Update()
	{
		if (Input.GetButton("Blink") && !GameSettings.Instance.IsCutScene)
		{
			GetComponent<Blinking>().eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
			if (calmingDown == null)
			{
				calmingDown = StartCoroutine(Calm());
			}
		}
		if (Input.GetButtonUp("Blink") && !GameSettings.Instance.IsCutScene)
		{
			GetComponent<Blinking>().eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
			StopCoroutine(calmingDown);
			calmingDown = null;
		}
		healthText.text = ((int)health).ToString() ?? "";
		TextMeshProUGUI textMeshProUGUI = heartRateText;
		int num = heartRate;
		textMeshProUGUI.text = num.ToString() ?? "";
		thirstText.text = ((int)thirst).ToString() ?? "";
		sanityText.text = ((int)sanity).ToString() ?? "";
		heartBeatSource.pitch = (float)heartRate / 90f;
		heartBeatSource.volume = (float)heartRate / 100f - 1f;
	}

	public void WakeUpRoom()
	{
		if (!awake)
		{
			StartCoroutine(GetComponent<Blinking>().WakeUpRoom());
			waking = StartCoroutine(WakeUpSequenceRoom());
		}
	}

	public void WakeUpOther()
	{
		if (!awake)
		{
			StartCoroutine(GetComponent<Blinking>().WakeUpOther());
			waking = StartCoroutine(WakeUpSequenceOther());
		}
	}

	public void Sleep()
	{
		if (awake)
		{
			sleeping = StartCoroutine(SleepSequence());
		}
	}

	public IEnumerator Calm()
	{
		while (true)
		{
			if (health < 100f)
			{
				health += 1f;
			}
			if (sanity < 100f)
			{
				sanity += 1f;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	public IEnumerator NaturalRegen()
	{
		while (health < 100f)
		{
			health += 1f;
			yield return new WaitForSeconds(0.25f);
		}
	}

	private IEnumerator WakeUpSequenceRoom()
	{
		GameSettings.Instance.setCutScene(tf: true);
		animator.speed = 0.3f;
		player.arms.SetActive(false);
		animator.SetBool("isSleeping", false);
		animator.SetBool("isWakingRoom", true);
		canMoveHead = false;
		player.animatorCamera.gameObject.SetActive(true);
		player.playerCamera.gameObject.SetActive(false);
		yield return new WaitForSeconds(8.61315f);
		animator.speed = 1f;
		yield return new WaitForSeconds(1.1f);
		player.gameObject.transform.position = new Vector3(2.668f, 2.997f, 1.12f);
		player.animatorCamera.gameObject.SetActive(false);
		player.playerCamera.gameObject.SetActive(true);
		canMoveHead = true;
		awake = true;
		animator.SetBool("isWakingRoom", false);
		player.arms.SetActive(true);
		GameSettings.Instance.setCutScene(tf: false);
	}

	private IEnumerator WakeUpSequenceOther()
	{
		GameSettings.Instance.setCutScene(tf: true);

		player.bodyAnim.SetBool("isSleeping", false);
		player.bodyAnim.SetBool("isWakingOther", true);

		canMoveHead = false;

		player.animatorCamera.gameObject.SetActive(true);
		player.playerCamera.gameObject.SetActive(false);

		animator.speed = 1f;

		yield return new WaitForSeconds(2f);

		player.animatorCamera.gameObject.SetActive(false);
		player.playerCamera.gameObject.SetActive(true);

		canMoveHead = true;
		awake = true;

		player.bodyAnim.SetBool("isWakingOther", false);

		GameSettings.Instance.setCutScene(tf: false);
	}

	private IEnumerator SleepSequence()
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
			hunger -= 0.5f;
			if (SceneManager.GetActiveScene().name != "RoomHomeScreen" && sanity > 0f)
			{
				sanity -= 0.1f;
			}
			if (sanity <= 0f)
			{
				sanity = 0f;
				health -= 1f;
			}
			yield return new WaitForSeconds(2f);
		}
	}

	public IEnumerator Run()
	{
		while (true)
		{
			ChangeHeartRate(1f);
			yield return new WaitForSeconds(Random.Range(1, 3));
		}
	}

	public IEnumerator ActivateAdrenaline()
	{
		adrenalineAudio.Play();
		adrenalineActive = true;
		player.adrenalineSpeedMultiplier = 1.35f;
		float pass = 20000f;
		while (adrenalineActive)
		{
			ChangeHeartRate(Random.Range(2f, 4f));
			health -= 0.1f;
			if (pass > 1500f)
			{
				pass -= 1500f;
				GameSettings.Instance.Master.SetFloat("cutoffFrequency", pass);
			}
			if (heartRate >= 150)
			{
				player.adrenalineSpeedMultiplier = 1f;
				adrenalineActive = false;
			}
			yield return new WaitForSeconds(0.5f);
		}
		canUseAdrenaline = false;
		adrenalineCoolDownActive = true;
	}

	public IEnumerator AdrenalineCooldown()
	{
		bool freqFix = true;
		GameSettings.Instance.Master.GetFloat("cutoffFrequency", out var pass);
		while (freqFix)
		{
			if (pass < 20000f)
			{
				pass += 1500f;
				GameSettings.Instance.Master.SetFloat("cutoffFrequency", pass);
			}
			else
			{
				freqFix = false;
			}
			yield return new WaitForSeconds(0.5f);
			Debug.Log("Fixing Audio");
		}
		adrenalineAudio.Stop();
		yield return new WaitForSeconds(300f);
		canUseAdrenaline = true;
	}

	public IEnumerator Walk()
	{
		while (true)
		{
			ChangeHeartRate(1f);
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
	}
}
