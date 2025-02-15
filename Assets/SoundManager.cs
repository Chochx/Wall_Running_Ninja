using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SoundType
{
    SWORD,
    JUMP,
    LAND,
    FOOTSTEPS,
    HURT,
    HURTENEMY,
    SLASHHITENEMY,
    SLASHHIT,
    END,
    SCOREPOINT,
    BLOOD
}
[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] audioList;
    private static SoundManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.audioList[(int)sound].Clips;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(randomClip, volume);
    }
#if (UNITY_EDITOR)
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref audioList, names.Length);
        for (int i = 0; i < audioList.Length; i++)
        {
            audioList[i].name = names[i];
        }
    }
#endif

}


[Serializable]
public struct SoundList
{
    public AudioClip[] Clips { get => audio; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] audio;
}
