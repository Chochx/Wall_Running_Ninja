using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAnimationExit : StateMachineBehaviour
{
    public SwordController swordController;
    private PlayerController playerController;
    private Collider2D animationCollider;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animationCollider = animator.GetComponentInChildren<HitBox>().hitCollider;
        playerController = animator.GetComponent<PlayerController>();
        swordController = animator.GetComponent<SwordController>();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animationCollider != null)
        {
            animationCollider.enabled = true;
        }
        if (playerController != null)
        {
            playerController.AttackEnded();
        }
        if (swordController != null)
        {
            swordController.DisableSwordCollider();
        }
    }

}
