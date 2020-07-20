using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    public GameObject[] asteroidType = new GameObject[3];
    float ts = 0;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        ts += Time.deltaTime;
        if (ts > 0.5f)
        {
            ts = 0;
            GameObject asteroid = GameObject.Instantiate(asteroidType[Random.Range(0, 2)], new Vector3(25, Random.Range(0f, 4f), Random.Range(-3f, 3f)), Quaternion.identity);
            asteroid.GetComponent<Rigidbody>().AddForce(new Vector3(-1, 0, 0) * Random.Range(2f, 10f), ForceMode.VelocityChange);
        }
    }
}
