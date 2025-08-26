using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Tier = Vertigo.Wheel.WheelTier;  

namespace Vertigo.Wheel
{
    public class LevelManager : Singleton<LevelManager>, IManager
    {
        [SerializeField] private ProgressionConfig progressionConfig;
        [SerializeField] private Wheel wheel;

        int _zoneIndex = 1; 
        public int CurrentZone => _zoneIndex;

        protected override void Awake()
        {
            base.Awake();
            EnsureProgressionConfig();
        }

        private void Start()
        {
            StartNewZone();
        }

        public void NextZone()
        {
            _zoneIndex++;
            UIManager.Instance.zoneNameText.text = "Zone " + _zoneIndex;
            StartNewZone();
        }

        void StartNewZone()
        {

            UIManager.Instance.ZoneEnterPanel.SetActive(true);

            int zone = CurrentZone;

            bool isSuper = progressionConfig != null && zone % progressionConfig.superEvery == 0;
            bool isSafe = progressionConfig != null && zone % progressionConfig.safeEvery == 0;

            Wheel.WheelType tier = isSuper ? Wheel.WheelType.Golden
                : isSafe ? Wheel.WheelType.Silver
                : Wheel.WheelType.Bronze;

            Tier progTier = isSuper ? Tier.Gold
                : isSafe ? Tier.Silver
                : Tier.Bronze;

            float multiplier = (progressionConfig != null)
                ? progressionConfig.EvaluateZoneMultiplier(zone, progTier)
                : 1f;

            wheel.GenerateWheel(tier, multiplier);
            
            const float delay = 0.15f;
            var anim = UIManager.Instance.zoneEnterAnimation;

            if (anim != null)
            {
                anim.OnComplete = () =>
                {
                    UIManager.Instance.ZoneEnterPanel.SetActive(false);
                    anim.OnComplete = null; // optional: one-shot safety
                };
                DOVirtual.DelayedCall(delay, anim.Play, true);
            }
            else
            {
                DOVirtual.DelayedCall(delay, () => UIManager.Instance.ZoneEnterPanel.SetActive(false), true);
            }


            wheel.OpenSpinButton();
            if (tier == Wheel.WheelType.Silver || tier == Wheel.WheelType.Golden)
                UIManager.Instance.LeavePanelShow();
        }




        // ------ Helpers ------

        static Wheel.WheelType GetTierForZone(int zone, ProgressionConfig cfg)
        {
            if (zone % cfg.superEvery == 0) return Wheel.WheelType.Golden; // super
            if (zone % cfg.safeEvery == 0) return Wheel.WheelType.Silver; // safe
            return Wheel.WheelType.Bronze; // risky
        }

        static float GetMultiplierForZone(int zone, ProgressionConfig cfg, Wheel.WheelType tier)
        {
            float baseMul = 1f + cfg.perZoneStep * (zone - 1);
            switch (tier)
            {
                case Wheel.WheelType.Golden: baseMul *= cfg.goldFactor; break;
                case Wheel.WheelType.Silver: baseMul *= cfg.silverFactor; break;
            }

            return Mathf.Clamp(baseMul, 1f, cfg.cap);
        }

        static string BuildZoneTitle(int zone, Wheel.WheelType tier)
        {
            switch (tier)
            {
                case Wheel.WheelType.Golden: return $"ZONE {zone} — GOLD (SUPER)";
                case Wheel.WheelType.Silver: return $"ZONE {zone} — SILVER (SAFE)";
                default: return $"ZONE {zone} — BRONZE";
            }
        }

        static bool IsSafeZone(int zone, ProgressionConfig cfg = null)
        {
            if (cfg == null) return zone % 5 == 0;
            return zone % cfg.safeEvery == 0;
        }

        static bool IsSuperZone(int zone, ProgressionConfig cfg = null)
        {
            if (cfg == null) return zone % 30 == 0;
            return zone % cfg.superEvery == 0;
        }

        public void RetryCurrentZone()
        {
            var ui = UIManager.Instance;
            if (!ui) return;

            // Recompute tier/multiplier for the same zone
            var tierEnum = progressionConfig.GetTierForZone(CurrentZone); // WheelTier
            var wtype = tierEnum == WheelTier.Gold
                ? Wheel.WheelType.Golden
                : tierEnum == WheelTier.Silver
                    ? Wheel.WheelType.Silver
                    : Wheel.WheelType.Bronze;

            float mul = progressionConfig.EvaluateZoneMultiplier(CurrentZone, tierEnum);

            ui.EndPanel.SetActive(false);
            wheel.GenerateWheel(wtype, mul);
            wheel.OpenSpinButton();
        }


        #region Config Fallback

        private void EnsureProgressionConfig()
        {
            if (progressionConfig != null) return;

            var loaded = Resources.Load<ProgressionConfig>("GameData/ProgressionConfig_Default");
            if (loaded != null)
            {
                progressionConfig = loaded;
                return;
            }

            progressionConfig = ScriptableObject.CreateInstance<ProgressionConfig>();
            progressionConfig.safeEvery = 5;
            progressionConfig.superEvery = 30;
            progressionConfig.perZoneStep = 0.15f;
            progressionConfig.cap = 10f;
            progressionConfig.silverFactor = 1.25f;
            progressionConfig.goldFactor = 1.75f;

            Debug.LogWarning("[LevelManager] Using runtime fallback ProgressionConfig.");
        }

        #endregion
    }
}