// Blinking
using System.Collections;
using UnityEngine;

public class Blinking : MonoBehaviour
{
	public GameObject eyelid;

	public IEnumerator Blink()
	{
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.1f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
	}

	public IEnumerator RandomBlinking()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(2f, 10f));
			eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
			yield return new WaitForSeconds(0.1f);
			eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		}
	}

	public IEnumerator WakeUpOther()
	{
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForEndOfFrame();
		yield return new WaitForFixedUpdate();
		yield return new WaitForSeconds(1f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.2f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.3f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.2f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.1f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
	}

	public IEnumerator WakeUpRoom()
	{
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForEndOfFrame();
		yield return new WaitForFixedUpdate();
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(2f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.25f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.5f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(2f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.25f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.5f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.25f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.5f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.25f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.5f);
		eyelid.GetComponent<Animator>().SetBool("eyesClosed", value: false);
	}
}
