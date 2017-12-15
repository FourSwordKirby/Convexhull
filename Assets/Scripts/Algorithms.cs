using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Algorithms
{
    public static void ConvexHullBasic(List<GameObject> p)
    {
        //Copy the list
        List<Vector2> points = p.Select(x => (Vector2)x.transform.position).ToList();
        Shuffle(points);

        Polygon hull = new Polygon(points.GetRange(0, 3));

        Vector2 interiorPoint = hull.vertices.Select(x => x.position).Aggregate((x, y) => x + y);
        interiorPoint = interiorPoint / 3.0f;

        //GameObject center = Instantiate(point);
        //center.name = "CenterPoint";
        //center.transform.position = interiorPoint;
        //center.GetComponent<SpriteRenderer>().color = Color.red;

        //Partitioning the remaining points by the side of the triangle they intersect
        Dictionary<Vector2, Vertex> buckets = new Dictionary<Vector2, Vertex>();

        for (int i = 3; i < points.Count; i++)
        {
            Vector2 candidatePoint = points[i];

            foreach(Vertex v in hull.vertices)
            {
                Vector2 p1 = v.position;
                Vector2 p2 = v.next.position;

                if (Intersects(candidatePoint, interiorPoint, p1, p2))
                {
                    buckets.Add(candidatePoint, v);

                    //Checking that the line side test works
                    /*
                    if (p1 == hull[0])
                        candidatePoint.GetComponent<SpriteRenderer>().color = Color.black;
                    if (p1 == hull[1])
                        candidatePoint.GetComponent<SpriteRenderer>().color = Color.cyan;
                    if (p1 == hull[2])
                        candidatePoint.GetComponent<SpriteRenderer>().color = Color.magenta;
                    break;
                    */
                }
            }
        }

        /*
        foreach(Vector2 candidate in buckets.Keys)
        {
            //BuildTent Code
            Vector2 p = candidate;
            Vertex p = new Vertex(p);
            Vertex e = buckets[candidate];

            List<Vertex> visibleEdges = new List<Vertex>() { e };
            Vertex next = e;
            Vertex prev = e;
            bool intersected = true;
            do
            {
                Vertex next_prime = e.next;
                intersected = Intersects(p, interiorPoint, next.position, next_prime.position);
                if(intersected)
                    next_p
            } while (intersected);

        }
        */

        //foreach (GameObject hp in hull)
        //{
        //    hp.GetComponent<SpriteRenderer>().color = Color.green;
        //}
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
}


public class Polygon
{
    public List<Vertex> vertices { get; private set; }

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