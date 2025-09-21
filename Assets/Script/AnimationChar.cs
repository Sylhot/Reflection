using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationChar : MonoBehaviour
{
    public Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void CharAnimationIdle()
    {
        animator.Play("char_idle");
    }
    public void CharAnimationWalk()
    {
        animator.Play("char_walk");
    }
    public void CharAnimationIdleFront()
    {
        animator.Play("char_idle_front");
    }
    public void CharAnimationWalkFront()
    {
        animator.Play("char_walk_front");
    }      
    public void CharAnimationIdleSide()
    {
        animator.Play("char_idle_side");
    }   
    public void CharAnimationWalkSide()
    {
        animator.Play("char_walk_side");
    }   
}
