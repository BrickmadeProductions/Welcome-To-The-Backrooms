using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DemoHandler : MonoBehaviour
{
    public void PlayDemoEnd()
    {

        GetComponent<VideoPlayer>().Play();
    }
}

