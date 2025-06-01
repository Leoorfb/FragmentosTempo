using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUpAnimation : MonoBehaviour
{
    public AnimationCurve opacityCurve;                                                         // Curva que define como a opacidade muda ao longo do tempo.
    public AnimationCurve scaleCurve;                                                           // Curva que define como a escala muda ao longo do tempo.
    public AnimationCurve heightCurve;                                                          // Curva que define como a altura (posição Y) muda ao longo do tempo.

    private TextMeshProUGUI tmp;                                                                // Referência ao componente de texto.
    private float time = 0;                                                                     // Controla o tempo de animação.
    private Vector3 origin;                                                                     // Armazena a posição inicial do pop-up.

    private void Awake()
    {
        tmp = transform.GetChild(0).GetComponent<TextMeshProUGUI>();                            // Obtém o componente TextMeshProUGUI que está como filho.
        origin = transform.position;                                                            // Armazena a posição inicial para base da animação.
    }

    private void Update()
    {
        tmp.color = new Color(1, 1, 1, opacityCurve.Evaluate(time));                            // Atualiza a opacidade do texto conforme a curva de opacidade.
        transform.localScale = Vector3.one * scaleCurve.Evaluate(time);                         // Atualiza a escala do pop-up conforme a curva de escala.
        transform.position = origin + new Vector3(0, 1 + heightCurve.Evaluate(time), 0);        // Atualiza a posição vertical conforme a curva de altura.
        time += Time.deltaTime;                                                                 // Incrementa o tempo, para que as curvas evoluam.
    }
}
