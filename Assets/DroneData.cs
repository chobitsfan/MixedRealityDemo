﻿using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DroneData : MonoBehaviour
{
    public InputField IpInputText;
    public GameObject NetworkText;
    public GameObject ApmMsg;
    //public GameObject ExplosionEffect;
    public GameObject FxCamera;
    public GameObject SpeedText;
    public GameObject Stage;
    Thread thread = null;
    bool gogo = true;
    bool gotPos = false;
    bool gotAtt = false;
    bool newPos = false;
    bool newAtt = false;
    Vector3 pos = Vector3.zero;
    Vector3 vel = Vector3.zero;
    Quaternion att = Quaternion.identity;
    Vector3 angSpeed = Vector3.zero;
    bool gotHb = false;
    uint lastPosTs = 0;
    uint lastAttTs = 0;
    uint posInt = 0;
    uint attInt = 0;
    byte avoidAngle = 0;    
    string apmMsg = null;
    long lastPosNetTs = 0;
    long posNetInt = 0;
    long lastAttNetTs = 0;
    long attNetInt = 0;
    float glitchTs = 0;
    UnityEngine.UI.Text SpeedText_text;

    IPEndPoint drone;
    MAVLink.MavlinkParse mavlinkParse;
    Socket sock;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("DroneIP")) IpInputText.text = PlayerPrefs.GetString("DroneIP");
        mavlinkParse = new MAVLink.MavlinkParse();
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            ReceiveTimeout = 1000
        };
        sock.Bind(new IPEndPoint(IPAddress.Any, 0));
        
        SpeedText_text = SpeedText.GetComponent<Text>();
    }

    private void Update()
    {
        if (apmMsg != null)
        {
            ApmMsg.GetComponent<Text>().text = apmMsg;
            apmMsg = null;
        }
        if (glitchTs > 0)
        {
            glitchTs -= Time.deltaTime;
            if (glitchTs <= 0)
            {
                Kino.AnalogGlitch glitch = FxCamera.GetComponent<Kino.AnalogGlitch>();
                glitch.enabled = false;
            }
        }
        if (newPos)
        {
            newPos = false;
            transform.localPosition = pos;
            SpeedText_text.text = "speed: " + vel.magnitude.ToString("F2") + " m/s";
        }
        else
        {
            transform.localPosition += vel * Time.deltaTime;
        }
        if (newAtt)
        {
            newAtt = false;
            transform.localRotation = att;
        }
        else
        {
            transform.Rotate(angSpeed.x * Time.deltaTime / Mathf.PI * 180f, angSpeed.y * Time.deltaTime / Mathf.PI * 180f, angSpeed.z * Time.deltaTime / Mathf.PI * 180f, Space.Self);
        }
    }

    public void OnConnClicked()
    {
        if (thread == null && IPAddress.TryParse("192.168.50." + IpInputText.text, out IPAddress ip))
        {
            PlayerPrefs.SetString("DroneIP", IpInputText.text);
            drone = new IPEndPoint(ip, 17500);
            thread = new Thread(new ThreadStart(RecvData));
            thread.Start();
        }
        else
        {
            Debug.LogWarning("cannot parse drone ip address");
        }
    }

    public void RebootApm()
    {
        MAVLink.mavlink_command_long_t msgOut = new MAVLink.mavlink_command_long_t()
        {
            target_system = 0,
            command = (ushort)MAVLink.MAV_CMD.PREFLIGHT_REBOOT_SHUTDOWN,
            param1 = 1
        };
        byte[] data = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, msgOut);
        sock.SendTo(data, drone);
    }

    private void FixedUpdate()
    {
        if (newPos || newAtt)
        {
            NetworkText.GetComponent<Text>().text = posInt + " " + attInt + " " + posNetInt + " " + attNetInt;
        }
    }

    void OnDestroy()
    {
        gogo = false;
        if (thread != null)
        {
            thread.Join();
        }
        sock.Close();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CheckPoint"))
        {
            Stage.GetComponent<GameStage>().PassCheckPoint();
        }
        else
        {
            SendDistSensor(5, 0);
            Kino.AnalogGlitch glitch = FxCamera.GetComponent<Kino.AnalogGlitch>();
            glitch.enabled = true;
            glitchTs = 0.5f;
            Stage.GetComponent<GameStage>().HitObstacle();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            SendDistSensor(5, 0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            SendDistSensor(5, 0);
        }
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

        Kino.AnalogGlitch glitch = FxCamera.GetComponent<Kino.AnalogGlitch>();
        glitch.enabled = true;
        glitchTs = 0.5f;
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
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        while (gogo)
        {
            if (!gotHb)
            {
                sock.SendTo(new byte[16], drone);
            }
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
                    long cur_ts = stopWatch.ElapsedMilliseconds;
                    if (msg_type == typeof(MAVLink.mavlink_heartbeat_t))
                    {
                        if (!gotHb)
                        {
                            gotHb = true;
                            apmMsg = "heartbeat received";
                        }
                        if (gotAtt && (cur_ts - lastAttNetTs > 5000)) //ardupilot may be rebooted
                        {
                            gotPos = gotAtt = false;
                            posInt = attInt = 0;
                        }
                        if (!gotPos || posInt > 100)
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
                            //Debug.Log("request LOCAL_POSITION_NED");
                        }
                        if (!gotAtt || attInt > 100)
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
                            //Debug.Log("request ATTITUDE_QUATERNION");
                        }
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_local_position_ned_t))
                    {
                        if (!gotPos)
                        {
                            gotPos = true;
                            lastPosTs = 0;
                            apmMsg = "local_position_ned received";
                        }
                        var data = (MAVLink.mavlink_local_position_ned_t)msg.data;
                        if (data.time_boot_ms > lastPosTs)
                        {
                            newPos = true;
                            posInt = data.time_boot_ms - lastPosTs;
                            lastPosTs = data.time_boot_ms;
                            posNetInt = cur_ts - lastPosNetTs;
                            lastPosNetTs = cur_ts;
                            pos.Set(data.y, -data.z, data.x); //unity z as north, x as east
                            vel.Set(data.vy, -data.vz, data.vx);
                        }
                    }
                    else if (msg_type == typeof(MAVLink.mavlink_attitude_quaternion_t))
                    {
                        if (!gotAtt)
                        {
                            gotAtt = true;
                            lastAttTs = 0;
                            apmMsg = "attitude_quaternion received";
                        }
                        var data = (MAVLink.mavlink_attitude_quaternion_t)msg.data;
                        if (data.time_boot_ms > lastAttTs)
                        {
                            newAtt = true;
                            attInt = data.time_boot_ms - lastAttTs;
                            lastAttTs = data.time_boot_ms;
                            attNetInt = cur_ts - lastAttNetTs;
                            lastAttNetTs = cur_ts;
                            att.Set(data.q3, -data.q4, data.q2, -data.q1);
                            angSpeed.Set(data.pitchspeed, data.yawspeed, data.rollspeed);
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
