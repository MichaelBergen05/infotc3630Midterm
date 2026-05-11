using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class earthSpin : MonoBehaviour
{
    public float rotationSpeed = 10f;   

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime, Space.Self);
    }
}
