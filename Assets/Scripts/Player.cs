using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speedMove;
    [SerializeField]
    private float speedRotate;
   
    private void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal") * speedRotate * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * speedMove * Time.deltaTime;

        transform.Translate(Vector3.forward * z);
        transform.Rotate(Vector3.up, x);
    }
}
