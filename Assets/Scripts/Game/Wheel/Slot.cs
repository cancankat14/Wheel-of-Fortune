using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Vertigo.Wheel
{
    
    public class Slot : MonoBehaviour
{
    [SerializeField] Image ui_image_icon;
    [SerializeField] TextMeshProUGUI ui_text_amount_value;

    [Serializable]
    public struct Slice
    {
        public Reward reward;
        public int baseAmount; // optional but recommended
        public int amount;     // effective (after multiplier)
    }

    [SerializeField] private Slice mySlice;     // make private to avoid silent in-place edits

    public Slice MySlice
    {
        get => mySlice;
    }
    public Slice CurrentSlice => mySlice;       // read-only accessor
    public int Amount { get; private set; }
    public bool IsBomb => mySlice.reward && mySlice.reward.type == RewardType.Bomb;
    // Preferred: a single API to set & refresh
    public void SetSlice(Slice slice) => UpdateAccordingToSliceInfo(slice);
    public void UpdateAccordingToSliceInfo(Slice slice)
    {
        mySlice = slice;

        if (ui_image_icon) ui_image_icon.sprite = mySlice.reward ? mySlice.reward.icon : null;
        if (!ui_text_amount_value) return;

        if (mySlice.reward == null)
        {
            ui_text_amount_value.text = "";
            Amount = 0;
            return;
        }

        switch (mySlice.reward.type)
        {
            case RewardType.Bomb:
                ui_text_amount_value.text = "Bomb";
                Amount = 0;
                break;

            case RewardType.Currency:
            case RewardType.Chest:
            case RewardType.Gold:
                Amount = mySlice.amount; // <-- important
                ui_text_amount_value.text = Amount > 0 ? $"+{Amount}" : "";
                break;

            default:
                ui_text_amount_value.text = "";
                Amount = 0;
                break;
        }
    }
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!ui_image_icon) ui_image_icon = GetComponentInChildren<Image>(true);
        if (!ui_text_amount_value) ui_text_amount_value = GetComponentInChildren<TextMeshProUGUI>(true);

        // Optional: live-refresh when you tweak the slice in Play Mode
        if (Application.isPlaying)
            UpdateAccordingToSliceInfo(mySlice);
    }
#endif
}
    
}