using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAGun : MonoBehaviour
{
    public GameObject bullet;    
    public GameObject gunStand;
    public GameObject gunBarrel;
    public GameObject shootPoint;
    public GameObject fireFx;
    GameObject player = null;
    float ts = 3f;
    bool play = false;
    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        ts = Random.Range(2f, 4f);
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
            ts -= Time.deltaTime;
            if (ts < 0)
            {
                ts = Random.Range(2f, 4f);
                GameObject bb = GameObject.Instantiate(bullet, shootPoint.transform.position, Quaternion.LookRotation(player.transform.position - transform.position));
                bb.GetComponent<Rigidbody>().AddForce((player.transform.position - transform.position).normalized, ForceMode.VelocityChange);
                fireFx.GetComponent<ParticleSystem>().Play();
            }
        }
        gunStand.transform.LookAt(new Vector3(player.transform.position.x, 0, player.transform.position.z));
        gunBarrel.transform.LookAt(player.transform);
    }
}
