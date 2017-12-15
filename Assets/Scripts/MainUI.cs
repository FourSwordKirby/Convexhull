using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour {

    public PointsController PointsControllerRef;

    private RectTransform ControlPanel;
    private Text SpawnedPointsText;

	// Use this for initialization
	void Start () {
        PointsControllerRef = GameObject.FindObjectOfType<PointsController>();

        ControlPanel = transform.Find("Control Panel").GetComponent<RectTransform>();
        SpawnedPointsText = ControlPanel.Find("Spawned Points Text").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        SpawnedPointsText.text = "Spawned Points: " + PointsControllerRef.SpawnedPoints.Count;
	}

    public void SetRecordingMouseClick(bool b)
    {
        PointsControllerRef.IsRecordingMouseClicks = b;
    }

    public void RunRIConvexHull()
    {
        PointsControllerRef.RunRIConvexHull();
    }

    public void RunOSConvexHull()
    {
        PointsControllerRef.RunOSConvexHull();
    }

    public void ClearPoints()
    {
        PointsControllerRef.ClearAllPoints();
    }

    public void SpawnRandomPoints()
    {
        PointsControllerRef.SpawnRandomPoints(100);
    }

    public void ClearShapes()
    {
        PointsControllerRef.ClearAllShapes();
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
