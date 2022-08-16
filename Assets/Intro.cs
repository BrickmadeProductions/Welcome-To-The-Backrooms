using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    Coroutine introC = null;
    public TextMeshProUGUI skip;
    bool hasClicked = false;
    // Start is called before the first frame update
    void Start()
    {
        introC = StartCoroutine(playIntro());
        
    }

    private void Update()
    {
      
        if (Input.GetButtonDown("Jump") && !hasClicked)
        {
            hasClicked = true;
            StopCoroutine(introC);
            GameSettings.Instance.LoadScene(SCENE.HOMESCREEN);
            GameSettings.Instance.setCutScene(false);
        }
    }

    // Update is called once per frame
    IEnumerator playIntro()
    {
        GameSettings.Instance.setCutScene(true);

        yield return new WaitForSecondsRealtime(9.5f);
        skip.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(8f);
        GameSettings.Instance.LoadScene(SCENE.HOMESCREEN);

        GameSettings.Instance.setCutScene(false);
    }
}
