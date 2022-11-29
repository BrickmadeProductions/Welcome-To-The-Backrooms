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

	public float armorReduction = 1;

	Quaternion damageTargetFXRotation = Quaternion.identity;

	//sanity effects
	public AudioSource sanitySource;

	public Material[] shakeMaterials;

	Vector2[] originalOffsetMainOffset;
	Vector2[] originalOffsetWetOffset;
	Vector2[] originalOffsetDetailOffset;
	Vector2[] originalOffsetDetailMaskOffset;

	float shake = 0;

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
		originalOffsetMainOffset = new Vector2[shakeMaterials.Length];
		originalOffsetWetOffset = new Vector2[shakeMaterials.Length];
		originalOffsetDetailOffset = new Vector2[shakeMaterials.Length];
		originalOffsetDetailMaskOffset = new Vector2[shakeMaterials.Length];

		player = GetComponent<PlayerController>();

		StartCoroutine(UpdateHealth());
		StartCoroutine(NaturalRegen());

		for (int i = 0; i < shakeMaterials.Length; i++)
		{
			originalOffsetMainOffset[i] = shakeMaterials[i].GetTextureOffset(Shader.PropertyToID("_MainTex"));
			originalOffsetWetOffset[i] = shakeMaterials[i].GetTextureOffset(Shader.PropertyToID("_WetMap"));
			originalOffsetDetailOffset[i] = shakeMaterials[i].GetTextureOffset(Shader.PropertyToID("_DetailAlbedoMap"));
			originalOffsetDetailMaskOffset[i] = shakeMaterials[i].GetTextureOffset(Shader.PropertyToID("_DetailMask"));
		}

	}
    private void OnDestroy()
    {
		for (int i = 0; i < shakeMaterials.Length; i++)
		{
			shakeMaterials[i].SetTextureOffset(Shader.PropertyToID("_MainTex"), originalOffsetMainOffset[i]);
			shakeMaterials[i].SetTextureOffset(Shader.PropertyToID("_WetMap"), originalOffsetWetOffset[i]);
			shakeMaterials[i].SetTextureOffset(Shader.PropertyToID("_DetailAlbedoMap"), originalOffsetDetailOffset[i]);
			shakeMaterials[i].SetTextureOffset(Shader.PropertyToID("_DetailMask"), originalOffsetDetailMaskOffset[i]);

		}
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
		hungerText.text = ((int)hunger).ToString() ?? "";
		heartBeatSource.pitch = (float)heartRate / 90f;
		heartBeatSource.volume = (float)heartRate / 100f - 1f;

		if (shake > 0)
		{
			Vector3 shakeValue = (UnityEngine.Random.insideUnitCircle * Mathf.Clamp01(1f - (sanity / 100f))) / 10f;
			shake -= Time.deltaTime / 2;

			if (sanity < 50f)
            
				sanitySource.volume = Mathf.Clamp01((1f - (sanity / 100f)) - 0.5f);
			else
				sanitySource.volume = Mathf.Clamp01(Mathf.Lerp(sanitySource.volume, 0, Time.deltaTime / 2f));

			if (sanity < 15f)

				foreach (Material mat in shakeMaterials)
				{
					mat.SetTextureOffset(Shader.PropertyToID("_MainTex"), shakeValue);
					mat.SetTextureOffset(Shader.PropertyToID("_WetMap"), shakeValue);
					mat.SetTextureOffset(Shader.PropertyToID("_DetailAlbedoMap"), shakeValue);
					mat.SetTextureOffset(Shader.PropertyToID("_DetailMask"), shakeValue);

				}
			
			
			GameSettings.Instance.Chrom.intensity.value = Mathf.Clamp01(Mathf.Lerp(GameSettings.Instance.Chrom.intensity.value, (0.081f + (1f - (sanity / 100f))) - 0.081f , Time.deltaTime / 2));
			GameSettings.Instance.Vignette.intensity.value = Mathf.Clamp(Mathf.Lerp(GameSettings.Instance.Vignette.intensity.value, (0.392f + (1f - (sanity / 100f))) - 0.392f, Time.deltaTime / 2), 0, 0.7f);
			GameSettings.Instance.ColorGrading.saturation.value = Mathf.Clamp01(Mathf.Lerp(GameSettings.Instance.ColorGrading.saturation.value, ((1f - (sanity / 100f))), Time.deltaTime / 2)) * -50f;

		}
		else
		{
			shake = 0.0f;
		}

		if (sanity < 75f)
		{
			shake = 1f;
		}
		else
		{
			sanitySource.volume = 0;

			for (int i = 0; i < shakeMaterials.Length; i++)
			{
				shakeMaterials[i].SetTextureOffset(Shader.PropertyToID("_MainTex"), originalOffsetMainOffset[i]);
				shakeMaterials[i].SetTextureOffset(Shader.PropertyToID("_WetMap"), originalOffsetWetOffset[i]);
				shakeMaterials[i].SetTextureOffset(Shader.PropertyToID("_DetailAlbedoMap"), originalOffsetDetailOffset[i]);
				shakeMaterials[i].SetTextureOffset(Shader.PropertyToID("_DetailMask"), originalOffsetDetailMaskOffset[i]);

			}
			GameSettings.Instance.Chrom.intensity.value = Mathf.Lerp(GameSettings.Instance.Chrom.intensity.value, 0.081f, Time.deltaTime);
			GameSettings.Instance.Vignette.intensity.value = Mathf.Lerp(GameSettings.Instance.Vignette.intensity.value, 0.392f, Time.deltaTime);
			GameSettings.Instance.ColorGrading.saturation.value = Mathf.Clamp01(Mathf.Lerp(GameSettings.Instance.ColorGrading.saturation.value, 0f, Time.deltaTime));
		}
		

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
			ChangeHunger(player.currentPlayerState == PlayerController.PLAYERSTATES.RUN ? -0.08f : -0.03f);
			ChangeThirst(player.currentPlayerState == PlayerController.PLAYERSTATES.RUN ? -0.35f : -0.2f);

			if (GameSettings.Instance.ActiveScene != SCENE.ROOM && sanity > 0f)
			{
				sanity -= 0.2f;
			}
			if (sanity <= 0f)
			{
				sanity = 0f;
				TakeDamage(5f, 0f, 2f, false, DAMAGE_TYPE.UNKNOWN);
			}
			if (thirst <= 0f)
			{
				TakeDamage(2f, 0f, 2f, false, DAMAGE_TYPE.UNKNOWN);
				canRun = false;
			}
			else if (thirst > 0)
				canRun = true;
			if (hunger <= 0f)
			{
				hunger = 0f;
				TakeDamage(0.5f, 0f, 1f, false, DAMAGE_TYPE.UNKNOWN);
			}
			yield return new WaitForSecondsRealtime(3f);
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
	public void ChangeSanity(float amount)
	{
		sanity += amount;
		sanity = Mathf.Clamp(sanity, 0f, 250f);
	}
	public void ChangeHealth(float amount)
	{
		health += amount;
		health = Mathf.Clamp(health, 0f, 100f);
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

	public void TakeDamage(float damageSubtraction, float sanityMultipler, float heartrateIncrease, bool knockBack, DAMAGE_TYPE damageType)
    {
		if (GameSettings.Instance.cheatSheet.invincible)
			return;

		if (knockBack)
        {
			GameSettings.GetLocalPlayer().rb.drag = 0f;
			GameSettings.GetLocalPlayer().rb.AddForce(Vector3.up * 15000f);
			GameSettings.GetLocalPlayer().rb.AddForce(-GameSettings.GetLocalPlayer().rb.velocity.normalized * 1000f * GameSettings.GetLocalPlayer().rb.velocity.magnitude);
			GameSettings.GetLocalPlayer().rb.AddForce(GameSettings.GetLocalPlayer().transform.forward * -15000f);

		}

		health -= damageSubtraction * armorReduction;
		sanity *= sanityMultipler;
		ChangeHeartRate(heartrateIncrease);

		if (!player.dead && health <= 0.0f)
        {
			
			StartCoroutine(player.Die());

			

			if (damageType == DAMAGE_TYPE.ENTITY)
			{
				Steam.AddAchievment("MEAT_CRAYON");
			}

		}

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
