using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour {

    public PointsController PointsControllerRef;

	// Use this for initialization
	void Start () {
        PointsControllerRef = GameObject.FindObjectOfType<PointsController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetRecordingMouseClick(bool b)
    {
        PointsControllerRef.IsRecordingMouseClicks = b;
    }


    public void DrawShape()
    {
        PointsControllerRef.DrawShapeForLayer(0);
    }

    public void AddModel()
    {
        PointsControllerRef.SpawnPointsForModel(0);
    }
}
