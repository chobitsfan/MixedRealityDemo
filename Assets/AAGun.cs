using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAGun : MonoBehaviour
{
    public GameObject bullet;
    public GameObject player;
    float ts = 0;
    bool play = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            play = !play;
        }
        if (play)
        {
            ts += Time.deltaTime;
            if (ts > 3)
            {
                ts = 0;
                GameObject bb = GameObject.Instantiate(bullet, transform.position, Quaternion.identity);
                bb.GetComponent<Rigidbody>().AddForce((player.transform.position - transform.position).normalized * 3, ForceMode.VelocityChange);
            }
        }
    }
}
