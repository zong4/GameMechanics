using UnityEngine;

namespace Utility
{
    public static class Math
    {
        public static Vector3 RoundVector3(Vector3 v, int digits = 3)
        {
            var p = Mathf.Pow(10f, digits);
            return new Vector3(Mathf.Round(v.x * p) / p, Mathf.Round(v.y * p) / p, Mathf.Round(v.z * p) / p);
        }

        // 把非对称视锥体的 near/far 平面边界，以及四条侧边线，画在 Scene 视图中
        public static void DrawFrustumDebug(Matrix4x4 worldToCam, float left, float right, float bottom, float top,
            float near, float far)
        {
            var camToWorld = worldToCam.inverse;

            var nearBL = new Vector3(left, bottom, -near);
            var nearBR = new Vector3(right, bottom, -near);
            var nearTL = new Vector3(left, top, -near);
            var nearTR = new Vector3(right, top, -near);

            var scaleFar = far / near;
            var farBL = new Vector3(left * scaleFar, bottom * scaleFar, -far);
            var farBR = new Vector3(right * scaleFar, bottom * scaleFar, -far);
            var farTL = new Vector3(left * scaleFar, top * scaleFar, -far);
            var farTR = new Vector3(right * scaleFar, top * scaleFar, -far);

            var wNearBL = camToWorld.MultiplyPoint(nearBL);
            var wNearBR = camToWorld.MultiplyPoint(nearBR);
            var wNearTL = camToWorld.MultiplyPoint(nearTL);
            var wNearTR = camToWorld.MultiplyPoint(nearTR);
            var wFarBL = camToWorld.MultiplyPoint(farBL);
            var wFarBR = camToWorld.MultiplyPoint(farBR);
            var wFarTL = camToWorld.MultiplyPoint(farTL);
            var wFarTR = camToWorld.MultiplyPoint(farTR);
            var wCamPos = camToWorld.MultiplyPoint(Vector3.zero); // 相机原点

            var color = Color.cyan;

            Debug.DrawLine(wNearBL, wNearBR, color);
            Debug.DrawLine(wNearBR, wNearTR, color);
            Debug.DrawLine(wNearTR, wNearTL, color);
            Debug.DrawLine(wNearTL, wNearBL, color);

            Debug.DrawLine(wFarBL, wFarBR, color);
            Debug.DrawLine(wFarBR, wFarTR, color);
            Debug.DrawLine(wFarTR, wFarTL, color);
            Debug.DrawLine(wFarTL, wFarBL, color);

            Debug.DrawLine(wNearBL, wFarBL, color);
            Debug.DrawLine(wNearBR, wFarBR, color);
            Debug.DrawLine(wNearTL, wFarTL, color);
            Debug.DrawLine(wNearTR, wFarTR, color);

            Debug.DrawLine(wCamPos, wNearBL, Color.yellow);
            Debug.DrawLine(wCamPos, wNearBR, Color.yellow);
            Debug.DrawLine(wCamPos, wNearTL, Color.yellow);
            Debug.DrawLine(wCamPos, wNearTR, Color.yellow);
        }

        // 构造非对称（off-center）透视投影矩阵，等价于 OpenGL 的 glFrustum(left, right, bottom, top, near, far)
        public static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near,
            float far)
        {
            var x = 2.0f * near / (right - left);
            var y = 2.0f * near / (top - bottom);
            var a = (right + left) / (right - left);
            var b = (top + bottom) / (top - bottom);
            var c = -(far + near) / (far - near);
            var d = -(2.0f * far * near) / (far - near);

            var m = new Matrix4x4
            {
                [0, 0] = x,
                [0, 1] = 0,
                [0, 2] = a,
                [0, 3] = 0,
                [1, 0] = 0,
                [1, 1] = y,
                [1, 2] = b,
                [1, 3] = 0,
                [2, 0] = 0,
                [2, 1] = 0,
                [2, 2] = c,
                [2, 3] = d,
                [3, 0] = 0,
                [3, 1] = 0,
                [3, 2] = -1.0f,
                [3, 3] = 0
            };
            return m;
        }
    }
}