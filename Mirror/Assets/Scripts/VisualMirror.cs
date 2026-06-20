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
    public Material mirrorMatTemplate;
    public Material mirrorDeadZoneMat;

    [Header("Render Texture")]
    public int rtWidth = 1024;
    public int rtHeight = 1024;
    public int rtDepth = 24;

    [Header("Dead Zone")]
    public bool useDeadZone = true;
    public bool useDeadZoneWhenMainCameraCannotSeeMirror = true;
    public bool requireFrontSide = true;
    public float minActiveDistance = 0.8f;
    public float minActiveDistanceExit = 1.0f;
    public float maxActiveDistance = 12f;
    public float maxActiveDistanceExit = 11f;
    public float maxViewAngle = 45f;
    public float maxViewAngleExit = 40f;

    [Header("Frustum Fit Mode")]
    public FrustumFitMode frustumFitMode = FrustumFitMode.Fit;

    private Camera _mirrorCam;
    private RenderTexture _mirrorRT;
    private Material _mirrorInstancedMat;
    private Renderer _mirrorRenderer;

    public enum FrustumFitMode
    {
        Contain, // 包含四个角点，保证镜面完全覆盖，但可能有部分镜面未被渲染
        Fit, // 刚好适合四个角点，可能有部分镜面未覆盖
        Average // 对四个角点求平均值，可能有部分镜面未覆盖
    }

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
        _mirrorRenderer = GetComponent<Renderer>();
        if (!_mirrorRenderer) return;
        if (!mirrorMatTemplate)
        {
            Debug.LogWarning($"[{name}] 未指定 mirrorMaterial，无法创建镜面材质", this);
            return;
        }

        _mirrorInstancedMat = new Material(mirrorMatTemplate)
        {
            name = $"{name}_MirrorMat",
            mainTextureScale = new Vector2(-1, 1), // 因为反射会导致左右镜像翻转，翻转贴图 U 方向纠正左右关系
            mainTextureOffset = new Vector2(1, 0),
            mainTexture = _mirrorRT
        };

        ApplyDeadZoneMaterial();
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

        if (_mirrorInstancedMat)
        {
            if (Application.isPlaying) Destroy(_mirrorInstancedMat);
            else DestroyImmediate(_mirrorInstancedMat);
            _mirrorInstancedMat = null;
        }
    }

    public enum MirrorViewState
    {
        ActiveReflection,
        DeadZone
    }


    [Header("Debug Info")]
    public bool showDebugInfo;
    [SerializeField] [ReadOnly] public MirrorViewState viewState = MirrorViewState.DeadZone;
    [SerializeField] [ReadOnly] public string deadZoneReason = "Initializing";
    [SerializeField] [ReadOnly] public List<Vector3> mainCamDirs = new();
    [SerializeField] [ReadOnly] public List<Vector3> mirrorCamDirs = new();

    private void LateUpdate()
    {
        if (!mainCam || !_mirrorCam || !_mirrorRT) return;

        // ===== 1. 根据摄像机与镜子的相对位置关系，判断是渲染镜面反射还是显示 Dead Zone =====
        var shouldUseDeadZone = ShouldUseDeadZone(out var reason);
        deadZoneReason = reason;
        if (shouldUseDeadZone)
        {
            viewState = MirrorViewState.DeadZone;
            ApplyDeadZoneMaterial();
            return;
        }
        viewState = MirrorViewState.ActiveReflection;
        ApplyMirrorMaterial();

        // ===== 2. 计算镜中相机的位置 =====
        var n = transform.forward;
        var p = transform.position;
        if (showDebugInfo) Debug.DrawLine(p, p + n, Color.green); // 镜面法线
        var inDir = p - mainCam.transform.position; // 相机 -> 镜子
        var outDir = Vector3.Reflect(inDir, n); // 镜子 -> 反射的相机 != 镜中的相机
        _mirrorCam.transform.position = p - outDir;

        // ===== 3. 计算镜中相机的朝向 =====
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

        // ===== 4. 计算非对称视锥体，使渲染范围精确对应镜面 =====
        SetAsymmetricFrustum();

        // ===== 5. 渲染到 RenderTexture =====
        _mirrorCam.Render();
        _mirrorCam.ResetProjectionMatrix();
    }

    private bool ShouldUseDeadZone(out string reason)
    {
        reason = string.Empty;
        if (!useDeadZone) return false;

        if (useDeadZoneWhenMainCameraCannotSeeMirror && !CanMainCameraSeeMirror())
        {
            reason = "Main camera cannot see mirror";
            return true;
        }

        var cameraToMirror = transform.position - mainCam.transform.position;
        if (cameraToMirror.sqrMagnitude < Mathf.Epsilon)
        {
            reason = "Camera is at mirror center";
            return true;
        }

        var mirrorToCamera = mainCam.transform.position - transform.position;
        var signedDistance = Vector3.Dot(mirrorToCamera, transform.forward);
        if (requireFrontSide && signedDistance <= 0f)
        {
            reason = "Camera is behind mirror";
            return true;
        }

        var distance = Mathf.Abs(signedDistance);
        var minDistanceEnter = Mathf.Min(minActiveDistance, minActiveDistanceExit);
        var minDistanceExit = Mathf.Max(minActiveDistance, minActiveDistanceExit);
        var minDistanceThreshold = viewState == MirrorViewState.DeadZone ? minDistanceExit : minDistanceEnter;
        if (distance < minDistanceThreshold)
        {
            reason = "Camera is too close";
            return true;
        }

        var maxDistanceEnter = Mathf.Max(maxActiveDistance, maxActiveDistanceExit);
        var maxDistanceExit = Mathf.Min(maxActiveDistance, maxActiveDistanceExit);
        var maxDistanceThreshold = viewState == MirrorViewState.DeadZone ? maxDistanceExit : maxDistanceEnter;
        if (distance > maxDistanceThreshold)
        {
            reason = "Camera is too far";
            return true;
        }

        var toMirror = cameraToMirror.normalized;
        var viewAngleEnter = Mathf.Max(maxViewAngle, maxViewAngleExit);
        var viewAngleExit = Mathf.Min(maxViewAngle, maxViewAngleExit);
        var viewAngleThreshold = viewState == MirrorViewState.DeadZone ? viewAngleExit : viewAngleEnter;
        var viewAngle = Vector3.Angle(mainCam.transform.forward, toMirror);
        if (viewAngle > viewAngleThreshold)
        {
            reason = "Camera is not looking at mirror";
            return true;
        }

        return false;
    }

    private bool CanMainCameraSeeMirror()
    {
        if (!mainCam || !_mirrorRenderer || !_mirrorRenderer.enabled) return false;
        if ((mainCam.cullingMask & (1 << _mirrorRenderer.gameObject.layer)) == 0) return false;
        var planes = GeometryUtility.CalculateFrustumPlanes(mainCam);
        return GeometryUtility.TestPlanesAABB(planes, _mirrorRenderer.bounds);
    }

    private void ApplyMirrorMaterial()
    {
        if (!_mirrorRenderer || !_mirrorInstancedMat) return;
        if (_mirrorRenderer.sharedMaterial == _mirrorInstancedMat) return;
        _mirrorRenderer.sharedMaterial = _mirrorInstancedMat;
    }

    private void ApplyDeadZoneMaterial()
    {
        _mirrorRenderer.sharedMaterial = mirrorDeadZoneMat;
    }

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
            // case FrustumFitMode.Default:
            //     left = Mathf.Min(nearBL.x, Mathf.Min(nearBR.x, Mathf.Min(nearTL.x, nearTR.x)));
            //     right = Mathf.Max(nearBL.x, Mathf.Max(nearBR.x, Mathf.Max(nearTL.x, nearTR.x)));
            //     bottom = Mathf.Min(nearBL.y, Mathf.Min(nearBR.y, Mathf.Min(nearTL.y, nearTR.y)));
            //     top = Mathf.Max(nearBL.y, Mathf.Max(nearBR.y, Mathf.Max(nearTL.y, nearTR.y)));
            //     break;
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
            case FrustumFitMode.Average:
                left = (nearBL.x + nearTL.x) * 0.5f;
                right = (nearBR.x + nearTR.x) * 0.5f;
                bottom = (nearBL.y + nearBR.y) * 0.5f;
                top = (nearTL.y + nearTR.y) * 0.5f;
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