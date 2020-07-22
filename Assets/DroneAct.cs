using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class DroneAct : MonoBehaviour
{
    MAVLink.MavlinkParse mavlinkParse = new MAVLink.MavlinkParse();
    byte avoidAngle = 0;
    Socket mavSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    IPEndPoint myGCS = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 17500);    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {        
        Vector3 impact = collision.gameObject.transform.position - transform.position;
        Debug.DrawRay(transform.position, impact, Color.red, 5, false);
        Vector2 impact2d = new Vector2(impact.x, impact.z);
        Vector3 heading = -transform.right;
        Vector2 heading2d = new Vector2(heading.x, heading.z);
        float angle = -Vector2.SignedAngle(heading2d,  impact2d); //https://forum.unity.com/threads/vector2-signedangle.507058/
        if (angle < 0) angle += 360f;
        //Debug.Log(angle.ToString("F4"));
        avoidAngle = (byte)(angle / 2f);
        Debug.Log("avoid angle:" + avoidAngle);
        SendDistSensor(5, avoidAngle);
    }

    private void OnCollisionStay(Collision collision)
    {
        SendDistSensor(5, avoidAngle);
    }

    private void SendDistSensor(byte dist, byte orientation)
    {
        MAVLink.mavlink_distance_sensor_t msg = new MAVLink.mavlink_distance_sensor_t
        {
            min_distance = 1,
            max_distance = 300,
            current_distance = dist,
            orientation = orientation
        };
        byte[] pkt = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.DISTANCE_SENSOR, msg);
        mavSock.SendTo(pkt, myGCS);
    }
}
