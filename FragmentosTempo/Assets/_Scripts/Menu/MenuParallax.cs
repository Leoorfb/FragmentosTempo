using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    public float offsetMultiplier = 1f;                         // Multiplicador de deslocamento para controlar a intensidade do efeito de parallax.
    public float smoothTime = 0.3f;                             // Tempo suave para a transi��o de movimento do efeito de parallax.

    private Vector2 startPosition;                              // Posi��o inicial do objeto, usada para calcular o deslocamento.
    private Vector3 velocity;                                   // Velocidade utilizada pelo SmoothDamp para suavizar o movimento.

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;                     // Armazena a posi��o inicial do objeto.
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = Camera.main.ScreenToViewportPoint(Input.mousePosition);        // Converte a posi��o do mouse na tela para o sistema de coordenadas da viewport (0 a 1).

        // Atualiza a posi��o do objeto com base na posi��o do mouse, criando um efeito de parallax.
        // O movimento � suavizado pela fun��o SmoothDamp, utilizando a velocidade e o tempo de suaviza��o definidos.
        transform.position = Vector3.SmoothDamp(transform.position, startPosition + (offset * offsetMultiplier), ref velocity, smoothTime);
    }
}
