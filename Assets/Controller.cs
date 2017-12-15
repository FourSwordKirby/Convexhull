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

        Algorithms.ConvexHullBasic(points);
	}
	

    


}

