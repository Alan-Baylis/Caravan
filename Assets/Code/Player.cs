using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    /* MEMBER VARIABLES */
    [SerializeField]
    private GameObject playerText;

    // Movement
    [SerializeField]
    private float _movementSpeed = 1;

    private Vector2 _targetCoordinates;
    private Vector2 _currentCoordinates { get { return new Vector2(transform.position.x, transform.position.z); } }

    private bool _isMoving;

    // Inventory
    public int WoodAmount { get; set; }
    public int GoldAmount { get; set; }

    public Town currentTown;

    void Start()
    {
        WoodAmount = 5;
        GoldAmount = 130;
        UpdateOnScreenText();
    }

    void Update()
    {
        GetInput();
        ProcessMovement();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.GetComponent<Town>() != null)
        {
            currentTown = col.GetComponent<Town>();

            GameObject uiTradeWindow = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
            uiTradeWindow.transform.position = col.transform.position;
            uiTradeWindow.SetActive(true);

        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.name == "Town")
        {
            GameObject uiTradeWindow = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
            uiTradeWindow.SetActive(false);

            currentTown = null;
        }
    }

    public void UpdateOnScreenText()
    {
        playerText.GetComponent<Text>().text = "Gold: " + GoldAmount + "\nWood: " + WoodAmount;
    }

    /* INTERNAL METHODS */
    private void GetInput()
    {
        // Set the players target position on RMB click 
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit raycastHit;
            Ray raycastRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(raycastRay, out raycastHit))
            {
                if (raycastHit.collider.GetComponent<Town>() != null)
                    SetTargetPosition(raycastHit.transform.position);

                else if (raycastHit.collider.gameObject.name == "Terrain")
                    SetTargetPosition(raycastHit.point);
            }
        }
    }

    /// <summary>
    /// Sets the target movement position of the player, which is the point it travels to
    /// </summary>
    /// <param name="targetPosition"></param>
    private void SetTargetPosition(Vector3 targetPosition)
    {
        _targetCoordinates = new Vector2(targetPosition.x, targetPosition.z);

        _isMoving = true;
    }

    /// <summary>
    /// Move the player toward the target position if it has one
    /// </summary>
    private void ProcessMovement()
    {
        if (_isMoving)
        {
            Vector3 directionalVector = new Vector3(_targetCoordinates.x, 0, _targetCoordinates.y) - transform.position;

            if (_movementSpeed * Time.deltaTime < directionalVector.magnitude)
                transform.position += (directionalVector.normalized * _movementSpeed) * Time.deltaTime;

            else
                transform.position = new Vector3(_targetCoordinates.x, 0, _targetCoordinates.y);
        }

        if (_currentCoordinates == _targetCoordinates)
            _isMoving = false;
    }
}
