
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel
{
    public class LeavePanelController : MonoBehaviour
    {
        [Header("List")] 
        [SerializeField] Transform content; // Grid/VerticalLayoutGroup parent
        [SerializeField] RewardCardView cardPrefab; // small card (icon/name/xN)
        [SerializeField] bool newestFirst = false;

        [Header("Buttons (auto-wired)")] 
        [SerializeField] Button btn_continue;
        [SerializeField] Button btn_leave;
        [Header("Flow")] [SerializeField] float restartDelay = 0.75f; // tiny feedback pause


        void OnEnable()
        {
            RegisterButtons();
            Refresh();
            if (InventoryManager.Instance)
                InventoryManager.Instance.OnStashChanged += Refresh;
        }

        void OnDisable()
        {
            UnregisterButtons();
            if (InventoryManager.Instance)
                InventoryManager.Instance.OnStashChanged -= Refresh;
        }

        // ---- UI list ----
        public void Refresh()
        {
            if (!content || !cardPrefab) return;

            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            var hist = InventoryManager.Instance?.GetStashHistory();
            if (hist == null || hist.Count == 0) return;

            if (newestFirst)
                for (int i = hist.Count - 1; i >= 0; i--)
                    Spawn(hist[i].reward, hist[i].amount);
            else
                for (int i = 0; i < hist.Count; i++)
                    Spawn(hist[i].reward, hist[i].amount);
        }

        void Spawn(Reward reward, int amount)
        {
            var row = Instantiate(cardPrefab, content);
            row.Bind(reward, amount, GetBgFor(reward));
        }

        Color GetBgFor(Reward reward)
        {
            switch (reward?.type)
            {
                case RewardType.Currency: return new Color32(0x2B, 0x9F, 0x6B, 0xFF);
                case RewardType.Chest: return new Color32(0x80, 0x6E, 0xBF, 0xFF);
                case RewardType.Upgrade: return new Color32(0xF2, 0x9F, 0x05, 0xFF);
                case RewardType.Cosmetic: return new Color32(0xE0, 0x5A, 0x87, 0xFF);
                case RewardType.Gold: return new Color32(0xFF, 0xB2, 0x5A, 0xFF);
                default: return new Color32(0x33, 0x33, 0x33, 0xFF);
            }
        }

        // ---- Buttons ----
        void RegisterButtons()
        {
            if (btn_continue)
            {
                btn_continue.onClick.RemoveListener(OnContinue);
                btn_continue.onClick.AddListener(OnContinue);
            }

            if (btn_leave)
            {
                btn_leave.onClick.RemoveListener(OnLeave);
                btn_leave.onClick.AddListener(OnLeave);
            }
        }

        void UnregisterButtons()
        {
            if (btn_continue) btn_continue.onClick.RemoveListener(OnContinue);
            if (btn_leave) btn_leave.onClick.RemoveListener(OnLeave);
        }

        void OnContinue()
        {
            gameObject.SetActive(false);
        }

        void OnLeave()
        {
            var inv = InventoryManager.Instance;
            if (inv == null)
            {
                gameObject.SetActive(false);
                return;
            }

            // 1) Snapshot current stash for logging (banking will clear it)
            var hist = inv.GetStashHistory();
            int goldStash = inv.GetGoldStash();

            Debug.Log("[Leave] Cashing out…");
            if (hist != null)
            {
                for (int i = 0; i < hist.Count; i++)
                {
                    var r = hist[i].reward;
                    Debug.Log($"[Leave]  • {r?.name ?? "Unknown"} x{hist[i].amount}");
                }
            }

            if (goldStash > 0)
                Debug.Log($"[Leave]  • GOLD x{goldStash}");

            // 2) Commit: move stash -> bank
            if (GameManager.Instance) GameManager.Instance.OnConfirmLeave();
            else inv.BankStash();

            // 3) Close panel + small feedback delay, then restart (debug flow)
            gameObject.SetActive(false);
            DOVirtual.DelayedCall(restartDelay, GameManager.Restart, true);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // auto-assign by child names; rename in hierarchy if needed
            if (!btn_continue)
                btn_continue = transform.Find("ui_button_continue")?.GetComponent<Button>();
            if (!btn_leave)
                btn_leave = transform.Find("ui_button_leave")?.GetComponent<Button>();

            if (!content)
                content = transform.Find("ui_content_list");

            if (!cardPrefab)
                cardPrefab = GetComponentInChildren<RewardCardView>(true);
        }
#endif
    }
}