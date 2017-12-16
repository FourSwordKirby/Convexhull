using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Controller : MonoBehaviour {

    public int spawnCount;
    public GameObject point;

    public RandomIncrementalConvexHull ch;
    public PointsController PointsControllerRef;

	// Use this for initialization
	void Start () {
        PointsControllerRef = GameObject.FindObjectOfType<PointsController>();

        List<GameObject> points  = new List<GameObject>();

		for(int i = 0; i < spawnCount; i++)
        {
            GameObject p = Instantiate(point);

            p.name = "Point" + i;

            float x = Random.Range(-10.0f, 10.0f);
            float y = Random.Range(-10.0f, 10.0f);

            p.transform.position = new Vector2(x, y);
            p.transform.localScale = 3.0f * Vector3.one;
            points.Add(p);
        }

        ch = new RandomIncrementalConvexHull(points.Select(p => (Vector2)p.transform.position).ToList());
        GameObject obj = new GameObject("Hull");

        Debug.Log("Current hull vertices: " + ch.hull.vertices.Count);
        foreach (Vector2 p in ch.hull.vertices.Select(x => (Vector2)x.position))
        {
            GameObject pointObject = Instantiate(point, obj.transform);
            pointObject.transform.position = p;
            pointObject.transform.localScale = 3 * Vector3.one;
            pointObject.GetComponent<Point>().DisplayColor = Color.red;
            pointObject.GetComponent<SpriteRenderer>().sortingOrder = 10;
        }

        //List<int> result = Algorithms.ConvexHullBasic(points);
        //for (int i = 0; i < result.Count; ++i)
        //{
        //    Vector2 a = points[result[i]].transform.position;
        //    Vector2 b = points[result[(i + 1) % result.Count]].transform.position;
        //    for (int j = 0; j < points.Count; ++j)
        //    {
        //        if (!(j == result[i] || j == result[(i + 1) % result.Count]))
        //        {
        //            Vector2 p = points[j].transform.position;
        //            if (Algorithms.isLeft(a, b, p))
        //            {
        //                Debug.Log("Failed on a: " + a + " b: " + b + " p: " + p);
        //            }
        //        }
        //    }

        //}

        //print(result.Count);

        //foreach (GameObject p in result.Select(x => points[x]))
        //{
        //    p.GetComponent<Point>().DisplayColor = Color.red;
        //    p.GetComponent<SpriteRenderer>().sortingOrder = 10;
        //}
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PointsControllerRef.ClearAllShapes();
            Debug.Log("Space");
            GameObject existingHull = GameObject.Find("Hull");
            if(existingHull)
            {
                Destroy(existingHull);
            }
            
            Vector2 candidate = ch.Step();
            
            GameObject obj = new GameObject("Hull");

            Debug.Log("Current hull vertices: " + ch.hull.vertices.Count);

            List<Vector2> finalHull = new List<Vector2>();

            Vertex tracer = ch.hull.vertices[0];
            finalHull.Add(tracer.position);

            while (tracer.next != ch.hull.vertices[0])
            {
                tracer = tracer.next;
                finalHull.Add(tracer.position);
            }
            
            foreach (Vector2 p in finalHull)
            {
                GameObject pointObject = Instantiate(point, obj.transform);
                pointObject.transform.position = p;
                pointObject.transform.localScale = 3 * Vector3.one;
                pointObject.GetComponent<Point>().DisplayColor = Color.red;
                pointObject.GetComponent<SpriteRenderer>().sortingOrder = 10;
            }
            PointsControllerRef.DrawShape(finalHull, 0.1f, Color.red);

            GameObject pointObject2 = Instantiate(point, obj.transform);
            pointObject2.transform.position = candidate;
            pointObject2.transform.localScale = 9 * Vector3.one;
            pointObject2.GetComponent<Point>().DisplayColor = Color.black;
            pointObject2.GetComponent<SpriteRenderer>().sortingOrder = 20;

        }
    }
}

