using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAnimationExit : StateMachineBehaviour
{
    public SwordController controller;
    private PlayerController playerController;

    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerController = FindFirstObjectByType<PlayerController>();
        playerController.AttackEnded(); 
        controller = FindFirstObjectByType<SwordController>();
        controller.DisableSwordCollider();
    }

}
