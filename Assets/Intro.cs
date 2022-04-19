using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    Coroutine introC = null;
    // Start is called before the first frame update
    void Start()
    {
        introC = StartCoroutine(playIntro());
        
    }

    private void Update()
    {
      
        if (Input.GetButtonDown("Jump"))
        {
            
            StopCoroutine(introC);
            GameSettings.Instance.LoadScene("HomeScreen");
            GameSettings.Instance.setCutScene(false);
        }
    }

    // Update is called once per frame
    IEnumerator playIntro()
    {
        GameSettings.Instance.setCutScene(true);

        yield return new WaitForSeconds(13.6f);
        GameSettings.Instance.LoadScene("HomeScreen");

        GameSettings.Instance.setCutScene(false);
    }
}
