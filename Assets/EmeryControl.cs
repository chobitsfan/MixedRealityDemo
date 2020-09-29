using System.Collections;

using System.Collections.Generic;
using UnityEngine;

public class EmeryControl : MonoBehaviour
{
    public GameObject Missile;
    GameObject player;
    //Rigidbody rb;
    float ts = 5f;
    bool missileLoaded = true;
    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (missileLoaded)
        {
            if (Vector3.Angle(transform.forward, player.transform.position-transform.position) < 20)
            {
                GameObject missile = GameObject.Instantiate(Missile, transform.position - transform.up * 0.1f, Quaternion.LookRotation(transform.forward));
                SparseDesign.ControlledFlight.MissileSupervisor missileSupervisor = missile.GetComponent<SparseDesign.ControlledFlight.MissileSupervisor>();
                missileSupervisor.m_guidanceSettings.m_target = player;
                //missile.GetComponent<Rigidbody>().AddForce(-transform.up * 0.5f, ForceMode.VelocityChange);
                //missileSupervisor.StartLaunchSequence();
                missileLoaded = false;
            }
        }
        else
        {
            ts -= Time.fixedDeltaTime;
            if (ts < 0)
            {
                ts = 5f;
                missileLoaded = true;
            }
        }
    }
}
