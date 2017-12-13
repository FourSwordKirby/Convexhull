using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Controller : MonoBehaviour {

    public int spawnCount;
    public GameObject point;

	// Use this for initialization
	void Start () {
        List<GameObject> points  = new List<GameObject>();

		for(int i = 0; i < spawnCount; i++)
        {
            GameObject p = Instantiate(point);

            float x = Random.Range(-10.0f, 10.0f);
            float y = Random.Range(-10.0f, 10.0f);

            p.transform.position = new Vector2(x, y);
            points.Add(p);
        }

        ConvexHullBasic(points);
	}
	

    void ConvexHullBasic(List<GameObject> p)
    {
        //Copy the list
        List<GameObject> points = new List<GameObject>();        
        for (int i = 0; i < p.Count; i++)
            points.Add(p[i]);

        Shuffle(points);

        List<GameObject> hull = new List<GameObject>();
        //Initialize the starting triangle
        for(int i = 0; i < 3; i++)
        {
            GameObject hullPoint = points[i];
            hull.Add(hullPoint);
        }

        Vector2 interiorPoint = hull.Select(x => x.transform.position).Aggregate((x, y) => x + y);
        interiorPoint = interiorPoint / 3.0f;
        GameObject center = Instantiate(point);
        center.name = "CenterPoint";
        center.transform.position = interiorPoint;
        center.GetComponent<SpriteRenderer>().color = Color.red;


        Dictionary<GameObject, GameObject> buckets = new Dictionary<GameObject, GameObject>();

        for (int i = 3; i < points.Count; i++)
        {
            GameObject candidatePoint = points[i];
            for(int j = 0; j < hull.Count; j++)
            {
                GameObject p1 = hull[j];
                GameObject p2 = j < hull.Count-1 ? hull[j + 1] : hull[0];

                if (Intersects(candidatePoint, center, p1, p2))
                {
                    buckets.Add(candidatePoint, p1);

                    //Checking that the line side test works
                    if (p1 == hull[0])
                        candidatePoint.GetComponent<SpriteRenderer>().color = Color.black;
                    if (p1 == hull[1])
                        candidatePoint.GetComponent<SpriteRenderer>().color = Color.cyan;
                    if (p1 == hull[2])
                        candidatePoint.GetComponent<SpriteRenderer>().color = Color.magenta;
                    break;
                }
            }
        }
        
        //Rest of the algorithm and build tent soon ...


        foreach (GameObject hp in hull)
        {
            hp.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    void Shuffle<GameObject>(IList<GameObject> list)
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
    public bool Intersects(GameObject p11, GameObject p12,
                                    GameObject p21, GameObject p22)
    {
        float x1 = p11.transform.position.x;
        float x2 = p12.transform.position.x;
        float x3 = p21.transform.position.x;
        float x4 = p22.transform.position.x;

        float y1 = p11.transform.position.y;
        float y2 = p12.transform.position.y;
        float y3 = p21.transform.position.y;
        float y4 = p22.transform.position.y;

        Matrix4x4 myMatrix = new Matrix4x4(new Vector4(x1, y1, 1.0f, 0.0f), new Vector4(x2, y2, 1.0f, 0.0f),
                                            new Vector4(-x3, -y3, 0.0f, 1.0f), new Vector4(-x4, -y4, 0.0f, 1.0f));

        Vector4 vec = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        Vector4 solution = myMatrix.inverse * vec;

        return solution[0] >= 0 && solution[1] >= 0 && solution[2] >= 0 && solution[3] >= 0;
    }
}

