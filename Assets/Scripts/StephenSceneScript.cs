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
        Color.white,
        Color.Lerp(Color.white, Color.red, .5f),
        Color.Lerp(Color.white, Color.yellow, .5f),
        Color.black,
        Color.Lerp(Color.white, Color.green, .5f),
        Color.Lerp(Color.black, Color.magenta, .5f)
    };

    // Use this for initialization
    void Start () {
        PointsControllerRef = GameObject.FindObjectOfType<PointsController>();
	}

    public int Stage = 0;
    public List<Vector2> Points;
    public OutputSensitiveConvexHull Alg;
    public int LastIndex;
    public List<int> TangentIndices;

    // Update is called once per frame
    void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Current stage: " + Stage);

            switch(Stage)
            {
                case 0:
                    Points = PointsControllerRef.SpawnedPoints.Select(p => (Vector2)p.transform.position).ToList();
                    Alg = new OutputSensitiveConvexHull(Points);
                    Alg.CreateSubproblems(3);
                    for (int subproblemIndex = 0; subproblemIndex < Alg.SubproblemPointsIndices.Count; ++subproblemIndex)
                    {
                        List<int> subproblem = Alg.SubproblemPointsIndices[subproblemIndex];
                        foreach (int pointIndex in subproblem)
                        {
                            PointsControllerRef.SpawnedPoints[pointIndex].SetColor(MyColors[subproblemIndex]);
                        }
                    }
                    ++Stage;
                    break;

                case 1:
                    foreach (Point p in PointsControllerRef.SpawnedPoints)
                    {
                        p.SetColor(Color.grey);
                    }

                    Alg.ComputeAllSubhulls();
                    for (int subproblemIndex = 0; subproblemIndex < Alg.SubproblemPointsIndices.Count; ++subproblemIndex)
                    {
                        List<int> subproblem = Alg.SubproblemPointsIndices[subproblemIndex];
                        List<int> subhullIndices = Alg.IndiciesForSubhulls[subproblemIndex].Select(i => subproblem[i]).ToList();
                        List<Vector2> subhull = Alg.Subhulls[subproblemIndex];
                        foreach (int pointIndex in subhullIndices)
                        {
                            PointsControllerRef.SpawnedPoints[pointIndex].SetColor(MyColors[subproblemIndex]);
                        }
                        PointsControllerRef.DrawShape(subhull, 0.1f, MyColors[subproblemIndex]);
                    }
                    ++Stage;
                    break;

                case 2:
                    Alg.StartWalk();
                    LastIndex = Alg.CurrentPointIndex;
                    PointsControllerRef.SpawnedPoints[LastIndex].transform.localScale *= 2;
                    PointsControllerRef.SpawnedPoints[LastIndex].SetColor(Color.black);
                    ++Stage;
                    break;

                case 3:
                    Alg.StepWalk();
                    foreach (int index in Alg.TangentIndices)
                    {
                        PointsControllerRef.SpawnedPoints[index].transform.localScale *= 2;
                        PointsControllerRef.SpawnedPoints[index].SetColor(Color.red);
                    }
                    PointsControllerRef.SpawnedPoints[Alg.CurrentPointIndex].SetColor(Color.blue);
                    List<Vector2> line = new List<Vector2>() { PointsControllerRef.SpawnedPoints[LastIndex].transform.position,
                                                               PointsControllerRef.SpawnedPoints[Alg.CurrentPointIndex].transform.position };
                    PointsControllerRef.DrawShape(line, 0.3f, Color.black);

                    LastIndex = Alg.CurrentPointIndex;

                    break;

                default:
                    Debug.Log("No more stages left.");
                    break;
            }
        }
	}
}
