using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementControllerMK : MovementController
{
    public float mouseSensitivity = 100.0f;
    public float speed = 1.0f;
    public float upDown = 0.1f;
    public GameObject optionsMenuPrefab;
    private float xRotation = 0.0f;
    private float nodeDistance = 0.0f;
    private Transform cameraTransform;
    private MKUIWrapper uiWrapper;
    private NetworkBuilder networkBuilder;
    private Transform connectedNode = null;
    private Image reticle;
    // Start is called before the first frame update
    void Start(){
        cameraTransform = Camera.main.transform;
        uiWrapper = GetComponent<MKUIWrapper>();
        networkBuilder = GameObject.Find("NetworkBuilder").GetComponent<NetworkBuilder>();
        reticle = GameObject.Find("Reticle").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (movementActive){
            if (Cursor.lockState != CursorLockMode.Locked){
                Cursor.lockState = CursorLockMode.Locked;
            }

            float mouseX = Input.GetAxis("Mouse X")*mouseSensitivity*Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y")*mouseSensitivity*Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f);
            transform.Rotate(Vector3.up*mouseX);
            
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right*x + transform.forward*z;
            transform.position += move*speed*Time.deltaTime;

            if (Input.GetKey(KeyCode.Space)){
                transform.position = new Vector3(transform.position.x,transform.position.y+upDown*Time.deltaTime,transform.position.z);
            } else if (Input.GetKey(KeyCode.LeftControl)){
                transform.position = new Vector3(transform.position.x,transform.position.y-upDown*Time.deltaTime,transform.position.z);
            }

            if (Input.GetMouseButtonDown(1)){
                //Debug.DrawRay(cameraTransform.position, cameraTransform.forward, Color.yellow, 1.0f);
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit) && hit.transform.parent.tag == "NodeGroup"){
                    connectedNode = hit.transform;
                    nodeDistance = Vector3.Distance(cameraTransform.position, connectedNode.position);
                }
            }

            if (connectedNode != null){
                if (Input.GetMouseButton(1)){
                    connectedNode.position = cameraTransform.position + nodeDistance*cameraTransform.forward;
                } else {
                    connectedNode = null;
                    nodeDistance = 0.0f;
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab) && connectedNode == null){
                uiWrapper.WrapUI((GameObject) Instantiate(optionsMenuPrefab,Vector3.zero,Quaternion.identity));
                movementActive=false;
            }
            
            if (Input.GetKeyDown(KeyCode.E) && connectedNode == null){
                networkBuilder.ChangeState(true);
            } else if (Input.GetKeyDown(KeyCode.Q) && connectedNode == null){
                networkBuilder.ChangeState(false);
            }

            if (Input.GetKeyDown(KeyCode.R)){
                Color oldColor = reticle.color;
                if (oldColor.a < 1.0f){
                    reticle.color = new Color(oldColor.r,oldColor.g,oldColor.b,1.0f);
                } else {
                    reticle.color = new Color(oldColor.r,oldColor.g,oldColor.b,0.0f);
                }
            }

        } else {
            if (Cursor.lockState == CursorLockMode.Locked){
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
