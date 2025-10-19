using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    [Header("Rubber Band")]
    public LineRenderer lrFront;
    public LineRenderer lrBack;
    public Transform bandFrontAnchor;   // child of Slingshot at front fork tip
    public Transform bandBackAnchor;    // child of Slingshot at back fork tip

    [Header("Audio")]
    public AudioSource audioSource;     // on Slingshot
    public AudioClip snapClip;          // short rubber-band snap

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        if (lrFront != null) { lrFront.positionCount = 2; lrFront.enabled = false; }
        if (lrBack  != null) { lrBack.positionCount  = 2; lrBack.enabled  = false; }

    }
    void OnMouseEnter()
    {
        //print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);

    }

    void OnMouseExit()
    {
        //print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;
        projectile = Instantiate(projectilePrefab) as GameObject;
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        if (lrFront != null && bandFrontAnchor != null)
        {
            lrFront.enabled = true;
            lrFront.SetPosition(0, bandFrontAnchor.position);
            lrFront.SetPosition(1, projectile.transform.position);
        }
        if (lrBack != null && bandBackAnchor != null)
        {
            lrBack.enabled = true;
            lrBack.SetPosition(0, bandBackAnchor.position);
            lrBack.SetPosition(1, projectile.transform.position);
        }

    }

void Update()
    {
        if (!aimingMode) return;

        // Get the current mouse position in 2D screen coordinates
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        // Find the delta from the launchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;
        // Limit mouseDelta to the radius of the Slingshot SphereCollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }
        // Move the projectile to this new position
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        if (lrFront != null) lrFront.SetPosition(1, projectile.transform.position);
        if (lrBack  != null) lrBack.SetPosition(1, projectile.transform.position);


        if (Input.GetMouseButtonUp(0))
        { 
            // The mouse has been released
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            // Snap sound
            if (audioSource != null && snapClip != null) audioSource.PlayOneShot(snapClip);

            // Hide bands
            if (lrFront != null) lrFront.enabled = false;
            if (lrBack  != null) lrBack.enabled  = false;


            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile; // Set the _MainCamera POI
            // Add a ProjectileLine to the Projectile
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;
            MissionDemolition.SHOT_FIRED();
        }
    }

}
