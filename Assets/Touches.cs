using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zPhys;

public class Touches : MonoBehaviour {
    public GameObject BallPrefab;
    // Use this for initialization
    void Start () {
        Input.simulateMouseWithTouches = true;
    }
	
	// Update is called once per frame
	void Update () {
        TouchProcess();

    }

    void TouchProcess()
    {
        
        if (!Input.GetMouseButtonDown(0)) return;
        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        var pos2D = new Vector2(pos.x, pos.y);
        var hit = Physics2D.Raycast(pos2D, Vector2.zero);
        if (hit.collider == null)
            Instantiate(BallPrefab, pos, transform.rotation);
        else hit.collider.transform.SendMessage("Hit", pos2D);

    }
}
