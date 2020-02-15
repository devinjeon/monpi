using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private TimeController timeController;
    private GameController gameController;
    public GameObject mouseClickEffect;
    private const int RIGHT_MOUSE_BUTTON = 1;
    private bool isMoving;
    public float maxSpeed;
    public float acceleration;
    private float speed = 0; // player speed
    private Vector3 newPosition; // destination point
    private Vector3 newCameraPosition;
    public float cameraMinX, cameraMaxX, cameraMinZ, cameraMaxZ;
    public float startZ;
    private Quaternion newRotation;
    private bool canCollide;
    public bool CanCollide
    {
        get { return canCollide; }
    }
    public GameObject explosionEffect;

    void Start()
    {
        timeController = GameObject.FindGameObjectWithTag("TimeController").
            GetComponent<TimeController>();
        gameController = GameObject.FindGameObjectWithTag("GameController").
            GetComponent<GameController>();
        newPosition = transform.position;
        newCameraPosition = Camera.main.transform.position;
        isMoving = false;
        gameController.Lock();
        canCollide = false;

        speed = 0;
    }

    void Update()
    {
        if (gameController.GetStatusIsLocked() == false && Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON))
        {
            SetNewPosition();
            // Create Mouse effect
            Instantiate(mouseClickEffect,
                newPosition,
                transform.rotation
            );
        }

        // Move to new Position
        if (isMoving == true)
            Move();


        newCameraPosition = GetNewCameraPosition(transform.position);
        MoveCamera();
    }

    void FixedUpdate()
    {
        
    }

    void SetNewPosition()
    {
        // Change 3D mouse point to plane point(like 2D)
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float point = 0f;

        if (plane.Raycast(ray, out point))
            newPosition = ray.GetPoint(point);

        newRotation = Quaternion.LookRotation(newPosition - transform.position);
        isMoving = true;
    }

    void MoveCamera()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        if (cameraPosition != newCameraPosition)
        {
            float speedWeight = 1;
            if (gameController.GetStatusIsLocked() == true)
                speedWeight = 1 + 50f * (cameraPosition.z / 800f);
            float cameraSpeed = maxSpeed * Time.deltaTime * speedWeight;

            Camera.main.transform.position = Vector3.MoveTowards(
                cameraPosition, // Start point
                newCameraPosition, // Destination 
                cameraSpeed // distance
            );
        }
        else
        {
            if (gameController.GetStatusIsLocked() == true)
                gameController.UnLock();
        }
    }

    void Move()
    {
        if (speed <= maxSpeed)
            speed += acceleration;
        if (speed > maxSpeed)
            speed = maxSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, speed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(
            transform.position, // Start point
            newPosition, // Destination 
            speed * Time.deltaTime // Distance
        );

        // Stop moving if player arrived at the newPosition
        if (transform.position == newPosition)
        {
            isMoving = false;
            speed = 0;
        }
    }

    Vector3 GetNewCameraPosition(Vector3 position)
    {
        // Get the new position of the main camera from the player position
        float newX, newZ;
        newX = position.x;
        newZ = position.z;

        float currentStageMinZ = 100f * (gameController.GetCurrentStage() - 1)
            - gameController.safeZoneObj.transform.localScale.z + cameraMinZ;
        if (gameController.GetCurrentStage() == 1)
            currentStageMinZ = cameraMinZ;

        float currentStageMaxZ = 100f * (gameController.GetCurrentStage()) - cameraMinZ;

        if (newX < cameraMinX)
            newX = cameraMinX;
        if (newX > cameraMaxX)
            newX = cameraMaxX;
        if (newZ < currentStageMinZ)
            newZ = currentStageMinZ;
        if (newZ > currentStageMaxZ)
            newZ = currentStageMaxZ;

        return new Vector3(newX, 10, newZ);
    }

    void SetPositionAtClearTime()
    {
        Vector3 startSafeZonePosition = gameController.GetCurrentStartZoneCollider().gameObject.transform.position;
        newPosition = new Vector3(startSafeZonePosition.x, 0, startSafeZonePosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        // No collision within a safezone
        if (other.tag == "SafeZone" &&
            other == gameController.GetCurrentEndZoneCollider())
        {
            gameController.ClearStage();
            gameController.Lock();
            SetPositionAtClearTime();
        }

        // Make player to be safe within the safezone
        if (other.tag == "SafeZone")
            canCollide = false;

        // Gameover if player collide a monster outside of safezone
        if (canCollide == true)
        {
            Instantiate(explosionEffect,
                    gameObject.transform.position,
                    gameObject.transform.rotation
            );
            gameController.GameOver(false);
            Destroy(gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "SafeZone" &&
            other == gameController.GetCurrentStartZoneCollider())
        {
            timeController.StartCountDown();
            canCollide = true;
        }
    }
}
