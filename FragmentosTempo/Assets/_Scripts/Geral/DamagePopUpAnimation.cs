using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUpAnimation : MonoBehaviour
{
    public AnimationCurve opacityCurve;                                                         // Curva que define como a opacidade muda ao longo do tempo.
    public AnimationCurve scaleCurve;                                                           // Curva que define como a escala muda ao longo do tempo.
    public AnimationCurve heightCurve;                                                          // Curva que define como a altura (posi��o Y) muda ao longo do tempo.

    private TextMeshProUGUI tmp;                                                                // Refer�ncia ao componente de texto.
    private float time = 0;                                                                     // Controla o tempo de anima��o.
    private Vector3 origin;                                                                     // Armazena a posi��o inicial do pop-up.

    private void Awake()
    {
        tmp = transform.GetChild(0).GetComponent<TextMeshProUGUI>();                            // Obt�m o componente TextMeshProUGUI que est� como filho.
        origin = transform.position;                                                            // Armazena a posi��o inicial para base da anima��o.
    }

    private void Update()
    {
        tmp.color = new Color(1, 1, 1, opacityCurve.Evaluate(time));                            // Atualiza a opacidade do texto conforme a curva de opacidade.
        transform.localScale = Vector3.one * scaleCurve.Evaluate(time);                         // Atualiza a escala do pop-up conforme a curva de escala.
        transform.position = origin + new Vector3(0, 1 + heightCurve.Evaluate(time), 0);        // Atualiza a posi��o vertical conforme a curva de altura.
        time += Time.deltaTime;                                                                 // Incrementa o tempo, para que as curvas evoluam.
    }
}
