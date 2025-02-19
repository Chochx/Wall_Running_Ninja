using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class SetVolume : MonoBehaviour
{

    [SerializeField] private List<GameObject> volumes = new List<GameObject>(); 
    public AudioMixer mixer;
    private void Start()
    {
        UpdateVolumeAmount();
    }

    public void SetVolumeUp(int volume = 10)
    {
        float currentVolume;
        mixer.GetFloat("Volume", out currentVolume);
        if (currentVolume >= 20)
        {
            mixer.SetFloat("Volume", 20f);

        }
        else
        {
            mixer.SetFloat("Volume", currentVolume + volume);
        }
        UpdateVolumeAmount();
    }

    public void SetVolumeDown(int volume = 10) 
    {
        float currentVolume;
        mixer.GetFloat("Volume", out currentVolume);
        if (currentVolume <= -80)
        {
            mixer.SetFloat("Volume", -80f);

        }
        else
        {
            mixer.SetFloat("Volume", currentVolume - volume);
        }
        UpdateVolumeAmount();
    }

    private void UpdateVolumeAmount()
    {
        float currentVolume;
        mixer.GetFloat("Volume", out currentVolume);

        //currentVolume = Mathf.Abs(currentVolume);
        float normalizedVolume = Mathf.InverseLerp(-80f, 20f, currentVolume) * 10f;
        int volumeAmount = Mathf.FloorToInt(normalizedVolume);
        for (int i = 0; i < volumeAmount; i++)
        {
            volumes[i].GetComponent<Image>().color = Color.black;
        }

        var nonActiveVolumes = volumes.Skip(volumeAmount).ToList();
        foreach (var volume in nonActiveVolumes)
        {
            volume.GetComponent<Image>().color = Color.white;
        }
        
    }
}
