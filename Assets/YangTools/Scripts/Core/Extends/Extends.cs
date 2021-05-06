using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// ��չ��
    /// </summary>
    public static partial class Extends
    {
        //TODO ����ʱ�䣬ͨ�÷����࣬�����ԣ�SDKManager,/*�¼�ϵͳ*/��/*��ʱ��*/����棬֧��

        #region �¼�ϵͳ��չ
        /// <summary>
        /// ����¼�����
        /// </summary>
        /// <param name="_object">������</param>
        /// <param name="_eventName">�¼�����</param>
        /// <param name="_eventCallback">�¼��ص�</param>
        public static EventInspector AddEventListener(this UnityEngine.Object _object, string _eventName, EventCallback<EventInfo> _eventCallback)
        {
            var ret = new EventInspector(_object, _eventName, _eventCallback);
            YangToolEventListener.Instance.Add(ret);
            return ret;
        }

        /// <summary>
        /// �Ƴ��¼�����
        /// </summary>
        /// <param name="_listener">�¼�������ʵ��</param>
        public static void RemoveEventListener(this UnityEngine.Object _object, EventInspector _listener)
        {
            YangToolEventListener.Instance.Remove(_listener);
        }

        /// <summary>
        /// �Ƴ��¼�����
        /// </summary>
        public static void RemoveEventListener(this UnityEngine.Object _object)
        {
            YangToolEventListener.Instance.Remove(_object);
        }

        /// <summary>
        /// �����¼�
        /// </summary>
        /// <param name="_object">����</param>
        /// <param name="_eventName">�¼�����</param>
        /// <param name="_args">�¼�����</param>
        public static void SendEvent(this UnityEngine.Object _object, string _eventName, params object[] _args)
        {
            YangToolEventListener.Instance.Send(_eventName, _args);
        }
        #endregion

    }

    //����THE END����
}