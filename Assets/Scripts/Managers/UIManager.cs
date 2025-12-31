using UnityEngine;
using UnityEngine.UI;

public class UIManager: MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] public PannelManager pannelManager;
    [Header("Pannels")]
    [SerializeField] public GameObject GamePannel;
    [SerializeField] public GameObject InventoryPannel;
    [SerializeField] public GameObject QuestPannel;
    [SerializeField] public GameObject HeroPannel;
    [SerializeField] public GameObject BuildingPannel;
    [SerializeField] public GameObject SummonPlayerPannel;
    [SerializeField] public GameObject GoToQuestPannel;
    [SerializeField] public GameObject BlackSmith;
    [SerializeField] public GameObject PauseMenuPannel;

    [Header("Buttons")]
    [SerializeField] public Button InventoryButton;
    [SerializeField] public Button QuestsButton;
    [SerializeField] public Button HeroesButton;
    [SerializeField] public Button BuildingsButton;
    [SerializeField] public Button GoToQuestButton;

    [SerializeField] public Button AddBerberian;
    [SerializeField] public Button DelBerberian;
    [SerializeField] public Button AddArcher;
    [SerializeField] public Button DelArcher;
    [SerializeField] public Button AddGiant;
    [SerializeField] public Button DelGiant;
    [SerializeField] public Button AddWiz;
    [SerializeField] public Button DelWiz;
    [SerializeField] public Button AddZimbie;
    [SerializeField] public Button DelZimbie;
    [SerializeField] public Button AddValkyri;
    [SerializeField] public Button DelValkyri;

    [SerializeField] public Button SummonButton;

    [SerializeField] public Button QAddBerberian;
    [SerializeField] public Button QDelBerberian;
    [SerializeField] public Button QAddArcher;
    [SerializeField] public Button QDelArcher;
    [SerializeField] public Button QAddGiant;
    [SerializeField] public Button QDelGiant;
    [SerializeField] public Button QAddWiz;
    [SerializeField] public Button QDelWiz;
    [SerializeField] public Button QAddZimbie;
    [SerializeField] public Button QDelZimbie;
    [SerializeField] public Button QAddValkyri;
    [SerializeField] public Button QDelValkyri;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


}
