using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController characterController;
    public Transform groundCheck;
    public LayerMask groundMask;
    public LayerMask playerLayer;
    public Camera playerCamera;
    public GameObject grapplingPoint;
    public GameObject ropeTravelPoint;
    private GameObject rope;
    public GameObject rifle;
    public GameObject grapplingHook;
    public GameObject aimingStaminaBar;
    public LineRenderer lineRenderer;
    public GameObject scoreText;
    public GameObject timerText;

    public CameraRotation cameraScript;

    public int fps = 100;

    public float score = 0;
    public float groundAccelerationSpeed;
    public float airAccelerationSpeed;
    public float groundBrakeSpeed;
    public float airBrakeSpeed;
    public float wallBrakeSpeed;
    private float xSpeed;
    private float ySpeed;
    private float zSpeed;
    public float maxSpeed;
    public float grapplingSpeed;
    public float ropeSpeed;
    public float releaseGrapplingSpeed;
    public float slowingStrength;
    public int maxAimingStamina;
    public float aimingTiringSpeed;
    public float aimingStaminaGainSpeed;
    private float currentSlowingStrength;

    private float verticalInputTargetSpeedX;
    private float verticalInputTargetSpeedZ;
    private float horizontalInputTargetSpeedX;
    private float horizontalInputTargetSpeedZ;

    public float groundCheckDistance = 0.4f;
    public float gravity;
    public float swingingGravitySpeed;
    private float swingingSpeed;
    private bool isGrounded = false;
    private bool isHooked = false;
    public bool isAiming = false;
    public bool isReloading = false;
    public float jumpHeight;
    public float airJumpHeight;
    private float zoom;
    private float actualZoom;
    private bool FOVnormalized = true;
    private bool blockAiming = false;
    public float shootStrength;
    public float reloadSpeed;
    private float currentReloadSpeed;
    private float ropeDistance;

    public int fieldOfView = 60;
    public float zoomSpeed;
    public float zoomNormalizationSpeed;
    public float defaultZoom;
    private float formerHorizontalSpeedBeforeHook;
    private float formerVerticalSpeedBeforeHook;
    private float formerXAngleBeforeHook;
    private float formerYAngleBeforeHook;

    private float grappleAngleY;
    private float grappleAngleX;

    private int timeSinceJumpWasPressed;
    private int timeAiming = 0;
    private float aimingStamina;
    private int animStage = 0;
    private bool grappleCheckSuccess;
    private bool isSwinging = false;
    private int milliseconds = 0;
    private int seconds = 0;
    private int minutes = 0;


    // Start is called before the first frame update
    void Start()
    {
        aimingStamina = maxAimingStamina;
        TimerTick();
    }

    // Update is called once per frame
    void Update()
    {
        Application.targetFrameRate = fps;

        if (isGrounded)
            GroundMovement();

        if (!isGrounded)
            AirMovement();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            zoom = defaultZoom;
        }
        if (Input.GetKey(KeyCode.Mouse0) && !isReloading && !blockAiming)
        {
            timeAiming += 1;
            if (timeAiming >= 10)
                Aim();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if(!isReloading && !blockAiming)
                Shoot();
            blockAiming = false;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            GrappleCheck();
        }
        if (Input.GetKey(KeyCode.Mouse1) && isHooked && grappleCheckSuccess)
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                Swing();
            }
            //if(Input.GetKey(KeyCode.Space))
            //{
            //    Grapple();
            //}
        }
        if (Input.GetKey(KeyCode.Mouse1) && !isHooked && grappleCheckSuccess)
        {
            GrappleShot();
        }
        if (Input.GetKeyDown(KeyCode.Space) && isHooked)
        {
            GrappleRelease();
        }
        if(Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Space) && isHooked)
        {
            isHooked = false;
            grappleCheckSuccess = false;
            Destroy(GameObject.FindGameObjectWithTag("Grappling Point"));
            Destroy(GameObject.FindGameObjectWithTag("Rope Travel Point"));
            lineRenderer.GetComponent<LineRenderer>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.Mouse1) && isSwinging)
        {
            isSwinging = false;
            swingingSpeed = 0;
            characterController.enabled = true;

        }

        //aimingStaminaBar.GetComponent<Image>().color = new Color(0, 0, 0, 100);

        if (!isAiming)
        {
            if(aimingStamina < maxAimingStamina)
                aimingStamina += aimingStaminaGainSpeed * Time.deltaTime;
        }

        //if(!isSwinging)
            
        characterController.Move(new Vector3(xSpeed, ySpeed, zSpeed) / 1000 / currentSlowingStrength);

        aimingStaminaBar.transform.localScale = new Vector3(aimingStamina / maxAimingStamina / 2, aimingStamina / maxAimingStamina / 40 + 0.01f, 0);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);

        if (timeSinceJumpWasPressed > 0)
            timeSinceJumpWasPressed -= 1;

        if (!FOVnormalized)
            NormalizeFOV();
        if (isAiming)
            currentSlowingStrength = slowingStrength;
        if (!isAiming)
            currentSlowingStrength = 1;

        //debug
        if (Input.GetKeyDown(KeyCode.E))
        {

        }
        //debug
        if (Input.GetKeyDown(KeyCode.R))
        {
            gameObject.transform.position = new Vector3(0, 22, -110);
            milliseconds = 0;
            seconds = 0;
            minutes = 0;
        }   
        scoreText.GetComponent<Text>().text = "Score: " + Convert.ToString(score);

    }

    public void GroundMovement()
    {
        Move(groundAccelerationSpeed);

        if(Input.GetKey(KeyCode.LeftShift))
        {
            maxSpeed = 300;
        }
        else
            //maxSpeed = 150;

        ySpeed = 0;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (timeSinceJumpWasPressed > 0)
        {
            Jump();
        }
        Brake(groundBrakeSpeed);
    }

    public void AirMovement()
    {
        Move(airAccelerationSpeed);

        if(!isAiming)
        {
            ySpeed -= gravity * Time.deltaTime;
        }
        if (isAiming)
        {
            ySpeed -= gravity * Time.deltaTime / slowingStrength;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            ySpeed += airJumpHeight * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            timeSinceJumpWasPressed = 20;
        }
        Brake(airBrakeSpeed);
    }

    private void Jump()
    {
        ySpeed =  Mathf.Sqrt(jumpHeight * 2f * gravity);
    }
    public void Brake(float brakeSpeed)
    {
        float xSpeedRatio = 0;
        float zSpeedRatio = 0;

        float speedSum = Math.Abs(xSpeed) + Math.Abs(zSpeed);
        xSpeedRatio = Math.Abs(xSpeed) / speedSum;
        zSpeedRatio = Math.Abs(zSpeed) / speedSum;


        if (Math.Abs(xSpeed) < brakeSpeed * xSpeedRatio * Time.deltaTime)
            xSpeed = 0;
        if (xSpeed != 0)
        {
            xSpeed -= brakeSpeed * xSpeedRatio * Math.Sign(xSpeed) * Time.deltaTime / currentSlowingStrength;
        }

        if (Math.Abs(zSpeed) < brakeSpeed * zSpeedRatio * Time.deltaTime)
            zSpeed = 0;
        if (zSpeed != 0)
        {
            zSpeed -= brakeSpeed * zSpeedRatio * Math.Sign(zSpeed) * Time.deltaTime / currentSlowingStrength;
        }
    }
    private void Move(float accelerationSpeed)
    {
        if (Input.GetKey(KeyCode.W))
        {
            verticalInputTargetSpeedX = maxSpeed * Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180));
            verticalInputTargetSpeedZ = maxSpeed * Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180));

            if (xSpeed * Math.Sign(verticalInputTargetSpeedX) < Math.Abs(verticalInputTargetSpeedX) + Math.Abs(horizontalInputTargetSpeedX))
                xSpeed += accelerationSpeed * Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180)) * Time.deltaTime;
            if (zSpeed * Math.Sign(verticalInputTargetSpeedZ) < Math.Abs(verticalInputTargetSpeedZ) + Math.Abs(horizontalInputTargetSpeedZ))
                zSpeed += accelerationSpeed * Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180)) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            verticalInputTargetSpeedX = -maxSpeed * Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180));
            verticalInputTargetSpeedZ = -maxSpeed * Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180));

            if (xSpeed * Math.Sign(verticalInputTargetSpeedX) < Math.Abs(verticalInputTargetSpeedX) + Math.Abs(horizontalInputTargetSpeedX))
                xSpeed -= accelerationSpeed * Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180)) * Time.deltaTime;
            if (zSpeed * Math.Sign(verticalInputTargetSpeedZ) < Math.Abs(verticalInputTargetSpeedZ) + Math.Abs(horizontalInputTargetSpeedZ))
                zSpeed -= accelerationSpeed * Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180)) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInputTargetSpeedX = maxSpeed * -Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180));
            horizontalInputTargetSpeedZ = maxSpeed * Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180));

            if (xSpeed * Math.Sign(horizontalInputTargetSpeedX) < Math.Abs(horizontalInputTargetSpeedX) + Math.Abs(verticalInputTargetSpeedX))
                xSpeed += accelerationSpeed * -Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180)) * Time.deltaTime;
            if (zSpeed * Math.Sign(horizontalInputTargetSpeedZ) < Math.Abs(horizontalInputTargetSpeedZ) + Math.Abs(verticalInputTargetSpeedZ))
                zSpeed += accelerationSpeed * Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180)) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            horizontalInputTargetSpeedX = maxSpeed * Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180));
            horizontalInputTargetSpeedZ = maxSpeed * -Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180));

            if (xSpeed * Math.Sign(horizontalInputTargetSpeedX) < Math.Abs(horizontalInputTargetSpeedX) + Math.Abs(verticalInputTargetSpeedX))
                xSpeed += accelerationSpeed * Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180)) * Time.deltaTime;
            if (zSpeed * Math.Sign(horizontalInputTargetSpeedZ) < Math.Abs(horizontalInputTargetSpeedZ) + Math.Abs(verticalInputTargetSpeedZ))
                zSpeed += accelerationSpeed * -Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180)) * Time.deltaTime;
        }
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            verticalInputTargetSpeedX = verticalInputTargetSpeedZ = 0;
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            horizontalInputTargetSpeedX = horizontalInputTargetSpeedZ = 0;
    }
    public void GrappleCheck()
    {
        RaycastHit raycastHit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit, 10000, playerLayer))
        {
            Vector3 localHit = transform.InverseTransformPoint(raycastHit.point);
        }

        if(raycastHit.collider != null)
        {
            Instantiate(grapplingPoint, raycastHit.point, Quaternion.identity);
            Instantiate(ropeTravelPoint, playerCamera.transform.position, new Quaternion(playerCamera.transform.eulerAngles.x, playerCamera.transform.eulerAngles.y, playerCamera.transform.eulerAngles.z, 0));
            grappleCheckSuccess = true;
            lineRenderer.GetComponent<LineRenderer>().enabled = true;
            //rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            //rope.GetComponent<CapsuleCollider>().enabled = false;
            //rope.tag = "Rope";
        }
        
    }
    public void GrappleShot()
    {
        GameObject ropeTravelPoint = GameObject.FindGameObjectWithTag("Rope Travel Point");
        ropeTravelPoint.transform.LookAt(GameObject.FindGameObjectWithTag("Grappling Point").transform);
        ropeTravelPoint.transform.position += ropeTravelPoint.transform.forward * ropeSpeed / currentSlowingStrength * Time.deltaTime;
        DrawRope(ropeTravelPoint.transform.position);
        if (Vector3.Distance(ropeTravelPoint.transform.position, GameObject.FindGameObjectWithTag("Grappling Point").transform.position) < 2)
        {
            isHooked = true;
            ropeDistance = Vector3.Distance(GameObject.FindGameObjectWithTag("Grappling Point").transform.position, gameObject.transform.position);
            //GameObject.FindGameObjectWithTag("Grappling Point").transform.eulerAngles = transform.LookAt(gameObject.transform.position);
            GameObject.FindGameObjectWithTag("Grappling Point").transform.LookAt(gameObject.transform.position);
        }
    }
    public void Swing()
    {
        GameObject grapplingPoint = GameObject.FindGameObjectWithTag("Grappling Point");
        DrawRope(grapplingPoint.transform.position);
        if(Vector3.Distance(GameObject.FindGameObjectWithTag("Grappling Point").transform.position, gameObject.transform.position) - 1 > ropeDistance && !isSwinging)
        {
            formerHorizontalSpeedBeforeHook = (Math.Abs(xSpeed) + Math.Abs(zSpeed) + Math.Abs(ySpeed)) / 1000;
            formerVerticalSpeedBeforeHook = Math.Abs(ySpeed);
            formerXAngleBeforeHook = GetAngle(GameObject.FindGameObjectWithTag("Grappling Point"), gameObject).x;
            formerYAngleBeforeHook = GetAngle(GameObject.FindGameObjectWithTag("Grappling Point"), gameObject).y;
            swingingSpeed = (180 * formerHorizontalSpeedBeforeHook) / (Mathf.PI * ropeDistance) * Math.Sign(ySpeed * Math.Sign(Mathf.Cos(formerYAngleBeforeHook * Mathf.PI / 180)));
            xSpeed = 0;
            ySpeed = 0;
            zSpeed = 0;
            isSwinging = true;
            //characterController.enabled = false;

        }
        if(isSwinging)
        {
            //formerXAngleBeforeHook += 1;
            //formerYAngleBeforeHook -= 1 + swingingSpeed;
            float cosX = Mathf.Cos(formerXAngleBeforeHook * Mathf.PI / 180);
            float sinX = Mathf.Sin(formerXAngleBeforeHook * Mathf.PI / 180);
            float sinY = Mathf.Sin(formerYAngleBeforeHook * Mathf.PI / 180);
            float cosY = Mathf.Cos(formerYAngleBeforeHook * Mathf.PI / 180);

            if(cosY < 0)
            {
                swingingSpeed += swingingGravitySpeed;
                formerYAngleBeforeHook += swingingSpeed;
            }
            if (cosY > 0)
            {
                swingingSpeed -= swingingGravitySpeed;
                formerYAngleBeforeHook += swingingSpeed;
            }
            //print("cos X: " + Convert.ToString(cosX) + ", " + "sin X: " + Convert.ToString(sinX) + ", sin Y: " + Convert.ToString(sinY) + ", angles: " + Convert.ToString(GetAngle(grapplingPoint, gameObject)));
            //xSpeed = 0;
            //ySpeed = 0;
            //zSpeed = 0;

            xSpeed = ((grapplingPoint.transform.position.x + (-sinX * cosY * ropeDistance)) - gameObject.transform.position.x) * 100;
            ySpeed = ((grapplingPoint.transform.position.y + (sinY * ropeDistance)) - gameObject.transform.position.y) * 100;
            zSpeed = ((grapplingPoint.transform.position.z + (-cosX * cosY * ropeDistance)) - gameObject.transform.position.z) * 100;
            //print(Convert.ToString(gameObject.transform.position.x) + ", " + Convert.ToString(grapplingPoint.transform.position.y + (sinY * ropeDistance)));
            //GameObject.FindGameObjectWithTag("Rope Travel Point").transform.position = (grapplingPoint.transform.position + new Vector3(-sinX * cosY * ropeDistance, sinY * ropeDistance, -cosX * cosY * ropeDistance));
            //print(Convert.ToString(Vector3.Distance(GameObject.FindGameObjectWithTag("Grappling Point").transform.position, gameObject.transform.position)) + Convert.ToString(ropeDistance));
        }
    }
    public void Grapple()
    {
        Vector3 grapplingPoint = GameObject.FindGameObjectWithTag("Grappling Point").transform.position;
        float distance = Vector3.Distance(grapplingHook.transform.position, grapplingPoint);
        if (playerCamera.GetComponent<Camera>().fieldOfView < 70)
        {
            playerCamera.GetComponent<Camera>().fieldOfView += 0.1f;
        }

        DrawRope(grapplingPoint);

        //rope.transform.eulerAngles = new Vector3(Mathf.Acos((grapplingPoint.y - rifle.transform.position.y) / distance) * 180 / Mathf.PI - 180, horizontalMovementAngle, 0);
            //rope.transform.eulerAngles = new Vector3(Mathf.Acos((grapplingPoint.y - rifle.transform.position.y) / distance) * 180 / Mathf.PI - 180, Mathf.Acos((grapplingPoint.z - rifle.transform.position.z) / distance2D) * -180 / Mathf.PI, 0);

        xSpeed = grapplingSpeed * ((grapplingPoint.x - gameObject.transform.position.x) / distance);
        zSpeed = grapplingSpeed * ((grapplingPoint.z - gameObject.transform.position.z) / distance);
        ySpeed = grapplingSpeed * ((grapplingPoint.y - gameObject.transform.position.y) / distance);

    }
    public void GrappleRelease()
    {
        FOVnormalized = false;
        if(GameObject.FindGameObjectWithTag("Grappling Point"))
        {
            playerCamera.GetComponent<Camera>().fieldOfView = 75f;
            xSpeed = releaseGrapplingSpeed * ((GameObject.FindGameObjectWithTag("Grappling Point").transform.position.x - gameObject.transform.position.x) / Vector3.Distance(rifle.transform.position, GameObject.FindGameObjectWithTag("Grappling Point").transform.position));
            zSpeed = releaseGrapplingSpeed * ((GameObject.FindGameObjectWithTag("Grappling Point").transform.position.z - gameObject.transform.position.z) / Vector3.Distance(rifle.transform.position, GameObject.FindGameObjectWithTag("Grappling Point").transform.position));
            ySpeed = releaseGrapplingSpeed * ((GameObject.FindGameObjectWithTag("Grappling Point").transform.position.y - gameObject.transform.position.y) / Vector3.Distance(rifle.transform.position, GameObject.FindGameObjectWithTag("Grappling Point").transform.position));
        }
    }
    public void DrawRope(Vector3 ropeEndPoint)
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, grapplingHook.transform.position);
        lineRenderer.SetPosition(1, ropeEndPoint);
    }
    public void Aim()
    {
        cameraScript.mouseSensitivity = 1f;
        rifle.transform.localPosition = new Vector3(0f, -0.7f, 1.5f);
        aimingStamina -= aimingTiringSpeed * Time.deltaTime;
        isAiming = true;
        zoom -= zoom / zoomSpeed;
        actualZoom += zoom;
        playerCamera.GetComponent<Camera>().fieldOfView = fieldOfView - actualZoom;
        if(aimingStamina <= 0)
        {
            Shoot();
            blockAiming = true;
        }
    }
    public void Shoot()
    {
        currentReloadSpeed = reloadSpeed;
        cameraScript.mouseSensitivity = 3f;
        playerCamera.GetComponent<Camera>().fieldOfView = 63f;
        FOVnormalized = false;
        rifle.transform.localPosition = new Vector3(0.7f, -0.7f, 1.5f);
        timeAiming = 0;
        zoom = defaultZoom;
        actualZoom = 0;
        xSpeed += shootStrength * -Convert.ToSingle(Math.Sin(transform.eulerAngles.y * Math.PI / 180)) * Convert.ToSingle(Math.Cos(playerCamera.transform.eulerAngles.x * Math.PI / 180));
        zSpeed += shootStrength * -Convert.ToSingle(Math.Cos(transform.eulerAngles.y * Math.PI / 180)) * Convert.ToSingle(Math.Cos(playerCamera.transform.eulerAngles.x * Math.PI / 180));
        ySpeed += shootStrength * Convert.ToSingle(Math.Sin(playerCamera.transform.eulerAngles.x * Math.PI / 180));
        //ySpeed += ySpeed / 1.5f;
        isAiming = false;
        RaycastHit raycastHit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit))
        {
            print(raycastHit.collider);
            EnemyScript enemyScript = raycastHit.transform.GetComponent<EnemyScript>();
            if(enemyScript != null)
            {
                enemyScript.TakeDamage(30);
            }
            EnemyHeadshotScript headshotScript = raycastHit.transform.GetComponent<EnemyHeadshotScript>();
            if (headshotScript != null)
            {
                headshotScript.TakeHeadshot();
                currentReloadSpeed = reloadSpeed * 3;
                aimingStamina += maxAimingStamina / 5;
                aimingStamina = Math.Clamp(aimingStamina, 0, maxAimingStamina);
            }
            TargetScript targetScript = raycastHit.transform.GetComponent<TargetScript>();
            if (targetScript != null)
            {
                score += 30;
            }
            TargetWeakpointScript weakpointScript = raycastHit.transform.GetComponent<TargetWeakpointScript>();
            if (weakpointScript != null)
            {
                score += 100;
                weakpointScript.Die();
                GameObject newScoreText = new GameObject("New Score Text");
                //newScoreText.transform.parent = GameObject.FindGameObjectWithTag("Player Canvas").transform;
                newScoreText.AddComponent<Text>().text = "dsa";
            }
        }
        //rifle.transform.localEulerAngles = new Vector3(-30,0,0);

        animStage = 0;
        isReloading = true;
        ReloadAnimation();
    }
    public void ReloadAnimation()
    {
        if (rifle.transform.localPosition.z < 1.6f)
            Invoke("ReloadAnimation", 0.01f);
        if (animStage == 0)
        {
            rifle.transform.localPosition += new Vector3(0, 0, -currentReloadSpeed);
            if (rifle.transform.localPosition.z <= 0)
                animStage = 1;
        }
        if (animStage == 1)
        {
            rifle.transform.localPosition += new Vector3(0, 0, currentReloadSpeed);
            if (rifle.transform.localPosition.z > 1.6f)
            {
                rifle.transform.localPosition = new Vector3(0.7f, -0.7f, 1.5f);
                //rifle.transform.localEulerAngles = new Vector3(0, 0, 0);
                isReloading = false;
                CancelInvoke("ReloadAnimation");
            }
        }
    }
    private void NormalizeFOV()
    {
        if(playerCamera.GetComponent<Camera>().fieldOfView > fieldOfView)
        {
            playerCamera.GetComponent<Camera>().fieldOfView -= zoomNormalizationSpeed;
        }
        if (playerCamera.GetComponent<Camera>().fieldOfView < fieldOfView)
        {
            playerCamera.GetComponent<Camera>().fieldOfView += zoomNormalizationSpeed;
        }
        if (playerCamera.GetComponent<Camera>().fieldOfView > fieldOfView - 0.5f && playerCamera.GetComponent<Camera>().fieldOfView < fieldOfView + 0.5f)
            FOVnormalized = true;
    }
    public (float y, float x) GetAngle(GameObject obj1, GameObject obj2)
    {
        float distance2D = Mathf.Sqrt(Mathf.Pow((obj1.transform.position.z - obj2.transform.position.z), 2) + Mathf.Pow((obj1.transform.position.x - obj2.transform.position.x), 2));
        float distance = Vector3.Distance(obj1.transform.position, obj2.transform.position);
        if ((obj1.transform.position.x - obj2.transform.position.x) / distance >= 0)
        {
            return(Mathf.Acos((obj1.transform.position.y - obj2.transform.position.y) / distance) * 180 / Mathf.PI - 90, Mathf.Acos((obj1.transform.position.z - obj2.transform.position.z) / distance2D) * 180 / Mathf.PI);
        }
        else
        {
            return (Mathf.Acos((obj1.transform.position.y - obj2.transform.position.y) / distance) * 180 / Mathf.PI - 90, Mathf.Acos((obj1.transform.position.z - obj2.transform.position.z) / distance2D) * -180 / Mathf.PI);
        }
    }

    public void TimerTick()
    {
        milliseconds += 1;
        if(milliseconds > 99)
        {
            milliseconds = 0;
            seconds += 1;
        }
        if(seconds > 59)
        {
            seconds = 0;
            minutes += 1;
        }
        Invoke("TimerTick", 0.01f);
        if(seconds < 10)
            timerText.GetComponent<Text>().text = Convert.ToString(minutes) + ":0" + Convert.ToString(seconds) + ":" + Convert.ToString(milliseconds);
        else if (milliseconds < 10)
            timerText.GetComponent<Text>().text = Convert.ToString(minutes) + ":" + Convert.ToString(seconds) + ":0" + Convert.ToString(milliseconds);
        else if (seconds < 10 && milliseconds < 10)
            timerText.GetComponent<Text>().text = Convert.ToString(minutes) + ":0" + Convert.ToString(seconds) + ":0" + Convert.ToString(milliseconds);
        else
            timerText.GetComponent<Text>().text = Convert.ToString(minutes) + ":" + Convert.ToString(seconds) + ":" + Convert.ToString(milliseconds);
    }
}
