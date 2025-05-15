using UnityEngine;

[ExecuteAlways]
public class CameraCutter : MonoBehaviour
{
    [SerializeField] Camera target;
    [SerializeField] RenderTexture renderTexture;

    [SerializeField] Transform window;

    void Update()
    {
        Camera camera = GetComponent<Camera>();
        ClipCamera(camera, target, window);

        // Clear RenderTexture
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Camera camera = GetComponent<Camera>();
        Hexahedron.GetFrustum(camera).DrawGizmo();

        Gizmos.color = Color.magenta;
        Gizmos.matrix = window.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new(1, 1, 0));
        Gizmos.DrawLine(Vector3.zero, Vector3.forward);

        Gizmos.color = Color.yellow;
        Rect windowRect = WindowOnScreen(target, window);

        // Draw Window Rect on Screen
        Gizmos.matrix = Matrix4x4.identity;

        Vector3 windowRT = new(windowRect.xMax, windowRect.yMax, target.nearClipPlane);
        Vector3 windowLT = new(windowRect.xMin, windowRect.yMax, target.nearClipPlane);
        Vector3 windowRB = new(windowRect.xMax, windowRect.yMin, target.nearClipPlane);
        Vector3 windowLB = new(windowRect.xMin, windowRect.yMin, target.nearClipPlane);

        Vector3 screenWindowRT = target.ViewportToWorldPoint(windowRT);
        Vector3 screenWindowLT = target.ViewportToWorldPoint(windowLT);
        Vector3 screenWindowRB = target.ViewportToWorldPoint(windowRB);
        Vector3 screenWindowLB = target.ViewportToWorldPoint(windowLB);

        Gizmos.DrawLine(screenWindowRT, screenWindowLT);
        Gizmos.DrawLine(screenWindowLT, screenWindowLB);
        Gizmos.DrawLine(screenWindowLB, screenWindowRB);
        Gizmos.DrawLine(screenWindowRB, screenWindowRT);
    }

    public void ClipCamera(Camera camera, Camera target, Transform window)
    {
        target.transform.GetPositionAndRotation(out Vector3 camPoint, out Quaternion camRotation);

        Matrix4x4 projectionMatrix = GetWindowProjection
            (camPoint, window.position, window.rotation, window.localScale, target.farClipPlane);

        camera.transform.SetPositionAndRotation(target.transform.position, window.rotation);
        camera.projectionMatrix = projectionMatrix;
              
        Rect rect = WindowOnScreen(target, window);
        camera.rect = rect;
    }

    static Rect WindowOnScreen(Camera camera, Transform window)
    {
        Vector2 windowSize = window.localScale;
        Vector3 center = window.position;
        Vector3 right = window.right * windowSize.x / 2;
        Vector3 up = window.up * windowSize.y / 2;

        Vector3 windowRT = center + right + up;
        Vector3 windowLT = center - right + up;
        Vector3 windowRB = center + right - up;
        Vector3 windowLB = center - right - up;
        //Debug.Log($"W: {windowRT}  {windowLT}  {windowRB}  {windowLB}");

        Vector2 screenWindowRT = camera.WorldToViewportPoint(windowRT);
        Vector2 screenWindowLT = camera.WorldToViewportPoint(windowLT);
        Vector2 screenWindowRB = camera.WorldToViewportPoint(windowRB);
        Vector2 screenWindowLB = camera.WorldToViewportPoint(windowLB);
        //Debug.Log($"S: {screenWindowRT}  {screenWindowLT}  {screenWindowRB}  {screenWindowLB}");

        float minX = Mathf.Min(screenWindowRT.x, screenWindowLT.x, screenWindowRB.x, screenWindowLB.x);
        float minY = Mathf.Min(screenWindowRT.y, screenWindowLT.y, screenWindowRB.y, screenWindowLB.y);
        float maxX = Mathf.Max(screenWindowRT.x, screenWindowLT.x, screenWindowRB.x, screenWindowLB.x);
        float maxY = Mathf.Max(screenWindowRT.y, screenWindowLT.y, screenWindowRB.y, screenWindowLB.y);

        Rect rect = new(minX, minY, maxX - minX, maxY - minY);
        return rect;
    }

    static Matrix4x4 GetWindowProjection(
        Vector3 cameraPosition,
        Vector3 windowPosition,
        Quaternion windowRotation,
        Vector2 windowSize,
        float farPlaneDistance)
    {
        Matrix4x4 worldToWindow = Matrix4x4.TRS(
            windowPosition,
            windowRotation,
            Vector3.one).inverse;


        // Transformed Camera Position
        Vector3 tcp = worldToWindow.MultiplyPoint(cameraPosition);
        float near = -tcp.z;

        float right = windowSize.x / 2 - tcp.x;
        float left = -windowSize.x / 2 - tcp.x;
        float top = windowSize.y / 2 - tcp.y;
        float bottom = -windowSize.y / 2 - tcp.y;

        Matrix4x4 projectionMatrix = Matrix4x4.Frustum(
            left,
            right,
            bottom,
            top,
            near,
            farPlaneDistance);
        return projectionMatrix;
    }


    Matrix4x4 Frustum( float left, float right, float bottom, float top, float near, float far)
    {
        // Equivalent to Unity's Matrix4x4.Frustum

        // w = width, h = height, d = depth
        // n = near, f = far
        // t = top, b = bottom
        // r = right, l = left

        // 2n/w     0       (r+l)/w      0
        // 0        2n/h    (t+b)/h      0
        // 0        0      -(f+n)/d   -2fn/d
        // 0        0         -1         0

        float w = right - left;
        float h = top - bottom;
        float d = far - near;  

        float x = 2.0f * near / w;
        float y = 2.0f * near / h;

        float _a = (right + left) / w;
        float _b = (top + bottom) / h;
        float _c = -(far + near) / d;
        float _e = -2.0f * far * near / d;

        // x   0   a   0
        // 0   y   b   0
        // 0   0   c   e
        // 0   0   -1  0

        Matrix4x4 m = new();
        m[0, 0] = x;
        m[0, 2] = _a;
        m[1, 1] = y;
        m[1, 2] = _b;
        m[2, 2] = _c;
        m[2, 3] = _e;
        m[3, 2] = -1;
        return m;
    }
}