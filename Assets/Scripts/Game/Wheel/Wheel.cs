using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using DG.Tweening;
namespace Vertigo.Wheel
{
public class Wheel : MonoBehaviour
{
    public enum WheelType
    {
        Golden,
        Silver,
        Bronze
    }
    
    public static Action OnSpin;
    
    [Header("REWARD CARD")]
    [SerializeField] GameObject rewardCardPrefab; // the prefab with RewardCardView + UIFlyToTargetExact
    [SerializeField] Color bronzeCardBG = new Color32(0xC9, 0x7A, 0x39, 0xFF);
    [SerializeField] Color silverCardBG = new Color32(0xB6, 0xC3, 0xD9, 0xFF);
    [SerializeField] Color goldCardBG = new Color32(0xFF, 0xB2, 0x5A, 0xFF);
    
    [Header("Multiplier")]
    public float currentMultiplier;
    public TextMeshProUGUI rewardMultiplierText;
    
    [Header("Configs")]
    [MustBeAssigned] [SerializeField] private WheelConfig goldenConfig;
    [MustBeAssigned] [SerializeField] private WheelConfig silverConfig;
    [MustBeAssigned] [SerializeField] private WheelConfig bronzeConfig;

    [Header("Wheel Dependencies")]
    public Slot[] slots;
    public GameObject spinButton;
    public Image wheelImage;
    public TextMeshProUGUI wheelText;
    public TextMeshProUGUI wheelDescription;
    public Reward bomb;
    
    
    private IUIAnimation spinButtonAnimation;
    private WheelType myCurrentType;
    private List<Reward> rewards;
    RectTransform spinButtonRT;
    Button spinBtn; // <- cached button
    IUIAnimation _wheelPop; // cache

    private void Awake()
    {
        spinButtonAnimation = spinButton.GetComponent<IUIAnimation>();
        wheelRotRoot = GetComponent<RectTransform>();
        spinButtonRT = spinButton.GetComponent<RectTransform>();
        spinBtn = spinButton ? spinButton.GetComponent<Button>() : null;
        EnsureRefs(); // runtime-safe wiring
        _wheelPop = GetComponent<IUIAnimation>(); // the UISizeChangeAnimation on the wheel root

    }
    void EnsureRefs()
    {
        if (!spinButtonRT && spinButton)
            spinButtonRT = spinButton.GetComponent<RectTransform>();

        if (spinBtn == null)
        {
            if (spinButton)
                spinBtn = spinButton.GetComponent<Button>();
            if (spinBtn == null) // last resort: look under this object
                spinBtn = GetComponentInChildren<Button>(includeInactive: true);
        }
    }

    void OnEnable()
    {
        if (spinBtn) spinBtn.onClick.AddListener(OnSpinClicked);
    }

    void OnDisable()
    {
        if (spinBtn) spinBtn.onClick.RemoveListener(OnSpinClicked);
    }

    void OnSpinClicked()
    {
        if (isSpinning) return;

        if (_wheelPop != null)
        {
            _wheelPop.Stop(); // reset scale if a previous pop was mid-play
            _wheelPop.OnComplete = null; // avoid stacking callbacks
            _wheelPop.OnComplete = RotateWheel; // spin AFTER the DOTween pop finishes
            _wheelPop.Play(); // kick the quick size pop
        }
        else
        {
            RotateWheel(); // fallback if no pop anim present
        }
    }
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!spinButtonRT && spinButton) spinButtonRT = spinButton.GetComponent<RectTransform>();
        if (!spinBtn && spinButton) spinBtn = spinButton.GetComponent<Button>();
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.G))
            GenerateWheel(WheelType.Bronze, 1);

        if (Input.GetKeyDown(KeyCode.R))
            RotateWheel(); // normal random

        if (Input.GetKeyDown(KeyCode.D))
            RotateWheelCheatToBomb(); // CHEAT: land on bomb
    }
#endif
    
    #region Generate Wheel
    private WheelConfig myConfig;

    Reward PickRewardByWeight(List<WheelConfig.RewardAndChance> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        float total = 0f;
        for (int i = 0; i < pool.Count; i++)
            total += Mathf.Max(0.0001f, pool[i].chance);

        float roll = Random.value * total;
        float acc = 0f;
        for (int i = 0; i < pool.Count; i++)
        {
            float w = Mathf.Max(0.0001f, pool[i].chance);
            acc += w;

            if (roll <= acc) return pool[i].reward;
        }

        return pool[pool.Count - 1].reward;
    }
    public void GenerateWheel(WheelType type, float multiplier)
    {
        myCurrentType = type;

        // pick config
        myConfig = type == WheelType.Golden ? goldenConfig
            : type == WheelType.Silver ? silverConfig
            : bronzeConfig;

        if (myConfig == null)
        {
            Debug.LogWarning("[Wheel] Missing WheelConfig for " + type);
            return;
        }
        _wheelPop?.Stop(); // ← resets localScale to base
        KeepButtonUpright(); //Reset spin button rotation
        wheelImage.sprite = myConfig.wheelSprite;
        if (wheelText) wheelText.text = $"{GetTierLabel(myCurrentType)} SPIN";
        ApplyTitleGradient(myCurrentType);
        
        // clamp multiplier (cap x10 per spec) + UI
        currentMultiplier = Mathf.Clamp(multiplier, 1f, 10f);
        if (rewardMultiplierText) rewardMultiplierText.text = $"x{currentMultiplier:0.##}";

        // fill all slots from weighted pool
        RandomRewardCreator();

        // insert bombs ONLY on Bronze
        if (type == WheelType.Bronze && bomb != null && myConfig.howManyBomb > 0)
            ChangeRewardToBomb(myConfig.howManyBomb);

        spinButtonAnimation.Play();

    }

// When a new wheel is created to make sure the button looks normal
    void KeepButtonUpright()
    {
        if (!spinButtonRT || !wheelRotRoot) return;
        // counter the parent’s Z so the button looks upright in world/canvas space
        float parentZ = wheelRotRoot.localEulerAngles.z;
        spinButtonRT.localEulerAngles = new Vector3(0f, 0f, -parentZ);
    }
    void RandomRewardCreator()
    {
        var pool = myConfig != null ? myConfig.possibleRewards : null;
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("[Wheel] Empty reward pool.");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            var picked = PickRewardByWeight(pool);
            if (myCurrentType != WheelType.Bronze && picked == bomb)
            {
                int guard = 0;
                while (picked == bomb && guard++ < 16)
                    picked = PickRewardByWeight(pool);
            }

            int baseAmt = picked ? Mathf.Max(0, picked.baseAmount) : 0;
            int effAmt  = (picked && picked.type != RewardType.Bomb)
                ? Mathf.FloorToInt(baseAmt * currentMultiplier)
                : 0;

            var slice = new Slot.Slice { reward = picked, baseAmount = baseAmt, amount = effAmt };
            slots[i].SetSlice(slice);
        }

    }
    
// places 'amount' unique bombs in random slots
    void ChangeRewardToBomb(int amount)
    {
        int toPlace = Mathf.Clamp(amount, 0, slots.Length);
        if (toPlace == 0 || bomb == null) return;

        var used = new HashSet<int>();
        int safety = 0;
        while (used.Count < toPlace && safety++ < 100)
        {
            int idx = Random.Range(0, slots.Length);
            if (used.Contains(idx)) continue;

            var slice = new Slot.Slice { reward = bomb, baseAmount = 0, amount = 0 };
            slots[idx].SetSlice(slice);
            used.Add(idx);
        }

    }
    static string GetTierLabel(Wheel.WheelType t) => t switch
    {
        WheelType.Golden => "GOLDEN",
        WheelType.Silver => "SILVER",
        WheelType.Bronze => "BRONZE",
        _ => "WHEEL"
    };
    void ApplyTitleGradient(WheelType type)
    {
        if (!wheelText) return;

        wheelText.enableVertexGradient = true;

        // pick gradient (top, bottom)
        var g = GetGradientFor(type);

        // same color for left/right to make a clean top->bottom gradient
        wheelText.colorGradient = new VertexGradient(g.top, g.top, g.bottom, g.bottom);
    }
    (Color top, Color bottom) GetGradientFor(WheelType t)
    {
        // GOLD (provided)
        if (t == WheelType.Golden)
            return (UtilityHelper.Hex("#FFE096FF"), UtilityHelper.Hex("#FFB25AFF"));

        // SILVER (soft cool silver)
        if (t == WheelType.Silver)
            return (UtilityHelper.Hex("#E8F0FFFF"), UtilityHelper.Hex("#B6C3D9FF"));

        // BRONZE (warm copper)
        return (UtilityHelper.Hex("#FFD5A1FF"), UtilityHelper.Hex("#C97A39FF"));
    }

    #endregion

    #region Rotate Wheel

    [SerializeField] float spinDuration = 2.2f;
    [SerializeField] int extraSpins = 4; // full 360° turns before landing
    [SerializeField] Ease spinEase = Ease.OutCubic;
    [SerializeField] RectTransform pointer; 

    bool isSpinning;
    RectTransform wheelRotRoot; // assign in Inspector (centered pivot). 
    public void RotateWheel()
    {
        if (isSpinning || slots == null || slots.Length == 0) return;
        isSpinning = true;
        OnSpin?.Invoke();
        if (spinBtn) spinBtn.interactable = false; // <—

        spinButtonAnimation.Stop();
        EnsureRotRoot(); // <-- important

        int targetIndex = Random.Range(0, slots.Length);

        float endZ = ComputeEndZForIndex(targetIndex, wheelRotRoot, extraSpins);
        wheelRotRoot.DOLocalRotate(new Vector3(0, 0, endZ), spinDuration, RotateMode.FastBeyond360)
            .SetEase(spinEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                isSpinning = false;
                KeepButtonUpright(); // if you still counter-rotate the button
                int idx = GetPointedSlotIndex();
                OnLanded(idx);
            });
    }

    const float flyDelay = 0.5f; // tweak as you like
    const float bombDelay = 0.35f; // small pause to show the bomb hit
    void OnLanded(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        var slot = slots[index];
        if (!slot) return;

        // Read from the slice (single source of truth)
        var slice = slot.CurrentSlice;
        var reward = slice.reward;
        
        if (slot.IsBomb || (reward && reward.type == RewardType.Bomb))
        {
            DOVirtual.DelayedCall(bombDelay, GameManager.Loose, true);
            return;
        }

        // Feedback
        GameManager.Instance?.CameraShake(0.1f, 0.1f);

        int amount = slice.amount;

        // Spawn the card view
        var ui = UIManager.Instance;
        if (rewardCardPrefab && ui)
        {
            var go = Instantiate(rewardCardPrefab, ui.AnimPanel);
            var view = go.GetComponent<RewardCardView>();
            if (view) view.Bind(reward, amount, GetCardBg(myCurrentType));

            var fx = go.GetComponent<UIFlyToTargetExact>();
            if (fx)
            {
                fx.animLayer = ui.AnimPanel;
                fx.OnComplete = () =>
                {
                    ui.LootImage.GetComponent<IUIAnimation>()?.Play();
                    InventoryManager.Instance?.AddToStash(reward, amount);
                    DOVirtual.DelayedCall(1f, () => LevelManager.Instance.NextZone(), true);
                };

                DOVirtual.DelayedCall(flyDelay, fx.Play, true);
                return;
            }
        }

        // Fallback: if no prefab/fx, grant immediately then proceed
        InventoryManager.Instance?.AddToStash(reward, amount);
        DOVirtual.DelayedCall(1f, () => LevelManager.Instance.NextZone(), true);
    }
    Color GetCardBg(WheelType t)
    {
        switch (t)
        {
            case WheelType.Golden: return goldCardBG;
            case WheelType.Silver: return silverCardBG;
            default: return bronzeCardBG;
        }
    }
    static void EnsureCenteredPivot(RectTransform rt)
    {
        if (!rt) return;
        if (rt.pivot == new Vector2(0.5f, 0.5f)) return;

        Vector3 before = rt.TransformPoint(rt.rect.center);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        Vector3 after = rt.TransformPoint(rt.rect.center);
        rt.position += (before - after);
    }
    float ComputeEndZForIndex(int index, RectTransform rotRoot, int fullSpins)
    {
        var slotRT = (RectTransform)slots[index].transform;
        
        Vector2 local = rotRoot.InverseTransformPoint(slotRT.position);
        float slotAngle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        
        float pointerAngle = 90f;

        float R = Mathf.DeltaAngle(slotAngle, pointerAngle);
        float signedFull = /* clockwise? */ -360f * fullSpins;

        return rotRoot.localEulerAngles.z + R + signedFull;
    }
    void EnsureRotRoot()
    {
        // Fallback to the wheel image’s rect
        if (!wheelRotRoot) wheelRotRoot = wheelImage.rectTransform;

        EnsureCenteredPivot(wheelRotRoot);
        
        if (wheelImage.rectTransform.parent != wheelRotRoot)
            wheelImage.rectTransform.SetParent(wheelRotRoot, worldPositionStays: false);

        foreach (var s in slots)
        {
            var rt = (RectTransform)s.transform;
            if (rt.parent != wheelRotRoot)
                rt.SetParent(wheelRotRoot, worldPositionStays: false);
        }
    }
    int GetPointedSlotIndex()
    {
        if (slots == null || slots.Length == 0 || !wheelRotRoot) return -1;

        // Pointer angle in the SAME space as the rot root
        float pointerAngle;
        {
            Vector2 pLocal = wheelRotRoot.InverseTransformPoint(
                pointer ? (Vector3)pointer.position : wheelRotRoot.TransformPoint(Vector3.up)
            );
            pointerAngle = Mathf.Atan2(pLocal.y, pLocal.x) * Mathf.Rad2Deg;
        }

        int best = -1;
        float bestDiff = float.MaxValue;

        for (int i = 0; i < slots.Length; i++)
        {
            var srt = (RectTransform)slots[i].transform;

            // SAMPLE A POINT AWAY FROM CENTER: the top edge of the slot rect.
            // This works whether the slot is positioned on the rim OR sits at center and is just rotated.
            Vector3 topEdgeWorld = srt.TransformPoint(new Vector3(0f, srt.rect.height * 0.5f, 0f));
            Vector2 sLocal = wheelRotRoot.InverseTransformPoint(topEdgeWorld);

            float slotAngle = Mathf.Atan2(sLocal.y, sLocal.x) * Mathf.Rad2Deg;
            float diff = Mathf.Abs(Mathf.DeltaAngle(slotAngle, pointerAngle));

            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = i;
            }
        }

        return best;
    }
    float GetPointerAngleDeg()
    {
        if (!wheelRotRoot) return 90f; // default up

        Vector2 pLocal = wheelRotRoot.InverseTransformPoint(
            pointer ? (Vector3)pointer.position : wheelRotRoot.TransformPoint(Vector3.up)
        );
        return Mathf.Atan2(pLocal.y, pLocal.x) * Mathf.Rad2Deg;
    }

// --- Compute RELATIVE deltaZ to land on a given slot index ---
    float ComputeDeltaZToIndex(int index, RectTransform rotRoot, int fullSpins)
    {
        var slotRT = (RectTransform)slots[index].transform;

        // angle of the slot (sample a point away from center so rotation is stable)
        Vector3 topEdgeWorld = slotRT.TransformPoint(new Vector3(0f, slotRT.rect.height * 0.5f, 0f));
        Vector2 sLocal = rotRoot.InverseTransformPoint(topEdgeWorld);
        float slotAngle = Mathf.Atan2(sLocal.y, sLocal.x) * Mathf.Rad2Deg;

        // pointer angle in the same space
        float pointerAngle = GetPointerAngleDeg();

        // shortest arc from slot to pointer
        float delta = Mathf.DeltaAngle(slotAngle, pointerAngle);

        // add extra full spins (negative = clockwise feel)
        float spinTurns = -360f * Mathf.Max(0, fullSpins);

        return delta + spinTurns; // <--- RELATIVE, not absolute
    }

    void RotateWheelToIndex(int targetIndex)
    {
        if (isSpinning || slots == null || slots.Length == 0) return;
        if (targetIndex < 0 || targetIndex >= slots.Length) return;

        isSpinning = true;
        spinButtonAnimation.Stop();
        EnsureRotRoot();

        float deltaZ = ComputeDeltaZToIndex(targetIndex, wheelRotRoot, extraSpins);

        wheelRotRoot
            .DOLocalRotate(new Vector3(0, 0, deltaZ), spinDuration, RotateMode.FastBeyond360)
            .SetRelative(true) // << important: rotate by delta
            .SetEase(spinEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                isSpinning = false;
                KeepButtonUpright();
                int idx = GetPointedSlotIndex();
                OnLanded(idx);
            });
    }

// --- Cheat spin: find the bomb index and go there ---
    public void RotateWheelCheatToBomb()
    {
        int bombIdx = FindBombIndex();
        if (bombIdx < 0)
        {
            Debug.Log("[Wheel] No bomb on this wheel (safe/super). Falling back to random spin.");
            RotateWheel();
            return;
        }

        RotateWheelToIndex(bombIdx);
    }
    int FindBombIndex()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (!s) continue;
            if (s.IsBomb || (s.MySlice.reward && s.MySlice.reward.type == RewardType.Bomb))
                return i;
        }

        return -1;
    }
    #endregion
    
    //Helpers
    public void OpenSpinButton()
    {
        if (spinBtn) spinBtn.interactable = true; // <—
    }
}

}