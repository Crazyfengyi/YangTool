//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "����.png")]
//[TaskCategory("RoleAI/Atk")]
//[TaskDescription("��Ҫ�ȼ�¼������")]
///// <summary>
///// ��Χ�˺�
///// </summary>
//public class AreaDamage : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// ��Χ����
//    /// </summary>
//    public HitCheckType areaDamageType;
//    /// <summary>
//    /// ��Χ(Բ��:�뾶 ����:�� ɡ��:ֱ��)
//    /// </summary>
//    public float rang = 10f;
//    /// <summary>
//    /// �Ƕ�(����:�Ƕ�)
//    /// </summary>
//    public float angle = 0f;
//    /// <summary>
//    /// ƫ����
//    /// </summary>
//    public Vector3 OffsetVector;
//    /// <summary>
//    /// ��ת
//    /// </summary>
//    public Vector3 Rotate;
//    /// <summary>
//    /// ������Ч
//    /// </summary>
//    public string hitEffectName;
//    /// <summary>
//    /// ��������
//    /// </summary>
//    public string hitSoundName;
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override TaskStatus OnUpdate()
//    {

//        switch (areaDamageType)
//        {
//            case HitCheckType.Բ��:
//                roleBase.CircleRangAtk(roleBase.recordPointList[0], rang, OffsetVector, Rotate, hitEffectName, hitSoundName);
//                break;
//            case HitCheckType.����:
//                roleBase.SectorRangAtk(roleBase.recordPointList[0], rang, angle, OffsetVector, Rotate, hitEffectName, hitSoundName);
//                break;
//            case HitCheckType.����:
//                roleBase.RectRangAtk(roleBase.recordPointList[0], rang, angle, OffsetVector, Rotate, hitEffectName, hitSoundName);
//                break;
//            default:
//                break;
//        }

//        return TaskStatus.Success;
//    }
//}
