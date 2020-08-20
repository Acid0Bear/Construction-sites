using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadePlatform2 : MonoBehaviour
{
    SliderJoint2D Elevator = null;
    [SerializeField] float MotorSpeed = 1;
    [SerializeField] float DelayInSeconds = 1;
    private void Awake()
    {
        Elevator = GetComponentInChildren<SliderJoint2D>();
        StartCoroutine(MovementRoutine());
    }
    IEnumerator MovementRoutine()
    {
        while (true)
        {
            var motor = Elevator.motor;
            if (Elevator.limitState == JointLimitState2D.UpperLimit)
            {
                yield return new WaitForSeconds(DelayInSeconds);
                motor.motorSpeed = MotorSpeed * -1;
                Elevator.motor = motor;
            }
            else if (Elevator.limitState == JointLimitState2D.LowerLimit)
            {
                yield return new WaitForSeconds(DelayInSeconds);
                motor.motorSpeed = MotorSpeed * 1;
                Elevator.motor = motor;
            }
            yield return null;
        }
    }
}
