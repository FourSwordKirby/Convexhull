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
    public Color DefaultShapeOutlineColor = Color.red;
    public float DefaultShapeOutlineWidth = 0.1f;

    public Point PointPrefab;
    public List<Model> ModelPrefabs = new List<Model>();

    public Camera MainCamera;
    public GameObject PointsContainer;
    public GameObject ShapesContainer;

    private void Awake()
    {
        PointsContainer = transform.Find("Points Container").gameObject;
        ShapesContainer = transform.Find("Shapes Container").gameObject;
        SpawnedPoints.AddRange(ShapesContainer.transform.GetComponentsInChildren<Point>());
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

    public void SpawnPoint(Vector2 loc)
    {
        Point p = Instantiate<Point>(PointPrefab, PointsContainer.transform);
        p.transform.position = loc;
        p.SetColor(InputPointColor);
        SpawnedPoints.Add(p);
    }

    public void SpawnPoint(Vector3 loc3d)
    {
        SpawnPoint(new Vector2(loc3d.x, loc3d.y));
    }

    /// <summary>
    /// Will draw a shape in order of points, closing the shape.
    /// </summary>
    public LineRenderer DrawShape(List<Vector2> points, float width, Color color)
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
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        
        return lr;
    }

    public void DrawShapeForLayer(int layerIndex)
    {
        DrawShape(SpawnedPoints.Select(p => (Vector2)p.transform.position).ToList(), DefaultShapeOutlineWidth, DefaultShapeOutlineColor);
    }

    public void SpawnPointsForModel(int modelIndex)
    {
        Debug.Log("Spawning points for model " + modelIndex);
        Model m = Instantiate<Model>(ModelPrefabs[modelIndex], transform);
        StartCoroutine(SpawnPointsCoroutine(m));
    }

    public void ClearAllShapes()
    {
        foreach (Transform child in ShapesContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator SpawnPointsCoroutine(Model m)
    {
        yield return new WaitForEndOfFrame();
        List<Vector2> points = m.GetVertices();
        foreach (Vector2 p in points)
        {
            SpawnPoint(p);
        }

        m.gameObject.SetActive(false);
        Debug.Log(points.Count + " points were spawned for " + m.Name);
    }
    
}
