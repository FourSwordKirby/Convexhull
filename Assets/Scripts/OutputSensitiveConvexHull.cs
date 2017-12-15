using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OutputSensitiveConvexHull {

    public List<Vector2> InputPoints;
    public int LeftmostPointIndex;
    
    public OutputSensitiveConvexHull(List<Vector2> points)
    {
        InputPoints = new List<Vector2>(points);

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
    }
    
    public List<int> ComputeConvexHullIndices()
    {
        Debug.Log("Computing output sensative convex hull on " + InputPoints.Count + " points");

        int h = 3;
        while (h < InputPoints.Count * InputPoints.Count)
        {
            List<int> result = ComputeConvexHullIndicesWithMaxSize(h);
            if (result != null)
            {
                return result;
            }

            h *= h;
        }

        throw new System.Exception("Output sensative convex hull did not return for h > n.");
    }

    public int H;
    public List<List<int>> SubproblemPointsIndices;
    public List<List<Vector2>> SubproblemPoints;
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
        SubproblemPoints = new List<List<Vector2>>(numSubproblems);
        Debug.Log("Expected number of subproblems: " + numSubproblems);

        IEnumerable<Vector2> remainingPoints = InputPoints;
        while (remainingPoints.Count() > 0)
        {
            List<Vector2> subproblem = remainingPoints.Take(h).ToList();
            SubproblemPoints.Add(subproblem);
            SubproblemPointsIndices.Add(Enumerable.Range(SubproblemPointsIndices.Count * h, subproblem.Count).ToList());
            remainingPoints = remainingPoints.Skip(h);
        }
    }

    public bool HasFailedSubhull = false;
    public int FailedSubhull = -1;

    public void ComputeAllSubhulls()
    {
        FailedSubhull = -1;
        Debug.Log("Computing subhulls for " + SubproblemPoints.Count + " subproblems");
        IndiciesForSubhulls = new List<List<int>>(SubproblemPoints.Count);
        Subhulls = new List<List<Vector2>>(SubproblemPoints.Count);
        for (int i = 0; i < SubproblemPoints.Count; ++i)
        {
            Debug.Log("Computing subhull problem " + i);
            List<int> subhullIndices = ComputeSubHullIndicies(SubproblemPoints[i]);

            if(HasFailedSubhull)
            {
                FailedSubhull = i;
                HasFailedSubhull = false;
            }

            List<Vector2> subhull = subhullIndices.Select(sIndex => SubproblemPoints[i][sIndex]).ToList();
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
        
        int n = (points.Count + 1) / 2;
        int i = 0;
        bool isForward = true;
        int extraAttempts = -1;
        while (true)
        {
            int nextI = (i + 1) % points.Count;
            int prevI = (i + points.Count - 1) % points.Count;

            Vector2 p = points[i];
            bool nextIsLeft = IsLeftOrColinear(refPoint, p, points[nextI]);
            bool prevIsLeft = IsLeftOrColinear(refPoint, p, points[prevI]);

            if (p != refPoint && nextIsLeft && prevIsLeft)
            {
                if(extraAttempts > 0)
                {
                    Debug.Log("Extra attempts: " + extraAttempts);
                }
                return i;
            }
            
            if (p == refPoint)
            {
                i = (i + points.Count - n) % points.Count;
                isForward = false;
            }
            else if (isForward)
            {
                if (nextIsLeft || !prevIsLeft)
                {
                    i = (i + points.Count - n) % points.Count;
                    isForward = false;
                }
                else
                {
                    i = (i + n) % points.Count;
                    isForward = true;
                }
            }
            else
            {
                if (prevIsLeft || !nextIsLeft)
                {
                    i = (i + n) % points.Count;
                    isForward = true;
                }
                else
                {
                    i = (i + points.Count - n) % points.Count;
                    isForward = false;
                }
            }
            
            int newN = (n + 1) / 2;
            if (n == newN)
            {
                ++extraAttempts;
            }

            n = newN;
        }
    }

    /// <summary>
    /// Returns a convex hull in CW order.
    /// </summary>
    private List<int> ComputeSubHullIndicies(List<Vector2> points)
    {
        List<Vector2> originalPoints = points.Select(x => x).ToList();
        List<Vector2> copy = originalPoints.Select(x => x).ToList();
        List<int> result = Algorithms.ConvexHullBasicOnVectors(copy);
        for (int i = 0; i < result.Count; ++i)
        {
            Vector2 a = originalPoints[result[i]];
            Vector2 b = originalPoints[result[(i + 1) % result.Count]];
            for (int j = 0; j < originalPoints.Count; ++j)
            {
                if (!(j == result[i] || j == result[(i + 1) % result.Count]))
                {
                    Vector2 p = originalPoints[j];
                    if (IsLeftOrColinear(a, b, p))
                    {
                        Debug.Log("Returned result is not convex!");
                        Debug.Log("Failed on a: " + a + " b: " + b + " p: " + p);
                        HasFailedSubhull = true;
                        return result;
                    }
                }
            }

        }
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
