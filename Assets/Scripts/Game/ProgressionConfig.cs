using UnityEngine;

namespace Vertigo.Wheel
{
    [CreateAssetMenu(menuName = "Wheel/Progression", fileName = "ProgressionConfig")]
    public class ProgressionConfig : ScriptableObject
    {
        [Header("Zone cadence")]
        public int safeEvery  = 5;   // 5,10,15,20,25...
        public int superEvery = 30;  // 30,60,90...

        [Header("Reward multiplier")]
        [Tooltip("Added per zone, starting at 1x on Zone 1")]
        public float perZoneStep = 0.15f;

        [Tooltip("Global hard cap for total multiplier")]
        public float cap = 10f;

        [Header("Tier factors (optional)")]
        public float silverFactor = 1.5f;
        public float goldFactor   = 3.0f;

        public bool IsSafe(int zone)  => zone % safeEvery  == 0;
        public bool IsSuper(int zone) => zone % superEvery == 0;
        
        public WheelTier GetTierForZone(int zone)
        {
            if (IsSuper(zone)) return WheelTier.Gold;    // 30, 60, ...
            if (IsSafe(zone))  return WheelTier.Silver;  // 5, 10, 15, ...
            return WheelTier.Bronze;
        }
        public float EvaluateZoneMultiplier(int zone, WheelTier tier)
        {
            zone = Mathf.Max(1, zone);
            float step = Mathf.Max(0f, perZoneStep);

            float baseMul = 1f + (zone - 1) * step;

            float tierMul = tier switch
            {
                WheelTier.Gold   => Mathf.Max(1f, goldFactor),
                WheelTier.Silver => Mathf.Max(1f, silverFactor),
                _                => 1f
            };

            float raw = baseMul * tierMul;
            float capped = Mathf.Min(Mathf.Max(1f, cap), raw); 
            return capped;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            safeEvery  = Mathf.Max(1, safeEvery);
            superEvery = Mathf.Max(1, superEvery);
            perZoneStep = Mathf.Max(0f, perZoneStep);
            cap = Mathf.Max(1f, cap);
            silverFactor = Mathf.Max(0.01f, silverFactor);
            goldFactor   = Mathf.Max(0.01f, goldFactor);
        }
#endif


    }
}