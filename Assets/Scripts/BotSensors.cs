using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSensors : MonoBehaviour
{
    private Dictionary<string, float> sensorAngles;
    private Dictionary<string, SensorData> sensorData;

    public bool DebugRendering = false;
    public float DebugLineLifetime = 0.01f;

    private void Awake()
    {
        sensorAngles = new Dictionary<string, float>();
        sensorData = new Dictionary<string, SensorData>();
    }

    public void AddSensor(string name, float rotation)
    {
        sensorAngles.Add(name, rotation);
        sensorData.Add(name, default(SensorData));
    }

    public SensorData GetSensorData(string name)
    {
        return sensorData[name];
    }

    public void CollectSensorData(Vector3 forwardDirection, float maxDistance)
    {
        foreach(var sensor in sensorAngles)
        {
            float angle = sensorAngles[sensor.Key];
            var rotation = Quaternion.Euler(0, angle, 0);
            var sensorDirection = rotation * forwardDirection;

            RaycastHit hitInfo;
            SensorData data = default(SensorData);
            if(Physics.Raycast(transform.position, sensorDirection, out hitInfo, maxDistance, LayerMask.GetMask("Wall")))
            {
                data.Hit = true;
                data.Distance = hitInfo.distance;
                if (DebugRendering)
                {
                    Debug.DrawLine(transform.position, transform.position + data.Distance * Vector3.Normalize(sensorDirection), Color.red, DebugLineLifetime);
                }
            }
            else
            {
                data.Hit = false;
            }
            sensorData[sensor.Key] = data;
        }
    }

    public struct SensorData
    {
        public bool Hit;
        public float Distance;
    }
}
