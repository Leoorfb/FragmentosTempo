using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriceratopsAnimationHandler : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayIdle() => animator.Play("Idle");
    public void PlayWalk() => animator.Play("Walk");
    public void PlayPrepareCharge() => animator.Play("Charge");
    public void PlayRun() => animator.Play("Run");
    public void PlayTailAttack() => animator.Play("TailWhip");
    public void PlayEarthquake() => animator.Play("Stomp");
}
