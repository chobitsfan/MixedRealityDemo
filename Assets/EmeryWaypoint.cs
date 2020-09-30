using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmeryWaypoint : MonoBehaviour
{
    public GameObject LocalWaypoint;
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
        /*ts -= Time.fixedDeltaTime;
        if (ts < 0)
        {
            ts = 1.5f;
            Vector3 tgt = player.transform.position + Random.onUnitSphere * 3;
            while ((tgt - rb.position).sqrMagnitude < 4)
            {
                tgt = player.transform.position + Random.onUnitSphere * 3;
            }
            rb.position = tgt;
        }*/
        if ((transform.position - emery.transform.position).sqrMagnitude < 1)
        {
            if (near)
            {
                near = false;
                Vector2 pos = Random.insideUnitCircle * 4;
                LocalWaypoint.transform.localPosition = new Vector3(-15, pos.y, pos.x);
                transform.position = LocalWaypoint.transform.position;
            }
            else
            {
                near = true;
                Vector2 pos = Random.insideUnitCircle * 0.5f;
                LocalWaypoint.transform.localPosition = new Vector3(-2, pos.y, pos.x);
                transform.position = LocalWaypoint.transform.position;
            }
        }
    }
    /*public void OnEmeryIntercept()
    {
        Debug.Log("OnEmeryIntercept" + near);
        if (near)
        {
            near = false;
            Vector2 pos = Random.insideUnitCircle * 5;
            transform.localPosition = new Vector3(-10, pos.y, pos.x);
        }
        else
        {
            near = true;
            Vector2 pos = Random.insideUnitCircle;
            transform.localPosition = new Vector3(-2, pos.y, pos.x);
        }
    }*/
}
