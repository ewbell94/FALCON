using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementController : MonoBehaviour
{
    public XRController teleportRay;
    public InputHelpers.Button teleportActivationButton;
    public float activationThreshold = 0.1f;
    public float scrollSpeed=0.5f;
    private InputDevice targetDevice;
    // Update is called once per frame
    void Start() {
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
        if (devices.Count > 0){
            targetDevice=devices[0];
        } 
    }
    void Update()
    {
        targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool down);
        targetDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool up);
        if (down){
            transform.position=new Vector3(transform.position.x,transform.position.y-scrollSpeed,transform.position.z);
        }

        if (up){
            transform.position=new Vector3(transform.position.x,transform.position.y+scrollSpeed,transform.position.z);
        }

        if (teleportRay){
            teleportRay.gameObject.SetActive(CheckIfActivated(teleportRay));
        }
    }
    
    public bool CheckIfActivated(XRController controller){
        InputHelpers.IsPressed(controller.inputDevice, teleportActivationButton, out bool isActivated, activationThreshold);
        return isActivated;
    }
}
