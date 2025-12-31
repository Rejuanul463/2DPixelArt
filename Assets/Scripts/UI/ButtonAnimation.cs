using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{

    [SerializeField] Animator startButton;
    [SerializeField] Animator settings;
    [SerializeField] Animator Continue;
    [SerializeField] int st = 0;
    [SerializeField] int set = 1;
    [SerializeField] int con = 2;



    private void Awake()
    {
        Continue.Play("Left");
        settings.Play("Right");
        startButton.Play("RightBack");
    }

    public void swipeRight()
    {
        startButton.Play(getRightName(st));
        st = (st+1) % 3;
        settings.Play(getRightName(set));
        set = (set+1) % 3;
        Continue.Play(getRightName(con));
        con = (con+1) % 3;
    }

    private string getRightName(int idx)
    {
        if (idx == 0)
        {
            return "RightBack";
        }
        else if (idx == 1)
        {
            return "Right";
        }
        else
        {
            return "RightLeft";
        }
    }

    private string getLeftName(int idx)
    {
        if (idx == 0)
        {
            return "LeftRight";
        }
        else if (idx == 1)
        {
            return "Left";
        }
        else
        {
            return "LeftBack";
        }
    }

    public void swipeLeft()
    {
        startButton.Play(getLeftName(st));
        st = (3 + st - 1) % 3;
        settings.Play(getLeftName(set));
        set = (3 +  set - 1) % 3;
        Continue.Play(getLeftName(con));
        con = (3 +  con - 1) % 3;
    }
}
