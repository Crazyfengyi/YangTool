/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-11-13 
*/
using YangTools;

public class GameManager : MonoSingleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        SceneLoader.Instance.OnSceneLoadPre += GameProjectileManager.Instance.OnSceneChange;
        SceneLoader.Instance.OnSceneLoadPre += GameUIManager.Instance.OnSceneChange;
        SceneLoader.Instance.OnSceneLoadPre += GameActorManager.Instance.OnSceneChange;
    }
}