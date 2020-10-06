using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMissileControl : MonoBehaviour
{
    public GameObject explosion;
    float ts = 4f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ts -= Time.deltaTime;
        if (ts < 0) Boom();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Boom();
    }

    void Boom()
    {
        Destroy(GameObject.Instantiate(explosion, transform.position, Quaternion.identity), 1);
        Destroy(gameObject);
    }
}
