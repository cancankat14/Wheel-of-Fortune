
using TMPro;
using UnityEngine;

namespace Vertigo.Wheel
{
    public class UIManager : Singleton<UIManager>, IManager
    {
        [Header("UI Dependencies")] [Tooltip(" Add Game Panel")] [SerializeField] [MustBeAssigned]
        private Canvas mainCanvas;
        public Canvas MainCanvas => mainCanvas;
        [SerializeField]
        [MustBeAssigned]
        private GameObject spinPanel;
        public GameObject SpinPanel => spinPanel;
    
        [SerializeField]
        [MustBeAssigned]
        private GameObject lootImage;
        public GameObject LootImage => lootImage;
        [Tooltip("Loose Panel")]
        [SerializeField]
        [MustBeAssigned]
        private GameObject endPanel;
        public GameObject EndPanel => endPanel;
    
        [Tooltip("Add Walk Away Panel")]
        [SerializeField]
        [MustBeAssigned]
        private GameObject leavePanel;
        public GameObject LeavePanel => leavePanel;
    
        [Tooltip("Add Zone Welcome Panel")]
        [SerializeField]  
        [MustBeAssigned]
        GameObject zoneEnterPanel;
        public GameObject ZoneEnterPanel => zoneEnterPanel;
        
        [Tooltip("Anim Layer")]
        [SerializeField]  
        [MustBeAssigned]
        RectTransform animPanel;
        public RectTransform AnimPanel => animPanel;

        [Header("SPIN PANEL RELATED")] 
        [SerializeField]
        [MustBeAssigned]
        private TextMeshProUGUI spinHeadlineText;
        public TextMeshProUGUI SpinHeadlineText => spinHeadlineText;
        [SerializeField]
        [MustBeAssigned]
        private TextMeshProUGUI spinDescriptionText;
        public TextMeshProUGUI SpinDescriptionText => spinDescriptionText;
        [SerializeField] [MustBeAssigned] 
        private Transform chestAnchorTransform;
        public Transform ChestAnchorTransform => chestAnchorTransform;
        [SerializeField] [MustBeAssigned] private Wheel wheel;
        
        
        
        [Header("ZONE PANEL")] 
        public TextMeshProUGUI zoneNameText;
        public IUIAnimation zoneEnterAnimation;

        protected override void Awake()
        {
            base.Awake();
            zoneEnterAnimation = zoneEnterPanel.GetComponent<IUIAnimation>();
        }

        public void Loose()
        {
            endPanel.SetActive(true);
            endPanel.transform.SetAsLastSibling();
        }

        public void LooseStash()
        {
            InventoryManager.Instance?.ClearStash();  // wipe current run
        }
        public void LeavePanelShow()
        {
            leavePanel.SetActive(true);
            leavePanel.transform.SetAsLastSibling();
        }
        public void ZoneEnter()
        {
            zoneEnterPanel.SetActive(true);
            zoneEnterPanel.transform.SetAsLastSibling();
        }
    }

}