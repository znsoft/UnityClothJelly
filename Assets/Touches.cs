using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zPhys;

public class Touches : MonoBehaviour
{
    public GameObject BallPrefab;
    Transform lastHit;
    // Use this for initialization
    void Start()
    {
        Input.simulateMouseWithTouches = true;
    }

    // Update is called once per frame
    void Update()
    {
        TouchProcess();

    }


    bool isDownMouse = false;
    void TouchProcess()
    {
        if (Input.GetMouseButtonUp(0)) isDownMouse = false;
        if (Input.GetMouseButtonDown(0)) isDownMouse = true;

        if (!isDownMouse)
        {
            if (lastHit != null) lastHit.SendMessage("UnHit");
            lastHit = null;
            return;
        }
        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        var pos2D = new Vector2(pos.x, pos.y);

        var hit = Physics2D.Raycast(pos2D, Vector2.up);
        if (hit.collider == null)
        {
            if (lastHit != null) lastHit.SendMessage("UnHit", pos2D);
            lastHit = null;
            Instantiate(BallPrefab, pos, transform.rotation);

        }
        else
        {
            lastHit = hit.collider.transform;
            lastHit.SendMessage("Hit", pos2D);
        }
    }
}
