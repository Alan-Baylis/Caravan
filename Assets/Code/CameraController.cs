using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Member variables
    [SerializeField]
    private float _normalMoveSpeed = 25.0f;
    [SerializeField]
    private float _fastMoveSpeed = 50.0f;

    [SerializeField]
    private float _rotationSpeed = 70.0f;

    [SerializeField]
    private float _zoomSpeed = 20.0f;

    private Transform _cameraTransform;


    // Start, Update
    void Start()
    {
        _cameraTransform = transform.GetChild(0);
    }

    void Update()
    {
        GetInput();
    }


    // Internal methods
    private void GetInput()
    {
        GetMovementInput();
        GetRotationInput();

        GetZoomInput();
    }

    private void GetMovementInput()
    {
        bool fastMovement = Input.GetKey(KeyCode.LeftShift);

        float movementSpeed = fastMovement ? _fastMoveSpeed : _normalMoveSpeed;
        movementSpeed *= Time.deltaTime;

        if (Input.GetKey(KeyCode.W))
            transform.position += transform.forward * movementSpeed;

        else if (Input.GetKey(KeyCode.S))
            transform.position += -transform.forward * movementSpeed;

        if (Input.GetKey(KeyCode.A))
            transform.position += -_cameraTransform.right * movementSpeed;

        else if (Input.GetKey(KeyCode.D))
            transform.position += _cameraTransform.right * movementSpeed;
    }

    private void GetRotationInput()
    {
        float rotationSpeed = _rotationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q))
            transform.Rotate(Vector3.up, -rotationSpeed);

        else if (Input.GetKey(KeyCode.E))
            transform.Rotate(Vector3.up, rotationSpeed);
    }

    private void GetZoomInput()
    {
        float zoomSpeed = (_zoomSpeed * 100) * Time.deltaTime;

        Vector3 cameraMovement = transform.GetChild(0).transform.forward * zoomSpeed;

        Vector3 previousPosition = transform.position;


        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            transform.localPosition -= cameraMovement;

        else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            transform.localPosition += cameraMovement;


        if (transform.position.y < 100.0f)
            transform.position = new Vector3(previousPosition.x, 100.0f, previousPosition.z);

        else if (transform.position.y > 300.0f)
            transform.position = new Vector3(previousPosition.x, 300.0f, previousPosition.z);
    }
}
