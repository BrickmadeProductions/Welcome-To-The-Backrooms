// Blinking
using System.Collections;
using UnityEngine;

public class Blinking : MonoBehaviour
{
	public GameObject eyeLids;

	public IEnumerator Blink()
	{
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.1f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
	}

	public IEnumerator RandomBlinking()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(2f, 10f));
			eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
			yield return new WaitForSeconds(0.1f);
			eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		}
	}

	public IEnumerator WakeUpOther()
	{
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForEndOfFrame();
		yield return new WaitForFixedUpdate();
		yield return new WaitForSeconds(1f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.2f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.3f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.2f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.1f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
	}

	public IEnumerator WakeUpRoom()
	{
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForEndOfFrame();
		yield return new WaitForFixedUpdate();
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(2f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.25f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.5f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(2f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.25f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.5f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.25f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.5f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
		yield return new WaitForSeconds(0.25f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: true);
		yield return new WaitForSeconds(0.5f);
		eyeLids.GetComponent<Animator>().SetBool("eyesClosed", value: false);
	}
}
