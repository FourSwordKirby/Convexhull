using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointsController : MonoBehaviour {

    public bool IsRecordingMouseClicks;
    public List<Point> SpawnedPoints = new List<Point>();
    public HashSet<Vector2> PointLocations = new HashSet<Vector2>();
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

    public void SpawnRandomPoints(int count)
    {
        Debug.Log("Spawning " + count + " random points");
        Vector3 botLeft = new Vector3(MainCamera.pixelWidth / 10f, MainCamera.pixelHeight / 10f, Mathf.Abs(MainCamera.transform.position.z));
        Vector3 botLeftWorldLoc = MainCamera.ScreenToWorldPoint(botLeft);

        Vector3 topRight = new Vector3(9 * MainCamera.pixelWidth / 10f, 9 * MainCamera.pixelHeight / 10f, Mathf.Abs(MainCamera.transform.position.z));
        Vector3 topRightWorldLoc = MainCamera.ScreenToWorldPoint(topRight);

        Vector3 span = (topRightWorldLoc - botLeftWorldLoc);
        float std = Mathf.Min(span.x / 4, span.y / 4); 
        
        for (int i = 0; i < count; i++)
        {
            float x = MainCamera.transform.position.x + Algorithms.NextGaussianFloat() * std;
            float y = MainCamera.transform.position.y + Algorithms.NextGaussianFloat() * std;
            Vector2 loc = new Vector2(x, y);
            SpawnPoint(loc);
        }
    }

    public void SpawnPoint(Vector2 loc)
    {
        if (!PointLocations.Contains(loc))
        {
            PointLocations.Add(loc);

            Point p = Instantiate<Point>(PointPrefab, PointsContainer.transform);
            p.transform.position = loc;
            p.SetColor(InputPointColor);
            SpawnedPoints.Add(p);
        }
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

    public void RunRIConvexHull()
    {
        List<Vector2> points = SpawnedPoints.Select(p => (Vector2)p.transform.position).ToList();

        Algorithms.LineSideTestCount = 0;
        Algorithms.IntersectionTests = 0;

        List<int> hullIndices = Algorithms.ConvexHullBasicOnVectors(points);
        Debug.Log("Random Incremental convex hull returned in " + Algorithms.LastConvexHullTime + " seconds.");

        int lstCount = Algorithms.LineSideTestCount;
        int intCount = Algorithms.IntersectionTests;

        Debug.Log("Line side tests: " + lstCount
                  + " Intersection: " + intCount
                  + " Total ops:" + (lstCount + intCount));

        if (!Algorithms.IsConvex(points, hullIndices))
        {
            Debug.LogError("RI Convex Hull returned something that isn't convex!");
        }

        List<Vector2> hull = hullIndices.Select(i => (Vector2)SpawnedPoints[i].transform.position).ToList();
        DrawShape(hull, DefaultShapeOutlineWidth, DefaultShapeOutlineColor);
    }

    public void RunOSConvexHull()
    {
        List<Vector2> points = SpawnedPoints.Select(p => (Vector2)p.transform.position).ToList();

        Algorithms.LineSideTestCount = 0;
        Algorithms.IntersectionTests = 0;

        OutputSensitiveConvexHull osch = new OutputSensitiveConvexHull(points);
        List<int> hullIndices = osch.ComputeConvexHullIndices();
        Debug.Log("Output sensitive convex hull returned in " + osch.TotalTime + " seconds.");

        int lstCount = Algorithms.LineSideTestCount + osch.LineSideTestCount;
        int intCount = Algorithms.IntersectionTests;

        Debug.Log("Line side tests: " + lstCount
                  + " Intersection: " + intCount
                  + " Total ops:" + (lstCount + intCount) );

        if (!Algorithms.IsConvex(points, hullIndices))
        {
            Debug.LogError("OS Convex Hull returned something that isn't convex!");
        }


        List<Vector2> hull = hullIndices.Select(i => (Vector2)SpawnedPoints[i].transform.position).ToList();
        DrawShape(hull, DefaultShapeOutlineWidth, DefaultShapeOutlineColor);
    }

    public void SpawnPointsForModel(int modelIndex)
    {
        Debug.Log("Spawning points for model " + modelIndex);
        Model m = Instantiate<Model>(ModelPrefabs[modelIndex], transform);
        m.transform.position = (Vector2)Camera.main.transform.position;
        m.transform.localScale = (Camera.main.orthographicSize / 10f) * Vector3.one;
        StartCoroutine(SpawnPointsCoroutine(m));
    }

    public void ClearAllShapes()
    {
        foreach (Transform child in ShapesContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void ClearAllPoints()
    {
        foreach (Transform child in PointsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        SpawnedPoints.Clear();
        PointLocations.Clear();
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
