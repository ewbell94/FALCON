using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementController : MonoBehaviour
{
    public XRController teleportRay;
    public float activationThreshold = 0.25f;
    public float scrollSpeed=0.5f;
    private InputDevice rightController;
    private InputDevice leftController;
    // Update is called once per frame
    void Start() {
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
        if (devices.Count > 0){
            rightController=devices[0];
        }

        devices=new List<InputDevice>(); 
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, devices);
        if (devices.Count > 0){
            leftController=devices[0];
        }
    }
    void Update()
    {
        rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStickPos);
        if (rightStickPos.y > activationThreshold || rightStickPos.y < -activationThreshold){
            transform.position=new Vector3(transform.position.x,transform.position.y+scrollSpeed*rightStickPos.y,transform.position.z);
        }

        leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStickPos);
        teleportRay.gameObject.SetActive(leftStickPos.y > activationThreshold);
    }
}
