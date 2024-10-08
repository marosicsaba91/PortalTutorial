using System;
using UnityEditor;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal otherPortal;
    public Vector2 portalSize = new(1, 2);
    public Color portalColor = Color.blue;
    public RenderTexture renderTexture;

    Camera portalCamera;
    Camera mainCamera;
    //Mesh mesh;

    void Start()
    {
        Setup();
    }

    void Setup()
    {
        if (portalCamera != null)
            return;

        portalCamera = GetComponentInChildren<Camera>();
        mainCamera = Camera.main;
        //MeshFilter meshFilter = GetComponent<MeshFilter>();
        //mesh = MeshUtility.CreateRectMesh(portalSize);
        //meshFilter.sharedMesh = mesh;
    }

    void Update()
    {
        renderTexture.Release();

        Matrix4x4 worldToPortal = WorldToPortalWorld();
        Vector3 cameraPoint = worldToPortal.MultiplyPoint(mainCamera.transform.position);
        Quaternion cameraRotation = worldToPortal.rotation * mainCamera.transform.rotation;

        portalCamera.transform.SetPositionAndRotation(cameraPoint, cameraRotation);

        portalCamera.aspect = mainCamera.aspect;
        portalCamera.fieldOfView = mainCamera.fieldOfView;
        portalCamera.nearClipPlane = Vector3.Distance(cameraPoint, otherPortal.transform.position);
        //portalCamera.nearClipPlane = mainCamera.nearClipPlane;

        GetCornerPointsUV(out Vector2 topRightUV, out Vector2 topLeftUV, out Vector2 bottomRightUV, out Vector2 bottomLeftUV);

        // Remap UVs to viewport space
        //mesh.SetRectUV(topRightUV, topLeftUV, bottomRightUV, bottomLeftUV);
        //mesh.SetRectUV(new (1,1), new (0, 1), new (1, 0), new (0, 0));

        portalCamera.rect = new Rect(0, 0, 1, 1);
    }

    Matrix4x4 WorldToPortalWorld()
    {
        Transform mainCam = mainCamera.transform;
        Matrix4x4 rotate180 = Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0)); // (Rotate m by 180 degrees around it's local y-axis)
        return  otherPortal.transform.localToWorldMatrix * rotate180 * transform.worldToLocalMatrix;
        // Matrix multiplication is NOT commutative
    }

    void OnDrawGizmos()
    {
        Setup();
        Gizmos.color = portalColor;
        Matrix4x4 worldToPortal = WorldToPortalWorld();
        Vector3 cameraPoint = worldToPortal.MultiplyPoint(mainCamera.transform.position);
        Vector3 cameraForward = worldToPortal.rotation * mainCamera.transform.forward;

        Transform mainCam = mainCamera.transform;
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

        float near = mainCamera.nearClipPlane;
        Vector3 cameraTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1,1, near));
        Vector3 cameraTopLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, near));
        Vector3 cameraBottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, near));
        Vector3 cameraBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, near));
        Gizmos.DrawWireSphere(worldToPortal.MultiplyPoint(cameraTopRight), 0.1f);
        Gizmos.DrawWireSphere(worldToPortal.MultiplyPoint(cameraTopLeft), 0.1f);
        Gizmos.DrawWireSphere(worldToPortal.MultiplyPoint(cameraBottomRight), 0.1f);
        Gizmos.DrawWireSphere(worldToPortal.MultiplyPoint(cameraBottomLeft), 0.1f);



        GUIStyle style = new GUIStyle();
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
        topRightUV = mainCamera.WorldToViewportPoint(topRight);
        topLeftUV = mainCamera.WorldToViewportPoint(topLeft);
        bottomRightUV = mainCamera.WorldToViewportPoint(bottomRight);
        bottomLeftUV = mainCamera.WorldToViewportPoint(bottomLeft);
    }
}
