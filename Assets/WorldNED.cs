using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNED : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("WorldNorth"))
        {
            //Debug.Log(PlayerPrefs.GetInt("WorldNorth"));
            transform.localEulerAngles = new Vector3(0, PlayerPrefs.GetInt("WorldNorth"), 0);
        }
        //Debug.Log(transform.localEulerAngles);
    }
    private void OnDestroy()
    {
        PlayerPrefs.SetInt("WorldNorth", (int)transform.localEulerAngles.y);
    }
}
