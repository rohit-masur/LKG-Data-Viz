using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloPlaySDK;

public class Scr_RotateKeyPress : MonoBehaviour
{

    private HoloPlay HoloScript;
    public GameObject RealHolo;

    public float zoommax = 78;
    public float zoommin = 24;

    void Awake()
    {
        HoloScript = RealHolo.GetComponent("HoloPlay") as HoloPlay;
    }


    void FixedUpdate()
    {

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
            transform.position = new Vector3(transform.position.x, (transform.position.y + (5 * Time.deltaTime)), transform.position.z);
            //transform.Rotate(Vector3.up * -100 * Time.deltaTime);
        }

        if (Input.GetKey("down"))
        {
            transform.position = new Vector3(transform.position.x, (transform.position.y - (5 * Time.deltaTime)), transform.position.z);
            //HoloScript.size = HoloScript.size - 0.95f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("Usage:");
            HoloScript.size = HoloScript.size - 0.95f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            HoloScript.size = HoloScript.size + 0.95f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.left * 100 * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.left * -100 * Time.deltaTime);
        }

        //   if (HoloScript.size > zoommax) { HoloScript.size = zoommax; }
        //   if (HoloScript.size < zoommin) { HoloScript.size = zoommin; }

    }



}
