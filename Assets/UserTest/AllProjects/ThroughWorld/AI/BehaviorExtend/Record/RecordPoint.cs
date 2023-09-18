//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "�̵�.png")]
//[TaskCategory("RoleAI/Record")]
//[TaskDescription("��¼��--��Щ��Ҫ��֤Ŀ�����")]
///// <summary>
///// ��¼������
///// </summary>
//public class RecordPoint : Conditional
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// ѡ������
//    /// </summary>
//    public RecordPointType recordPointType;
//    /// <summary>
//    /// ѡ����ʱ�б�
//    /// </summary>
//    private List<Vector3> tempList = new List<Vector3>();
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override TaskStatus OnUpdate()
//    {
//        tempList.Clear();
//        try
//        {
//            switch (recordPointType)
//            {
//                case RecordPointType.��¼������Ծ�������ߵ�ѡ��:
//                    {
//                        //TODO:������
//                        //��ȡ��ɫ��Χ1�ĵ�
//                        //List<Position> list = RoomsMgr.Ins.GetPlayerRangPoint(RoleManager.Ins.Heros[0].transform.position, 1);
//                        //Vector3 result = roleBase.transform.position;

//                        //float minDistance = int.MaxValue;
//                        //for (int i = 0; i < list.Count; i++)
//                        //{
//                        //    Vector3 pos = new Vector3(list[i].x, 0, list[i].z);
//                        //    float distance = Vector3.Distance(roleBase.transform.position, pos);
//                        //    if (distance < minDistance)
//                        //    {
//                        //        minDistance = distance;
//                        //        result = pos;
//                        //    }
//                        //}

//                        Vector3 result = RoleManager.Ins.Heros.Count > 0 ? RoleManager.Ins.Heros[0].transform.position : roleBase.transform.position;
//                        tempList.Add(result);
//                    }
//                    break;
//                case RecordPointType.����Ŀ��λ��:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.Foot).position);
//                    break;
//                case RecordPointType.����Ŀ������λ��:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.RightHand).position);
//                    break;
//                case RecordPointType.����Ŀ������λ��:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.LeftHand).position);
//                    break;
//                case RecordPointType.����Ŀ��ͷ��λ��:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.Head).position);
//                    break;
//                case RecordPointType.����Ŀ������λ��:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.Body).position);
//                    break;
//                case RecordPointType.����λ��:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.Foot).position);
//                    break;
//                case RecordPointType.��������λ��:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.LeftHand).position);
//                    break;
//                case RecordPointType.��������λ��:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.RightHand).position);
//                    break;
//                case RecordPointType.����ͷ��λ��:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.Head).position);
//                    break;
//                case RecordPointType.��������λ��:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.Body).position);
//                    break;
//                case RecordPointType.��¼��ʼ��ת�������б�:
//                    tempList.Add(roleBase.initialDirection);
//                    break;
//                case RecordPointType.��¼Զ�̹�����_������Ŀ���:
//                    tempList.Add(roleBase.GetAtkPointForBessel());
//                    break;
//                default:
//                    tempList.Add(roleBase.transform.position);
//                    Debuger.ToError($"{roleBase.gameObject.name}:ѡ��ʧ��,����default");
//                    break;
//            }
//        }
//        catch (System.Exception)
//        {
//            Debuger.ToError($"��ɫ:{roleBase.transform.name}�ļ�¼��������,���ȱ�֤��Ŀ��");
//            throw;
//        }

//        roleBase.RecordPoints(tempList.ToArray());

//        return TaskStatus.Success;
//    }
//}

///// <summary>
///// ��¼��λ��
///// </summary>
//public enum RecordPointType
//{
//    None,
//    ��¼������Ծ�������ߵ�ѡ��,
//    ����Ŀ��λ��,
//    ����Ŀ������λ��,
//    ����Ŀ������λ��,
//    ����Ŀ��ͷ��λ��,
//    ����Ŀ������λ��,
//    ����λ��,
//    ��������λ��,
//    ��������λ��,
//    ����ͷ��λ��,
//    ��������λ��,
//    ��¼��ʼ��ת�������б�,
//    ��¼Զ�̹�����_������Ŀ���,
//}
