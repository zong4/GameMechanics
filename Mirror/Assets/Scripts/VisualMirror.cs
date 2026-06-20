using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Utility;

// Hack
// 1. forward 必须朝向摄像机所在的一侧
[ExecuteInEditMode]
public class VisualMirror : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("默认使用场景主摄像机")] public Camera mainCam;
    public Camera mirrorCamPrefab;
    [Tooltip("镜子材质模板，脚本会基于它创建一份实例")] public Material mirrorMaterial;

    [Header("Render Texture Settings")]
    public int rtWidth = 1024;
    public int rtHeight = 1024;
    public int rtDepth = 24;

    private Camera _mirrorCam;
    private RenderTexture _mirrorRT;
    private Material _instancedMat;

    private void OnEnable()
    {
        ResolveMainCamera();
        SetupMirrorCamera();
        SetupRenderTexture();
        SetupMaterial();
    }

    private void OnDisable()
    {
        Cleanup();
    }

    private void ResolveMainCamera()
    {
        if (mainCam) return;
        mainCam = Camera.main;
        if (!mainCam) Debug.LogWarning($"[{name}] 未指定 mainCam，且场景中没有 tag 为 MainCamera 的摄像机", this);
    }

    private void SetupMirrorCamera()
    {
        _mirrorCam = transform.GetComponentInChildren<Camera>();
        if (_mirrorCam) return;
        _mirrorCam = Instantiate(mirrorCamPrefab, transform);
        _mirrorCam.name = $"{name}_MirrorCam";
        _mirrorCam.enabled = false;
    }

    private void SetupRenderTexture()
    {
        if (_mirrorRT) return;
        _mirrorRT = new RenderTexture(rtWidth, rtHeight, rtDepth)
        {
            name = $"{name}_MirrorRT",
            antiAliasing = 2,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Trilinear
        };
        _mirrorRT.Create();
        if (_mirrorCam) _mirrorCam.targetTexture = _mirrorRT;
        else Debug.LogWarning($"[{name}] 无法将 RenderTexture 赋给镜中相机，因为镜中相机未创建成功", this);
    }

    private void SetupMaterial()
    {
        var rendererComp = GetComponent<Renderer>();
        if (!rendererComp) return;
        if (!mirrorMaterial)
        {
            Debug.LogWarning($"[{name}] 未指定 mirrorMaterial，无法创建镜面材质", this);
            return;
        }

        _instancedMat = new Material(mirrorMaterial)
        {
            name = $"{name}_MirrorMat",
            mainTextureScale = new Vector2(-1, 1), // 因为反射会导致左右镜像翻转，翻转贴图 U 方向纠正左右关系
            mainTextureOffset = new Vector2(1, 0),
            mainTexture = _mirrorRT
        };
        rendererComp.material = _instancedMat;
    }

    private void Cleanup()
    {
        if (_mirrorCam)
        {
            if (Application.isPlaying) Destroy(_mirrorCam.gameObject);
            else DestroyImmediate(_mirrorCam.gameObject);
            _mirrorCam = null;
        }

        if (_mirrorRT)
        {
            _mirrorRT.Release();
            if (Application.isPlaying) Destroy(_mirrorRT);
            else DestroyImmediate(_mirrorRT);
            _mirrorRT = null;
        }

        if (_instancedMat)
        {
            if (Application.isPlaying) Destroy(_instancedMat);
            else DestroyImmediate(_instancedMat);
            _instancedMat = null;
        }
    }

    [Header("Debug Info")]
    public bool showDebugInfo;
    public List<Vector3> mainCamDirs = new();
    public List<Vector3> mirrorCamDirs = new();

    private void LateUpdate()
    {
        if (!mainCam || !_mirrorCam || !_mirrorRT) return;

        // ===== 1. 计算镜中相机的位置 =====
        var n = transform.forward;
        var p = transform.position;
        if (showDebugInfo) Debug.DrawLine(p, p + n, Color.green); // 镜面法线
        var inDir = p - mainCam.transform.position; // 相机 -> 镜子
        var outDir = Vector3.Reflect(inDir, n); // 镜子 -> 反射的相机 != 镜中的相机
        _mirrorCam.transform.position = p - outDir;

        // ===== 2. 计算镜中相机的朝向 =====
        _mirrorCam.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(mainCam.transform.forward, n),
            Vector3.Reflect(mainCam.transform.up, n));
        mainCamDirs = new List<Vector3>
        {
            Math.RoundVector3(mainCam.transform.forward),
            Math.RoundVector3(mainCam.transform.up),
            Math.RoundVector3(mainCam.transform.right)
        };
        mirrorCamDirs = new List<Vector3>
        {
            Math.RoundVector3(_mirrorCam.transform.forward),
            Math.RoundVector3(_mirrorCam.transform.up),
            Math.RoundVector3(_mirrorCam.transform.right)
        };

        // ===== 3. 计算非对称视锥体，使渲染范围精确对应镜面 =====
        SetAsymmetricFrustum();

        // ===== 4. 渲染到 RenderTexture =====
        _mirrorCam.Render();
        _mirrorCam.ResetProjectionMatrix();
    }

    public enum FrustumFitMode
    {
        Default, // 对完整的四个角点求 AABB 包围盒，保证完全覆盖但可能有多余渲染
        Contain, // 每次只对边上的两个点求边界，保证完全覆盖但可能有多余渲染
        Fit // 刚好适合四个角点，可能有部分镜面未覆盖
    }

    public FrustumFitMode frustumFitMode = FrustumFitMode.Fit;

    private void SetAsymmetricFrustum()
    {
        // --- 镜面在 Cube 本地空间中的四个角点 ---
        var localBL = new Vector3(-0.5f, -0.5f, 0.5f); // 左下
        var localBR = new Vector3(0.5f, -0.5f, 0.5f); // 右下
        var localTL = new Vector3(-0.5f, 0.5f, 0.5f); // 左上
        var localTR = new Vector3(0.5f, 0.5f, 0.5f); // 右上

        // --- 转换到世界坐标（应用物体的位置/旋转/缩放） ---
        var worldBL = transform.TransformPoint(localBL);
        var worldBR = transform.TransformPoint(localBR);
        var worldTL = transform.TransformPoint(localTL);
        var worldTR = transform.TransformPoint(localTR);
        if (showDebugInfo)
        {
            Debug.DrawLine(worldBL, worldBR, Color.red);
            Debug.DrawLine(worldBR, worldTR, Color.red);
            Debug.DrawLine(worldTR, worldTL, Color.red);
            Debug.DrawLine(worldTL, worldBL, Color.red);
        }

        // --- 转换到 mirrorCam 的相机空间 ---
        var worldToCam = _mirrorCam.worldToCameraMatrix;
        var camBL = worldToCam.MultiplyPoint(worldBL);
        var camBR = worldToCam.MultiplyPoint(worldBR);
        var camTL = worldToCam.MultiplyPoint(worldTL);
        var camTR = worldToCam.MultiplyPoint(worldTR);
        // Debug.Log($"[{name}] 镜面四个角点的相机空间坐标：BL={camBL}, BR={camBR}, TL={camTL}, TR={camTR}");

        // --- 确定 near/far 平面距离 ---
        var near = Mathf.Min(-camBL.z, -camBR.z, -camTL.z, -camTR.z); // 相机空间中，相机正前方的点 z 为负值
        var far = _mirrorCam.farClipPlane;

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
        float left, right, bottom, top;
        switch (frustumFitMode)
        {
            case FrustumFitMode.Default:
                left = Mathf.Min(nearBL.x, Mathf.Min(nearBR.x, Mathf.Min(nearTL.x, nearTR.x)));
                right = Mathf.Max(nearBL.x, Mathf.Max(nearBR.x, Mathf.Max(nearTL.x, nearTR.x)));
                bottom = Mathf.Min(nearBL.y, Mathf.Min(nearBR.y, Mathf.Min(nearTL.y, nearTR.y)));
                top = Mathf.Max(nearBL.y, Mathf.Max(nearBR.y, Mathf.Max(nearTL.y, nearTR.y)));
                break;
            case FrustumFitMode.Contain:
                left = Mathf.Min(nearBL.x, nearTL.x);
                right = Mathf.Max(nearBR.x, nearTR.x);
                bottom = Mathf.Min(nearBL.y, nearBR.y);
                top = Mathf.Max(nearTL.y, nearTR.y);
                break;
            case FrustumFitMode.Fit:
                left = Mathf.Max(nearBL.x, nearTL.x);
                right = Mathf.Min(nearBR.x, nearTR.x);
                bottom = Mathf.Max(nearBL.y, nearBR.y);
                top = Mathf.Min(nearTL.y, nearTR.y);
                break;
            default:
                left = right = bottom = top = 0;
                Debug.LogError($"[{name}] 未知的 frustumFitMode={frustumFitMode}", this);
                break;
        }
        if (showDebugInfo) Math.DrawFrustumDebug(worldToCam, left, right, bottom, top, near, far);

        // --- 构造非对称透视投影矩阵并应用 ---
        var proj = Math.PerspectiveOffCenter(left, right, bottom, top, near, far);
        _mirrorCam.projectionMatrix = proj;
    }
}