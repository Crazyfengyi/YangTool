using UnityEngine;

namespace CMF
{
    //This script turns a gameobject toward the target controller's velocity direction;
    //这个脚本将游戏物体转向目标控制器的速度方向;
    public class TurnTowardControllerVelocity : MonoBehaviour
    {

        //Target controller;
        public Controller controller;

        //Speed at which this gameobject turns toward the controller's velocity;
        //游戏物体转向控制器的速度;
        public float turnSpeed = 500f;

        Transform parentTransform;
        Transform tr;

        //Current (local) rotation around the (local) y axis of this gameobject;
        float currentYRotation = 0f;

        //If the angle between the current and target direction falls below 'fallOffAngle', 'turnSpeed' becomes progressively slower (and eventually approaches '0f');
        //This adds a smoothing effect to the rotation;
        //如果当前和目标方向之间的角度低于'fallOffAngle'， 'turnSpeed'会逐渐变慢(最终接近'0f');这为旋转增加了平滑效果;
        float fallOffAngle = 90f;

        //Whether the current controller momentum should be ignored when calculating the new direction;
        //计算新方向时是否忽略当前控制器动量;
        public bool ignoreControllerMomentum = false;

        //Setup;
        void Start()
        {
            tr = transform;
            parentTransform = tr.parent;

            //Throw warning if no controller has been assigned;
            if (controller == null)
            {
                Debug.LogWarning("No controller script has been assigned to this 'TurnTowardControllerVelocity' component!", this);
                this.enabled = false;
            }
        }

        void LateUpdate()
        {

            //Get controller velocity;
            Vector3 _velocity;
            if (ignoreControllerMomentum)
                _velocity = controller.GetMovementVelocity();
            else
                _velocity = controller.GetVelocity();

            //Project velocity onto a plane defined by the 'up' direction of the parent transform;
            //将速度投影到父变换的“向上”方向定义的平面上;
            _velocity = Vector3.ProjectOnPlane(_velocity, parentTransform.up);

            float _magnitudeThreshold = 0.001f;

            //If the velocity's magnitude is smaller than the threshold, return;
            //如果速度的大小小于阈值，返回;
            if (_velocity.magnitude < _magnitudeThreshold)
                return;

            //Normalize velocity direction;
            _velocity.Normalize();

            //Get current 'forward' vector;
            //获得当前的“向前”矢量
            Vector3 _currentForward = tr.forward;

            //Calculate (signed) angle between velocity and forward direction;
            //计算速度与前进方向之间的夹角(带符号)
            float _angleDifference = VectorMath.GetAngle(_currentForward, _velocity, parentTransform.up);

            //Calculate angle factor;
            //计算角因素
            float _factor = Mathf.InverseLerp(0f, fallOffAngle, Mathf.Abs(_angleDifference));

            //Calculate this frame's step;
            //计算这个坐标系的步长
            float _step = Mathf.Sign(_angleDifference) * _factor * Time.deltaTime * turnSpeed;

            //Clamp step;
            if (_angleDifference < 0f && _step < _angleDifference)
                _step = _angleDifference;
            else if (_angleDifference > 0f && _step > _angleDifference)
                _step = _angleDifference;

            //Add step to current y angle;
            currentYRotation += _step;

            //Clamp y angle;
            if (currentYRotation > 360f)
                currentYRotation -= 360f;
            if (currentYRotation < -360f)
                currentYRotation += 360f;

            //Set transform rotation using Quaternion.Euler;
            tr.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

        }

        void OnDisable()
        {
        }

        void OnEnable()
        {
            currentYRotation = transform.localEulerAngles.y;
        }
    }
}
