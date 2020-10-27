using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rings : MonoBehaviour
{
    public GameObject Player;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = Player.transform.position - Player.transform.right * 6;
            transform.forward = Player.transform.forward;
        }
    }
}
