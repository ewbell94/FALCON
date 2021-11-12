using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementController : MonoBehaviour
{
    public XRController teleportRay;
    public GameObject optionsMenuPrefab;
    public float activationThreshold = 0.25f;
    public float scrollSpeed=7.5f;
    public bool movementActive=false;
    private InputDevice rightController;
    private InputDevice leftController;
    // Update is called once per frame
    void Start() {
        teleportRay.gameObject.SetActive(false);
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
        if (movementActive){
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStickPos);
            if (rightStickPos.y > activationThreshold || rightStickPos.y < -activationThreshold){
                transform.position=new Vector3(transform.position.x,transform.position.y+scrollSpeed*Time.deltaTime*rightStickPos.y,transform.position.z);
            }

            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStickPos);
            teleportRay.gameObject.SetActive(leftStickPos.y > activationThreshold);

            bool buttonPressed=false;
            leftController.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed);
            if (!buttonPressed){
                leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonPressed);
                if (!buttonPressed){
                    rightController.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed);
                    if (!buttonPressed){
                        rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonPressed);
                    }
                }
            }

            if (buttonPressed){
                Vector3 optPos=Camera.main.transform.position+2.5f*Camera.main.transform.forward;
                Instantiate(optionsMenuPrefab,new Vector3(optPos.x,Camera.main.transform.position.y+1.0f,optPos.z),Quaternion.Euler(0.0f,Camera.main.transform.rotation.eulerAngles.y,0.0f));
                movementActive=false;
            }
        }
    }
}
