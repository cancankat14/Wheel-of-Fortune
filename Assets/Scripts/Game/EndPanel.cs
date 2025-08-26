using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Vertigo.Wheel
{
    public class EndPanel : MonoBehaviour
    {
        [Header("Buttons (auto-wired)")] 
        [SerializeField] Button btn_give_up;
        [SerializeField] Button btn_revive;
        [SerializeField] Button btn_revive_ad;

        [Header("Revive")] 
        [SerializeField] int reviveCost = 2;

        [SerializeField] private TextMeshProUGUI reviveText; 

        Sequence _seq;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!btn_give_up) btn_give_up = transform.Find("ui_button_giveup")?.GetComponent<Button>();
            if (!btn_revive) btn_revive = transform.Find("ui_button_revive_currency")?.GetComponent<Button>();
            if (!btn_revive_ad) btn_revive_ad = transform.Find("ui_button_revive_ad")?.GetComponent<Button>();
            if(!reviveText) reviveText = transform.Find("ui_text_gold_value")?.GetComponent<TextMeshProUGUI>();
        }
#endif
        void OnEnable()
        {
            Register();
            RefreshReviveUI();
            FadeIn();
        }

        void OnDisable()
        {
            Unregister();
            _seq?.Kill();
        }

        void Register()
        {
            if (btn_give_up) btn_give_up.onClick.AddListener(OnGiveUp);
            if (btn_revive) btn_revive.onClick.AddListener(OnRevive);
            if (btn_revive_ad) btn_revive_ad.onClick.AddListener(OnReviveAd);

            var inv = InventoryManager.Instance;
            if (inv != null)
            {
                inv.OnBankChanged += RefreshReviveUI;
                inv.OnStashChanged += RefreshReviveUI; // <-- also refresh when stash gold changes
            }
        }

        void Unregister()
        {
            if (btn_give_up) btn_give_up.onClick.RemoveListener(OnGiveUp);
            if (btn_revive) btn_revive.onClick.RemoveListener(OnRevive);
            if (btn_revive_ad) btn_revive_ad.onClick.RemoveListener(OnReviveAd);

            var inv = InventoryManager.Instance;
            if (inv != null)
            {
                inv.OnBankChanged -= RefreshReviveUI;
                inv.OnStashChanged -= RefreshReviveUI;
            }
        }

        void RefreshReviveUI()
        {
            int goldTotal = InventoryManager.Instance ? InventoryManager.Instance.GetGoldTotal() : 0;
            if (reviveText) reviveText.text = reviveCost.ToString(); 
            if (btn_revive) btn_revive.interactable = goldTotal >= reviveCost;
        }

        void OnRevive()
        {
            var inv = InventoryManager.Instance;
            if (inv != null && inv.TrySpendGoldAny(reviveCost)) // spend from bank then stash
            {
                Hide();
                LevelManager.Instance?.RetryCurrentZone();
            }
            else
            {
                Debug.Log($"[EndPanel] Not enough gold. Need {reviveCost}, have {inv?.GetGoldTotal() ?? 0}.");
                RefreshReviveUI();
            }

            reviveCost *= 2;
        }
        void OnGiveUp()
        {
            InventoryManager.Instance?.ClearStash(); // wipe current run
            GameManager.Restart(); 
        }
        void OnReviveAd()
        {
            // No Ad for now
            Hide();
            LevelManager.Instance?.RetryCurrentZone();
        }
        void FadeIn()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}