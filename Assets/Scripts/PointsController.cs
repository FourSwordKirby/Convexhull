using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointsController : MonoBehaviour {

    public bool IsRecordingMouseClicks;
    public List<Point> SpawnedPoints = new List<Point>();
    public Color InputPointColor = Color.black;
    public Material ShapeOutlineMaterial;
    public float ShapeOutlineWidth = 0.1f;

    public Point PointPrefab;
    public List<Model> ModelPrefabs = new List<Model>();

    public Camera MainCamera;
    public GameObject PointsContainer;
    public GameObject ShapesContainer;

    private void Awake()
    {
        PointsContainer = transform.Find("Points Container").gameObject;
        ShapesContainer = transform.Find("Shapes Container").gameObject;
    }

    // Use this for initialization
    void Start () {
        MainCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {

        if (IsRecordingMouseClicks)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Click Registered.");
                SpawnPointAtMouseLoc();
            }
        }
    }

    public void ToggleMouseRecording(bool b)
    {
        IsRecordingMouseClicks = b;
    }

    public void SpawnPointAtMouseLoc()
    {
        Vector3 mouseLoc = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(MainCamera.transform.position.z));
        Vector3 worldLoc = MainCamera.ScreenToWorldPoint(mouseLoc);
        SpawnPoint(worldLoc);
    }

    public void SpawnPoint(Vector3 loc)
    {
        Point p = Instantiate<Point>(PointPrefab, PointsContainer.transform);
        p.transform.position = loc;
        p.SetColor(InputPointColor);
        SpawnedPoints.Add(p);
    }

    /// <summary>
    /// Will draw a shape in order of points, closing the shape.
    /// </summary>
    public LineRenderer DrawShape(List<Vector3> points)
    {
        GameObject obj = new GameObject("Shape");
        obj.transform.SetParent(ShapesContainer.transform);
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        lr.positionCount = points.Count;
        for (int i = 0; i < points.Count; ++i)
        {
            lr.SetPosition(i, points[i]);
        }
        lr.loop = true;

        lr.material = ShapeOutlineMaterial;
        lr.startWidth = ShapeOutlineWidth;
        lr.endWidth = ShapeOutlineWidth;
        
        return lr;
    }

    public void DrawShapeForLayer(int layerIndex)
    {
        DrawShape(SpawnedPoints.Select(p => p.transform.position).ToList());
    }

    public void SpawnPointsForModel(int modelIndex)
    {
        Debug.Log("Spawning points for model " + modelIndex);
        Model m = Instantiate<Model>(ModelPrefabs[modelIndex], transform);
        StartCoroutine(SpawnPointsCoroutine(m));
    }

    private IEnumerator SpawnPointsCoroutine(Model m)
    {
        yield return new WaitForEndOfFrame();
        List<Vector3> points = m.GetVertices();
        foreach (Vector3 p in points)
        {
            SpawnPoint(p);
        }

        m.gameObject.SetActive(false);
        Debug.Log(points.Count + " points were spawned for " + m.Name);
    }
    
}
