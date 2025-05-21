using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    public float offsetMultiplier = 1f;                         // Multiplicador de deslocamento para controlar a intensidade do efeito de parallax.
    public float smoothTime = 0.3f;                             // Tempo suave para a transição de movimento do efeito de parallax.

    private Vector2 startPosition;                              // Posição inicial do objeto, usada para calcular o deslocamento.
    private Vector3 velocity;                                   // Velocidade utilizada pelo SmoothDamp para suavizar o movimento.

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;                     // Armazena a posição inicial do objeto.
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = Camera.main.ScreenToViewportPoint(Input.mousePosition);        // Converte a posição do mouse na tela para o sistema de coordenadas da viewport (0 a 1).

        // Atualiza a posição do objeto com base na posição do mouse, criando um efeito de parallax.
        // O movimento é suavizado pela função SmoothDamp, utilizando a velocidade e o tempo de suavização definidos.
        transform.position = Vector3.SmoothDamp(transform.position, startPosition + (offset * offsetMultiplier), ref velocity, smoothTime);
    }
}
