using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class HelperFunctions {

    public static bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
        if (IsTriangleOrientedClockwise(A, B, C)) {
            Vector2 temp = C;
            C = B;
            B = temp;
        }

        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;
 
        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;
 
        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;
 
        return (((aCROSSbp >= 0.0f)) && 
                ((bCROSScp >= 0.0f)) && 
                ((cCROSSap >= 0.0f)));
    }

    public static bool IsVertexConvex (Vector2 vP, Vector2 v, Vector2 vN, bool isPathClockwise) {
        return isPathClockwise == IsTriangleOrientedClockwise(vP, v, vN);                
    }

    public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3) {
        bool isClockWise = true;

        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

        if (determinant > 0f) {
            isClockWise = false;
        }

        return isClockWise;
    }


    public static bool IsOrientedClockwise(Vector2[] vertices) {
        int clockWiseCount = 0;
        int counterClockWiseCount = 0;
        Vector2 p1 = vertices[0];

        for (int i = 1; i < vertices.Length; i++)
        {
            Vector2 p2 = vertices[i];
            Vector2 p3 = vertices[(i + 1) % vertices.Length];

            Vector2 e1 = p1 - p2;
            Vector2 e2 = p3 - p2;

            if (e1.x * e2.y - e1.y * e2.x >= 0)
                clockWiseCount++;
            else
                counterClockWiseCount++;

            p1 = p2;
        }

        return (clockWiseCount > counterClockWiseCount)
            ? true
            : false;
    }

    public static Intersection AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2)
    {
        //To avoid floating point precision issues we can add a small value
        float epsilon = 0.00001f;

        Intersection intersectionPoint = null;
        float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

        //Make sure the denominator is > 0, if not the lines are parallel
        if (!Mathf.Approximately(denominator, 0.0f))
        {
            float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
            float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
            //Debug.Log("I am here : " + l1_p1.ToString() + ", " + l1_p2.ToString() + ", " + l2_p1.ToString() + ", " + l2_p2.ToString());
            //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
            bool aEquals0 = Mathf.Approximately(u_a, 0.0f);
            bool aEquals1 = Mathf.Approximately(u_a, 1.0f);
            bool bEquals0 = Mathf.Approximately(u_b, 0.0f);
            bool bEquals1 = Mathf.Approximately(u_b, 1.0f);
            bool aBetween01 = u_a > 0.0f && u_a < 1.0f;
            bool bBetween01 = u_b > 0.0f && u_b < 1.0f;
            if ((aEquals0 || aEquals1 || aBetween01) && (bEquals0 || bEquals1 || bBetween01)) {
                intersectionPoint = new Intersection(l1_p1.x + u_a * (l1_p2.x - l1_p1.x), l1_p1.y + u_a * (l1_p2.y - l1_p1.y));
            }
            //  if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
            // {
            //     intersectionPoint = new Intersection(l1_p1.x + u_a * (l1_p2.x - l1_p1.x), l1_p1.y + u_a * (l1_p2.y - l1_p1.y));
            // }
        }

        return intersectionPoint;
    }

    public static bool ArePointsOnTheSameSideOfLine(Vector2 pt1, Vector2 pt2, Vector2 linePt1, Vector2 linePt2) {
        // perform cross product between AB and AC, do the same for AB and AD, if they are facing the same
        // direction then they are on same side, otherwise opposite direction
        Vector3 AB = linePt1 - linePt2;
        Vector3 AC = linePt1 - pt1;
        Vector3 AD = linePt1 - pt2;
        return Vector3.Dot(Vector3.Normalize(Vector3.Cross(AB, AC)), Vector3.Normalize(Vector3.Cross(AB, AD))) >= 0;
        
    }

    public static void InitializeAABBForPath (Path path) {
        foreach(Vector2 point in path.path){
            path.xMin = Math.Min(path.xMin, point.x);
            path.yMin = Math.Min(path.yMin, point.y);
            path.xMax = Math.Max(path.xMax, point.x);
            path.yMax = Math.Max(path.yMax, point.y);
        }
    }

    // public static bool IsInsidePolygon (Vector3 point) {
    //     int count = 0;
    //     Vector2 point2D = new Vector2(point.x, point.z);
    //     // we have a ray starting from point2D and extending towards positive x axis parallely
    //     // we want to check how many times this ray intersects with other paths
    //     // if its odd, then we are inside the polygon, otherwise we are outside  
    //     foreach (Path path in paths) {
    //         // does the ray intersect with bounding box of the path
    //         if (path.xMax > point2D.x && path.yMin < point2D.y && path.yMax > point2D.y) {
    //             // check for intersection with all the edges of the path
    //             bool flag = false;
    //             for (int i = 1; i < path.path.Count; i++){
    //                 // if (i == path.path.Count - 1 && skipEnd) {
    //                 //     continue;
    //                 // }
    //                 Vector2 point1 = path.path[i - 1];
    //                 Vector2 point2 = path.path[i];
    //                 if (point1.x > point2D.x || point2.x > point2D.x) {
                        
    //                     if (Mathf.Approximately(point1.y, point2D.y) || Mathf.Approximately(point2.y, point2D.y)) {
    //                         if (!flag) {
    //                             count++;
    //                             //Debug.Log("Alligned Original ->" + point2D.x +"," +point2D.y + " Collisions : " + point1.x+","+point1.y + " & " + point2.x + "," + point2.y);

    //                         }
    //                         flag = true;
    //                     }
    //                     else if ((point1.y < point2D.y && point2.y > point2D.y) || (point1.y > point2D.y && point2.y < point2D.y)) 
    //                     {
    //                         count++;
    //                         //Debug.Log("Original ->" + point2D.x +"," +point2D.y + " Collisions : " + point1.x+","+point1.y + " & " + point2.x + "," + point2.y);
    //                     }

    //                 }
    //             }
    //         }
    //     }
    //     return count % 2 == 1;
    // }

    // draw a line segment from the bounding box minimum point to the point to be checked
    // if the number of intersections is even, it means point is outside, else its inside
    public static bool IsInsidePolygon(Path path, Vector2 p1) {
        //Vector2 p2 = new Vector2(Mathf.Approximately(path.xMin, p1.x) ? path.xMax : path.xMin, Mathf.Approximately(p1.y, path.yMin) ? path.yMax : path.yMin);
        Vector2 p2 = new Vector2(path.xMin, path.yMin);
        int count = 0;
        for (int i = 1; i < path.path.Count; i++) {
            Vector2 p3 = path.path[i - 1];
            Vector2 p4 = path.path[i];
            count += AreLinesIntersecting(p1, p2, p3, p4) != null ? 1 : 0;
        }
        return count % 2 == 1;
    }

    public static bool CheckForSelfIntersection (Vector2 start, Vector2 end, Path path) {
        for (int i = 1; i < path.path.Count - 1; i++) {
            Vector2 point1 = path.path[i - 1];
            Vector2 point2 = path.path[i];
            if (HelperFunctions.AreLinesIntersecting(point1, point2, start, end) != null)
                return true; 
        }
        return false;
    }

    // adds depth to a planar polygon, and returns the indices of the mesh
    public static List<int> Make3D (List<Vector3> vertices, float depth) {
        int n = vertices.Count;
        // add the vertices with depth
        for (int i = 0; i < n; i++) {
            Vector3 v = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z);
            v.y -= depth;
            vertices.Add(v);
        }
        // join them by creating indices
        List<int> indices = new List<int>();
        int MOD = vertices.Count;
        for (int i = 0; i < n; i++){
            indices.Add(i);
            indices.Add((i + n + 1) % MOD);
            indices.Add((i + n) % MOD);

            indices.Add(i);
            indices.Add((i + 1) % MOD);
            indices.Add((i + n + 1) % MOD);
        }
        
        return indices;
    }


}
