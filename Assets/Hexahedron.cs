using UnityEngine;

class Hexahedron
{
    public Vector3[] corners = new Vector3[8];

    public Vector3 LeftBottomBack => corners[0];
    public Vector3 RightBottomBack => corners[1];
    public Vector3 RightTopBack => corners[2];
    public Vector3 LeftTopBack => corners[3];
    public Vector3 LeftBottomFront => corners[4];
    public Vector3 RightBottomFront => corners[5];
    public Vector3 RightTopFront => corners[6];
    public Vector3 LeftTopFront => corners[7];

    public Hexahedron(Vector3 center, Vector3 size)
    {
        Vector3 extent = size * 0.5f;
        corners[0] = center + new Vector3(-extent.x, -extent.y, -extent.z);
        corners[1] = center + new Vector3(extent.x, -extent.y, -extent.z);
        corners[2] = center + new Vector3(extent.x, extent.y, -extent.z);
        corners[3] = center + new Vector3(-extent.x, extent.y, -extent.z);
        corners[4] = center + new Vector3(-extent.x, -extent.y, extent.z);
        corners[5] = center + new Vector3(extent.x, -extent.y, extent.z);
        corners[6] = center + new Vector3(extent.x, extent.y, extent.z);
        corners[7] = center + new Vector3(-extent.x, extent.y, extent.z);
    }

    Hexahedron(Vector3[] points) => corners = points;

    public static Hexahedron CreateFrustum(
    Vector3 center,
    Quaternion rotation,
    float fieldOfView,
    float farClipPlane,
    float nearClipPlane,
    float aspect)
    {
        float halfFOV = fieldOfView * 0.5f;
        float halfHeightNear = Mathf.Tan(halfFOV * Mathf.Deg2Rad) * nearClipPlane;
        float halfWidthNear = halfHeightNear * aspect;
        float halfHeightFar = Mathf.Tan(halfFOV * Mathf.Deg2Rad) * farClipPlane;
        float halfWidthFar = halfHeightFar * aspect;
        Vector3 forward = rotation * Vector3.forward;
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;
        Vector3[] points = 
        {
            center - right * halfWidthNear - up * halfHeightNear + forward * nearClipPlane,
            center + right * halfWidthNear - up * halfHeightNear + forward * nearClipPlane,
            center + right * halfWidthNear + up * halfHeightNear + forward * nearClipPlane,
            center - right * halfWidthNear + up * halfHeightNear + forward * nearClipPlane,
            center - right * halfWidthFar - up * halfHeightFar + forward * farClipPlane,
            center + right * halfWidthFar - up * halfHeightFar + forward * farClipPlane,
            center + right * halfWidthFar + up * halfHeightFar + forward * farClipPlane,
            center - right * halfWidthFar + up * halfHeightFar + forward * farClipPlane
        };
        return new Hexahedron(points);
    }

    public static Hexahedron GetFrustum(Camera camera)
    {
        Hexahedron hex = new(Vector3.zero, Vector3.one * 2);
        hex.Apply_WithPerspectiveDivide(camera.projectionMatrix.inverse);
        hex.ApplyFast_NoPerspectiveDivide(camera.cameraToWorldMatrix);
        return hex;
    }

    public void DrawGizmo()
    {
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);
        Gizmos.DrawLine(corners[4], corners[5]);
        Gizmos.DrawLine(corners[5], corners[6]);
        Gizmos.DrawLine(corners[6], corners[7]);
        Gizmos.DrawLine(corners[7], corners[4]);
        Gizmos.DrawLine(corners[0], corners[4]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[3], corners[7]);
    }

    public void ApplyFast_NoPerspectiveDivide(Matrix4x4 matrix)
    {
        for (int i = 0; i < corners.Length; i++)
            corners[i] = matrix.MultiplyPoint3x4(corners[i]);
    }

    public void Apply_WithPerspectiveDivide(Matrix4x4 matrix)
    {
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 p = corners[i];
            Vector4 v = matrix * new Vector4(p.x, p.y, p.z, 1);

            v /= v.w;   // Perspective divide

            corners[i] = v; 
        }
    }
}