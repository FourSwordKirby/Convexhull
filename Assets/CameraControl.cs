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

    private Vector3 DragOrigin;
    private float DragSpeed = 0.5f;
    private float ScrollSpeed = 2f;

	// Update is called once per frame
	void Update () {
        float delta = Input.GetAxis("Mouse ScrollWheel");
        CameraRef.orthographicSize -= ScrollSpeed * delta;

        if (Input.GetMouseButtonDown(2))
        {
            DragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(2)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - DragOrigin);
        Vector3 move = new Vector3(pos.x * DragSpeed, pos.y * DragSpeed, 0);

        transform.Translate(move, Space.World);

    }
}
