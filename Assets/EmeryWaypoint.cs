using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmeryWaypoint : MonoBehaviour
{
    GameObject player;
    GameObject emery;
    bool near = false;
    //Rigidbody rb;
    //float ts = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        emery = GameObject.FindGameObjectWithTag("Emery");
    }

    void FixedUpdate()
    {
        if ((transform.position - emery.transform.position).sqrMagnitude < 1)
        {
            if (near)
            {
                near = false;
                Vector2 pos = Random.insideUnitCircle * 2;
                Vector3 localPos = new Vector3(-15, pos.y, pos.x);
                transform.position = player.transform.TransformPoint(localPos);
            }
            else
            {
                near = true;
                Vector2 pos = Random.insideUnitCircle * 0.5f;
                Vector3 localPos = new Vector3(-2, pos.y, pos.x);
                transform.position = player.transform.TransformPoint(localPos);
            }
        }
    }
}
