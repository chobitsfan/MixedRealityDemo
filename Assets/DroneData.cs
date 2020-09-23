using System;
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
    bool newPos = false;
    bool newAtt = false;
    Vector3 pos = Vector3.zero;
    Quaternion att = Quaternion.identity;
    //long recv_intvl = 0;
    Rigidbody rb;
    bool got_hb = false;
    // Start is called before the first frame update
    void Start()
    {
        thread = new Thread(new ThreadStart(RecvData));
        thread.Start();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (newPos)
        {
            newPos = false;
            rb.MovePosition(pos);
        }
        if (newAtt)
        {
            newAtt = false;
            rb.MoveRotation(att);
        }
    }

#if false
    // Update is called once per frame
    void Update()
    {
        if (gotAtt && gotPos)
        {
            transform.localPosition = pos;
            transform.localRotation = att;
            /*if (recv_intvl > 0)
            {
                Debug.Log("recv_intvl " + recv_intvl);
                recv_intvl = 0;
            }
            else
            {
                Debug.Log("no update");
            }*/
        }
    }
#endif

    void OnDestroy()
    {
        gogo = false;        
        thread.Join();
    }


    void RecvData()
    {
        //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //stopwatch.Start();
        //long recv_ts = 0;
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint myGCS = (EndPoint)sender;
        byte[] buf = new byte[MAVLink.MAVLINK_MAX_PACKET_LEN];
        MAVLink.MavlinkParse mavlinkParse = new MAVLink.MavlinkParse();
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sock.ReceiveTimeout = 1000;
        sock.Bind(new IPEndPoint(IPAddress.Any, 17500));
        while (gogo)
        {
            int recvBytes = 0;
            try
            {
                recvBytes = sock.ReceiveFrom(buf, ref myGCS);
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
                        if (!got_hb)
                        {
                            got_hb = true;
                            Debug.Log("got heartbeat");
                        }
                        if (!gotPos)
                        {
                            MAVLink.mavlink_command_long_t msgOut = new MAVLink.mavlink_command_long_t()
                            {
                                target_system = 0,
                                command = (ushort)MAVLink.MAV_CMD.SET_MESSAGE_INTERVAL,
                                param1 = (float)MAVLink.MAVLINK_MSG_ID.LOCAL_POSITION_NED,
                                param2 = 15000
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
                                param2 = 15000
                            };
                            byte[] data = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msgOut);
                            sock.SendTo(data, myGCS);
                        }
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_local_position_ned_t))
                    {
                        if (!gotPos)
                        {
                            gotPos = true;
                            Debug.Log("local_position_ned received");
                        }
                        newPos = true;
                        var data = (MAVLink.mavlink_local_position_ned_t)msg.data;
                        pos = new Vector3(-data.x, -data.z, data.y);
                        //Debug.Log("recv local_position_ned " + pos.ToString("F4"));
                        //recv_intvl = stopwatch.ElapsedMilliseconds - recv_ts;
                        //recv_ts = stopwatch.ElapsedMilliseconds;
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_attitude_quaternion_t))
                    {
                        if(!gotAtt)
                        {
                            gotAtt = true;
                            Debug.Log("attitude_quaternion received");
                        }
                        newAtt = true;
                        var data = (MAVLink.mavlink_attitude_quaternion_t)msg.data;
                        att = new Quaternion(-data.q2, -data.q4, data.q3, -data.q1);
                    }
                }
            }
        }
        //stopwatch.Stop();
    }
}
