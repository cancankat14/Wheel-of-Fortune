using System;
using System.Collections.Generic;
using UnityEngine;


namespace Vertigo.Wheel
{
    [CreateAssetMenu(menuName = "Wheel/WheelConfig", fileName = "Wheel_")]
    public class WheelConfig : ScriptableObject
    {
        public Sprite wheelSprite;

        [Serializable]
        public struct RewardAndChance
        {
            public Reward reward; 
            [Range(0f, 100f)] public float chance; // weight; doesn’t need to sum to 100
        }

        [Tooltip("Pool to sample from for this wheel (Bronze/Silver/Gold)")]
        public List<RewardAndChance> possibleRewards = new();

        [Tooltip("For Bronze only. Silver/Gold must be 0.")] [Min(0)]
        public int howManyBomb = 1;
    }
}