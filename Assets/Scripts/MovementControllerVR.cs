using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

//Everything you can do as a VR player
public class MovementControllerVR : MovementController
{
    public XRController teleportRay;
    public GameObject optionsMenuPrefab;
    
    private NetworkBuilder networkBuilder;
    private InputDevice rightController = new InputDevice();
    private InputDevice leftController = new InputDevice();
    private GameObject openMenu = null;
    private float activationThreshold = 0.25f; //Dead zone for control sticks 
    private float scrollSpeed=7.5f; //Speed for going up/down
    private bool alreadyChanged = false;
    private bool prevPressed = false; //Necessary because it checks for "OnButton" not "OnButtonDown"

    void Start() {
        teleportRay.gameObject.SetActive(false);
        networkBuilder = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
    }

    private InputDevice GetDeviceWithCharacteristics(InputDeviceCharacteristics characteristics){
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
        if (devices.Count > 0){
            return devices[0];
        } else {
            Debug.Log("No device was found!");
            return new InputDevice();
        }
    }

    void Update()
    {
        if (rightController.name == null || leftController.name == null){
            InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            rightController = GetDeviceWithCharacteristics(rightControllerCharacteristics);
            InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            leftController = GetDeviceWithCharacteristics(leftControllerCharacteristics);
        }
        //Check if a face button is pressed (any of them, really)
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
                if (!alreadyChanged){
                    networkBuilder.ChangeState(true);
                    alreadyChanged = true;
                }
            } else if (reverseState) {
                if (!alreadyChanged){
                    networkBuilder.ChangeState(false);
                    alreadyChanged = true;
                }
            } else {
                alreadyChanged = false;
            }

            //Controls opening the menu (press any button)
            if (buttonPressed){
                if (!prevPressed){
                    Vector3 optPos=Camera.main.transform.position+2.5f*Camera.main.transform.forward;
                    openMenu = (GameObject) Instantiate(optionsMenuPrefab,new Vector3(optPos.x,Camera.main.transform.position.y,optPos.z),Quaternion.Euler(0.0f,Camera.main.transform.rotation.eulerAngles.y,0.0f));
                    movementActive=false;
                    prevPressed = true;
                }
            } else {
                prevPressed = false;
            }
        } else {
            //Close the menu if it's open and a button is pressed
            if (buttonPressed){
                if (openMenu != null && openMenu.activeSelf && !prevPressed){
                    Destroy(openMenu);
                    openMenu = null;
                    movementActive = true;
                    prevPressed = true; //Seriously why is there no OnButtonDown
                }
            } else {
                prevPressed = false;
            }
        }
    }
}

