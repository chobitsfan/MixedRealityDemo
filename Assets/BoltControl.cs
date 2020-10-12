using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltControl : MonoBehaviour
{
    public float speed;
    public GameObject explosion;
    private Rigidbody rb;
    float ts = 3f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
        rb.AddTorque(transform.forward * 5, ForceMode.VelocityChange);
    }

    private void Reset()
    {
        speed = 5f;
    }

    // Update is called once per frame
    void Update()
    {
        ts -= Time.deltaTime;
        if (ts < 0) Boom();
    }

    void OnCollisionEnter(Collision collision)
    {
        Boom();
    }

    void Boom()
    {
        Destroy(GameObject.Instantiate(explosion, transform.position, Quaternion.identity), 1);
        Destroy(gameObject);
    }
}
