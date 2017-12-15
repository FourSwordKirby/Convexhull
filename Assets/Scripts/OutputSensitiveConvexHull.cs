using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OutputSensitiveConvexHull {

    public List<Vector2> InputPoints;
    public int LeftmostPointIndex;

    public float TotalTime;

    public OutputSensitiveConvexHull(List<Vector2> points)
    {
        InputPoints = new List<Vector2>(points);
        ComputeLeftmostPoint();
    }

    public int ComputeLeftmostPoint()
    {
        // Find and assign leftmost point in linear time.
        LeftmostPointIndex = 0;
        for (int i = 1; i < InputPoints.Count; ++i)
        {
            if (InputPoints[i].x < InputPoints[LeftmostPointIndex].x)
            {
                LeftmostPointIndex = i;
                continue;
            }

            if (InputPoints[i].x == InputPoints[LeftmostPointIndex].x)
            {
                if (InputPoints[i].y > InputPoints[LeftmostPointIndex].y)
                {
                    LeftmostPointIndex = i;
                    continue;
                }
            }
        }
        return LeftmostPointIndex;
    }
    
    public List<int> ComputeConvexHullIndices()
    {
        Debug.Log("Computing output sensative convex hull on " + InputPoints.Count + " points");

        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        ComputeLeftmostPoint();
        int restarts = 0;
        int h = 39;
        while (restarts <= 4)
        {
            List<int> result = ComputeConvexHullIndicesWithMaxSize(h);
            if (result != null)
            {
                watch.Stop();
                TotalTime = watch.ElapsedMilliseconds / 1000f;
                Debug.Log("Restarts required: " + restarts);
                return result;
            }

            h *= h;
            ++restarts;
        }

        throw new System.Exception("Output sensative convex hull did not return for h > n.");
    }

    public int H;
    public List<List<int>> SubproblemPointsIndices;
    public List<List<int>> IndiciesForSubhulls;
    public List<List<Vector2>> Subhulls;
    public List<int> CompleteHull;
    public int CurrentPointIndex;
    public int HullIndex;
    public List<int> TangentIndices;
    int RightmostTangentIndex;
    Vector2 RightmostTanget;

    public void CreateSubproblems(int h)
    {
        Debug.Log("Creating size " + h + " subproblems");
        H = h;

        int numSubproblems = ((InputPoints.Count - 1) / h) + 1;
        SubproblemPointsIndices = new List<List<int>>(numSubproblems);
        Debug.Log("Expected number of subproblems: " + numSubproblems);

        for (int subproblemIndex = 0; subproblemIndex < numSubproblems; ++subproblemIndex)
        {
            List<int> indicies = new List<int>(H);
            for (int i = subproblemIndex * H;  i < Mathf.Min((subproblemIndex + 1) * H, InputPoints.Count); ++i)
            {
                indicies.Add(i);
            }
            
            SubproblemPointsIndices.Add(indicies);
        }
    }
    
    public void ComputeAllSubhulls()
    {
        Debug.Log("Computing subhulls for " + SubproblemPointsIndices.Count + " subproblems");
        IndiciesForSubhulls = new List<List<int>>(SubproblemPointsIndices.Count);
        Subhulls = new List<List<Vector2>>(SubproblemPointsIndices.Count);
        for (int i = 0; i < SubproblemPointsIndices.Count; ++i)
        {
            List<Vector2> subproblem = SubproblemPointsIndices[i].Select(sIndex => InputPoints[sIndex]).ToList();
            List<int> subhullIndices = ComputeSubHullIndicies(subproblem);
            List<Vector2> subhull = subhullIndices.Select(sIndex => subproblem[sIndex]).ToList();
            IndiciesForSubhulls.Add(subhullIndices);
            Subhulls.Add(subhull);
        }
    }

    public void StartWalk()
    {
        Debug.Log("Begining walk along rightmost edges...");
        CompleteHull = new List<int>(H);
        CurrentPointIndex = LeftmostPointIndex;
        CompleteHull.Add(CurrentPointIndex);
        HullIndex = 1;
    }

    public bool StepWalk()
    {
        // Find the rightmost point from our current hull point.
        Vector2 currentPoint = InputPoints[CurrentPointIndex];
        TangentIndices = new List<int>(Subhulls.Count);
        int tangentResult = ComputeTangentIndex(Subhulls[0], currentPoint);
        RightmostTangentIndex = SubproblemPointsIndices[0][IndiciesForSubhulls[0][tangentResult]];
        RightmostTanget = Subhulls[0][tangentResult];
        TangentIndices.Add(SubproblemPointsIndices[0][RightmostTangentIndex]);

        for (int i = 1; i < Subhulls.Count; ++i)
        {
            tangentResult = ComputeTangentIndex(Subhulls[i], currentPoint);
            int tangentIndex = SubproblemPointsIndices[i][IndiciesForSubhulls[i][tangentResult]];
            Vector2 tangent = Subhulls[i][tangentResult];
            TangentIndices.Add(tangentIndex);

            if (!IsLeftOrColinear(currentPoint, RightmostTanget, tangent))
            {
                RightmostTangentIndex = tangentIndex;
                RightmostTanget = tangent;
            }
        }

        // Compute the index into our original list.
        int nextHullPointIndex = RightmostTangentIndex;
        if (nextHullPointIndex == LeftmostPointIndex)
        {
            Debug.Log("Walk has reached the initial convex hull point.");
            return true;
        }

        if (HullIndex < H)
        {
            CompleteHull.Add(nextHullPointIndex);
            CurrentPointIndex = nextHullPointIndex;
        }

        ++HullIndex;
        return false;
    }

    public List<int> ComputeConvexHullIndicesWithMaxSize(int h)
    {
        Debug.Log("Computing hull with max hull size of " + h);
        CreateSubproblems(h);
        ComputeAllSubhulls();

        StartWalk();
        while(HullIndex < H + 1)
        {
            if(StepWalk())
            {
                Debug.Log("Convex hull found with " + CompleteHull.Count + " points.");
                CompleteHull.Reverse();
                return CompleteHull;
            }
        }

        Debug.Log("Failed to find convex hull with at most " + h + " points.");
        return null;
    }

    public Vector2 ComputeTangent(List<Vector2> points, Vector2 refPoint)
    {
        return points[ComputeTangentIndex(points, refPoint)];
    }

    public int ComputeTangentIndex(List<Vector2> points, Vector2 refPoint)
    {
        if (points.Count == 1)
        {
            return 0;
        }

        if (points.Count == 2)
        {
            if (IsLeftOrColinear(refPoint, points[0], points[1]))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }


        int n = points.Count;

        if (points[0] == refPoint)
        {
            return n - 1;
        }

        if (IsLeftOrColinear(refPoint, points[0], points[n-1])
            && !IsLeftOrColinear(refPoint, points[1], points[0]))
        {
            return 0;
        }

        int a = 0;
        int b = n;
        while (a != b)
        {
            int c = (a + b) / 2; // Should always be safe because we already checked 0 = n
            int cNext = (c + 1) % n;
            int cPrev = (c + n - 1) % n;

            if (points[c] == refPoint)
            {
                return cPrev;
            }

            if (points[cNext] == refPoint)
            {
                return c;
            }

            if (points[cPrev] == refPoint)
            {
                return (cPrev + n - 1) % n;
            }

            bool downC = !IsLeftOrColinear(refPoint, points[cNext], points[c]);
            if (downC && IsLeftOrColinear(refPoint, points[c], points[cPrev]))
            {
                return c;
            }

            int aNext = (a + 1) % n;
            if (points[aNext] == refPoint)
            {
                return a;
            }

            bool upA = !IsLeftOrColinear(refPoint, points[a], points[aNext]);
            if (upA)
            {
                if (downC)
                {
                    b = c;
                }
                else
                {
                    if (!IsLeftOrColinear(refPoint, points[c], points[a]))
                    {
                        // a is more up than c
                        b = c;
                    }
                    else
                    {
                        a = c;
                    }
                }
            }
            else
            {
                if(!downC)
                {
                    a = c;
                }
                else
                {
                    if (!IsLeftOrColinear(refPoint, points[a], points[c]))
                    {
                        // c is more up than a
                        b = c;
                    }
                    else
                    {
                        a = c;
                    }
                }
            }
        }

        throw new System.Exception("a == b in Compute tangent!");
    }

    /// <summary>
    /// Returns a convex hull in CW order.
    /// </summary>
    private List<int> ComputeSubHullIndicies(List<Vector2> points)
    {
        List<int> result = Algorithms.ConvexHullBasicOnVectors(points);
        return result;
    }

    private bool IsLeftOrColinear(Vector2 a, Vector2 b, Vector2 p)
    {
        // cross product of (b - a) and (p - a),
        // then check if the result 3d vector has positive or negative Z.
        // 0 means p is colinear to ab.
        return ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)) >= 0;
    }
}
