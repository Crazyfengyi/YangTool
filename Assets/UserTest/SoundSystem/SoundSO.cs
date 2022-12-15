/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-07-02 
*/
using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct RangedFloat
{
    public float minValue;
    public float maxValue;
}
public abstract class AudioEvent : ScriptableObject
{
    public abstract void Play(AudioSource source);

}
[CreateAssetMenu(menuName = "YangTool/SoundSO")]
public class SoundSO : AudioEvent
{
    public AudioClip[] clips;
    [MinMaxRange(0, 2)]
    public RangedFloat volume;
    [MinMaxRange(0, 2)]
    public RangedFloat pitch;

    public override void Play(AudioSource source)
    {
        if (clips.Length == 0) return;

        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = Random.Range(volume.minValue, volume.maxValue);
        source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
        source.Play();
    }
}