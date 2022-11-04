using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SubtitleSection
{
    public string description;
    public float timeStampEnd;
}

[CreateAssetMenu(fileName = "AudioClipData", menuName = "ScriptableObjects/AudioClipWithSubtitles", order = 1)]
public class AudioClipData : ScriptableObject
{
    public AudioClip clip;
    public List<SubtitleSection> subtitles;
}