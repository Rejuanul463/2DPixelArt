using TMPro;
using UnityEngine;

public class QuestButton : MonoBehaviour
{
    [SerializeField] public string QuestDetails;
    

    public void showQuest()
    {
        QuestManager.Instance.questUI.showQuest(QuestDetails);
    }


}
