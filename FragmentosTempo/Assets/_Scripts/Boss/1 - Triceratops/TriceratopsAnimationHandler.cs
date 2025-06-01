using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriceratopsAnimationHandler : MonoBehaviour
{
    private Animator animator;                                      // Declaração de uma variável privada do tipo Animator.

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();                        // Obtém o componente Animator associado a este GameObject e armazena na variável animator.
    }

    public void PlayIdle() => animator.Play("Idle");                // Método público para executar a animação "Idle".
    public void PlayWalk() => animator.Play("Walk");                // Método público para executar a animação "Walk".
    public void PlayPrepareCharge() => animator.Play("Charge");     // Método público para executar a animação "Charge".
    public void PlayRun() => animator.Play("Run");                  // Método público para executar a animação "Run".
    public void PlayTailAttack() => animator.Play("TailWhip");      // Método público para executar a animação "TailWhip".
    public void PlayEarthquake() => animator.Play("Stomp");         // Método público para executar a animação "Stomp".
}
