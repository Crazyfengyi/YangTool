//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[TaskIcon(BehaviorSetting.path + "��ʶ_̾��.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("���ξ�������Action")]
///// <summary>
///// ������Ч
///// </summary>
//public class PlayWarningRectangle : Action
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
//    /// ����
//    /// </summary>
//    public float length = 5f;
//    /// <summary>
//    /// ���
//    /// </summary>
//    public float width = 2;

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

//        float len = length / 4 + 1;

//        //��Ч
//        GameObject effectPrefabs = Resources.Load<GameObject>($"����Ԥ��");
//        GameObject effectObj = GameObject.Instantiate(effectPrefabs, nowPos, Quaternion.identity);
//        effectObj.transform.forward = roleBase.modelInfo.Root.transform.forward;
//        //����
//        Vector3 resultScale = Vector3.one;
//        resultScale.z *= len;
//        resultScale.x *= width;
//        effectObj.transform.localScale = resultScale;

//        return TaskStatus.Success;
//    }
//}
