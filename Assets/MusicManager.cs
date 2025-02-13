using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum MusicType
{
    MAINMUSIC,
    REDLEVEL
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private MusicList[] musicList;
    private static MusicManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
    }

    public static void PlayMusic(MusicType music, float volume = 1)
    {
        AudioClip[] clips = instance.musicList[(int)music].Clips;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        StopMusic();

        instance.audioSource.clip = randomClip;
        instance.audioSource.volume = volume;
        instance.audioSource.Play();
    }

    public static void StopMusic()
    {
        instance.audioSource.Stop();
    }

    
#if (UNITY_EDITOR)
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(MusicType));
        Array.Resize(ref musicList, names.Length);
        for (int i = 0; i < musicList.Length; i++)
        {
            musicList[i].name = names[i];
        }
    }
#endif

}
[Serializable]
public struct MusicList
{
    public AudioClip[] Clips { get => audio; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] audio;
}


