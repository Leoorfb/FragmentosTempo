using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossIntroManager : MonoBehaviour
{
    public float delayToActivate = 2f;                          // Tempo de atraso antes de ativar a barra de vida do Boss.

    // Start is called before the first frame update
    void Start()
    {        
        BossHealthManager.Instance.ShowBar(delayToActivate);    // Chama o método de ativação da barra de vida do Boss após um tempo de atraso.
    }    
}
