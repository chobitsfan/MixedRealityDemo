using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltControl : MonoBehaviour
{
    public float speed;
    public GameObject explosion;
    private Rigidbody rb;
    float ts = 4f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        ts -= Time.deltaTime;
        if (ts < 0) Boom();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Emery")) Boom();
    }

    void Boom()
    {
        Destroy(GameObject.Instantiate(explosion, transform.position, Quaternion.identity), 1);
        Destroy(gameObject);
    }
}
