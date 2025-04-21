using Unity.Netcode.Components;
using UnityEngine;

public class PlayerCharacterAnim : NetworkAnimator
{
    private NetworkAnimator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<NetworkAnimator>();
    }

    public void PlayAnimationPass()
    {
        animator.SetTrigger("Pass");
    }

    public void PlayAnimationMove()
    {
        animator.SetTrigger("Move");
    }

    public void PlayAnimationDribble()
    {
        animator.SetTrigger("Dribble");
    }

    public void PlayAnimationShoot()
    {
        animator.SetTrigger("Shoot");
    }

    public void PlayAnimationTackle()
    {
        animator.SetTrigger("Tackle");
    }

    public void PlayAnimationBlock()
    {
        animator.SetTrigger("Block");
    }

    public void PlayAnimationReceive()
    {
        animator.SetTrigger("Receive");
    }

    public void PlayAnimationIdle()
    {
        animator.SetTrigger("Idle");
    }
    public void PlayAnimationTrip()
    {
        animator.SetTrigger("Trip");
    }
}
