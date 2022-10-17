/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-10-17 
*/
using UnityEngine;
using System.Collections;
using YangTools;

public class GameSoundManager : MonoSingleton<GameSoundManager>
{
    public void PlayMusic(string name)
    {
        YangAudioManager.Instance.PlayBackgroundMusic(name);
    }
    public void PlaySound(string name)
    {
        YangAudioManager.Instance.PlaySoundAudio(name);
    }
}