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
    public MirrorSamplingMode samplingMode = MirrorSamplingMode.ScreenSpaceUV;
    public Material asymmetricFrustumMatTemplate;
    public Material screenSpaceMatTemplate;
    public Material deadZoneMat;

    [Header("Render Texture")]
    public int rtWidth = 1024;
    public int rtHeight = 1024;
    public int rtDepth = 24;

    private Camera _mirrorCam;
    private RenderTexture _mirrorRT;
    private Renderer _mirrorRenderer;
    private Material _mirrorInstancedMat;
    private const string VisibleMirrorLayerName = "Default";
    private const string IgnoreMirrorLayerName = "Mirror";
    private int _visibleMirrorLayer = -1;
    private int _ignoreMirrorLayer = -1;

    public enum MirrorSamplingMode
    {
        AsymmetricFrustum,
        ScreenSpaceUV
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

    private void OnEnable()
    {
        ResolveMirrorLayers();
        ResolveMainCamera();
        SetupMirrorCamera();
        SetupRenderTexture();
        SetupMaterial();
    }

    private void OnDisable()
    {
        Cleanup();
    }

    private void LateUpdate()
    {
        if (!mainCam || !_mirrorCam || !_mirrorRT) return;

        // ==== 1. 判断是否需要显示 Dead Zone =====
        var shouldUseDeadZone = ShouldUseDeadZone(out var reason);
        if (shouldUseDeadZone)
        {
            viewState = MirrorViewState.DeadZone;
            deadZoneReason = reason;
            ApplyDeadZoneMaterial();
            return;
        }

        // ===== 2. 更新镜中相机的位置和朝向 =====
        viewState = MirrorViewState.ActiveReflection;
        deadZoneReason = string.Empty;
        ApplyMirrorMaterial();
        UpdateMirrorCameraTransform();

        // ===== 3. 根据采样模式设置镜中相机的投影矩阵 =====
        switch (samplingMode)
        {
            case MirrorSamplingMode.AsymmetricFrustum:
                SetAsymmetricFrustum();
                break;
            case MirrorSamplingMode.ScreenSpaceUV:
                ApplyMainCameraProjectionToMirrorCamera();
                break;
            default:
                Debug.LogWarning($"[{name}] 未知的采样模式 {samplingMode}", this);
                break;
        }

        // ==== 4. 渲染镜中相机 =====
        RenderMirrorCamera();
        _mirrorCam.ResetProjectionMatrix();
    }

    private void ResolveMirrorLayers()
    {
        _visibleMirrorLayer = LayerMask.NameToLayer(VisibleMirrorLayerName);
        _ignoreMirrorLayer = LayerMask.NameToLayer(IgnoreMirrorLayerName);
        if (_visibleMirrorLayer < 0)
            Debug.LogWarning($"[{name}] Layer '{VisibleMirrorLayerName}' does not exist", this);
        if (_ignoreMirrorLayer < 0) Debug.LogWarning($"[{name}] Layer '{IgnoreMirrorLayerName}' does not exist", this);
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

        switch (samplingMode)
        {
            case MirrorSamplingMode.AsymmetricFrustum:
                _mirrorInstancedMat = new Material(asymmetricFrustumMatTemplate)
                {
                    name = $"{name}_MirrorAsymmetricFrustumMat",
                    mainTextureScale = new Vector2(-1, 1), // 因为反射会导致左右镜像翻转，翻转贴图 U 方向纠正左右关系
                    mainTextureOffset = new Vector2(1, 0),
                    mainTexture = _mirrorRT
                };
                break;
            case MirrorSamplingMode.ScreenSpaceUV:
                _mirrorInstancedMat = new Material(screenSpaceMatTemplate)
                {
                    name = $"{name}_MirrorScreenSpaceMat", mainTexture = _mirrorRT
                };
                // _mirrorInstancedMat.SetFloat("_FlipX", 1f); // 因为反射会导致左右镜像翻转，翻转贴图 X 方向纠正左右关系
                // _mirrorInstancedMat.SetFloat("_FlipY", 0f);
                break;
            default:
                Debug.LogWarning($"[{name}] 未知的采样模式 {samplingMode}", this);
                break;
        }

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

    private void UpdateMirrorCameraTransform()
    {
        var normal = transform.forward;
        var mirrorPosition = transform.position;
        if (showDebugInfo) Debug.DrawLine(mirrorPosition, mirrorPosition + normal, Color.green);

        var inDir = mirrorPosition - mainCam.transform.position;
        var outDir = Vector3.Reflect(inDir, normal);
        _mirrorCam.transform.position = mirrorPosition - outDir;
        _mirrorCam.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(mainCam.transform.forward, normal),
            Vector3.Reflect(mainCam.transform.up, normal));

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
    }

    private void RenderMirrorCamera()
    {
        var previousLayer = gameObject.layer;
        gameObject.layer = _ignoreMirrorLayer; // 避免镜中相机渲染到自身
        try { _mirrorCam.Render(); }
        finally { gameObject.layer = previousLayer; }
    }

    private bool ShouldUseDeadZone(out string reason)
    {
        reason = string.Empty;

        if (!CanMainCameraSeeMirror())
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
        if (signedDistance <= 0f)
        {
            reason = "Camera is behind mirror";
            return true;
        }

        return false;
    }

    private bool CanMainCameraSeeMirror()
    {
        if (!mainCam || !_mirrorRenderer || !_mirrorRenderer.enabled) return false;
        var planes = GeometryUtility.CalculateFrustumPlanes(mainCam);
        return GeometryUtility.TestPlanesAABB(planes, _mirrorRenderer.bounds);
    }

    private void ApplyMirrorMaterial()
    {
        if (!_mirrorRenderer || !_mirrorInstancedMat) return;
        gameObject.layer = _visibleMirrorLayer;
        _mirrorRenderer.sharedMaterial = _mirrorInstancedMat;
    }

    private void ApplyDeadZoneMaterial()
    {
        if (!_mirrorRenderer || !deadZoneMat) return;
        gameObject.layer = _ignoreMirrorLayer;
        _mirrorRenderer.sharedMaterial = deadZoneMat;
    }

    private void ApplyMainCameraProjectionToMirrorCamera()
    {
        _mirrorCam.ResetProjectionMatrix();
        _mirrorCam.nearClipPlane = mainCam.nearClipPlane;
        _mirrorCam.farClipPlane = mainCam.farClipPlane;
        _mirrorCam.orthographic = mainCam.orthographic;
        _mirrorCam.orthographicSize = mainCam.orthographicSize;
        _mirrorCam.fieldOfView = mainCam.fieldOfView;
        _mirrorCam.projectionMatrix = mainCam.projectionMatrix;
    }

    private void SetAsymmetricFrustum()
    {
        var localBL = new Vector3(-0.5f, -0.5f, 0.5f);
        var localBR = new Vector3(0.5f, -0.5f, 0.5f);
        var localTL = new Vector3(-0.5f, 0.5f, 0.5f);
        var localTR = new Vector3(0.5f, 0.5f, 0.5f);

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

        var worldToCam = _mirrorCam.worldToCameraMatrix;
        var camBL = worldToCam.MultiplyPoint(worldBL);
        var camBR = worldToCam.MultiplyPoint(worldBR);
        var camTL = worldToCam.MultiplyPoint(worldTL);
        var camTR = worldToCam.MultiplyPoint(worldTR);

        var near = Mathf.Min(-camBL.z, -camBR.z, -camTL.z, -camTR.z);
        var far = _mirrorCam.farClipPlane;

        var scaleBL = near / -camBL.z;
        var scaleBR = near / -camBR.z;
        var scaleTL = near / -camTL.z;
        var scaleTR = near / -camTR.z;
        var nearBL = new Vector3(camBL.x * scaleBL, camBL.y * scaleBL, -near);
        var nearBR = new Vector3(camBR.x * scaleBR, camBR.y * scaleBR, -near);
        var nearTL = new Vector3(camTL.x * scaleTL, camTL.y * scaleTL, -near);
        var nearTR = new Vector3(camTR.x * scaleTR, camTR.y * scaleTR, -near);

        var left = Mathf.Max(nearBL.x, nearTL.x);
        var right = Mathf.Min(nearBR.x, nearTR.x);
        var bottom = Mathf.Max(nearBL.y, nearBR.y);
        var top = Mathf.Min(nearTL.y, nearTR.y);
        if (showDebugInfo) Math.DrawFrustumDebug(worldToCam, left, right, bottom, top, near, far);

        _mirrorCam.projectionMatrix = Math.PerspectiveOffCenter(left, right, bottom, top, near, far);
    }
}