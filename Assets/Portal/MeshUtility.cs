
using System;
using UnityEngine;

public static class MeshUtility
{
    public static void Clamp01(this ref Rect rect) => rect = GetClamped01(rect);

    public static Rect GetClamped01(this Rect rect)
    {
        float x = Mathf.Clamp01(rect.x);
        float y = Mathf.Clamp01(rect.y);
        float xMax = Mathf.Clamp01(rect.xMax);
        float yMax = Mathf.Clamp01(rect.yMax);
        float width = xMax - x;
        float height = yMax - y;
        return new Rect(x, y, width, height);
    }

}