using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path {
    public Path () {
        xMin = float.MaxValue;
        yMin = float.MaxValue;
        xMax = float.MinValue;
        yMax = float.MinValue;
        path = new List<Vector2>();
        relativePath = new List<Vector2>();
        indexToIntersectionMap = new Dictionary<int, List<Intersection>>();
    }
    // Bounding Box of the path
    public float xMin, yMin;
    public float xMax, yMax;
    // the actual path
    public List<Vector2> path;
    // the relative path for ground 2D, i.e. we need to translate the current coordinates to match with ground2D
    public List<Vector2> relativePath; 
    public float constantY;
    public Dictionary<int, List<Intersection>> indexToIntersectionMap;
    public GameObject gameObject;
}
