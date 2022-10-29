using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class LoadingScreenChooser : MonoBehaviour
{
    public VideoClip[] possibleLoadingScreens;
    public VideoPlayer player;
    private void Awake()
    {
        player.clip = possibleLoadingScreens[Random.Range(0, possibleLoadingScreens.Length)];
        player.Play();
    }
}
