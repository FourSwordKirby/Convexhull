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

        System.Diagnostics.Stopwatch Watch = new System.Diagnostics.Stopwatch();
        Watch.Start();
        List<int> hull = Algorithms.ConvexHullBasic(points);
        Watch.Stop();
        Debug.Log("Search completed in: " + (Watch.ElapsedMilliseconds / 1000f) + " sec");

        print(hull.Count);

        foreach (GameObject p in hull.Select(x => points[x]))
        {
            p.GetComponent<Point>().DisplayColor = Color.red;
            p.GetComponent<SpriteRenderer>().sortingOrder = 10;
        }
    }
}

