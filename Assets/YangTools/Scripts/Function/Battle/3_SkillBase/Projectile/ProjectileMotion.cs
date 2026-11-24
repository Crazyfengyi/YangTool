using UnityEngine;

/// <summary>
/// 单发弹体轨迹计算器 使用累计时间计算位移
/// </summary>
internal sealed class ProjectileMotion
{
    private readonly ProjectileRuntime data; //运行数据
    private readonly Vector3 start, forward, right, destination, radial; //固定轨迹基准
    private readonly float radius; //初始圆周半径
    private Vector3 tracking; //追踪方向
    internal Vector3 Position { get; private set; }
    internal Vector3 Direction { get; private set; }
    internal bool Finished { get; private set; }

    /// <summary>
    /// 固定发射基准与目标点
    /// </summary>
    internal ProjectileMotion(ProjectileRuntime runtime, Vector3 position)
    {
        data = runtime; start = position; Position = position;
        forward = Horizontal(data.direction); tracking = forward; Direction = forward;
        right = Vector3.Cross(Vector3.up, forward);
        destination = data.context.Target != null ? data.context.Target.transform.position : data.context.Point;
        if ((destination - start).sqrMagnitude < 0.0001f) destination = start + forward * data.flight.distance;
        radial = Horizontal(start - data.center);
        radius = Vector3.ProjectOnPlane(start - data.center, Vector3.up).magnitude;
        if (radius < 0.001f) { radius = Mathf.Max(0.01f, data.flight.radius); radial = -right; }
        if (data.flight.mode == ProjectileFlightMode.SpecifyTargetFlyLine) forward = Horizontal(destination - start);
    }

    /// <summary>
    /// 将方向投影至地面并处理零向量
    /// </summary>
    internal static Vector3 Horizontal(Vector3 value)
    {
        value.y = 0;
        return value.sqrMagnitude > 0.0001f ? value.normalized : Vector3.forward;
    }

    /// <summary>
    /// 计算累计时间对应的位置和当前切线
    /// </summary>
    internal void Step(float age, float delta)
    {
        Vector3 previous = Position;
        var f = data.flight;
        float speed = Mathf.Max(0, data.speed);
        if (data.stopDistance > 0)
        {
            Position = start + forward * Mathf.Min(speed * age, data.stopDistance);
        }
        else switch (f.mode)
        {
            case ProjectileFlightMode.Lobbed:
                float u = Mathf.Clamp01(age / Mathf.Max(0.01f, f.duration));
                Position = Vector3.Lerp(start, destination, u) + Vector3.up * (4 * f.height * u * (1 - u));
                Finished = u >= 1;
                break;
            case ProjectileFlightMode.Lobbed2:
                float angle = f.elevation * Mathf.Deg2Rad;
                float endTime = 2 * speed * Mathf.Sin(angle) / Mathf.Max(0.01f, f.gravity);
                float time = Mathf.Min(age, endTime);
                Position = start + forward * (speed * Mathf.Cos(angle) * time)
                    + Vector3.up * (speed * Mathf.Sin(angle) * time - 0.5f * f.gravity * time * time);
                Finished = age >= endTime;
                break;
            case ProjectileFlightMode.Circle:
            case ProjectileFlightMode.Orbit:
                float r = f.mode == ProjectileFlightMode.Orbit ? Mathf.Max(0.01f, radius + f.radialSpeed * age) : radius;
                // 圆心偏移确保从实际生成位置出发而不跳位
                Vector3 center = start - radial * radius;
                Position = center + Quaternion.AngleAxis(f.angularSpeed * age, Vector3.up) * radial * r;
                break;
            case ProjectileFlightMode.Sine:
                Position = start + forward * speed * age + right * (f.amplitude * Mathf.Sin(2 * Mathf.PI * f.frequency * age));
                break;
            case ProjectileFlightMode.Tracking:
                if (data.context.Target != null)
                    tracking = Vector3.RotateTowards(tracking, Horizontal(data.context.Target.transform.position - Position),
                        Mathf.Max(0, f.turnSpeed) * Mathf.Deg2Rad * delta, 0);
                Position += tracking * speed * delta;
                break;
            case ProjectileFlightMode.FlyAwayAndBack:
                float distance = Mathf.Max(0.01f, f.distance);
                float travel = speed * age;
                Position = start + forward * Mathf.Max(0, travel <= distance ? travel : 2 * distance - travel);
                Finished = travel >= 2 * distance;
                break;
            case ProjectileFlightMode.LaunchCircle:
                float launchTime = Mathf.Max(0.01f, f.distance) / Mathf.Max(0.01f, speed);
                if (age <= launchTime) Position = start + forward * speed * age;
                else
                {
                    // 三次曲线衔接初始直线切线与圆周切线
                    float t = age - launchTime;
                    float transition = Mathf.Max(0.01f, f.turnDuration);
                    float w = f.angularSpeed * Mathf.Deg2Rad;
                    Vector3 launchEnd = start + forward * Mathf.Max(0.01f, f.distance);
                    float sign = w < 0 ? -1 : 1;
                    Vector3 tangent = right * sign;
                    Vector3 end = launchEnd + (forward + tangent) * (speed * transition * 0.5f);
                    if (t < transition)
                    {
                        float v = t / transition;
                        Vector3 p1 = launchEnd + forward * (speed * transition / 3);
                        Vector3 p2 = end - tangent * (Mathf.Abs(w) * f.radius * transition / 3);
                        Position = Mathf.Pow(1 - v, 3) * launchEnd + 3 * (1 - v) * (1 - v) * v * p1
                            + 3 * (1 - v) * v * v * p2 + v * v * v * end;
                    }
                    else
                    {
                        float turn = Mathf.Abs(w) * (t - transition);
                        Position = end + tangent * (Mathf.Sin(turn) * f.radius) - forward * ((1 - Mathf.Cos(turn)) * f.radius);
                    }
                }
                break;
            default:
                float movingTime = age;
                if (f.mode == ProjectileFlightMode.Linear && f.acceleration < 0)
                    movingTime = Mathf.Min(age, speed / -f.acceleration);
                float acceleration = f.mode == ProjectileFlightMode.Linear ? f.acceleration : 0;
                Position = start + forward * (speed * movingTime + 0.5f * acceleration * movingTime * movingTime);
                break;
        }
        Vector3 movement = Position - previous;
        if (movement.sqrMagnitude > 0.000001f) Direction = movement.normalized;
    }
}
