//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "music.jpeg")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("��������Action")]
///// <summary>
///// ʹ�õ�ǰ����
///// </summary>
//public class PlayMusic : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// ��Դ����
//    /// </summary>
//    public string name;

//    /// <summary>
//    /// loop ֵ
//    /// </summary>
//    public bool loop = false;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        if (name == null || name == "") return TaskStatus.Failure;
//        roleBase.PlayAudio(name,loop);
//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
