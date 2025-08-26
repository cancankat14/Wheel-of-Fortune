using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vertigo.Wheel
{
    [DefaultExecutionOrder(-100)]
    public class GameManager : Singleton<GameManager>, IManager
    {
        public static Action OnGameOver;
        public Camera MainCamera { get; private set; }

        protected override void Awake()
        {
            Application.targetFrameRate = 60;
            MainCamera = Camera.main;
            base.Awake();
        }
        public static void Restart()
        {
            var idx = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(idx);
        }

        public static void Loose()
        {
            Instance.CameraShake(0.3f,  0.5f);    // stronger
            UIManager.Instance?.Loose();
            OnGameOver?.Invoke();
            
        }
        Tweener _camPosShake, _camRotShake, _uiShake;
        [SerializeField] RectTransform uiShakeRoot;
        void OnDisable()
        {
            _camPosShake?.Kill();
            _camRotShake?.Kill();
            _uiShake?.Kill();
            if (MainCamera)
            {
                var t = MainCamera.transform;
                t.localPosition = Vector3.zero; // or cache & restore starts if you offset cam
                t.localRotation = Quaternion.identity;
            }
        }
        public void CameraShake(float intensity, float duration)
        {
            if (uiShakeRoot)
            {
                _uiShake?.Kill();
                Vector2 start = uiShakeRoot.anchoredPosition;

                float px = intensity * 60f;
                _uiShake = uiShakeRoot
                    .DOShakeAnchorPos(duration, new Vector2(px, px), vibrato: 15, randomness: 90f, snapping: false, fadeOut: true)
                    .SetUpdate(true)
                    .OnComplete(() => uiShakeRoot.anchoredPosition = start);
                return;
            }

            if (!MainCamera) MainCamera = Camera.main;
            if (!MainCamera) return;

            Transform t = MainCamera.transform;

            _camPosShake?.Kill();
            _camRotShake?.Kill();

            Vector3 startPos = t.position;
            float   startZ   = t.eulerAngles.z;

            // Position shake in X/Y only
            _camPosShake = t.DOShakePosition(
                    duration,
                    strength: new Vector3(intensity, intensity, 0f),
                    vibrato: 18,
                    randomness: 90f,
                    snapping: false,
                    fadeOut: true)
                .SetUpdate(true)
                .OnComplete(() => t.position = startPos);

            // Small Z rotation wobble for extra feel
            _camRotShake = t.DOShakeRotation(
                    duration,
                    strength: new Vector3(0f, 0f, intensity * 20f),
                    vibrato: 12,
                    randomness: 90f,
                    fadeOut: true)
                .SetUpdate(true)
                .OnComplete(() => t.rotation = Quaternion.Euler(0f, 0f, startZ));
        }
        public void OnConfirmLeave()
        {
            InventoryManager.Instance?.BankStash();
            // proceed to next screen / show totals / etc.
        }
    
       
    }
}


