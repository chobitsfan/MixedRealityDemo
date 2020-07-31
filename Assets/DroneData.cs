using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class DroneData : MonoBehaviour
{
    Thread thread;
    bool gogo = true;
    bool gotPos = false;
    bool gotAtt = false;
    Vector3 pos = Vector3.zero;
    Quaternion att = Quaternion.identity;
    OptitrackRigidBody rigidBody;
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = gameObject.GetComponent<OptitrackRigidBody>();
        thread = new Thread(new ThreadStart(RecvData));
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (!rigidBody.updated)
        {
            transform.localPosition = pos;
            transform.localRotation = att;
        }
    }

    void OnDestroy()
    {
        gogo = false;        
        thread.Join();
    }


    void RecvData()
    {
        IPEndPoint myGCS = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 17500);
        byte[] buf = new byte[MAVLink.MAVLINK_MAX_PACKET_LEN];
        MAVLink.MavlinkParse mavlinkParse = new MAVLink.MavlinkParse();
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sock.ReceiveTimeout = 1000;
        sock.Bind(new IPEndPoint(IPAddress.Loopback, 17501));
        while (gogo)
        {
            int recvBytes = 0;
            try
            {
                recvBytes = sock.Receive(buf);
            }
            catch (SocketException)
            {

            }
            if (recvBytes > 0)
            {
                MAVLink.MAVLinkMessage msg = mavlinkParse.ReadPacket(buf);
                if (msg != null && msg.data != null)
                {
                    System.Type msg_type = msg.data.GetType();
                    //Debug.Log("recv "+msg_type);
                    if (msg_type == typeof(MAVLink.mavlink_heartbeat_t))
                    {
                        if (!gotPos)
                        {
                            MAVLink.mavlink_command_long_t msgOut = new MAVLink.mavlink_command_long_t()
                            {
                                target_system = 0,
                                command = (ushort)MAVLink.MAV_CMD.SET_MESSAGE_INTERVAL,
                                param1 = (float)MAVLink.MAVLINK_MSG_ID.LOCAL_POSITION_NED,
                                param2 = 100000
                            };
                            byte[] data = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msgOut);
                            sock.SendTo(data, myGCS);
                        }
                        if (!gotAtt)
                        {
                            MAVLink.mavlink_command_long_t msgOut = new MAVLink.mavlink_command_long_t()
                            {
                                target_system = 0,
                                command = (ushort)MAVLink.MAV_CMD.SET_MESSAGE_INTERVAL,
                                param1 = (float)MAVLink.MAVLINK_MSG_ID.ATTITUDE_QUATERNION,
                                param2 = 100000
                            };
                            byte[] data = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msgOut);
                            sock.SendTo(data, myGCS);
                        }
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_local_position_ned_t))
                    {
                        if (!gotPos)
                        {
                            Debug.Log("local_position_ned received");
                        }
                        gotPos = true;
                        var data = (MAVLink.mavlink_local_position_ned_t)msg.data;
                        pos = new Vector3(-data.x, -data.z, data.y);
                        //Debug.Log("recv local_position_ned " + pos.ToString("F4"));
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_attitude_quaternion_t))
                    {
                        if(!gotAtt)
                        {
                            Debug.Log("attitude_quaternion received");
                        }
                        gotAtt = true;
                        var data = (MAVLink.mavlink_attitude_quaternion_t)msg.data;
                        att = new Quaternion(-data.q2, -data.q4, data.q3, -data.q1);
                    }
                }
            }
        }
    }
}
