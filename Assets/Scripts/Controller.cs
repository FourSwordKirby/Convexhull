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

            p.name = "Point" + i;

            float x = Random.Range(-10.0f, 10.0f);
            float y = Random.Range(-10.0f, 10.0f);

            p.transform.position = new Vector2(x, y);
            p.transform.localScale = 3.0f * Vector3.one;
            points.Add(p);
        }

        List<int> result = Algorithms.ConvexHullBasic(points);
        for (int i = 0; i < result.Count; ++i)
        {
            Vector2 a = points[result[i]].transform.position;
            Vector2 b = points[result[(i + 1) % result.Count]].transform.position;
            for (int j = 0; j < points.Count; ++j)
            {
                if (!(j == result[i] || j == result[(i + 1) % result.Count]))
                {
                    Vector2 p = points[j].transform.position;
                    if (Algorithms.isLeft(a, b, p))
                    {
                        Debug.Log("Returned result is not convex!");
                        Debug.Log("Failed on a: " + a + " b: " + b + " p: " + p);
                    }
                }
            }

        }

        print(result.Count);

        foreach (GameObject p in result.Select(x => points[x]))
        {
            p.GetComponent<Point>().DisplayColor = Color.red;
            p.GetComponent<SpriteRenderer>().sortingOrder = 10;
        }
    }
}

