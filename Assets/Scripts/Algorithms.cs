using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Algorithms
{
    public static List<GameObject> ConvexHullBasic(List<GameObject> p)
    {
        Shuffle(p);
        Dictionary<Vector2, GameObject> mapping = new Dictionary<Vector2, GameObject>();

        //Copy the list
        List<Vector2> points = p.Select(x => (Vector2)x.transform.position).ToList();
        foreach (GameObject g in p)
        {
            mapping.Add((Vector2)g.transform.position, g);
        }

        List<Vector2> triangle;
        if (isLeft(points[1], points[2], points[0]))
            triangle = new List<Vector2>() { points[0], points[2], points[1] };
        else
            triangle = new List<Vector2>() { points[0], points[1], points[2] };
        Polygon hull = new Polygon(triangle);

        Vector2 interiorPoint = hull.vertices.Select(x => x.position).Aggregate((x, y) => x + y);
        interiorPoint = interiorPoint / 3.0f;

        //GameObject center = Instantiate(point);
        //center.name = "CenterPoint";
        //center.transform.position = interiorPoint;
        //center.GetComponent<SpriteRenderer>().color = Color.red;

        //Partitioning the remaining points by the side of the triangle they intersect
        Dictionary<Vertex, HashSet<Vector2>> buckets = new Dictionary<Vertex, HashSet<Vector2>>();
        Dictionary<Vector2, Vertex> candidateEdgeMap = new Dictionary<Vector2, Vertex>();

        for (int i = 3; i < points.Count; i++)
        {
            Vector2 candidatePoint = points[i];

            foreach(Vertex v in hull.vertices)
            {
                Vector2 p1 = v.position;
                Vector2 p2 = v.next.position;

                if (Intersects(candidatePoint, interiorPoint, p1, p2))
                {
                    if (!buckets.ContainsKey(v))
                        buckets.Add(v, new HashSet<Vector2>());
                    buckets[v].Add(candidatePoint);
                    candidateEdgeMap.Add(candidatePoint, v);
                }
            }
        }

        Vertex hullVertex = hull.vertices[0];
        List<Vector2> candidates = candidateEdgeMap.Keys.Select(x => x).ToList();
        foreach(Vector2 candidate in candidates)
        {
            if (candidateEdgeMap[candidate] == null)
            {
                mapping[candidate].GetComponent<Point>().DisplayColor = Color.black;
                continue;
            }

            mapping[candidate].GetComponent<Point>().DisplayColor = Color.cyan;

            //BuildTent Code
            Vertex c = new Vertex(candidate);
            Vertex v = candidateEdgeMap[candidate];
            Vertex next = v.next;
            Vertex prev = v;
            bool lineSide = true;

            //Marking a starting point of our hull
            hullVertex = c;

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

                bool intersect1 = Intersects(r, interiorPoint, v.position, c.position);
                bool intersect2 = Intersects(r, interiorPoint, c.position, next.position);

                if(!intersect1)
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
                else if(!intersect1 && !intersect2)
                {
                    candidateEdgeMap[r] = null;
                }
            }

            //Forward checks
            do
            {
                Vertex next_prime = next.next;
                lineSide = isLeft(next.position, next_prime.position, c.position);
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

                            bool intersect = Intersects(r, interiorPoint, c.position, next_prime.position);
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

                next = next_prime;
            } while (lineSide);

            //Backwards checks
            do
            {
                Vertex prev_prime = prev.prev;
                lineSide = isLeft(prev_prime.position, prev.position, c.position);
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

                            bool intersect = Intersects(r, interiorPoint, prev_prime.position, c.position);
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

                prev = prev_prime;
            } while (lineSide);
        }


        List<Vector2> finalHull = new List<Vector2>();

        Vertex tracer = hullVertex;
        finalHull.Add(tracer.position);

        int count = 0;
        while (tracer.next != hullVertex)
        {
            tracer = tracer.next;
            finalHull.Add(tracer.position);
        }

        return finalHull.Select(x => mapping[x]).ToList();
    }

    static void Shuffle<GameObject>(IList<GameObject> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            GameObject value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    //We could replace this with a faster intersection test http://www.stefanbader.ch/faster-line-segment-intersection-for-unity3dc/
    public static bool Intersects(Vector2 p11, Vector2 p12,
                                    Vector2 p21, Vector2 p22)
    {
        float x1 = p11.x;
        float x2 = p12.x;
        float x3 = p21.x;
        float x4 = p22.x;

        float y1 = p11.y;
        float y2 = p12.y;
        float y3 = p21.y;
        float y4 = p22.y;

        Matrix4x4 myMatrix = new Matrix4x4(new Vector4(x1, y1, 1.0f, 0.0f), new Vector4(x2, y2, 1.0f, 0.0f),
                                            new Vector4(-x3, -y3, 0.0f, 1.0f), new Vector4(-x4, -y4, 0.0f, 1.0f));

        Vector4 vec = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        Vector4 solution = myMatrix.inverse * vec;

        return solution[0] >= 0 && solution[1] >= 0 && solution[2] >= 0 && solution[3] >= 0;
    }

    public static bool isLeft(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) >= 0.0f;
    }
}


public class Polygon
{
    public List<Vertex> vertices { get; private set; }

    /// <summary>
    /// Polygon must be initially constructed in order
    /// </summary>
    /// <param name="points"></param>
    public Polygon(List<Vector2> points)
    {
        vertices = points.Select(x => new Vertex(x)).ToList();

        for(int i = 0; i < vertices.Count; i++)
        {
            Vertex prev = i == 0 ? vertices[vertices.Count - 1] : vertices[i - 1];
            Vertex next = i == vertices.Count - 1 ? vertices[0] : vertices[i + 1];

            vertices[i].prev = prev;
            vertices[i].next = next;
        }
    }

    public void AddVertex(Vertex v)
    {
        vertices.Add(v);
    }
}

public class Vertex
{
    public Vector2 position;
    public Vertex prev;
    public Vertex next;

    public Vertex(Vector2 position)
    {
        this.position = position;
    }
}