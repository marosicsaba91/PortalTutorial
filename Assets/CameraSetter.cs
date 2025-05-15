using System.Collections.Generic;
using UnityEngine;

public class CameraSetter : MonoBehaviour
{
    [SerializeField] Camera _camera;

    [SerializeField] Vector3 center;
    [SerializeField] Vector3 size;

    void OnValidate()
    {
        if (_camera == null)
            _camera = GetComponent<Camera>();
    }

    void OnDrawGizmos()
    {
        Hexahedron cube = new (center, size);
        Hexahedron frustum = Hexahedron.CreateFrustum(_camera.transform.position, _camera.transform.rotation, _camera.fieldOfView, _camera.farClipPlane, _camera.nearClipPlane, _camera.aspect);

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.black;
        cube.DrawGizmo();
        frustum.DrawGizmo();


        Gizmos.color = Color.red;

        // Matrix4x4 matrix = _camera.cameraToWorldMatrix;
        Matrix4x4 matrix = _camera.projectionMatrix * _camera.worldToCameraMatrix;

        cube.Apply_WithPerspectiveDivide(matrix);
        frustum.Apply_WithPerspectiveDivide(matrix);
        cube.DrawGizmo();
        frustum.DrawGizmo();


        Gizmos.color = Color.green;
        Hexahedron full = Hexahedron.GetFrustum(_camera);
        full.DrawGizmo();

        /*
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.DrawFrustum(Vector3.zero, _camera.fieldOfView, _camera.farClipPlane, _camera.nearClipPlane, _camera.aspect);

        Matrix4x4 matrix = _camera.projectionMatrix * _camera.worldToCameraMatrix;
        Gizmos.matrix = matrix;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.DrawFrustum(Vector3.zero, _camera.fieldOfView, _camera.farClipPlane, _camera.nearClipPlane, _camera.aspect);
        */
    }
}
