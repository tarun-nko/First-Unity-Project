using UnityEngine;
using System.Collections.Generic;
using System;
// Implementation of Ear Clipping Algorithm
// Runtime : O(n^2)

public class Triangulator
{
    // for internal use only
    class Vertex {
        public int index;
        public Vector2 position;
        public Vertex(int index, Vector2 position) {
            this.index = index;
            this.position = position;
        }
        public override string ToString() {
            return this.position.x + ", " + this.position.y + " : " + this.index;
        }
    }

    public static Tuple<List<int>, bool> Triangulate (List<Vector2> path) {
        if (path != null && path.Count != 0) {
            path.RemoveAt(path.Count - 1);
        }
         
        List<int> indices = new List<int> ();
        LinkedList<Vertex> ll = new LinkedList<Vertex> ();
        // create a Vertex doubly linked list, wish .Net had a circular linked list as well, alas
        for (int i = 0; i < path.Count; i++) {
            ll.AddLast(new Vertex(i, path[i]));
        }
        int count1 = 0;
        bool isPathClockwise = HelperFunctions.IsOrientedClockwise(path.ToArray());
        Debug.Log("Path is clockwise : " + isPathClockwise);
        bool isFirst = true;
        LinkedListNode<Vertex> node = ll.First;
        while (ll.Count >= 3) {
            LinkedListNode<Vertex> prevNode = node.Previous == null ? ll.Last : node.Previous;
            LinkedListNode<Vertex> nextNode = node.Next == null ? ll.First : node.Next;
            bool isVertexConvex = HelperFunctions.IsVertexConvex(prevNode.Value.position, node.Value.position, nextNode.Value.position, isPathClockwise);
            if (isFirst) {
                Debug.Log(prevNode.Value.ToString() + ", " + node.Value.ToString() + ", " + nextNode.Value.ToString());
                Debug.Log("IsVertexConvex : " + isVertexConvex);
                isFirst = false;
            }
            if (++count1 > 10000){
                Debug.Log("Stuck in an infinite outer loop dawg.");
                break;
            }
            if (isVertexConvex) {
                //Debug.Log("Vertex is convex : " + node.Value.ToString());
                // check if its an Ear
                // the triangle formed by the 3 adjacent nodes shouldn't contain any vertex from the rest of the polygon
                LinkedListNode<Vertex> iteratorNode = ll.First;
                bool isEar = true;
                while (iteratorNode.Next != null) {
                    if (!(iteratorNode == node || iteratorNode == prevNode || iteratorNode == nextNode)) {
                        if (HelperFunctions.InsideTriangle(prevNode.Value.position, node.Value.position, nextNode.Value.position, iteratorNode.Value.position)) {
                            isEar = false;
                           // Debug.Log("Vertex is not an ear : " + node.Value.ToString());
                            break;
                        }
                    }
                    iteratorNode = iteratorNode.Next;
                }
                if (isEar) {
                   // Debug.Log("Vertex is an ear : " + node.Value.ToString());
                    // remove the ear
                    ll.Remove(node);
                    // add to the indices in clockwise winding order, since thats front facing in Unity
                    if (isPathClockwise){//HelperFunctions.IsTriangleOrientedClockwise(prevNode.Value.position, node.Value.position, nextNode.Value.position)){
                        indices.Add(prevNode.Value.index);
                        indices.Add(node.Value.index);
                        indices.Add(nextNode.Value.index);
                     //   Debug.Log("INDEX : " + prevNode.Value.index + ", " + node.Value.index + ", " + nextNode.Value.index);
                    } 
                    else {
                        indices.Add(nextNode.Value.index);
                        indices.Add(node.Value.index);
                        indices.Add(prevNode.Value.index);
                       // Debug.Log("INDEX : " + nextNode.Value.index + ", " + node.Value.index + ", " + prevNode.Value.index);
                    }
                }
                node = nextNode;
            }
            else {
                node = node.Next == null ? ll.First : node.Next;
            }
        }
        return Tuple.Create(indices, isPathClockwise);
    }
     
    // private float Area () {
    //     int n = m_points.Count;
    //     float A = 0.0f;
    //     for (int p = n - 1, q = 0; q < n; p = q++) {
    //         Vector2 pval = m_points[p];
    //         Vector2 qval = m_points[q];
    //         A += pval.x * qval.y - qval.x * pval.y;
    //     }
    //     return (A * 0.5f);
    // }
  
}
