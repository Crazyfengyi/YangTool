//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "��ʶ_̾��.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("Բ�ξ�������Action")]
///// <summary>
///// ������Ч
///// </summary>
//public class PlayWarningCircular : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// ����ʱ��
//    /// </summary>
//    public float time = 1;

//    /// <summary>
//    /// �뾶
//    /// </summary>
//    public float radius = 5f;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override TaskStatus OnUpdate()
//    {
//        //Ŀ���
//        Vector3 targetPos = roleBase.recordPointList[0];
//        targetPos.y = 0.002f;

//        //����λ��
//        Vector3 nowPos = transform.position;
//        nowPos.y = 0.002f;
//        Vector3 direction = targetPos - nowPos;

//        //��Ч
//        GameObject effectPrefabs = Resources.Load<GameObject>($"Բ��Ԥ��");
//        GameObject effectObj = GameObject.Instantiate(effectPrefabs, targetPos, Quaternion.identity);
//        effectObj.transform.localScale = Vector3.one * radius;
//        effectObj.GetComponent<DelayDestory>().InitAndStart(time);

//        return TaskStatus.Success;
//    }
//}
