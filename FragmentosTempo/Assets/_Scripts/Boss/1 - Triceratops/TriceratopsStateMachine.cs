using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriceratopsState { Idle, Patrol, Chase, PrepareCharge, Charge, Stuck, TailAttack, Earthquake }

[RequireComponent(typeof(Animator))]
public class TriceratopsStateMachine : MonoBehaviour
{
    public TriceratopsState currentState = TriceratopsState.Idle;

    private TriceratopsBoss boss;
    private TriceratopsAnimationHandler animHandler;

    // Start is called before the first frame update
    void Start()
    {
        boss = GetComponent<TriceratopsBoss>();
        animHandler = GetComponent<TriceratopsAnimationHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case TriceratopsState.Idle:
                animHandler.PlayIdle();
                boss.Idle();
                break;

            case TriceratopsState.Patrol:
                animHandler.PlayWalk();
                boss.Patrol();
                break;

            case TriceratopsState.Chase:
                animHandler.PlayRun();
                boss.ChasePlayer();
                break;

            case TriceratopsState.PrepareCharge:
                animHandler.PlayPrepareCharge();
                boss.PrepareCharge();
                break;

            case TriceratopsState.Charge:
                animHandler.PlayRun();
                boss.Charge();
                break;

            case TriceratopsState.Stuck:
                animHandler.PlayIdle();
                boss.HandleStuck();
                break;

            case TriceratopsState.TailAttack:
                animHandler.PlayTailAttack();
                boss.TailAttack();
                break;

            case TriceratopsState.Earthquake:
                animHandler.PlayEarthquake();
                boss.Earthquake();
                break;
        }
    }

    public void ChangeState(TriceratopsState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
        }
    }
}
