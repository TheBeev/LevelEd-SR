using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerHand { LeftHand, RightHand };

public class SCR_OculusControllerVibrations : MonoBehaviour 
{
    public static SCR_OculusControllerVibrations instance;

    private WaitForSeconds previousWaitForDuration;
    private float previousDuration;

    private IEnumerator leftVibrations;
    private IEnumerator rightVibrations;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ControllerVibrations(float duration, float intensity, ControllerHand controllerHand)
    {
        //StopAllCoroutines();

        if (controllerHand == ControllerHand.LeftHand)
        {
            if (leftVibrations != null)
            {
                StopCoroutine(leftVibrations);
            }
            leftVibrations = StartVibrationsLeft(duration, intensity);
            StartCoroutine(leftVibrations);
        }

        if (controllerHand == ControllerHand.RightHand)
        {
            if (rightVibrations != null)
            {
                StopCoroutine(rightVibrations);
            }
            rightVibrations = StartVibrationsRight(duration, intensity);
            StartCoroutine(rightVibrations);
        }

    }

    IEnumerator StartVibrationsRight(float duration, float intensity)
    {
        if (duration != previousDuration)
        {
            previousWaitForDuration = new WaitForSeconds(duration);
            previousDuration = duration;
        }
        
        OVRInput.SetControllerVibration(0.1f, intensity, OVRInput.Controller.RTouch);

        yield return previousWaitForDuration;

        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);

        yield return null;
    }

    IEnumerator StartVibrationsLeft(float duration, float intensity)
    {
        if (duration != previousDuration)
        {
            previousWaitForDuration = new WaitForSeconds(duration);
            previousDuration = duration;
        }

        OVRInput.SetControllerVibration(0.1f, intensity, OVRInput.Controller.LTouch);

        yield return previousWaitForDuration;

        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);

        yield return null;
    }
}
