using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public GameObject explosionEffect;
    GameStage stage;
    float endTs = 5;
    // Start is called before the first frame update
    void Start()
    {
        stage = GameObject.FindGameObjectWithTag("Stage").GetComponent<GameStage>();
    }

    // Update is called once per frame
    void Update()
    {
        endTs -= Time.deltaTime;
        if (endTs < 0)
        {
            GameObject.Destroy(gameObject);
            GameObject explosion = GameObject.Instantiate(explosionEffect, transform.position, Quaternion.identity);
            GameObject.Destroy(explosion, 1);
        }
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, 1))
        {
            stage.Warning();
        }
        else
        {
            stage.Warning(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = false; 
            }
            GameObject explosion = GameObject.Instantiate(explosionEffect, transform.position, Quaternion.identity);
            GameObject.Destroy(explosion, 1);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameObject.Destroy(gameObject);
            stage.Warning(false);
        }
    }
}
