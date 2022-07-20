using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonMouseListener : MonoBehaviour
{
    public AudioClip[] hoverNoises;
    public AudioClip[] clickNoises;

    public void Hover()
    {
        GameSettings.Instance.gameObject.GetComponent<AudioSource>().clip = hoverNoises[Random.Range(0, hoverNoises.Length)];
        GameSettings.Instance.gameObject.GetComponent<AudioSource>().pitch = Random.Range(0.95f, 1.05f);
        GameSettings.Instance.gameObject.GetComponent<AudioSource>().Play();
    }
    public void Click()
    {
        GameSettings.Instance.gameObject.GetComponent<AudioSource>().clip = clickNoises[Random.Range(0, hoverNoises.Length)];
        GameSettings.Instance.gameObject.GetComponent<AudioSource>().pitch = Random.Range(0.95f, 1.05f);
        GameSettings.Instance.gameObject.GetComponent<AudioSource>().Play();
    }
}
