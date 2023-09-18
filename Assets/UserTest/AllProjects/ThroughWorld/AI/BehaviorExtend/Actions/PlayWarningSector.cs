//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "��ʶ_̾��.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("���ξ�������Action")]
///// <summary>
///// ������Ч
///// </summary>
//public class PlayWarningSector : Action
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
//    /// �Ƕ�(����:�Ƕ�)
//    /// </summary>
//    public float angle = 36f;

//    /// <summary>
//    /// ���ΰ뾶
//    /// </summary>
//    public float radius = 1;

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
//        GameObject effectPrefabs = Resources.Load<GameObject>($"ɡ��Ԥ��");
//        //Quaternion quat = Quaternion.Euler(direction);
//        GameObject effectObj = GameObject.Instantiate(effectPrefabs);
//        effectObj.transform.position = roleBase.transform.position;
//        effectObj.transform.GetChild(0).transform.localScale = new Vector3(radius,1, radius);
//        effectObj.transform.LookAt(roleBase.GetTarget().transform.position);

//        effectObj.GetComponent<DelayDestory>().InitAndStart(time);

//        return TaskStatus.Success;
//    }
//}
