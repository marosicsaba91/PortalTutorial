using UnityEngine;

[ExecuteAlways]
public class CameraTester :MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float horizontalAngle = 90;
    [SerializeField] float verticalAngle = 90;
    [SerializeField] float near = 0.1f;
    [SerializeField] float far = 100f;
    [SerializeField] bool useCustomProjection = false;

    void Update()
    { 
        Debug.Log("--------------");
        Debug.Log(cam.cameraToWorldMatrix);
        Debug.Log(cam.worldToCameraMatrix);
        Debug.Log(cam.nonJitteredProjectionMatrix);
        Debug.Log(cam.projectionMatrix); 
        if (useCustomProjection)
        {
            cam.projectionMatrix =
                GetProjectionMatrix(horizontalAngle * Mathf.Deg2Rad, verticalAngle * Mathf.Deg2Rad, near, far);
        }
    }

    Matrix4x4 GetProjectionMatrix(float horizontalAngle, float aspect, float near, float far ) 
    { 
        float xScale = 1.0f / Mathf.Tan(horizontalAngle * 0.5f);
        float yScale = xScale * aspect;
        float q = far / (far - near);
        Matrix4x4 m = new();
        m[0, 0] = xScale;
        m[1, 1] = yScale;
        m[2, 2] = q;
        m[2, 3] = -q * near;
        m[3, 2] = 1;
        return m;
    }
}