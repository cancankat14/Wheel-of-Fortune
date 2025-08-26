using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace Vertigo.Wheel
{
    public class RewardCardView : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] Image background;
        [SerializeField] TextMeshProUGUI amountText;
        public void Bind(Reward reward, int amount, Color bg)
        {
            if (icon) icon.sprite = reward ? reward.icon : null;
            if (background) background.color = bg;

            if (!amountText) return;
            if (!reward)
            {
                amountText.text = "";
                return;
            }

            amountText.text =
                (reward.type == RewardType.Currency || reward.type == RewardType.Chest)
                    ? $"+{amount}"
                    : (reward.type == RewardType.Bomb ? "" : $"x{amount}");
        }

        
#if UNITY_EDITOR
        void OnValidate()
        {
           
            //only find in children 
            if (!icon)
                icon =UtilityHelper.FindChildComponentDeep<Image>(transform, "ui_image_card");
            if (!background)
                background =UtilityHelper.FindChildComponentDeep<Image>(transform, "ui_image_card_background");
            if (!amountText)
                amountText =UtilityHelper.FindChildComponentDeep<TextMeshProUGUI>(transform, "ui_text_amount_value");
            
        }
#endif
    }
    
}