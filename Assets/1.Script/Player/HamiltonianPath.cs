using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamiltonianPath : MonoBehaviour
{
    private List<GameObject> targetList;   // 해킹에서 받아옴.
    private LineRenderer     lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        targetList   = PlayerHacking.instance.targetObjectList;
        FindHamiltonianPath();
    }
    
    private void FindHamiltonianPath()
    {
        List<Vector3> points = new List<Vector3>();
        foreach (GameObject target in targetList)
        {
            points.Add(target.transform.position);
        }

        float minDistance = float.MaxValue;
        List<Vector3> minPath = null;
        
        foreach (var path in Permutations(points))
        {
            float distance = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                distance += Vector3.Distance(path[i], path[i + 1]);
            }

            if (distance < minDistance)
            {
                minDistance = distance;
                minPath = new List<Vector3>(path);
            }
        }

        List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>();
        for (int i = 0; i < minPath.Count - 1; i++)
        {
            edges.Add((minPath[i], minPath[i + 1]));
        }

        for (int i = 0; i < edges.Count; i++)
        {
            for (int j = i + 1; j < edges.Count; j++)
            {
                if (IsIntersect(edges[i], edges[j]))
                {
                    (edges[i], edges[j]) = (edges[j], edges[i]);
                }
            }
        }

        lineRenderer.positionCount = minPath.Count;
        lineRenderer.SetPositions(minPath.ToArray());

        List<GameObject> newList = new List<GameObject>();
        for (int j = minPath.Count - 1; j >= 0; j--)
        {
            float distance    = float.MaxValue;
            int   selectedNum = 0;
            
            for (int i = 0; i < PlayerHacking.instance.targetObjectList.Count; i++)
            {
                if (distance > Vector2.Distance(minPath[j],PlayerHacking.instance.targetObjectList[i].transform.position))
                {
                    distance   = Vector2.Distance(minPath[j], PlayerHacking.instance.targetObjectList[i].transform.position);
                    selectedNum = i;
                }
            }
            newList.Add(PlayerHacking.instance.targetObjectList[selectedNum]);
        }
        
        PlayerHacking.instance.targetObjectList.Clear();
        newList.Reverse();
        PlayerHacking.instance.targetObjectList = newList;

        Destroy(gameObject);
    }
    
    private bool IsIntersect((Vector3, Vector3) line1, (Vector3, Vector3) line2)
    {
        bool CCW(Vector3 A, Vector3 B, Vector3 C)
        {
            return (C.y - A.y) * (B.x - A.x) > (B.y - A.y) * (C.x - A.x);
        }

        Vector3 A = line1.Item1;
        Vector3 B = line1.Item2;
        Vector3 C = line2.Item1;
        Vector3 D = line2.Item2;

        return CCW(A, C, D) != CCW(B, C, D) && CCW(A, B, C) != CCW(A, B, D);
    }

    private IEnumerable<List<T>> Permutations<T>(List<T> list)
    {
        if (list.Count == 0)
        {
            yield return new List<T>();
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                T item = list[i];
                List<T> remaining = new List<T>(list);
                remaining.RemoveAt(i);

                foreach (var permutation in Permutations(remaining))
                {
                    permutation.Insert(0, item);
                    yield return permutation;
                }
            }
        }
    }
}
