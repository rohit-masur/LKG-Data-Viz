using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloPlaySDK;

public class Scr_RotateKeyPress : MonoBehaviour {

    private HoloPlay HoloScript;
    public GameObject RealHolo;

    public float zoommax = 78;
    public float zoommin = 24;

    void Awake()
    {
        HoloScript = RealHolo.GetComponent("HoloPlay") as HoloPlay;
    }


    void FixedUpdate () {

        if (Input.GetKey("left"))
        {
            transform.Rotate(Vector3.up * 100 * Time.deltaTime);
        }

        if (Input.GetKey("right"))
        {
            transform.Rotate(Vector3.up * -100 * Time.deltaTime);
        }

        if (Input.GetKey("up"))
        {
            HoloScript.size = HoloScript.size + 0.95f;
        }

        if (Input.GetKey("down"))
        {
            HoloScript.size = HoloScript.size - 0.95f;
        }

     //   if (HoloScript.size > zoommax) { HoloScript.size = zoommax; }
     //   if (HoloScript.size < zoommin) { HoloScript.size = zoommin; }

    }



}
