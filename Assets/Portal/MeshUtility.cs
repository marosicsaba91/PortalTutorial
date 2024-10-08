
using System;
using UnityEngine;

public static class MeshUtility
{
    public static Mesh Copy(this Mesh mesh)
    {
        Mesh copy = new()
        {
            vertices = Copy(mesh.vertices),
            triangles = Copy(mesh.triangles),
            normals = Copy(mesh.normals),
            uv = Copy(mesh.uv)
        };

        static T[] Copy<T>(T[] original)
        {
            T[] copy = new T[original.Length];
            Array.Copy(original, copy, original.Length);
            return copy;
        }

        return copy;
    }
    public static Mesh CreateRectMesh(Vector2 size)
    {
        size /= 2;
        Mesh mesh = new();
        mesh.vertices = new Vector3[]
        {
            new (-size.x, -size.y, 0),
            new (size.x, -size.y, 0),
            new (size.x, size.y, 0),
            new (-size.x, size.y, 0)
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        mesh.uv = new Vector2[]
        {
            new (0, 0),
            new (1, 0),
            new (1, 1),
            new (0, 1)
        };
        mesh.RecalculateBounds();
        return mesh;
    }

    public static void SetRectUV(this Mesh mesh, Vector2 topRightUV, Vector2 topLeftUV, Vector2 bottomRightUV, Vector2 bottomLeftUV)
    {
        mesh.SetUVs(0, new Vector2[] { bottomLeftUV, bottomRightUV, topRightUV, topLeftUV });
    }


}