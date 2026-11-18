using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace YangTools.Demos
{
    /// <summary>
    /// 管理演示靶标和每轮弹体的发射与清理
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class HomingMissileDemoController : MonoBehaviour
    {
        [SerializeField] private Transform launcher; // 发射点
        [SerializeField] private Transform target; // 靶标中心
        [SerializeField] private HomingMissileDemoShot shotPrefab; // 演示弹体
        [SerializeField] private ParticleSystem arrivalPrefab; // 到达时的闪光与火花
        [SerializeField] private HomingMissileDemoLaser laserPrefab; // 持续激光特效
        [SerializeField, Tooltip("勾选使用闪电曲线 取消勾选使用直线")]
        private bool useCurvedLaser = true; // 激光线形

        [Header("发射设置")]
        [SerializeField, Range(1, 16)] private int shotsPerRound = 8; // 每轮弹数
        [SerializeField, Min(0f)] private float shotInterval = 0.08f; // 发射间隔
        [SerializeField, Min(0f)] private float fanWidth = 11f; // 扇形展开宽度
        [SerializeField, Min(0.05f)] private float spreadDuration = 0.55f; // 散开时长
        [SerializeField, Min(0.05f)] private float wanderDuration = 1.15f; // 摆动时长
        [SerializeField, Min(0.05f)] private float gatherDuration = 0.75f; // 收束时长
        [SerializeField, Min(0f)] private float wanderStrength = 3.2f; // 摆动幅度

        [Header("移动靶设置")]
        [SerializeField] private bool movingTarget = true; // 初始靶标模式
        [SerializeField, Min(0f)] private float targetTravel = 4.25f; // 靶标横向移动幅度
        [SerializeField, Min(0.1f)] private float targetPeriod = 4.5f; // 往返周期

        /// <summary>
        /// 保存一轮发射的独立进度
        /// </summary>
        private struct LaunchRound
        {
            public float NextShotAt; // 下一枚的发射时间
            public int ShotsLaunched; // 本轮已发射数量
        }

        private readonly List<LaunchRound> activeRounds = new List<LaunchRound>(); // 尚未发完的轮次
        private readonly List<GameObject> activeEffects = new List<GameObject>(32); // 已生成的弹体和命中特效
        private Vector3 targetCenter; // 靶标运动中心
        private Vector3 targetAxis; // 靶标移动方向
        private Action<Vector3> playArrival; // 缓存的特效回调
        private float targetPhase; // 靶标往返进度
        private HomingMissileDemoLaser laser; // 按住激光键时复用的实例

        /// <summary>
        /// 检查场景绑定并安排首轮发射
        /// </summary>
        private void Start()
        {
            if (launcher == null || target == null || shotPrefab == null || arrivalPrefab == null)
            {
                Debug.LogError("跟踪弹演示缺少发射点 靶标 弹体或到达特效绑定", this);
                enabled = false;
                return;
            }

            targetCenter = target.position;
            targetAxis = launcher.right;
            playArrival = PlayArrival;
            BeginRound();
        }

        /// <summary>
        /// 响应键盘输入并在弹体更新前移动靶标
        /// </summary>
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            bool launch = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
            bool toggle = keyboard != null && keyboard.tKey.wasPressedThisFrame;
            bool fireLaser = keyboard != null && keyboard.lKey.isPressed;
#else
            bool launch = Input.GetKeyDown(KeyCode.Space);
            bool toggle = Input.GetKeyDown(KeyCode.T);
            bool fireLaser = Input.GetKey(KeyCode.L);
#endif
            if (toggle)
                movingTarget = !movingTarget;
            if (target == null || launcher == null)
            {
                activeRounds.Clear();
                UpdateLaser(false);
                return;
            }
            if (launch)
                BeginRound();

            if (movingTarget)
            {
                targetPhase = Mathf.Repeat(targetPhase + Time.deltaTime / targetPeriod, 1f);
                target.position = targetCenter + targetAxis * (Mathf.Sin(targetPhase * 2f * Mathf.PI) * targetTravel);
            }

            UpdateLaser(fireLaser);
            AdvanceRounds();
        }

        /// <summary>
        /// 更新激光端点或停止发射光束与火花
        /// </summary>
        private void UpdateLaser(bool firing)
        {
            if (!firing)
            {
                if (laser != null)
                    laser.StopBeam();
                return;
            }

            if (laserPrefab == null)
                return;
            if (laser == null)
                laser = Instantiate(laserPrefab, transform);

            laser.DrawBeam(launcher.position, target.position, useCurvedLaser);
        }

        /// <summary>
        /// 推进所有尚未发完的轮次
        /// </summary>
        private void AdvanceRounds()
        {
            float now = Time.time;
            for (int i = activeRounds.Count - 1; i >= 0; i--)
            {
                LaunchRound round = activeRounds[i];
                while (round.ShotsLaunched < shotsPerRound && now >= round.NextShotAt)
                {
                    LaunchShot(round.ShotsLaunched);
                    round.ShotsLaunched++;
                    round.NextShotAt += shotInterval;
                }

                if (round.ShotsLaunched >= shotsPerRound)
                    activeRounds.RemoveAt(i);
                else
                    activeRounds[i] = round;
            }
        }

        /// <summary>
        /// 追加一轮发射并移除已自然退场的对象引用
        /// </summary>
        private void BeginRound()
        {
            activeEffects.RemoveAll(effect => effect == null);
            activeRounds.Add(new LaunchRound { NextShotAt = Time.time });
        }

        /// <summary>
        /// 按当前弹序分配扇形位置和独立摆动种子
        /// </summary>
        private void LaunchShot(int shotIndex)
        {
            float offset = shotsPerRound == 1 ? 0f : (float)shotIndex / (shotsPerRound - 1) - 0.5f;
            var shot = Instantiate(shotPrefab, launcher.position, launcher.rotation, transform);
            activeEffects.Add(shot.gameObject);
            var times = new Vector3(spreadDuration, wanderDuration, gatherDuration)
                * UnityEngine.Random.Range(0.9f, 1.1f);
            shot.Launch(target, offset * fanWidth, UnityEngine.Random.Range(1f, 2000f), times, wanderStrength, playArrival);
        }

        /// <summary>
        /// 在靶标到达点播放一次预制特效
        /// </summary>
        private void PlayArrival(Vector3 position)
        {
            var effect = Instantiate(arrivalPrefab, position, Quaternion.identity, transform);
            activeEffects.Add(effect.gameObject);
            effect.Play();
        }

        /// <summary>
        /// 退出演示时停止发射并清理所有弹体和粒子
        /// </summary>
        private void OnDisable()
        {
            if (laser != null)
            {
                laser.gameObject.SetActive(false);
                Destroy(laser.gameObject);
                laser = null;
            }
            activeRounds.Clear();
            foreach (GameObject item in activeEffects)
            {
                if (item == null)
                    continue;
                item.SetActive(false);
                Destroy(item);
            }
            activeEffects.Clear();
        }

        /// <summary>
        /// 在游戏画面显示操作与靶标状态
        /// </summary>
        private void OnGUI()
        {
            const string controls = "Space  Launch     T  Toggle target";
            string mode = movingTarget ? "TARGET / MOVING" : "TARGET / FIXED";
            GUI.Box(new Rect(16f, 16f, 242f, 78f), GUIContent.none);
            GUI.Label(new Rect(29f, 24f, 218f, 20f), controls);
            GUI.Label(new Rect(29f, 44f, 218f, 20f), "Hold L  Laser");
            GUI.Label(new Rect(29f, 64f, 218f, 20f), mode);
        }
    }
}
