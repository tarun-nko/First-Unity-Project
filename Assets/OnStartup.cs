using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class OnStartup : MonoBehaviour
{
    public PolygonCollider2D hole2D;
    public PolygonCollider2D ground2D;
    public MeshCollider meshCollider;
    public GameObject ground;
    Mesh mesh;
    Camera camera;
    Vector3 hitPos;
    Path currentPath; 
    List<Path> paths;
    Color color;
    Color currentPathColor;
    bool pathHasChanged;
    class Path {
        public Path () {
            xMin = float.MaxValue;
            yMin = float.MaxValue;
            xMax = float.MinValue;
            yMax = float.MinValue;
            path = new List<Vector2>();
            indexToIntersectionMap = new Dictionary<int, List<Intersection>>();
        }
        // Bounding Box of the path
        public float xMin, yMin;
        public float xMax, yMax;
        // the actual path
        public List<Vector2> path; 
        public float constantY;
        public Dictionary<int, List<Intersection>> indexToIntersectionMap;
    }

    class Intersection {
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

    private void Start() {
        camera = Camera.main;
        hitPos = -1 * Vector3.one;
        currentPath = new Path();
        paths = new List<Path>();
        color = new Color(0.0f, 0.0f, 1.0f);
        currentPathColor = new Color(1.0f, 0.0f, 0.0f);
        pathHasChanged = true;
    }

    // Start is called before the first frame update
    private void FixedUpdate()
    {
        // update the mesh collider if a new path has been added
        if (pathHasChanged) {
            pathHasChanged = false;
                
            ground2D.pathCount = paths.Count + 1;
            for (int i = 0; i < paths.Count; i++){
                ground2D.SetPath(i + 1, paths[i].path);
            }
            if (mesh != null) Destroy(mesh);
            mesh = ground2D.CreateMesh(true, true);
            meshCollider.sharedMesh = mesh;
        }

        // draw the currently being created path
        if (currentPath != null && currentPath.path.Count > 0) {
            for (int i = 1; i < currentPath.path.Count; i++) {
                Vector2 p1 = currentPath.path[i - 1];
                Vector2 p2 = currentPath.path[i];
                Debug.DrawLine(new Vector3(p1.x, currentPath.constantY, p1.y), new Vector3(p2.x, currentPath.constantY, p2.y), currentPathColor);
            }
        }

        // draw the existing paths
        foreach (Path path in paths) {
            for (int i = 1; i < path.path.Count; i++) {
                Vector3 p1 = new Vector3(path.path[i - 1].x, path.constantY, path.path[i - 1].y);
                Vector3 p2 = new Vector3(path.path[i].x, path.constantY, path.path[i].y);
                Debug.DrawLine(p1, p2, color);
            }
        } 
        
    }

    void OnMouseUp() {
        if (currentPath != null && currentPath.path.Count > 2){
            if (Vector2.Distance(currentPath.path[0], currentPath.path[currentPath.path.Count - 1]) <= 0.2f) {
                currentPath.path.Add(currentPath.path[0]);
                paths.Add(currentPath);
                Debug.Log("Added to paths, length : " + paths.Count + " : " + 
                            currentPath.xMin + ", " + currentPath.yMin + ", " + currentPath.xMax + ", " + currentPath.yMax);
                pathHasChanged = true;
                mergePaths();
            }
        }
        ResetCurrentPath();
    }

    void ResetCurrentPath () {
        foreach (Path path in paths){
            path.indexToIntersectionMap = new Dictionary<int, List<Intersection>>();
        }
        currentPath = new Path();
    }

    void OnMouseDrag(){
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int n = currentPath.path.Count;
        if (Physics.Raycast(ray, out hit)){
            hitPos = hit.point;
        }
        Vector2 currentPoint = new Vector2(hitPos.x, hitPos.z);
        // its the start point always record it
        if (n == 0) {
            currentPath.constantY = hitPos.y;
            currentPath.path.Add(currentPoint);
        }
        // do not record the point if the distance between the previous point and the current point is less than 0.1f
        // we are doing this to avoid unnecessary points in our path, which can add to unnecessary processing in our mesh collider 

        // == checks for approx equals in Vector2
        else if (!(currentPoint == currentPath.path[n - 1]) && Vector2.Distance(currentPoint, currentPath.path[n - 1]) > 0.15f) {
            // if there's any self intersection cancel the path
            if (CheckForSelfIntersection (currentPath.path[n - 1], currentPoint, currentPath)) {
                ResetCurrentPath();
                return;
            }

            currentPath.path.Add(currentPoint);

            // update the paths AABB
            currentPath.xMin = Math.Min(currentPath.xMin, currentPoint.x);
            currentPath.yMin = Math.Min(currentPath.yMin, currentPoint.y);
            currentPath.xMax = Math.Max(currentPath.xMax, currentPoint.x);
            currentPath.yMax = Math.Max(currentPath.yMax, currentPoint.y);

            int index = currentPath.path.Count - 2;

            //Debug.Log("Checking points for intersection " + currentPath.path[index].ToString() + ", " + currentPath.path[index + 1].ToString());
            List<Intersection> intersectionList = DoesLineIntersectWithAnyPath(currentPath.path[index], currentPath.path[index + 1]);
            if (intersectionList != null && intersectionList.Count > 0) {
                // case where the start or end vertex is the same as intersection point
                if (intersectionList.Count == 1) {
                    Intersection intersection = intersectionList[0];
                    bool overlapWithStart = intersection.point == currentPath.path[index];
                    bool overlapWithEnd = intersection.point == currentPath.path[index + 1];
                    if (overlapWithStart || overlapWithEnd) { 
                        intersection.newPathI = overlapWithEnd ? index + 1 : index;
                        
                        Debug.Log("Intersection at vertex of new line " + intersection.ToString() + " COUNT : " + currentPath.path.Count + " LINE SEG 1 : "
                                + currentPath.path[index].x + ", " + currentPath.path[index]. y + " LINE SEG 2 : " + currentPath.path[index + 1].x + ", " + currentPath.path[index + 1].y);

                        if (CheckIfPointAlreadyPresent (currentPath, intersection.newPathI, intersection.point)) {
                            Debug.Log("Not adding to the intersection map");
                        }
                        else {
                            currentPath.indexToIntersectionMap.Add(intersection.newPathI, intersectionList);
                            Debug.Log("Added to Intersection Map of new path : " + intersection.newPathI + " Size : " + currentPath.indexToIntersectionMap.Count);
                        }
                    }
                    else {
                        intersection.newPathI = index;
                        Debug.Log("Intersection of new line " + intersection.ToString() + " COUNT : " + currentPath.path.Count + " LINE SEG 1 : "
                     + currentPath.path[index].x + ", " + currentPath.path[index]. y + " LINE SEG 2 : " + currentPath.path[index + 1].x + ", " + currentPath.path[index + 1].y);
                        currentPath.indexToIntersectionMap.Add(index, intersectionList);
                    }
                    
                    return;
                }

                for (int i1 = 0; i1 < intersectionList.Count; i1++) {
                    intersectionList[i1].newPathI = index;
                }
                currentPath.indexToIntersectionMap.Add(index, intersectionList);
                int i = 0;
                foreach(Intersection in1 in intersectionList) {
                    Debug.Log("Intersection at point " + in1.ToString() + " COUNT : " + currentPath.path.Count + " INDEX : " + ++i + " LINE SEG 1 : "
                     + currentPath.path[index].x + ", " + currentPath.path[index]. y + " LINE SEG 2 : " + currentPath.path[index + 1].x + ", " + currentPath.path[index + 1].y);
                }
            }
        }
    }

    void mergePaths () {
        if (paths.Count > 1) {
            List<Path> mergedPathList = new List<Path>();
            Path newPath = paths[paths.Count - 1];
            Path curPath = newPath;
            Debug.Log("Merging : Intersection count : " + curPath.indexToIntersectionMap.Count);

            Path otherPath = null;
            if (curPath.indexToIntersectionMap.Count == 0)
                return;

            HashSet<Intersection> hasIntersectionBeenCovered = new HashSet<Intersection>();
            foreach (KeyValuePair<int, List<Intersection>> entry in newPath.indexToIntersectionMap) {
                foreach (Intersection intersection in entry.Value) {
                    Intersection currentIntersection = intersection;
                    Intersection firstIntersection = intersection;
                    if (hasIntersectionBeenCovered.Contains(currentIntersection))
                        continue;
                    Debug.Log("Starting a new path!!");
                    Path mergedPath = new Path();
                    mergedPathList.Add(mergedPath);
                    int mC = 0;
                    while(true) {
                        mC++;
                        if (mC > 5000) {
                            Debug.Log("Stuck in an infinite loop dawg. It aint pretty up here." + currentIntersection.ToString());
                        }
                        // break if we encounter the same intersection again. it means we have completed a loop.
                        if (hasIntersectionBeenCovered.Contains (currentIntersection))
                            break;

                        Debug.Log("NExt chosen intersection : " + currentIntersection);
                        // mark this intersection as covered
                        hasIntersectionBeenCovered.Add(currentIntersection);
                        if (currentIntersection == null) {
                            Debug.Log("NPR :" + currentIntersection);
                        }
                        mergedPath.path.Add(currentIntersection.point);
                        int index = 0;
                        // if the paths are same then switch to new path
                        if (curPath == currentIntersection.path) {
                            curPath = newPath;
                            otherPath = currentIntersection.path;
                            index = currentIntersection.newPathI;
                        }
                        else {
                            curPath = currentIntersection.path;
                            otherPath = newPath;
                            index = currentIntersection.oldPathI;
                        }
                        // left, right points and all the intersections between them
                        int leftIndex = index;
                        int rightIndex = mod(index + 1, curPath.path.Count);
                        Vector2 leftPoint = curPath.path[leftIndex];
                        Vector2 rightPoint = curPath.path[rightIndex];
                        if (!curPath.indexToIntersectionMap.ContainsKey (index)) {
                            Debug.Log("INDEX NOT FOUND : " + index + " keys : ");
                            foreach (int key in curPath.indexToIntersectionMap.Keys) {
                                Debug.Log(key);
                            }
                        }
                        List<Intersection> intersections = new List<Intersection> (curPath.indexToIntersectionMap[index]);
                        
                        // if we have overlapping intersection point and path vertex, then we need to consider 
                        // additional left or right vertex. 
                        // Also remove the duplicate intersection point from the intersection list 
                        if (currentIntersection.point == leftPoint) {
                            leftIndex = mod(index - 1, curPath.path.Count);
                            if (curPath.indexToIntersectionMap.ContainsKey (leftIndex)) {
                                List<Intersection> newIntersectionList = new List<Intersection> (curPath.indexToIntersectionMap[leftIndex]);
                                if (newIntersectionList.Count > 0 && currentIntersection.point == newIntersectionList[newIntersectionList.Count - 1].point) {
                                    Debug.Log("Removing currentIntersection from new List");
                                    newIntersectionList.RemoveAt (newIntersectionList.Count - 1); 
                                }
                                intersections.AddRange(newIntersectionList);
                            }
                            leftPoint = curPath.path[leftIndex];
                        }
                        else if (currentIntersection.point == rightPoint) {
                            int prevIndex = rightIndex;
                            rightIndex = mod(index + 2, curPath.path.Count);
                            if (curPath.indexToIntersectionMap.ContainsKey (prevIndex)) {
                                List<Intersection> newIntersectionList = new List<Intersection> (curPath.indexToIntersectionMap[prevIndex]);
                                if (newIntersectionList.Count > 0 && currentIntersection.point == newIntersectionList[0].point){
                                    Debug.Log("Removing currentIntersection from new List");
                                    newIntersectionList.RemoveAt (0);
                                }
                                intersections.AddRange(newIntersectionList);
                            }
                            rightPoint = curPath.path[rightIndex];
                        }

                        // get the adjacent points of the intersection point
                        // sort it first
                        intersections.Sort(delegate (Intersection i1, Intersection i2) {
                            float distance1 = Vector2.Distance(leftPoint, i1.point);
                            float distance2 = Vector2.Distance(leftPoint, i2.point);
                            if (Mathf.Approximately (distance1, distance2))
                                return 0;
                            else if (distance1 > distance2)
                                return 1;
                            else 
                                return -1;
                        });
                        Intersection leftIntersection = null;
                        Intersection rightIntersection = null;
                        Vector2 adjLeft = leftPoint;
                        Vector2 adjRight = rightPoint;
                        for (int i = 0; i < intersections.Count; i++) {
                            Debug.Log(intersections[i]);
                            if (currentIntersection == intersections[i]) {
                                if (i != 0) {
                                    leftIntersection = intersections[i - 1];
                                    adjLeft = leftIntersection.point;
                                }
                                if (i != intersections.Count - 1) {
                                    rightIntersection = intersections[i + 1];
                                    adjRight = rightIntersection.point;
                                }
                            }
                        }
                        // get the midpoint of the points on the left and right to the intersection point
                        adjLeft = (adjLeft + currentIntersection.point) * 0.5f;
                        adjRight = (adjRight + currentIntersection.point) * 0.5f;

                        bool isLeftPointInside = IsInsidePolygon(otherPath, adjLeft);
                        bool isRightPointInside = IsInsidePolygon(otherPath, adjRight);
                        
                        int dir = 1;
                        int j;
                        Debug.Log ("IP : " + currentIntersection.point.x + ", " + currentIntersection.point.y + " LEFT : " + leftPoint.x + ", " + leftPoint.y + " LEFT INDEX : " + leftIndex +
                                " RIGHT : " + rightPoint.x + ", " + rightPoint.y + "RIGHT INDEX : " + rightIndex + "ADJ LEFT : " + adjLeft.x + ", " + adjLeft.y + " RIGHT ADJ " + adjRight.x + ", " + adjRight.y
                                 + " IS LEFT POINT INSIDE : " + isLeftPointInside + " IS RIGHT POINT INSIDE : " + isRightPointInside);

                        if ((!isLeftPointInside && !isRightPointInside) || (isLeftPointInside && isRightPointInside)) {
                            Debug.Log("Something is not right both are inside : " + (isLeftPointInside && isRightPointInside) + " or Outside");
                            break;
                        }
                        else if (isLeftPointInside) {
                            if (rightIntersection != null) {
                                continue;
                            }
                            mergedPath.path.Add(rightPoint);
                            j = mod(rightIndex + 1, curPath.path.Count);
                        }
                        else {
                            if (leftIntersection != null) {
                                Debug.Log("Taking the left intersection");
                                currentIntersection = leftIntersection;
                                continue;
                            }
                            dir = -1;
                            mergedPath.path.Add(leftPoint);
                            j = mod(leftIndex - 1, curPath.path.Count);
                        }
                        Debug.Log ("GOING SOLO!! with J : " + j + " dir : " + dir);
                        int k = j;
                        while (!curPath.indexToIntersectionMap.ContainsKey(j)) {
                            mergedPath.path.Add(curPath.path[j]);
                            j = mod(j + dir, curPath.path.Count);
                            if (k == j) {
                                Debug.Log("Stuck in an infinite loop. Kind of sucks here : J : " + j + " key collection : " + curPath.indexToIntersectionMap.Keys);
                                throw new Exception("THIS AINT WORKING DAWG!!");
                            }
                        }

                        // if there are multiple intersections with the same line segment, then find the closest intersection to the current point.
                        List<Intersection> intersectionList = curPath.indexToIntersectionMap[j];
                        int l = dir == -1 ? mod(j - dir, curPath.path.Count) : j;
                        Vector2 initialPoint = curPath.path[l];
                        float minDistance = float.MaxValue;
                        for (int i = 0; i < intersectionList.Count; i++) {
                            float curDistance = Vector2.Distance(intersectionList[i].point, initialPoint);
                            if (curDistance < minDistance) {
                                currentIntersection = intersectionList[i];
                                curDistance = minDistance;
                            }
                        }
                        
                    }
                    // add the final point.
                    mergedPath.path.Add(currentIntersection.point);
                    InitializeAABBForPath(mergedPath);
                }
            }

            // remove the merged paths
            List<Path> pathsToBeRemoved = new List<Path>();
            for (int i = 0; i < paths.Count; i++) {
                if (paths[i].indexToIntersectionMap.Count > 0) {
                    pathsToBeRemoved.Add(paths[i]);
                }
            }
            foreach (Path p in pathsToBeRemoved) {
                paths.Remove(p);
            }
            
            paths.AddRange(mergedPathList);
        }
    }

    void InitializeAABBForPath (Path path) {
        foreach(Vector2 point in path.path){
            path.xMin = Math.Min(path.xMin, point.x);
            path.yMin = Math.Min(path.yMin, point.y);
            path.xMax = Math.Max(path.xMax, point.x);
            path.yMax = Math.Max(path.yMax, point.y);
        }
    }

    bool IsInsidePolygon (Vector3 point) {
        int count = 0;
        Vector2 point2D = new Vector2(point.x, point.z);
        // we have a ray starting from point2D and extending towards positive x axis parallely
        // we want to check how many times this ray intersects with other paths
        // if its odd, then we are inside the polygon, otherwise we are outside  
        foreach (Path path in paths) {
            // does the ray intersect with bounding box of the path
            if (path.xMax > point2D.x && path.yMin < point2D.y && path.yMax > point2D.y) {
                // check for intersection with all the edges of the path
                bool flag = false;
                for (int i = 1; i < path.path.Count; i++){
                    // if (i == path.path.Count - 1 && skipEnd) {
                    //     continue;
                    // }
                    Vector2 point1 = path.path[i - 1];
                    Vector2 point2 = path.path[i];
                    if (point1.x > point2D.x || point2.x > point2D.x) {
                        
                        if (Mathf.Approximately(point1.y, point2D.y) || Mathf.Approximately(point2.y, point2D.y)) {
                            if (!flag) {
                                count++;
                                //Debug.Log("Alligned Original ->" + point2D.x +"," +point2D.y + " Collisions : " + point1.x+","+point1.y + " & " + point2.x + "," + point2.y);

                            }
                            flag = true;
                        }
                        else if ((point1.y < point2D.y && point2.y > point2D.y) || (point1.y > point2D.y && point2.y < point2D.y)) 
                        {
                            count++;
                            //Debug.Log("Original ->" + point2D.x +"," +point2D.y + " Collisions : " + point1.x+","+point1.y + " & " + point2.x + "," + point2.y);
                        }

                    }
                }
            }
        }
        return count % 2 == 1;
    }
    // Axis Alligned Bounding Box
    bool IsInsideAABB (Path path, Vector2 point) {
        return path.xMin < point.x && path.yMin < point.y && path.xMax > point.x && path.yMax > point.y;
    } 

    // draw a line segment from the bounding box minimum point to the point to be checked
    // if the number of intersections is even, it means point is outside, else its inside
    bool IsInsidePolygon(Path path, Vector2 p1) {
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

    bool IsInsidePolygon1(Path path, Vector2 p1) {
        //Vector2 p2 = new Vector2(Mathf.Approximately(path.xMin, p1.x) ? path.xMax : path.xMin, Mathf.Approximately(p1.y, path.yMin) ? path.yMax : path.yMin);
        Vector2 p2 = new Vector2(path.xMax, path.yMax);
        int count = 0;
        for (int i = 1; i < path.path.Count; i++) {
            Vector2 p3 = path.path[i - 1];
            Vector2 p4 = path.path[i];
            count += AreLinesIntersecting(p1, p2, p3, p4) != null ? 1 : 0;
        }
        return count % 2 == 1;
    }


    List<Intersection> DoesLineIntersectWithAnyPath (Vector2 start, Vector2 end) {
        List<Intersection> intersectionList = new List<Intersection> ();
        foreach (Path path in paths) {
            if (IsInsideAABB (path, start) || IsInsideAABB (path, end)) {
                for (int i = 1; i < path.path.Count; i++) {
                    Vector2 point1 = path.path[i - 1];
                    Vector2 point2 = path.path[i];

                    Intersection intersectionPoint = AreLinesIntersecting (start, end, point1, point2);
                    if (intersectionPoint != null) {
                        intersectionPoint.path = path;
                        // if intersection point overlaps with old path's vertices or new path's vertices, then dont look for further intersections 
                        bool overlapsWithOldPath1 = point1 == intersectionPoint.point;
                        bool overlapsWithOldPath2 = point2 == intersectionPoint.point;
                        bool overlapsWithNewPathStartOrEnd = (start == intersectionPoint.point) || (end == intersectionPoint.point);
                        if (overlapsWithOldPath1 || overlapsWithOldPath2 || overlapsWithNewPathStartOrEnd) {
                            Debug.Log("Intersects with old path 1 : " + overlapsWithOldPath1 + " old path 2 : " + overlapsWithOldPath2 + " overlapsWithNewPath : " + overlapsWithNewPathStartOrEnd);
                            int index = overlapsWithOldPath2 ? i : i - 1;
                            
                            if (CheckIfPointAlreadyPresent (path, index, intersectionPoint.point))
                                return intersectionList;

                            intersectionPoint.oldPathI = index; 
                            if (path.indexToIntersectionMap.ContainsKey(index)) 
                                path.indexToIntersectionMap[index].Add(intersectionPoint);
                            else 
                                path.indexToIntersectionMap.Add(index, new List<Intersection> {intersectionPoint});

                            return new List<Intersection> {intersectionPoint};
                        }
                        
                        // // check if the intersection point overlaps with point1 or point2 
                        // bool overlapWithPoint1 = point1 == intersectionPoint.point;
                        // bool overlapWithPoint2 = point2 == intersectionPoint.point;
                        // if (overlapWithPoint1 || overlapWithPoint2) {
                        //     // we want to avoid capturing this intersection scenario, 
                        //     // since this intersection is useless in merging algo
                        //     //
                        //     //                point2  point3
                        //     //                   \    /
                        //     //                    \  /
                        //     //  start______________\/_____________ end
                        //     //         intersection point/point1

                        //     Vector2 pt2;
                        //     Vector2 pt3;
                        //     if (overlapWithPoint1) {
                        //         pt3 = path.path[i];
                        //         pt2 = path.path[mod(i - 2, path.path.Count)];
                        //     }
                        //     else {
                        //         pt3 = path.path[mod(i + 1, path.path.Count)];
                        //         pt2 = path.path[i - 1];
                        //     }
                        //     if (ArePointsOnTheSameSideOfLine(pt2, pt3, start, end)) {
                        //         Debug.Log("Unnecessary Intersection detected. aIgnoring");
                        //         continue;
                        //     }

                        // }   
                        // add the intersectionPoint to path's intersection map as well.
                       // Debug.Log("Intersecting with PATH : XMIN,YMIN ->" + path.xMin + ", " + path.yMin + " : XMAX,YMAX -> " + path.xMax + ", " + path.yMax);
                        intersectionPoint.oldPathI = i - 1; 

                        if (path.indexToIntersectionMap.ContainsKey(i - 1)) {
                            path.indexToIntersectionMap[i - 1].Add(intersectionPoint);
                            if (path.indexToIntersectionMap[i - 1].Count > 2) {
                                Debug.Log("Same line segment is intersecting with more than 2 lines. Should be avoided");
                            }
                        }
                        else {
                            path.indexToIntersectionMap.Add(i - 1, new List<Intersection> {intersectionPoint});
                        }
                        intersectionList.Add(intersectionPoint);
                    }
                }
            }            
        }
        return intersectionList;
    }

    bool CheckForSelfIntersection (Vector2 start, Vector2 end, Path path) {
        for (int i = 1; i < path.path.Count - 1; i++) {
            Vector2 point1 = path.path[i - 1];
            Vector2 point2 = path.path[i];
            if (AreLinesIntersecting(point1, point2, start, end) != null)
                return true; 
        }
        return false;
    }

    bool CheckIfPointAlreadyPresent (Path path, int index, Vector2 intersectionPoint) {
        if (path.indexToIntersectionMap.ContainsKey(index)) {
                List<Intersection> intersectionList = path.indexToIntersectionMap[index];
                bool flag = false;
                foreach (Intersection i1 in intersectionList) {
                    if (i1.point == intersectionPoint)
                        flag = true;
                }
                return flag;
        }
        return false;
    }

    Intersection AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2)
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

    bool ArePointsOnTheSameSideOfLine(Vector2 pt1, Vector2 pt2, Vector2 linePt1, Vector2 linePt2) {
        // perform cross product between AB and AC, do the same for AB and AD, if they are facing the same
        // direction then they are on same side, otherwise opposite direction
        Vector3 AB = linePt1 - linePt2;
        Vector3 AC = linePt1 - pt1;
        Vector3 AD = linePt1 - pt2;
        return Vector3.Dot(Vector3.Normalize(Vector3.Cross(AB, AC)), Vector3.Normalize(Vector3.Cross(AB, AD))) >= 0;
        
    }

    int mod (int val, int size) {
        return ((val) % size + size) % size; 
    }
}