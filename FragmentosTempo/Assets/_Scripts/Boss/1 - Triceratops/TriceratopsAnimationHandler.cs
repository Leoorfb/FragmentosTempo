using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriceratopsAnimationHandler : MonoBehaviour
{
    private Animator animator;                                      // Declara��o de uma vari�vel privada do tipo Animator.

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();                        // Obt�m o componente Animator associado a este GameObject e armazena na vari�vel animator.
    }

    public void PlayIdle() => animator.Play("Idle");                // M�todo p�blico para executar a anima��o "Idle".
    public void PlayWalk() => animator.Play("Walk");                // M�todo p�blico para executar a anima��o "Walk".
    public void PlayPrepareCharge() => animator.Play("Charge");     // M�todo p�blico para executar a anima��o "Charge".
    public void PlayRun() => animator.Play("Run");                  // M�todo p�blico para executar a anima��o "Run".
    public void PlayTailAttack() => animator.Play("TailWhip");      // M�todo p�blico para executar a anima��o "TailWhip".
    public void PlayEarthquake() => animator.Play("Stomp");         // M�todo p�blico para executar a anima��o "Stomp".
}
