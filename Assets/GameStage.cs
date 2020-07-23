using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    public GameObject[] asteroidType = new GameObject[3];
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
            if (ts > 0.5)
            {
                ts = 0;
                GameObject asteroid = GameObject.Instantiate(asteroidType[Random.Range(0, 2)], new Vector3(5, Random.Range(0f, 4f), Random.Range(-4f, 4f)), Quaternion.identity);
                asteroid.GetComponent<Rigidbody>().AddForce(new Vector3(-1, 0, 0) * Random.Range(2f, 5f), ForceMode.VelocityChange);
            }
        }
    }
}
