using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enumera��o que define todos os estados poss�veis do Triceratops.
public enum TriceratopsState { Idle, Patrol, Chase, PrepareCharge, Charge, Stuck, TailAttack, Earthquake }

[RequireComponent(typeof(Animator))]                                    // Garante que o componente Animator est� presente no GameObject.
public class TriceratopsStateMachine : MonoBehaviour
{
    public TriceratopsState currentState = TriceratopsState.Idle;       // Estado atual do Triceratops, come�a como Idle.

    private TriceratopsBoss boss;                                       // Refer�ncia ao script principal do comportamento do Triceratops.
    private TriceratopsAnimationHandler animHandler;                    // Refer�ncia ao controlador de anima��es.
    private AudioSource stateAudioSource;                               // Fonte de �udio.
    private string currentLoopSound = "";                               // Armazena o nome do som atual.

    // Start is called before the first frame update
    void Start()
    {
        boss = GetComponent<TriceratopsBoss>();                         // Obt�m o componente TriceratopsBoss.
        animHandler = GetComponent<TriceratopsAnimationHandler>();      // Obt�m o componente de anima��es.
        stateAudioSource = GetComponent<AudioSource>();                 // Obt�m o componente de som.
        stateAudioSource.loop = true;                                   // Sons cont�nuos devem ser loop.
        stateAudioSource.spatialBlend = 1f;                             // Garantir que � 3D.
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)                               // Verifica o estado atual e executa a anima��o e comportamento correspondente.
        {
            case TriceratopsState.Idle:                     // Estado de Idle.
                animHandler.PlayIdle();                     // Toca a anima��o de idle.
                boss.Idle();                                // Executa a l�gica de idle.
                StopLoopSound();                            // Parar sons cont�nuos.
                break;

            case TriceratopsState.Patrol:                   // Estado de patrulha.
                animHandler.PlayWalk();                     // Toca a anima��o de Walk.
                boss.Patrol();                              // Executa a l�gica de patrulha.
                PlayLoopSound("TriceWalk");                 // Inicia o som cont�nuo de Walk.
                break;

            case TriceratopsState.Chase:                    // Estado de Chase.
                animHandler.PlayRun();                      // Toca a anima��o de Run.
                boss.ChasePlayer();                         // Executa a l�gica de Chase.
                PlayLoopSound("TriceRun");                  // Inicia o som cont�nuo de Run.
                break;

            case TriceratopsState.PrepareCharge:            // Estado de PrepareCharge.
                animHandler.PlayPrepareCharge();            // Toca a anima��o de PrepareCharge.
                boss.PrepareCharge();                       // Executa a l�gica de PrepareCharge.
                StopLoopSound();                            // Parar sons cont�nuos.
                break;

            case TriceratopsState.Charge:                   // Estado de Charge.
                animHandler.PlayRun();                      // Toca a anima��o de Run.
                boss.Charge();                              // Executa a l�gica de Charge.
                PlayLoopSound("TriceCharge");               // Inicia o som cont�nuo de Investida.
                break;

            case TriceratopsState.Stuck:                    // Estado de Stuck.
                animHandler.PlayIdle();                     // Fica parado, toca anima��o de Idle.
                boss.HandleStuck();                         // Executa a l�gica de estar preso.
                StopLoopSound();                            // Parar sons cont�nuos.
                break;

            case TriceratopsState.TailAttack:               // Estado de TailAttack.
                animHandler.PlayTailAttack();               // Toca a anima��o de TailAttack.
                boss.TailAttack();                          // Executa a l�gica de TailAttack.
                StopLoopSound();                            // Parar sons cont�nuos.
                break;

            case TriceratopsState.Earthquake:               // Estado de Earthquake.
                animHandler.PlayEarthquake();               // Toca a anima��o de Earthquake.
                boss.Earthquake();                          // Executa a l�gica de Earthquake.
                StopLoopSound();                            // Parar sons cont�nuos.
                break;
        }
    }

    public void ChangeState(TriceratopsState newState)      // M�todo para mudar o estado atual do Triceratops.
    {
        if (currentState != newState)                       // S� muda se o novo estado for diferente do atual.
        {
            currentState = newState;

            // Sons �nicos para determinados estados
            if (newState == TriceratopsState.PrepareCharge)
            {
                SoundManager.Instance.PlaySound3D("TricePrepare", transform.position);      // Som 3D de Prepara��o.
            }
            else if (newState == TriceratopsState.TailAttack)
            {
                SoundManager.Instance.PlaySound3D("TriceTail", transform.position);         // Som 3D de TailAttack.
            }
        }
    }

    private void PlayLoopSound(string soundName)            // M�todo para iniciar um som cont�nuo (loop).
    {
        if (currentLoopSound == soundName) return;          // Se j� est� tocando esse som, n�o faz nada.

        AudioClip clip = SoundManager.Instance.GetClipByName(soundName);        // Obt�m o �udio pelo nome.
        if (clip != null)
        {
            stateAudioSource.clip = clip;                   // Define o �udio na fonte de som.
            stateAudioSource.Play();                        // Reproduz o som.
            currentLoopSound = soundName;                   // Marca como o som atual.
        }
    }

    private void StopLoopSound()                            // M�todo para parar o som cont�nuo.
    {
        if (stateAudioSource.isPlaying)                     // Se est� tocando.
        {
            stateAudioSource.Stop();                        // Para o som.
            currentLoopSound = "";                          // Limpa a marca��o do som atual.
        }
    }
}
