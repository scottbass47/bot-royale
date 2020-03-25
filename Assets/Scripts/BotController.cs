using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [SerializeField] private Camera botCamera;
    [SerializeField] [Range(0, 200)] private float mouseSensitivity = 10f;
    [SerializeField] [Range(0, 50)] private float maxSensorDistance;

    [Header("NEAT Parameters")]
    [SerializeField] [Range(0, 20)] private float moveSpeed = 1f;
    [SerializeField] [Range(0, 180)] private float turnSpeed = 1f;
    [SerializeField] [Range(0, 5)] private float moveThreshold = 2f;
    [SerializeField] [Range(0, 5)] private float turnThreshold = 2f;
    [SerializeField] [Range(0, 5)] private float turnRightThreshold = 2f;
    [SerializeField] [Range(0, 60)] private int queriesPerSecond = 10;
    [SerializeField] [Range(0, 10)] private float maxIdleTime = 1f;

    private CharacterController characterController;
    private BotSensors sensors;
    private IBlackBox brain;

    private bool isTurning;
    private float currentTurnSpeed;
    private float currentMoveSpeed;
    private float elapsedTime;
    private Vector3 startPosition;

    private float QueryTimeThreshold => 1f / queriesPerSecond;

    private double fitness;
    public double Fitness => fitness; 

    public uint BotId;
    public delegate void CollisionEvent(GameObject bot);
    public event CollisionEvent OnCollide;

    public delegate void ExceedMaxIdleTime(GameObject bot);
    public event ExceedMaxIdleTime OnExceedMaxIdleTime;

    private float timeIdling = 0;

    public void SetBrain(IBlackBox brain)
    {
        this.brain = brain;
    }

    public void SetStartPosition(Vector3 start) 
    {
        this.startPosition = start;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        sensors = GetComponent<BotSensors>();

        sensors.AddSensor("forward", 0);
        sensors.AddSensor("left", 45);
        sensors.AddSensor("right", -45);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        timeIdling += Time.deltaTime;

        // Ask bot for input
        if(elapsedTime >= QueryTimeThreshold)
        {
            sensors.CollectSensorData(botCamera.transform.TransformDirection(Vector3.forward), maxSensorDistance);
            QueryBotAction();
            elapsedTime -= QueryTimeThreshold;
        }

        if(isTurning) botCamera.transform.Rotate(transform.up * currentTurnSpeed * Time.deltaTime);

        var rot = botCamera.transform.rotation;
        var moveDirection = rot * Vector3.forward;
        moveDirection.Normalize();

        var motion = moveDirection * currentMoveSpeed * Time.deltaTime;
        characterController.Move(motion);

        fitness = (transform.position - startPosition).magnitude;
        var distance = motion.magnitude;
        if(distance > 0)
        {
            timeIdling = 0;
        }

        if(timeIdling >= maxIdleTime)
        {
            OnExceedMaxIdleTime?.Invoke(this.gameObject);
            return;
        }
    }

    private void QueryBotAction()
    {
        brain.ResetState();

        brain.InputSignalArray[0] = NormalizeSensorData("forward");
        brain.InputSignalArray[1] = NormalizeSensorData("left");
        brain.InputSignalArray[2] = NormalizeSensorData("right");

        brain.Activate();

        var moveOutput = (float)brain.OutputSignalArray[0];
        var turnOutput = (float)brain.OutputSignalArray[1];
        var turnDirOutput = (float)brain.OutputSignalArray[2];

        currentMoveSpeed = GetActionOutput(moveOutput, moveSpeed, 0, moveThreshold);
        isTurning = GetActionOutput(turnOutput, true, false, turnThreshold);
        currentTurnSpeed = GetActionOutput(turnDirOutput, turnSpeed, -turnSpeed, turnRightThreshold);
    }

    private T GetActionOutput<T>(float output, T action, T nonAction, float threshold)
    {
        if (output > threshold)
        {
            return action;
        }
        else
        {
            return nonAction;
        }
    }

    private float NormalizeSensorData(string sensor)
    {
        var data = sensors.GetSensorData(sensor);
        if (!data.Hit)
        {
            return 0;
        }
        else
        {
            return Mathf.Sqrt(1 -  data.Distance / maxSensorDistance);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        OnCollide?.Invoke(this.gameObject);
    }

    public void TurnOnDebugRendering()
    {
        sensors.DebugRendering = true;
        sensors.DebugLineLifetime = QueryTimeThreshold;
    }

    public void TurnOffDebugRendering()
    {
        sensors.DebugRendering = false;
    }

    public bool IsDebugRendering()
    {
        return sensors.DebugRendering;
    }
}
