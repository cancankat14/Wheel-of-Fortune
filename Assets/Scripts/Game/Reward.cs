// Assets/Scripts/Game/Reward.cs

using System;
using UnityEngine;
namespace Vertigo.Wheel
{
    [CreateAssetMenu(menuName = "Wheel/Reward", fileName = "Reward_")]
    [Serializable]
    public class Reward : ScriptableObject
    {
        [Tooltip("Unique id (e.g., 'bomb', 'coins', 'chest_bronze', 'skin_neon')")]
        public string id;

        public RewardType type;

        [Tooltip("Icon shown on the slice")] public Sprite icon;

        [Tooltip("Base amount for numeric rewards (Currency/Chest). 0 for Bomb/Cosmetic/Upgrade.")]
        public int baseAmount;
    }
}