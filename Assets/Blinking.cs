using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blinking : MonoBehaviour
{
    public GameObject eyelid;

    public IEnumerator Blink()
    {
        
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
        yield return new WaitForSeconds(0.1f);
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);

    }

    public IEnumerator RandomBlinking()
    {
        while (true)
        {
            
            yield return new WaitForSeconds(Random.Range(2f, 10f));
            eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
            yield return new WaitForSeconds(0.1f);
            eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
            
            
        }
        
    }

    public IEnumerator WakeUp()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
        yield return new WaitForSeconds(2f); //closed
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
        yield return new WaitForSeconds(0.25f); //opened
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
        yield return new WaitForSeconds(0.5f); //closed
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
        yield return new WaitForSeconds(2f); //closed
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
        yield return new WaitForSeconds(0.25f); //opened
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
        yield return new WaitForSeconds(0.5f); //closed
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
        yield return new WaitForSeconds(0.25f); //opened
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
        yield return new WaitForSeconds(0.5f); //closed
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
        yield return new WaitForSeconds(0.25f); //opened
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", true);
        yield return new WaitForSeconds(0.5f); //closed
        eyelid.GetComponent<Animator>().SetBool("eyesClosed", false);
    }
}
