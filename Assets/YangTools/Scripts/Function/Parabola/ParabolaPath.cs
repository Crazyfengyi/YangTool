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
        public bool isClampStartEnd;
        private Vector3 m_start;//起点
        private Vector3 m_end;//终点
        private float m_height;//高度
        private float m_gravity;// 重力加速度
        private float m_upTime;//上升时间
        private float m_downTime;//下降时间
        private float m_totalTime;//总运动时间
        private Vector3 m_velocityStart;//初始速度
        private Vector3 m_position;//根据时间的当前位置
        private float m_time;//当前时间
        #endregion

        #region 属性
        /// <summary> 
        /// 起点 
        /// </summary>
        public Vector3 Start { get { return m_start; } }
        /// <summary>
        /// 终点 
        /// </summary>
        public Vector3 End { get { return m_end; } }
        /// <summary> 
        /// 目标高度 
        /// </summary>
        public float Height { get { return m_height; } }
        /// <summary> 
        /// 重力加速度 
        /// </summary>
        public float Gravity { get { return m_gravity; } }
        /// <summary> 
        /// 上升时间 
        /// </summary>
        public float UpTime { get { return m_upTime; } }
        /// <summary> 
        /// 下降时间 
        /// </summary>
        public float DownTime { get { return m_downTime; } }
        /// <summary> 
        /// 总运动时间 
        /// </summary>
        public float TotalTime { get { return m_totalTime; } }
        /// <summary> 
        /// 顶点 
        /// </summary>
        public Vector3 Top { get { return GetPosition(m_upTime); } }
        /// <summary> 
        /// 初始速度 
        /// </summary>
        public Vector3 VelocityStart { get { return m_velocityStart; } }
        /// <summary>
        /// 当前位置 
        /// </summary>
        public Vector3 Position { get { return m_position; } }
        /// <summary> 
        /// 当前速度 
        /// </summary>
        public Vector3 Velocity { get { return GetVelocity(m_time); } }

        /// <summary> 
        /// 当前时间 
        /// </summary>
        public float Time
        {
            get { return m_time; }
            set
            {
                if (isClampStartEnd) value = Mathf.Clamp(value, 0, m_totalTime);
                m_time = value;
                m_position = GetPosition(value);
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

            m_start = start;
            m_end = end;
            m_height = height;
            m_gravity = gravity;
            m_upTime = t1;
            m_downTime = t2;
            m_totalTime = t;
            m_velocityStart = new Vector3(vX, vY, vZ);
            m_position = m_start;
            m_time = 0;
        }
        /// <summary> 
        /// 获取某个时间点的位置 
        /// </summary>
        public Vector3 GetPosition(float time)
        {
            if (time == 0) return m_start;
            if (time == m_totalTime) return m_end;
            float dY = 0.5f * m_gravity * time * time;//自由落体运动---h=1/2*g*t*t
            return m_start + m_velocityStart * time + new Vector3(0, dY, 0);
        }
        /// <summary> 
        /// 获取某个时间点的速度 
        /// </summary>
        public Vector3 GetVelocity(float time)
        {
            if (time == 0) return m_velocityStart;
            //抛物线运动--只有y方向受重力影响力,x方向受力不变(初速度)
            return m_velocityStart + new Vector3(0, m_velocityStart.y + m_gravity * time, 0);
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
                float time = ratio * m_totalTime;
                Vector3 pos = GetPosition(time);
                path[i] = pos;
            }

            return path;
        }
        #endregion
    }
}