using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    private Camera CameraRef;

    private void Awake()
    {
        CameraRef = GetComponent<Camera>();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float delta = Input.GetAxis("Mouse ScrollWheel");
        CameraRef.orthographicSize += delta;

    }
}
