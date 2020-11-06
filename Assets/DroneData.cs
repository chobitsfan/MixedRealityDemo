using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DroneData : MonoBehaviour
{
    public int MAVLinkPort;
    //public GameObject Gun;
    public GameObject Bullet;
    public GameObject MsgUI;
    public GameObject HudText;
    public GameObject ApmMsg;
    public GameObject ExplosionEffect;
    public GameObject FxCamera;
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
    bool gotHb = false;
    bool shoot = false;
    //bool shooting = false;
    float shootingTs = 1f;
    uint lastPosTs = 0;
    uint lastAttTs = 0;
    uint posInt = 0;
    uint attInt = 0;
    Text msgText;
    byte avoidAngle = 0;
    float hudTs = 0f;
    string apmMsg = null;

    IPEndPoint sender;
    EndPoint drone;
    MAVLink.MavlinkParse mavlinkParse;
    Socket sock;

    // Start is called before the first frame update
    void Start()
    {
        sender = new IPEndPoint(IPAddress.Any, 0);
        drone = (EndPoint)sender;
        mavlinkParse = new MAVLink.MavlinkParse();
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            ReceiveTimeout = 1000
        };
        sock.Bind(new IPEndPoint(IPAddress.Any, MAVLinkPort));

        thread = new Thread(new ThreadStart(RecvData));
        thread.Start();
        rb = GetComponent<Rigidbody>();
        msgText = MsgUI.GetComponent<Text>();
    }

    private void Update()
    {
        if (hudTs > 0)
        {
            hudTs -= Time.deltaTime;
            if (hudTs <= 0)
            {
                HudText.SetActive(false);
            }
        }
        if (apmMsg != null)
        {
            UnityEngine.UI.Text txt = ApmMsg.GetComponent<UnityEngine.UI.Text>();
            txt.text = apmMsg;
            apmMsg = null;
        }
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
            rb.MovePosition(transform.parent.TransformPoint(pos));
            //msgText.text = "pos:" + posInt + " ms";
            msgText.text = "vel:" + vel.magnitude;
        }
        else
        {
            rb.MovePosition(transform.position + transform.parent.TransformDirection(vel) * Time.deltaTime);
        }
        if (newAtt)
        {
            newAtt = false;
            rb.MoveRotation(transform.parent.rotation * att);
            //msgText.text = "att:" + attInt + " ms";
        }
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
        sock.Close();
    }

    IEnumerator StopCameraGlitch()
    {
        yield return new WaitForSeconds(0.5f);
        Kino.AnalogGlitch glitch = FxCamera.GetComponent<Kino.AnalogGlitch>();
        glitch.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
#if false
        Vector3 impact = collision.gameObject.transform.position - transform.position;
        Debug.DrawRay(transform.position, impact, Color.red, 5, false);
        Vector2 impact2d = new Vector2(impact.x, impact.z);
        Vector2 heading2d = new Vector2(transform.forward.x, transform.forward.z);
        float angle = -Vector2.SignedAngle(heading2d, impact2d); //https://forum.unity.com/threads/vector2-signedangle.507058/
        if (angle < 0) angle += 360f;
        avoidAngle = (byte)(angle / 2f);
#endif
        SendDistSensor(5, avoidAngle);

        UnityEngine.UI.Text text = HudText.GetComponent<UnityEngine.UI.Text>();
        text.text = "BAD";
        hudTs = 1f;
        HudText.SetActive(true);
        //Debug.Log("hit " + collision.gameObject.name);

        //GameObject exp = GameObject.Instantiate(ExplosionEffect, collision.GetContact(0).point, Quaternion.identity);
        //Destroy(exp, 2);
        Kino.AnalogGlitch glitch = FxCamera.GetComponent<Kino.AnalogGlitch>();
        glitch.enabled = true;
        StartCoroutine(StopCameraGlitch());
    }

    private void OnCollisionStay(Collision collision)
    {
        SendDistSensor(5, avoidAngle);
    }

    private void OnCollisionExit(Collision collision)
    {
        SendDistSensor(5, avoidAngle);
    }

    private void SendDistSensor(ushort dist, byte orientation)
    {
        if (gotHb)
        {
            MAVLink.mavlink_distance_sensor_t msg = new MAVLink.mavlink_distance_sensor_t
            {
                type = 10, //shock
                min_distance = 1,
                max_distance = 300,
                current_distance = dist,
                orientation = orientation
            };
            byte[] pkt = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.DISTANCE_SENSOR, msg);
            sock.SendTo(pkt, drone);
        }
    }

    void RecvData()
    {
        byte[] buf = new byte[MAVLink.MAVLINK_MAX_PACKET_LEN];
        while (gogo)
        {
            int recvBytes = 0;
            try
            {
                recvBytes = sock.ReceiveFrom(buf, ref drone);
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
                        if (!gotHb)
                        {
                            gotHb = true;
                            Debug.Log("heartbeat received");
                        }
                        if (!gotRC)
                        {
                            MAVLink.mavlink_command_long_t msgOut = new MAVLink.mavlink_command_long_t()
                            {
                                target_system = 0,
                                command = (ushort)MAVLink.MAV_CMD.SET_MESSAGE_INTERVAL,
                                param1 = (float)MAVLink.MAVLINK_MSG_ID.RC_CHANNELS_RAW,
                                param2 = 20000
                            };
                            byte[] data = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msgOut);
                            sock.SendTo(data, drone);
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
                            sock.SendTo(data, drone);
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
                            sock.SendTo(data, drone);
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
                            posInt = data.time_boot_ms - lastPosTs;
                            lastPosTs = data.time_boot_ms;                            
                            pos.Set(data.y, -data.z, data.x); //unity z as north, x as east
                            vel.Set(data.vy, -data.vz, data.vx);
                        }
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_attitude_quaternion_t))
                    {
                        if (!gotAtt)
                        {
                            gotAtt = true;
                            Debug.Log("attitude_quaternion received");
                        }
                        var data = (MAVLink.mavlink_attitude_quaternion_t)msg.data;
                        if (data.time_boot_ms > lastAttTs)
                        {
                            newAtt = true;
                            attInt = data.time_boot_ms - lastAttTs;
                            lastAttTs = data.time_boot_ms;
                            att.Set(data.q3, -data.q4, data.q2, -data.q1);
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
                    else if (msg_type == typeof(MAVLink.mavlink_statustext_t))
                    {
                        var data = (MAVLink.mavlink_statustext_t)msg.data;
                        apmMsg = System.Text.Encoding.ASCII.GetString(data.text);
                    }
                }
            }
        }
    }
}
