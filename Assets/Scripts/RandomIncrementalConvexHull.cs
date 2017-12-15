using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RandomIncrementalConvexHull
{
    public List<Vector2> InputPoints;

    public Polygon hull;
    public Vector2 interiorPoint;
    public Dictionary<Vertex, HashSet<Vector2>> buckets = new Dictionary<Vertex, HashSet<Vector2>>();
    public Dictionary<Vector2, Vertex> candidateEdgeMap = new Dictionary<Vector2, Vertex>();

    //Assumes points were given in randomly
    public RandomIncrementalConvexHull(List<Vector2> points)
    {
        InputPoints = new List<Vector2>(points);

        List<Vector2> triangle;
        if (Algorithms.isLeft(points[1], points[2], points[0]))
            triangle = new List<Vector2>() { points[0], points[2], points[1] };
        else
            triangle = new List<Vector2>() { points[0], points[1], points[2] };
        Polygon hull = new Polygon(triangle);

        interiorPoint = hull.vertices.Select(x => x.position).Aggregate((x, y) => x + y);
        interiorPoint = interiorPoint / 3.0f;

        for (int i = 3; i < points.Count; i++)
        {
            Vector2 candidatePoint = points[i];

            foreach (Vertex v in hull.vertices)
            {
                Vector2 p1 = v.position;
                Vector2 p2 = v.next.position;

                if (Algorithms.Intersects(candidatePoint, interiorPoint, p1, p2))
                {
                    if (!buckets.ContainsKey(v))
                        buckets.Add(v, new HashSet<Vector2>());
                    buckets[v].Add(candidatePoint);
                    candidateEdgeMap.Add(candidatePoint, v);
                }
            }
        }
    }

    public void AddPoint(Vector2 p)
    {
        Vector2 candidatePoint = p;
        
        foreach (Vertex v in hull.vertices)
        {
            Vector2 p1 = v.position;
            Vector2 p2 = v.next.position;
        
            if (Algorithms.Intersects(candidatePoint, interiorPoint, p1, p2))
            {
                if (!buckets.ContainsKey(v))
                    buckets.Add(v, new HashSet<Vector2>());
                buckets[v].Add(candidatePoint);
                candidateEdgeMap.Add(candidatePoint, v);
            }
        }
    }

    public void Step()
    {
        Vector2 candidate = candidateEdgeMap.Keys.ElementAt(0);

        if (candidateEdgeMap[candidate] == null)
        {
            candidateEdgeMap.Remove(candidate);
            return;
        }

        //mapping[candidate].GetComponent<Point>().DisplayColor = Color.cyan;

        //BuildTent Code
        Vertex c = new Vertex(candidate);
        Vertex v = candidateEdgeMap[candidate];
        Vertex next = v.next;
        Vertex prev = v;
        bool lineSide = true;


        //Initial tent building
        v.next = c;
        c.prev = v;
        c.next = next;
        next.prev = c;



        List<Vector2> initialReassignedPoints = buckets[v].Select(x => x).ToList();
        foreach (Vector2 r in initialReassignedPoints)
        {
            if (r == c.position)
                continue;

            bool intersect1 = Algorithms.Intersects(r, interiorPoint, v.position, c.position);
            bool intersect2 = Algorithms.Intersects(r, interiorPoint, c.position, next.position);

            if (!intersect1)
            {
                buckets[v].Remove(r);
            }

            if (intersect2)
            {
                if (!buckets.ContainsKey(c))
                    buckets.Add(c, new HashSet<Vector2>());
                buckets[c].Add(r);
                candidateEdgeMap[r] = c;
            }
            else if (!intersect1 && !intersect2)
            {
                candidateEdgeMap[r] = null;
            }
        }

        //Forward checks
        do
        {
            Vertex next_prime = next.next;
            lineSide = Algorithms.isLeft(next.position, next_prime.position, c.position);
            if (lineSide)
            {
                c.next = next_prime;
                next_prime.prev = c;

                if (buckets.ContainsKey(next))
                {
                    List<Vector2> reassignedPoints = buckets[next].Select(x => x).ToList();
                    //Reassigning things
                    foreach (Vector2 r in reassignedPoints)
                    {
                        if (r == c.position)
                            continue;
                        buckets[next].Remove(r);

                        bool intersect = Algorithms.Intersects(r, interiorPoint, c.position, next_prime.position);
                        if (intersect)
                        {
                            if (!buckets.ContainsKey(c))
                                buckets.Add(c, new HashSet<Vector2>());
                            buckets[c].Add(r);
                            candidateEdgeMap[r] = c;
                        }
                        else
                            candidateEdgeMap[r] = null;
                    }
                }
            }

            hull.vertices.Remove(next);
            next = next_prime;
        } while (lineSide);

        //Backwards checks
        do
        {
            Vertex prev_prime = prev.prev;
            lineSide = Algorithms.isLeft(prev_prime.position, prev.position, c.position);
            if (lineSide)
            {
                c.prev = prev_prime;
                prev_prime.next = c;

                if (buckets.ContainsKey(prev))
                {
                    List<Vector2> reassignedPoints = buckets[prev].Select(x => x).ToList();
                    //Reassigning things
                    foreach (Vector2 r in reassignedPoints)
                    {
                        if (r == c.position)
                            continue;
                        buckets[prev].Remove(r);

                        bool intersect = Algorithms.Intersects(r, interiorPoint, prev_prime.position, c.position);
                        if (intersect)
                        {
                            if (!buckets.ContainsKey(prev_prime))
                                buckets.Add(prev_prime, new HashSet<Vector2>());
                            buckets[prev_prime].Add(r);
                            candidateEdgeMap[r] = prev_prime;
                        }
                        else
                            candidateEdgeMap[r] = null;
                    }
                }
            }

            hull.vertices.Remove(prev);
            prev = prev_prime;
        } while (lineSide);
    }
}
