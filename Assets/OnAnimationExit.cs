using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAnimationExit : StateMachineBehaviour
{
    public SwordController controller;

    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = FindAnyObjectByType<SwordController>();
        controller.DisableSwordCollider();
    }

}
