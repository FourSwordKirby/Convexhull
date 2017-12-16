using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StephenSceneScript : MonoBehaviour {

    private PointsController PointsControllerRef;

    public static Color[] MyColors = new Color[] {
        Color.red,
        Color.cyan,
        Color.yellow,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.Lerp(Color.white, Color.red, .5f),
        Color.Lerp(Color.white, Color.yellow, .5f),
        Color.black,
        Color.Lerp(Color.white, Color.green, .5f),
        Color.Lerp(Color.black, Color.magenta, .5f)
    };

    // Use this for initialization
    void Start () {
        PointsControllerRef = GameObject.FindObjectOfType<PointsController>();
        OutputH = 3;
	}

    public int Stage = 0;
    public int OutputH;
    public List<Vector2> Points;
    public OutputSensitiveConvexHull Alg;
    public int LastIndex;
    public List<int> TangentIndices;
    public bool IsDone = false;

    // Update is called once per frame
    void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Current stage: " + Stage);

            switch(Stage)
            {
                case 0:
                    PointsControllerRef.ClearAllShapes();
                    foreach (Point p in PointsControllerRef.SpawnedPoints)
                    {
                        p.SetColor(Color.black);
                        p.transform.localScale = new Vector3(1, 1, 1);
                    }

                    Points = PointsControllerRef.SpawnedPoints.Select(p => (Vector2)p.transform.position).ToList();
                    Alg = new OutputSensitiveConvexHull(Points);

                    Debug.Log("Current H is: " + OutputH);
                    Alg.CreateSubproblems(OutputH);
                    for (int subproblemIndex = 0; subproblemIndex < Alg.SubproblemPointsIndices.Count; ++subproblemIndex)
                    {
                        List<int> subproblem = Alg.SubproblemPointsIndices[subproblemIndex];
                        foreach (int pointIndex in subproblem)
                        {
                            PointsControllerRef.SpawnedPoints[pointIndex].SetColor(MyColors[subproblemIndex % MyColors.Length]);
                        }
                    }
                    ++Stage;
                    break;

                case 1:
                    foreach (Point p in PointsControllerRef.SpawnedPoints)
                    {
                        p.SetColor(Color.gray);
                    }

                    Alg.ComputeAllSubhulls();
                    
                    for (int subproblemIndex = 0; subproblemIndex < Alg.SubproblemPointsIndices.Count; ++subproblemIndex)
                    {
                        List<int> subproblem = Alg.SubproblemPointsIndices[subproblemIndex];
                        List<int> subhullIndices = Alg.IndiciesForSubhulls[subproblemIndex].Select(i => subproblem[i]).ToList();
                        List<Vector2> subhull = Alg.Subhulls[subproblemIndex];
                        foreach (int pointIndex in subhullIndices)
                        {
                            PointsControllerRef.SpawnedPoints[pointIndex].SetColor(MyColors[subproblemIndex % MyColors.Length]);
                        }
                        PointsControllerRef.DrawShape(subhull, 0.1f, MyColors[subproblemIndex % MyColors.Length]);
                    }

                    ++Stage;
                    
                    break;

                case 2:
                    Alg.StartWalk();
                    LastIndex = Alg.CurrentPointIndex;
                    PointsControllerRef.SpawnedPoints[LastIndex].transform.localScale = 2 * Vector3.one;
                    PointsControllerRef.SpawnedPoints[LastIndex].SetColor(Color.black);
                    ++Stage;
                    break;

                case 3:
                    foreach (Point p in PointsControllerRef.SpawnedPoints)
                    {
                        p.SetColor(Color.black);
                        p.transform.localScale = Vector3.one;
                    }

                    List<Vector2> line;
                    bool ret = Alg.StepWalk();
                    if (ret)
                    {
                        IsDone = true;

                        line = new List<Vector2>() { PointsControllerRef.SpawnedPoints[LastIndex].transform.position,
                                                               PointsControllerRef.SpawnedPoints[Alg.CurrentPointIndex].transform.position };
                        PointsControllerRef.DrawShape(line, 0.3f, Color.black);

                        ++Stage;
                        break;
                    }

                    if (Alg.HullIndex >= OutputH + 1)
                    {
                        Debug.Log("No convex hull for current size.");
                        OutputH *= OutputH;
                        Stage = 0;
                        break;
                    }

                    foreach (int index in Alg.TangentIndices)
                    {
                        PointsControllerRef.SpawnedPoints[index].transform.localScale = 2 * Vector3.one;
                        PointsControllerRef.SpawnedPoints[index].SetColor(Color.red);
                    }
                    PointsControllerRef.SpawnedPoints[Alg.CurrentPointIndex].SetColor(Color.blue);
                    line = new List<Vector2>() { PointsControllerRef.SpawnedPoints[LastIndex].transform.position,
                                                               PointsControllerRef.SpawnedPoints[Alg.CurrentPointIndex].transform.position };
                    PointsControllerRef.DrawShape(line, 0.3f, Color.black);

                    LastIndex = Alg.CurrentPointIndex;


                    break;

                default:
                    Debug.Log("No more stages left.");
                    PointsControllerRef.ClearAllShapes();
                    foreach (Point p in PointsControllerRef.SpawnedPoints)
                    {
                        p.SetColor(Color.gray);
                        p.transform.localScale = Vector3.one;
                    }

                    foreach(int hullIndex in Alg.CompleteHull)
                    {
                        PointsControllerRef.SpawnedPoints[hullIndex].SetColor(Color.red);
                        PointsControllerRef.SpawnedPoints[hullIndex].transform.localScale = 2 * Vector3.one;
                    }
                    PointsControllerRef.DrawShape(Alg.CompleteHull.Select(index => (Vector2)PointsControllerRef.SpawnedPoints[index].transform.position).ToList(),
                        0.3f, Color.black);
                    break;
            }
        }
	}
}
