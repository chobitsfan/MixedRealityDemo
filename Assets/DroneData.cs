using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class DroneData : MonoBehaviour
{
    public int MAVLinkPort;
    //public GameObject Gun;
    public GameObject Bullet;
    Thread thread;
    bool gogo = true;
    bool gotPos = false;
    bool gotAtt = false;
    bool gotRC = false;
    bool newPos = false;
    bool newAtt = false;
    Vector3 pos = Vector3.zero;
    Vector3 vel = Vector3.zero;
    Quaternion att = Quaternion.identity;
    Rigidbody rb;
    bool got_hb = false;
    bool shoot = false;
    //bool shooting = false;
    float shootingTs = 1f;
    uint lastPosTs = 0;
    uint lastAttTs = 0;

    // Start is called before the first frame update
    void Start()
    {
        thread = new Thread(new ThreadStart(RecvData));
        thread.Start();
        rb = GetComponent<Rigidbody>();
    }

    private void Reset()
    {
        MAVLinkPort = 17500;
    }

    private void FixedUpdate()
    {
        if (newPos)
        {
            newPos = false;
            rb.MovePosition(pos);
        }
        else
        {
            rb.MovePosition(transform.position + new Vector3(vel.x * Time.fixedDeltaTime, vel.y * Time.fixedDeltaTime, vel.z * Time.fixedDeltaTime));
        }
        if (newAtt)
        {
            newAtt = false;
            rb.MoveRotation(att);
        }
        /*if (shoot && !shooting)
        {
            shooting = true;
            Gun.GetComponent<ParticleSystem>().Play();
        }
        else if (!shoot && shooting)
        {
            shooting = false;
            Gun.GetComponent<ParticleSystem>().Stop();
        }*/
        if (shoot)
        {
            shootingTs -= Time.fixedDeltaTime;
            if (shootingTs < 0)
            {
                shootingTs = 1f;
                GameObject.Instantiate(Bullet, transform.position - transform.up * 0.1f, Quaternion.LookRotation(-transform.right));
            }
        }
    }

    void OnDestroy()
    {
        gogo = false;        
        thread.Join();
    }


    void RecvData()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint myGCS = (EndPoint)sender;
        byte[] buf = new byte[MAVLink.MAVLINK_MAX_PACKET_LEN];
        MAVLink.MavlinkParse mavlinkParse = new MAVLink.MavlinkParse();
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sock.ReceiveTimeout = 1000;
        sock.Bind(new IPEndPoint(IPAddress.Any, MAVLinkPort));
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
                            Debug.Log("heartbeat received");
                        }
                        if (!gotRC)
                        {
                            MAVLink.mavlink_command_long_t msgOut = new MAVLink.mavlink_command_long_t()
                            {
                                target_system = 0,
                                command = (ushort)MAVLink.MAV_CMD.SET_MESSAGE_INTERVAL,
                                param1 = (float)MAVLink.MAVLINK_MSG_ID.RC_CHANNELS_RAW,
                                param2 = 50000
                            };
                            byte[] data = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msgOut);
                            sock.SendTo(data, myGCS);
                        }
                        if (!gotPos)
                        {
                            MAVLink.mavlink_command_long_t msgOut = new MAVLink.mavlink_command_long_t()
                            {
                                target_system = 0,
                                command = (ushort)MAVLink.MAV_CMD.SET_MESSAGE_INTERVAL,
                                param1 = (float)MAVLink.MAVLINK_MSG_ID.LOCAL_POSITION_NED,
                                param2 = 20000
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
                                param2 = 20000
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
                        var data = (MAVLink.mavlink_local_position_ned_t)msg.data;
                        if (data.time_boot_ms > lastPosTs)
                        {
                            newPos = true;
                            lastPosTs = data.time_boot_ms;
                            pos.Set(-data.x, -data.z, data.y);
                            vel.Set(-data.vx, -data.vz, data.vy);
                        }
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_attitude_quaternion_t))
                    {
                        if(!gotAtt)
                        {
                            gotAtt = true;
                            Debug.Log("attitude_quaternion received");
                        }                        
                        var data = (MAVLink.mavlink_attitude_quaternion_t)msg.data;
                        if (data.time_boot_ms > lastAttTs)
                        {
                            newAtt = true;
                            lastAttTs = data.time_boot_ms;
                            att.Set(-data.q2, -data.q4, data.q3, -data.q1);
                        }
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_rc_channels_raw_t))
                    {
                        if (!gotRC)
                        {
                            gotRC = true;
                            Debug.Log("rc_channels_raw received");
                        }
                        var data = (MAVLink.mavlink_rc_channels_raw_t)msg.data;
                        if (data.chan6_raw > 1600)
                        {
                            shoot = true;
                        }
                        else
                        {
                            shoot = false;
                        }
                    }
                }
            }
        }
    }
}
