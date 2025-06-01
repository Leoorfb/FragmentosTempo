using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enumeração que define todos os estados possíveis do Triceratops.
public enum TriceratopsState { Idle, Patrol, Chase, PrepareCharge, Charge, Stuck, TailAttack, Earthquake }

[RequireComponent(typeof(Animator))]                                    // Garante que o componente Animator está presente no GameObject.
public class TriceratopsStateMachine : MonoBehaviour
{
    public TriceratopsState currentState = TriceratopsState.Idle;       // Estado atual do Triceratops, começa como Idle.

    private TriceratopsBoss boss;                                       // Referência ao script principal do comportamento do Triceratops.
    private TriceratopsAnimationHandler animHandler;                    // Referência ao controlador de animações.
    private AudioSource stateAudioSource;                               // Fonte de áudio.
    private string currentLoopSound = "";                               // Armazena o nome do som atual.

    // Start is called before the first frame update
    void Start()
    {
        boss = GetComponent<TriceratopsBoss>();                         // Obtém o componente TriceratopsBoss.
        animHandler = GetComponent<TriceratopsAnimationHandler>();      // Obtém o componente de animações.
        stateAudioSource = GetComponent<AudioSource>();                 // Obtém o componente de som.
        stateAudioSource.loop = true;                                   // Sons contínuos devem ser loop.
        stateAudioSource.spatialBlend = 1f;                             // Garantir que é 3D.
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)                               // Verifica o estado atual e executa a animação e comportamento correspondente.
        {
            case TriceratopsState.Idle:                     // Estado de Idle.
                animHandler.PlayIdle();                     // Toca a animação de idle.
                boss.Idle();                                // Executa a lógica de idle.
                StopLoopSound();                            // Parar sons contínuos.
                break;

            case TriceratopsState.Patrol:                   // Estado de patrulha.
                animHandler.PlayWalk();                     // Toca a animação de Walk.
                boss.Patrol();                              // Executa a lógica de patrulha.
                PlayLoopSound("TriceWalk");                 // Inicia o som contínuo de Walk.
                break;

            case TriceratopsState.Chase:                    // Estado de Chase.
                animHandler.PlayRun();                      // Toca a animação de Run.
                boss.ChasePlayer();                         // Executa a lógica de Chase.
                PlayLoopSound("TriceRun");                  // Inicia o som contínuo de Run.
                break;

            case TriceratopsState.PrepareCharge:            // Estado de PrepareCharge.
                animHandler.PlayPrepareCharge();            // Toca a animação de PrepareCharge.
                boss.PrepareCharge();                       // Executa a lógica de PrepareCharge.
                StopLoopSound();                            // Parar sons contínuos.
                break;

            case TriceratopsState.Charge:                   // Estado de Charge.
                animHandler.PlayRun();                      // Toca a animação de Run.
                boss.Charge();                              // Executa a lógica de Charge.
                PlayLoopSound("TriceCharge");               // Inicia o som contínuo de Investida.
                break;

            case TriceratopsState.Stuck:                    // Estado de Stuck.
                animHandler.PlayIdle();                     // Fica parado, toca animação de Idle.
                boss.HandleStuck();                         // Executa a lógica de estar preso.
                StopLoopSound();                            // Parar sons contínuos.
                break;

            case TriceratopsState.TailAttack:               // Estado de TailAttack.
                animHandler.PlayTailAttack();               // Toca a animação de TailAttack.
                boss.TailAttack();                          // Executa a lógica de TailAttack.
                StopLoopSound();                            // Parar sons contínuos.
                break;

            case TriceratopsState.Earthquake:               // Estado de Earthquake.
                animHandler.PlayEarthquake();               // Toca a animação de Earthquake.
                boss.Earthquake();                          // Executa a lógica de Earthquake.
                StopLoopSound();                            // Parar sons contínuos.
                break;
        }
    }

    public void ChangeState(TriceratopsState newState)      // Método para mudar o estado atual do Triceratops.
    {
        if (currentState != newState)                       // Só muda se o novo estado for diferente do atual.
        {
            currentState = newState;

            // Sons únicos para determinados estados
            if (newState == TriceratopsState.PrepareCharge)
            {
                SoundManager.Instance.PlaySound3D("TricePrepare", transform.position);      // Som 3D de Preparação.
            }
            else if (newState == TriceratopsState.TailAttack)
            {
                SoundManager.Instance.PlaySound3D("TriceTail", transform.position);         // Som 3D de TailAttack.
            }
        }
    }

    private void PlayLoopSound(string soundName)            // Método para iniciar um som contínuo (loop).
    {
        if (currentLoopSound == soundName) return;          // Se já está tocando esse som, não faz nada.

        AudioClip clip = SoundManager.Instance.GetClipByName(soundName);        // Obtém o áudio pelo nome.
        if (clip != null)
        {
            stateAudioSource.clip = clip;                   // Define o áudio na fonte de som.
            stateAudioSource.Play();                        // Reproduz o som.
            currentLoopSound = soundName;                   // Marca como o som atual.
        }
    }

    private void StopLoopSound()                            // Método para parar o som contínuo.
    {
        if (stateAudioSource.isPlaying)                     // Se está tocando.
        {
            stateAudioSource.Stop();                        // Para o som.
            currentLoopSound = "";                          // Limpa a marcação do som atual.
        }
    }
}
