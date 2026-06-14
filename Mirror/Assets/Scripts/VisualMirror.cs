using UnityEngine;

[ExecuteInEditMode]
public class VisualMirror : MonoBehaviour
{
    public Camera mainCam;
    public Camera mirrorCam;

    private void Awake()
    {
        // 因为反射会导致左右镜像翻转（手性改变），这里通过翻转贴图的 U 方向来纠正，使镜子里的画面左右关系正确
        var mat = GetComponent<Renderer>().sharedMaterial;
        mat.mainTextureScale = new Vector2(-1, 1);
        mat.mainTextureOffset = new Vector2(1, 0);
    }

    private void LateUpdate()
    {
        if (!mainCam || !mirrorCam) return;

        // 镜子的位置和法线方向（forward 必须朝向摄像机所在的一侧）
        var n = transform.forward;
        var p = transform.position;

        // ===== 1. 计算镜中相机的位置 =====
        var inDir = p - mainCam.transform.position;
        var outDir = Vector3.Reflect(inDir, n);
        mirrorCam.transform.position = p - outDir;

        // ===== 2. 计算镜中相机的朝向 =====
        mirrorCam.transform.rotation =
            Quaternion.LookRotation(
                Vector3.Reflect(mainCam.transform.forward, n),
                Vector3.Reflect(mainCam.transform.up, n)
            );

        // ===== 3. 计算非对称视锥体，使渲染范围精确对应镜面 =====
        SetAsymmetricFrustum();

        // ===== 4. 渲染到 RenderTexture =====
        mirrorCam.Render();
        mirrorCam.ResetProjectionMatrix();
        Debug.DrawRay(transform.position, transform.forward * 2, Color.green);
    }

    private void SetAsymmetricFrustum()
    {
        // --- 镜面在 Cube 本地空间中的四个角点（左下/右下/左上/右上） ---
        // 假设镜面位于 Cube 的 +Z 面
        var localBL = new Vector3(-0.5f, -0.5f, 0.5f); // Bottom Left
        var localBR = new Vector3(0.5f, -0.5f, 0.5f); // Bottom Right
        var localTL = new Vector3(-0.5f, 0.5f, 0.5f); // Top Left
        var localTR = new Vector3(0.5f, 0.5f, 0.5f); // Top Right

        // --- 转换到世界坐标（应用物体的位置/旋转/缩放） ---
        var worldBL = transform.TransformPoint(localBL);
        var worldBR = transform.TransformPoint(localBR);
        var worldTL = transform.TransformPoint(localTL);
        var worldTR = transform.TransformPoint(localTR);

        // --- 转换到 mirrorCam 的相机空间 ---
        var worldToCam = mirrorCam.worldToCameraMatrix;
        var camBL = worldToCam.MultiplyPoint(worldBL);
        var camBR = worldToCam.MultiplyPoint(worldBR);
        var camTL = worldToCam.MultiplyPoint(worldTL);
        var camTR = worldToCam.MultiplyPoint(worldTR);

        // --- 确定 near/far 平面距离 ---
        // near 取四个角点到相机距离与原始 nearClipPlane 中的最小值的最大值，确保 near 面不会切穿镜面本身
        var near = Mathf.Max(mirrorCam.nearClipPlane,
            Mathf.Min(-camBL.z, -camBR.z, -camTL.z, -camTR.z)); //相机空间中，相机正前方的点 z 为负值
        var far = mirrorCam.farClipPlane;

        // --- 将四个角点投影到 near 平面上 ---
        // 透视投影下，点 (x,y,z) 投影到距离为 near 的平面上时，屏幕坐标按比例 near/(-z) 缩放
        var scaleBL = near / -camBL.z;
        var scaleBR = near / -camBR.z;
        var scaleTL = near / -camTL.z;
        var scaleTR = near / -camTR.z;
        var nearBL = new Vector3(camBL.x * scaleBL, camBL.y * scaleBL, -near);
        var nearBR = new Vector3(camBR.x * scaleBR, camBR.y * scaleBR, -near);
        var nearTL = new Vector3(camTL.x * scaleTL, camTL.y * scaleTL, -near);
        var nearTR = new Vector3(camTR.x * scaleTR, camTR.y * scaleTR, -near);

        // --- 由投影后的四个角点求轴对齐包围盒（AABB），作为视锥体边界 ---
        // 摄像机斜对镜子时，投影后的四边形不是轴对齐矩形，必须取四个角点 x/y 的 min/max 才能保证视锥体完全覆盖镜面
        var left = Mathf.Min(nearBL.x, Mathf.Min(nearBR.x, Mathf.Min(nearTL.x, nearTR.x)));
        var right = Mathf.Max(nearBL.x, Mathf.Max(nearBR.x, Mathf.Max(nearTL.x, nearTR.x)));
        var bottom = Mathf.Min(nearBL.y, Mathf.Min(nearBR.y, Mathf.Min(nearTL.y, nearTR.y)));
        var top = Mathf.Max(nearBL.y, Mathf.Max(nearBR.y, Mathf.Max(nearTL.y, nearTR.y)));

        // 用同一个 worldToCam 的精确逆来画 debug 视锥体，保证和角点计算一致
        DrawFrustumDebug(worldToCam, left, right, bottom, top, near, far);

        // --- 构造非对称透视投影矩阵并应用 ---
        var proj = PerspectiveOffCenter(left, right, bottom, top, near, far);
        mirrorCam.projectionMatrix = proj;
    }

    // 把非对称视锥体的 near/far 平面边界，以及四条侧边线，画在 Scene 视图中
    private static void DrawFrustumDebug(Matrix4x4 worldToCam, float left, float right, float bottom, float top,
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
    private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near,
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
            [0, 0] = x, [0, 1] = 0, [0, 2] = a, [0, 3] = 0,
            [1, 0] = 0, [1, 1] = y, [1, 2] = b, [1, 3] = 0,
            [2, 0] = 0, [2, 1] = 0, [2, 2] = c, [2, 3] = d,
            [3, 0] = 0, [3, 1] = 0, [3, 2] = -1.0f, [3, 3] = 0
        };
        return m;
    }
}