using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementController : MonoBehaviour
{
    public XRController teleportRay;
    public GameObject optionsMenuPrefab;
    
    
    public bool movementActive=false;
    private NetworkBuilder networkBuilder;
    private InputDevice rightController;
    private InputDevice leftController;
    private float activationThreshold = 0.25f;
    private float scrollSpeed=7.5f;
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

        networkBuilder = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
    }

    void Update()
    {
        if (movementActive){
            //Controls up/down motion
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStickPos);
            if (rightStickPos.y > activationThreshold || rightStickPos.y < -activationThreshold){
                transform.position=new Vector3(transform.position.x,transform.position.y+scrollSpeed*Time.deltaTime*rightStickPos.y,transform.position.z);
            }

            //Controls teleport ray appearance/disappearance
            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStickPos);
            teleportRay.gameObject.SetActive(leftStickPos.y > activationThreshold);

            //Controls switching between network states
            rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool forwardState);
            leftController.TryGetFeatureValue(CommonUsages.triggerButton, out bool reverseState);
            if (forwardState){
                networkBuilder.ChangeState(true);
            } else if (reverseState) {
                networkBuilder.ChangeState(false);
            }

            //Controls opening the menu (press any button)
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
                Instantiate(optionsMenuPrefab,new Vector3(optPos.x,Camera.main.transform.position.y,optPos.z),Quaternion.Euler(0.0f,Camera.main.transform.rotation.eulerAngles.y,0.0f));
                movementActive=false;
            }

            
        }
    }
}
