// PlayerHealthSystem
using System;
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

	public bool isBeingDamaged = false;

	public bool isJustDamaged = false;

	public Coroutine waking;

	public Coroutine sleeping;

	public Coroutine calmingDown;

	public Animator animator;

	public AudioSource heartBeatSource;

	public AudioMixerGroup heartBeatMixer;

	public AudioSource earStatusAudio;

	public Animator attackIndicator;

	Quaternion damageTargetFXRotation = Quaternion.identity;

	public void LoadInData(PlayerSaveData saveData)
    {
		health = saveData.healthSaved;

		hunger = saveData.hungerSaved;

		thirst = saveData.thirstSaved;

		sanity = saveData.sanitySaved;

		stamina = saveData.staminaSaved;

		bodyTemperature = saveData.bodyTemperatureSaved;


		canWalk = saveData.canWalkSaved;

		canRun = saveData.canWalkSaved;

		canJump = saveData.canJumpSaved;

		canCrouch = saveData.canCrouchSaved;

	}
	private void Awake()
	{
		player = GetComponent<PlayerController>();

		StartCoroutine(UpdateHealth());
		StartCoroutine(NaturalRegen());
	}

	void Update()
	{
		if (Input.GetButton("Blink") && !GameSettings.Instance.IsCutScene)
		{
			GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);
			if (calmingDown == null)
			{
				calmingDown = StartCoroutine(Calm());
			}
		}
		if (Input.GetButtonUp("Blink") && !GameSettings.Instance.IsCutScene)
		{
			GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", false);
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
			//StartCoroutine(GetComponent<Blinking>().WakeUpRoom());
			//waking = StartCoroutine(WakeUpSequenceRoom());
		}
	}

	public void WakeUpOther()
	{
		if (!awake)
		{
			//StartCoroutine(GetComponent<Blinking>().WakeUpOther());
			//waking = StartCoroutine(WakeUpSequenceOther());
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
		yield return new WaitForSecondsRealtime(2f);

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
			yield return new WaitForSecondsRealtime(1f);
		}
	}

	public IEnumerator NaturalRegen()
	{
		while (true)
        {
			if (health < 100f)
			{
				health += UnityEngine.Random.Range(0.25f, 0.5f);
				yield return new WaitForSecondsRealtime(2f);
			}
			else
			{
				health = 100f;
				yield return null;
			}
		}
		
	}

	private IEnumerator SleepSequence()
	{
		animator.SetBool("isSleeping", true);
		animator.SetBool("isWaking", false);
		canMoveHead = false;
		player.playerCamera.gameObject.SetActive(false);
		yield return new WaitForSecondsRealtime(4.417f);
		player.gameObject.transform.position = new Vector3(2.668f, 2.997f, 1.12f);
		player.playerCamera.gameObject.SetActive(true);
		canMoveHead = true;
		awake = false;
	}

	public IEnumerator UpdateHealth()
	{
		while (true)
		{
			ChangeHunger(player.currentPlayerState == PlayerController.PLAYERSTATES.RUN ? -0.5f : -0.23f);
			ChangeThirst(player.currentPlayerState == PlayerController.PLAYERSTATES.RUN ? -0.35f : -0.2f);

			if (SceneManager.GetActiveScene().name != "RoomHomeScreen" && sanity > 0f)
			{
				sanity -= 0.2f;
			}
			if (sanity <= 0f)
			{
				sanity = 0f;
				TakeDamage(5f, 0f, 2f);
			}
			if (thirst <= 0f)
			{
				TakeDamage(2f, 0f, 2f);
				canRun = false;
			}
			else if (thirst > 0)
				canRun = true;
			/*if (hunger <= 0f)
			{
				hunger = 0f;
				health -= 3f;
			}*/
			yield return new WaitForSecondsRealtime(2f);
		}
	}

	public IEnumerator Run()
	{
		while (true)
		{
			ChangeHeartRate(1f);
			yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(1, 3));
		}
	}

	public IEnumerator ActivateAdrenaline()
	{
		earStatusAudio.Play();
		adrenalineActive = true;
		player.adrenalineSpeedMultiplier = 1.45f;
		float pass = 20000f;

		while (adrenalineActive)
		{
			ChangeHeartRate(UnityEngine.Random.Range(2f, 4f));
			health -= 0.1f;
			if (pass > 1500f)
			{
				pass -= 1500f;
				GameSettings.Instance.audioHandler.master.SetFloat("cutoffFrequency", pass);
			}
			if (heartRate >= 150)
			{
				player.adrenalineSpeedMultiplier = 1f;
				adrenalineActive = false;
			}
			yield return new WaitForSecondsRealtime(0.5f);
		}

		canUseAdrenaline = false;
		adrenalineCoolDownActive = true;
	}

	public IEnumerator AdrenalineCooldown()
	{
		bool freqFix = true;
		GameSettings.Instance.audioHandler.master.GetFloat("cutoffFrequency", out var pass);
		while (freqFix)
		{
			if (pass < 20000f)
			{
				pass += 1500f;
				GameSettings.Instance.audioHandler.master.SetFloat("cutoffFrequency", pass);
			}
			else
			{
				freqFix = false;
			}
			yield return new WaitForSecondsRealtime(0.5f);
			Debug.Log("Fixing Audio");
		}
		earStatusAudio.Stop();
		yield return new WaitForSecondsRealtime(300f);
		canUseAdrenaline = true;
	}

	public IEnumerator Walk()
	{
		while (true)
		{
			ChangeHeartRate(1f);
			yield return new WaitForSecondsRealtime(10f);
		}
	}

	public void DecreaseSanity(float amount)
	{
		sanity -= amount;
	}

	public IEnumerator RandomHeartRate()
	{
		while (true)
		{
			ChangeHeartRate(UnityEngine.Random.Range(-2, 2));
			yield return new WaitForSecondsRealtime(5f);
		}
	}

	public IEnumerator ReviveHeartRate()
	{
		while (true)
		{
			ChangeHeartRate(UnityEngine.Random.Range(-3, -4));
			yield return new WaitForSecondsRealtime(2f);
		}
	}

	public IEnumerator ChangeStaminaOverTime(float amount)
	{
		while (true)
		{
			stamina += amount;
			yield return new WaitForSecondsRealtime(0.5f);
		}
	}
	public void ChangeStamina(float amount)
	{
		stamina += amount;
		stamina = Mathf.Clamp(stamina, 0f, 100f);

	}
	public void ChangeThirst(float amount)
	{
		thirst += amount;
		thirst = Mathf.Clamp(thirst, 0f, 100f);
		
	}

	public void ChangeHunger(float amount)
	{
		hunger += amount;
		hunger = Mathf.Clamp(hunger, 0f, 100f);

	}

	public void ChangeHeartRate(float amount)
	{
		heartRate += (int)amount;
		heartRate = Mathf.Clamp(heartRate, 0, 200);
	}

	public void TakeDamage(float damageSubtraction, float sanityMultipler, float heartrateIncrease)
    {
		player.playerHealth.health -= damageSubtraction;
		player.playerHealth.sanity *= sanityMultipler;
		player.playerHealth.ChangeHeartRate(heartrateIncrease);

		attackIndicator.ResetTrigger("Hit");

		StartCoroutine(TakeDamageFX(damageSubtraction));

		attackIndicator.SetTrigger("Hit");

		player.playerNoises.clip = player.hitSounds[UnityEngine.Random.Range(0, player.hitSounds.Length)];
		player.playerNoises.Play();

	}
	IEnumerator TakeDamageFX(float damageTaken)
    {
		isBeingDamaged = true;

		isJustDamaged = true;


		while (isBeingDamaged)
        {
			if (isJustDamaged)
			{
				float x = UnityEngine.Random.Range(-3, 3f);
				float y = UnityEngine.Random.Range(-3, 3f);
				float z = UnityEngine.Random.Range(-3, 3f);

				x = (Mathf.Round(x) + 1) * damageTaken / 2f;
				y = (Mathf.Round(y) + 1) * damageTaken / 2f;
				z = (Mathf.Round(z) + 1) * damageTaken / 2f;

				damageTargetFXRotation = Quaternion.Euler(x, y, z);
				isJustDamaged = false;
				isBeingDamaged = true;
			}

			DamageCameraFX(damageTargetFXRotation);

			yield return new WaitForEndOfFrame();
		}
		
    }
	void DamageCameraFX(Quaternion lerpTo)
    {
		//Debug.Log(player.playerCamera.transform.localRotation.eulerAngles + " " + Quaternion.Euler(amountX, amountY, amountZ));

		if (Mathf.Abs(player.playerCamera.transform.localRotation.x) < Mathf.Abs(lerpTo.x) - 1
			&& Mathf.Abs(player.playerCamera.transform.localRotation.y) < Mathf.Abs(lerpTo.y) - 1
			&& Mathf.Abs(player.playerCamera.transform.localRotation.z) < Mathf.Abs(lerpTo.z) - 1)
		{
			player.playerCamera.transform.localRotation = Quaternion.Lerp(player.playerCamera.transform.localRotation, lerpTo, Time.deltaTime * 25f);

		}
		else isBeingDamaged = false;

	}

}
