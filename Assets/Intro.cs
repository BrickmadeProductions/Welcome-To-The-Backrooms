using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    Coroutine introC = null;
    public TextMeshProUGUI skip;
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
            GameSettings.Instance.LoadScene(GameSettings.SCENE.HOMESCREEN);
            GameSettings.Instance.setCutScene(false);
        }
    }

    // Update is called once per frame
    IEnumerator playIntro()
    {
        GameSettings.Instance.setCutScene(true);

        yield return new WaitForSeconds(9.5f);
        skip.gameObject.SetActive(false);
        yield return new WaitForSeconds(9f);
        GameSettings.Instance.LoadScene(GameSettings.SCENE.HOMESCREEN);

        GameSettings.Instance.setCutScene(false);
    }
}
