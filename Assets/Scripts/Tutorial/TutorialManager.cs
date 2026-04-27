using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public Animator animator;

    public void PlayAnimation()
    {
        animator.SetTrigger("PlayAnimation");
    }
}