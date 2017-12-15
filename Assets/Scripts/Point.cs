using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour {

    public Color DisplayColor = Color.white;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color c)
    {
        DisplayColor = c;
    }
	
	// Update is called once per frame
	void Update () {
        sr.color = DisplayColor;
	}
}
