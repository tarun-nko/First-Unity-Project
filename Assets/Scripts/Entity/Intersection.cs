using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection {
    public Intersection(float x, float y) {
        point = new Vector2(x, y);
    }
    // the intersection point
    public Vector2 point;
    // the references of the existing path with which the new path has intersected 
    public Path path;
    // the location of the start point of the intersecting line segments in their corresponding paths
    public int oldPathI, newPathI;
    public override string ToString() {
        return "Point : " + point.x + " : " + point.y + ", intersecting Path length : " + path.path.Count + ", Ref Old Path I : " + oldPathI + ", Ref New Path I : " + newPathI;
    }
}

