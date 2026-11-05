using UnityEngine;

namespace SignInSystem
{
    /// <summary>兼容签到奖励旋转组件</summary>
    public class RotateScript : MonoBehaviour
    {
        public float speed = 1f;

        /// <summary>按速度旋转对象</summary>
        private void Update()
        {
            Vector3 rotation = transform.localEulerAngles;
            rotation.z += speed * Time.deltaTime;
            transform.localEulerAngles = rotation;
        }
    }
}
