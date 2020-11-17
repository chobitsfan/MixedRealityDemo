using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    Vector3 axis;
    // Start is called before the first frame update
    void Start()
    {
        axis = Random.onUnitSphere;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(axis, 0.1f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
    }
}
