using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 抛物线运动轨迹
    /// </summary>
    public class ParabolaPath
    {
        #region 字段
        /// <summary>
        /// 是否夹紧起点和终点
        /// </summary>
        public bool IsClampStartEnd;

        private float mHeight;//高度
        private float mGravity;// 重力加速度
        private float mUpTime;//上升时间
        private float mDownTime;//下降时间
        private float mTotalTime;//总运动时间
        private Vector3 mVelocityStart;//初始速度
        private Vector3 mPosition;//根据时间的当前位置
        private float mTime;//当前时间
        #endregion

        #region 属性
        /// <summary> 
        /// 起点 
        /// </summary>
        public Vector3 Start { get; private set; }

        /// <summary>
        /// 终点 
        /// </summary>
        public Vector3 End { get; private set; }

        /// <summary> 
        /// 目标高度 
        /// </summary>
        public float Height => mHeight;

        /// <summary> 
        /// 重力加速度 
        /// </summary>
        public float Gravity => mGravity;

        /// <summary> 
        /// 上升时间 
        /// </summary>
        public float UpTime => mUpTime;

        /// <summary> 
        /// 下降时间 
        /// </summary>
        public float DownTime => mDownTime;

        /// <summary> 
        /// 总运动时间 
        /// </summary>
        public float TotalTime => mTotalTime;

        /// <summary> 
        /// 顶点 
        /// </summary>
        public Vector3 Top => GetPosition(mUpTime);

        /// <summary> 
        /// 初始速度 
        /// </summary>
        public Vector3 VelocityStart => mVelocityStart;

        /// <summary>
        /// 当前位置 
        /// </summary>
        public Vector3 Position => mPosition;

        /// <summary> 
        /// 当前速度 
        /// </summary>
        public Vector3 Velocity => GetVelocity(mTime);

        /// <summary> 
        /// 当前时间 
        /// </summary>
        public float Time
        {
            get => mTime;
            set
            {
                if (IsClampStartEnd) value = Mathf.Clamp(value, 0, mTotalTime);
                mTime = value;
                mPosition = GetPosition(value);
            }
        }
        #endregion

        #region 方法
        /// <summary> 
        /// 初始化抛物线运动轨迹 
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="height">高度(相对于两个点的最高位置,高出多少)</param>
        /// <param name="gravity">重力加速度(负数)</param>
        public ParabolaPath(Vector3 start, Vector3 end, float height = 10, float gravity = -9.8f)
        {
            Init(start, end, height, gravity);
        }
        /// <summary>
        /// 初始化抛物线运动轨迹 
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="height">高度(相对于两个点的最高位置,高出多少)</param>
        /// <param name="gravity">重力加速度(负数)</param>
        public void Init(Vector3 start, Vector3 end, float height = 10, float gravity = -9.8f)
        {
            //最高点
            float topY = Mathf.Max(start.y, end.y) + height;
            //上升阶段-高度
            float d1 = topY - start.y;
            //下降阶段-高度
            float d2 = topY - end.y;
            //根据公式s=vt-(at*t)/2变形得到 t = Mathf.Sqrt(-2h/g)
            float t1 = Mathf.Sqrt((-2 * d1) / gravity);
            //根据公式s=ut+(at*t)/2变形得到 t = Mathf.Sqrt(2(P-h)/g)  p:topY下落起点
            float t2 = Mathf.Sqrt((2 * -d2) / gravity);
            //总时间
            float t = t1 + t2;
            //X方向初速度--路程/时间
            float vX = (end.x - start.x) / t;
            //Z方向初速度
            float vZ = (end.z - start.z) / t;
            //Y方向初速度
            float vY = -gravity * t1;

            Start = start;
            End = end;
            mHeight = height;
            mGravity = gravity;
            mUpTime = t1;
            mDownTime = t2;
            mTotalTime = t;
            mVelocityStart = new Vector3(vX, vY, vZ);
            mPosition = Start;
            mTime = 0;
        }
        /// <summary> 
        /// 获取某个时间点的位置 
        /// </summary>
        public Vector3 GetPosition(float time)
        {
            if (time == 0) return Start;
            if (Mathf.Approximately(time, mTotalTime)) return End;
            float dY = 0.5f * mGravity * time * time;//自由落体运动---h=1/2*g*t*t
            return Start + mVelocityStart * time + new Vector3(0, dY, 0);
        }
        /// <summary> 
        /// 获取某个时间点的速度 
        /// </summary>
        public Vector3 GetVelocity(float time)
        {
            if (time == 0) return mVelocityStart;
            //抛物线运动--只有y方向受重力影响力,x方向受力不变(初速度)
            return mVelocityStart + new Vector3(0, mVelocityStart.y + mGravity * time, 0);
        }
        /// <summary>
        /// 获得抛物线位置数组
        /// </summary>
        public Vector3[] GetParabolaList(int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];

            for (int i = 0; i <= segmentNum; i++)
            {
                float ratio = i / (float)segmentNum;
                float time = ratio * mTotalTime;
                Vector3 pos = GetPosition(time);
                path[i] = pos;
            }

            return path;
        }
        #endregion
    }
}