using UnityEditor;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Portal otherPortal;
    [SerializeField] Vector2 portalSize = new(1, 2);
    [SerializeField] Color portalColor = Color.blue;

    [SerializeField] Material targetMaterial;

    [SerializeField, HideInInspector] Camera _portalCamera;
    [SerializeField, HideInInspector] MeshRenderer _meshRenderer;
    [SerializeField, HideInInspector] MeshFilter _meshFilter;

    [SerializeField, HideInInspector] RenderTexture _renderTexture;
    [SerializeField, HideInInspector] Material _renderMaterial;


    Camera _mainCamera;

    void Start() => Setup();

    void OnValidate() => Setup();

    void Setup()
    {
        if (_portalCamera == null)
            _portalCamera = GetComponentInChildren<Camera>();
        if (_meshRenderer == null)
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (_meshFilter == null)
            _meshFilter = GetComponentInChildren<MeshFilter>();

        if (_renderMaterial == null)
        {
            _renderMaterial = new(Shader.Find("Unlit/PortalShader"));
            _renderMaterial.name = $"{name} Render Material";
        }

        _meshRenderer.material = _renderMaterial;

        if (_renderTexture == null)
        {
            int displayWidth = Screen.width;
            int displayHeight = Screen.height;
            _renderTexture = new(displayWidth, displayHeight, 24);
            _renderTexture.name = $"{name} Render Texture";
        }
        else
            TryUpdateTextureSize();

        _renderMaterial.SetTexture("_MainTex", _renderTexture);

        if (targetMaterial != null)
            targetMaterial.SetTexture("_MainTex", _renderTexture);
        _portalCamera.targetTexture = _renderTexture;

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        _portalCamera.enabled = false;
    }

    void TryUpdateTextureSize()
    {
        bool isSceneView = Camera.current != null && Camera.current.cameraType == CameraType.SceneView;
        if (isSceneView)
            return;

        int displayWidth = Screen.width;
        int displayHeight = Screen.height;
        if (_renderTexture.width == displayWidth && _renderTexture.height == displayHeight)
            return;

        Debug.Log($"Updating {name} Render Texture Size\n" +
            $"from ({_renderTexture.width}, {_renderTexture.height}) to ({displayWidth}, {displayHeight})");


        RenderTexture newRenderTexture = new(displayWidth, displayHeight, 24);
        newRenderTexture.name = $"{name} Render Texture";
        _renderMaterial.SetTexture("_MainTex", newRenderTexture);
        _portalCamera.targetTexture = newRenderTexture;

        _renderTexture.Release();
        _renderTexture = newRenderTexture;
    }

    void LateUpdate()
    {
        // Return if the portal is not visible to the camera
        if (!IsInsideCamera(_mainCamera, _meshRenderer.bounds))
            return;

        // Update the render texture size if the screen size changes
        TryUpdateTextureSize();

        // Transformation Matrix
        Matrix4x4 worldToPortal = WorldToPortalWorld();

        // Camera Position & Rotation
        Vector3 cameraPoint = worldToPortal.MultiplyPoint(_mainCamera.transform.position);
        Quaternion cameraRotation = worldToPortal.rotation * _mainCamera.transform.rotation;
        _portalCamera.transform.SetPositionAndRotation(cameraPoint, cameraRotation);

        // Camera projection 
        Vector3 otherPortalForward = otherPortal.transform.forward;
        Vector3 otherPortalPoint = otherPortal.transform.position;

        SetObliqueClippingPlane(_portalCamera, _mainCamera.aspect, _mainCamera.fieldOfView, otherPortalForward, otherPortalPoint, GetRenderWindow());
        //SetScissorRect(_portalCamera, GetRenderWindow());

        // Camera viewport to render only the necessary part of the screen.
        // _portalCamera.rect = new Rect(0, 0, 1, 1);
        // _portalCamera.rect = GetRenderWindow();

        if (_portalCamera.nearClipPlane <= 0)
            return;

        _portalCamera.Render();

        //Rect scissor = (int)Time.time % 2 == 0 ? new Rect(0, 0, 0.5f, 1) : new Rect(0, 0, 1, 1);
        //SetScissorRect(_mainCamera, scissor);
    }

    void SetObliqueClippingPlane(
        Camera camera,
        float aspect,
        float fieldOfView,
        Vector3 nearClippingPlaneNormal,
        Vector3 nearClippingPlanePosition,
        Rect rect)
    {
        float normal = Mathf.Sign(Vector3.Dot(nearClippingPlaneNormal, nearClippingPlanePosition - camera.transform.position));  // 1 or -1

        Vector3 cameraSpacePoint = camera.worldToCameraMatrix.MultiplyPoint(nearClippingPlanePosition);
        Vector3 cameraSpaceNormal = camera.worldToCameraMatrix.MultiplyVector(nearClippingPlaneNormal).normalized * normal;
        float camSpaceDistance = -Vector3.Dot(cameraSpacePoint, cameraSpaceNormal);
        Vector4 clipPlaneV4 = new(cameraSpaceNormal.x, cameraSpaceNormal.y, cameraSpaceNormal.z, camSpaceDistance);

        rect.Clamp01();
        Matrix4x4 m2 = Matrix4x4.TRS(new(1 / rect.width - 1, 1 / rect.height - 1, 0), Quaternion.identity, new(1 / rect.width, 1 / rect.height, 1));
        Matrix4x4 m3 = Matrix4x4.TRS(new(-rect.x * 2 / rect.width, -rect.y * 2 / rect.height, 0), Quaternion.identity, Vector3.one);

        Matrix4x4 obliqueMatrix = camera.CalculateObliqueMatrix(clipPlaneV4);
        camera.projectionMatrix = m3 * m2 * obliqueMatrix;
        camera.aspect = aspect;
        camera.fieldOfView = fieldOfView;
        camera.rect = rect;
    }

    Rect GetRenderWindow()
    {
        GetCornerPointsUV(out Vector2 topRightUV, out Vector2 topLeftUV, out Vector2 bottomRightUV, out Vector2 bottomLeftUV);
        float minX = Mathf.Min(topRightUV.x, topLeftUV.x, bottomRightUV.x, bottomLeftUV.x);
        float maxX = Mathf.Max(topRightUV.x, topLeftUV.x, bottomRightUV.x, bottomLeftUV.x);
        float maxY = Mathf.Max(topRightUV.y, topLeftUV.y, bottomRightUV.y, bottomLeftUV.y);
        float minY = Mathf.Min(topRightUV.y, topLeftUV.y, bottomRightUV.y, bottomLeftUV.y);
        return new Rect(minX, minY, maxX - minX, maxY - minY).GetClamped01();
    }

    bool IsInsideCamera(Camera camera, Bounds bounds)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    // Rotate m by 180 degrees around it's local y-axis
    static readonly Matrix4x4 rotate180 = Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0));

    // (Matrix multiplication is NOT commutative)
    Matrix4x4 WorldToPortalWorld() =>
        otherPortal.transform.localToWorldMatrix * rotate180 * transform.worldToLocalMatrix;

    void OnDrawGizmos()
    {
        if (otherPortal == null)
            return;

        Setup();
        Gizmos.color = portalColor;
        Matrix4x4 worldToPortal = WorldToPortalWorld();
        Vector3 cameraPoint = worldToPortal.MultiplyPoint(_mainCamera.transform.position);
        Vector3 cameraForward = worldToPortal.rotation * _mainCamera.transform.forward;

        Transform mainCam = _mainCamera.transform;
        Gizmos.DrawSphere(cameraPoint, 0.25f);
        float d = Vector3.Distance(cameraPoint, otherPortal.transform.position);
        Gizmos.DrawLine(cameraPoint, cameraPoint + cameraForward * d);
        Gizmos.DrawLine(mainCam.position, mainCam.position + mainCam.forward * d);

        GetCornerPoints(out Vector3 topRight, out Vector3 topLeft, out Vector3 bottomRight, out Vector3 bottomLeft);
        GetCornerPointsUV(out Vector2 topRightUV, out Vector2 topLeftUV, out Vector2 bottomRightUV, out Vector2 bottomLeftUV);

        Gizmos.DrawSphere(worldToPortal.MultiplyPoint(topRight), 0.1f);
        Gizmos.DrawSphere(worldToPortal.MultiplyPoint(topLeft), 0.1f);
        Gizmos.DrawSphere(worldToPortal.MultiplyPoint(bottomRight), 0.1f);
        Gizmos.DrawSphere(worldToPortal.MultiplyPoint(bottomLeft), 0.1f);

        Rect r = GetRenderWindow();
        float z = _mainCamera.nearClipPlane + 0.25f;
        Vector3 windowBottomLeft = _mainCamera.ViewportToWorldPoint(new(r.xMin, r.yMin, z));
        Vector3 windowTopLeft = _mainCamera.ViewportToWorldPoint(new(r.xMin, r.yMax, z));
        Vector3 windowTopRight = _mainCamera.ViewportToWorldPoint(new(r.xMax, r.yMax, z));
        Vector3 windowBottomRight = _mainCamera.ViewportToWorldPoint(new(r.xMax, r.yMin, z));
        Gizmos.DrawLine(windowBottomLeft, windowTopLeft);
        Gizmos.DrawLine(windowTopLeft, windowTopRight);
        Gizmos.DrawLine(windowTopRight, windowBottomRight);
        Gizmos.DrawLine(windowBottomRight, windowBottomLeft);

        GUIStyle style = new();
        style.normal.textColor = portalColor;
        Handles.Label(topRight, $"{topRightUV}", style);
        Handles.Label(topLeft, $"{topLeftUV}", style);
        Handles.Label(bottomRight, $"{bottomRightUV}", style);
        Handles.Label(bottomLeft, $"{bottomLeftUV}", style);

    }

    void GetCornerPoints(out Vector3 topRight, out Vector3 topLeft, out Vector3 bottomRight, out Vector3 bottomLeft)
    {
        Vector3 center = transform.position;

        Vector3 right = transform.right * portalSize.x / 2;
        Vector3 up = transform.up * portalSize.y / 2;

        topRight = center + right + up;
        topLeft = center - right + up;
        bottomRight = center + right - up;
        bottomLeft = center - right - up;
    }

    void GetCornerPointsUV(out Vector2 topRightUV, out Vector2 topLeftUV, out Vector2 bottomRightUV, out Vector2 bottomLeftUV)
    {
        GetCornerPoints(out Vector3 topRight, out Vector3 topLeft, out Vector3 bottomRight, out Vector3 bottomLeft);
        topRightUV = WToV(topRight);
        topLeftUV = WToV(topLeft);
        bottomRightUV = WToV(bottomRight);
        bottomLeftUV = WToV(bottomLeft);

        Vector2 WToV(Vector3 p)
        {
            float dir = Mathf.Sign(Vector3.Dot(p - _mainCamera.transform.position, _mainCamera.transform.forward));
            return _mainCamera.WorldToViewportPoint(p) * dir;
        }
    }
}
