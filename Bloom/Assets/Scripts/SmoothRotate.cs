using UnityEngine;

public class SmoothRotate : MonoBehaviour
{
    // 旋转速度（每秒旋转的角度）
    public float rotationSpeed = 45.0f;

    // 摆动幅度（最大左右旋转角度）
    public float swingAngle = 30.0f;

    // 时间变量，用于计算正弦波
    private float timeCounter = 0.0f;

    void Update()
    {
        // 增加时间计数器
        timeCounter += Time.deltaTime;

        // 使用正弦函数生成平滑的摆动角度
        float angle = Mathf.Sin(timeCounter * rotationSpeed * Mathf.Deg2Rad) * swingAngle;

        // 应用旋转到物体（绕Y轴旋转）
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }
}