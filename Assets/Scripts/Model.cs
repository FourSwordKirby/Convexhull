using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Model : MonoBehaviour {

    public string Name;

    public MeshFilter[] ContainedMeshFilters;

    private void Awake()
    {
        ContainedMeshFilters = GetComponentsInChildren<MeshFilter>();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public List<Vector2> GetVertices()
    {
        // Mesh vertices must first be transformed by the owning Transform.
        // Then they must be transformed by this Model instance's Transform.
        return ContainedMeshFilters.SelectMany(mf => 
            mf.mesh.vertices.Select(v => {
                Vector3 worldSpacePoint = mf.transform.TransformPoint(v);
                return new Vector2(worldSpacePoint.x, worldSpacePoint.y);
            })
        ).ToList();
    }
}
