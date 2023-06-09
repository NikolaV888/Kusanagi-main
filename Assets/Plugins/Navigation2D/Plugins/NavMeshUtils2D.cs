﻿using UnityEngine;

public class NavMeshUtils2D
{
    // project 3D world point to 2D
    // we always drop the y component so this one only needs one version for
    // both points and objects!
    public static Vector2 ProjectTo2D(Vector3 v) =>
        new Vector2(v.x, v.z);

    // project 2D world point to 3D world navmesh
    // => baked navmesh is always at y=0
    public static Vector3 ProjectPointTo3D(Vector2 v) =>
        new Vector3(v.x, 0, v.y);

    // project 2D world object to 3D world navmesh
    // object's feet need to be at baked navmesh y=0, so object needs to be
    // at y=0.5 since we always create it with height = 1!
    public static Vector3 ProjectObjectTo3D(Vector2 v) =>
        new Vector3(v.x, NavMesh2D.ProjectedObjectY, v.y);

    // project 2D rotation to 3D
    public static Vector3 RotationTo3D(Vector3 v) =>
        new Vector3(0, -v.z, 0);

    // project 2D scale to 3D
    public static Vector3 ScaleTo3D(Vector3 v) =>
        new Vector3(v.x, 1, v.y);

    public static Vector2[] AdjustMinMax(Collider2D co, Vector2 min, Vector2 max)
    {
        min.x = Mathf.Min(co.bounds.min.x, min.x);
        min.y = Mathf.Min(co.bounds.min.y, min.y);
        max.x = Mathf.Max(co.bounds.max.x, max.x);
        max.y = Mathf.Max(co.bounds.max.y, max.y);
        return new Vector2[]{min, max};
    }

    public static Vector3 ScaleFromBoxCollider2D(BoxCollider2D co) =>
        // transform.localScale * collider size (but with components swapped for 3d)
        Vector3.Scale(ScaleTo3D(co.transform.localScale), new Vector3(co.size.x, 1, co.size.y));

    public static Vector3 ScaleFromCircleCollider2D(CircleCollider2D co) =>
        // transform.localScale * collider size (but with components swapped for 3d)
        // radius * 2 because diameter := radius * 2
        Vector3.Scale(ScaleTo3D(co.transform.localScale), new Vector3(co.radius*2, 1, co.radius*2));

    public static Vector3 ScaleFromPolygonCollider2D(PolygonCollider2D co) =>
        // transform.localScale * collider size (but with components swapped for 3d)
        ScaleTo3D(co.transform.localScale);

    public static Vector3 ScaleFromEdgeCollider2D(EdgeCollider2D co) =>
        // transform.localScale * collider size (but with components swapped for 3d)
        ScaleTo3D(co.transform.localScale);
}
