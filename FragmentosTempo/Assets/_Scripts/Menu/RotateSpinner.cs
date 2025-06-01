using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpinner : MonoBehaviour
{
    public float rotateSpeed;                       // Velocidade de rotação do spinner.

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0, 0, Time.deltaTime * rotateSpeed);       // Rotaciona o objeto no eixo Z (como um spinner), de acordo com a velocidade configurada e o deltaTime.
    }
}
