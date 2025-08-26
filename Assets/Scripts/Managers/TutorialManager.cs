using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel
{
    public class TutorialManager : MonoBehaviour
    {
        [MustBeAssigned] [SerializeField] private GameObject clickTutorial;

        private void Awake()
        {
            if (PlayerPrefs.GetInt("FirstTime") == 0)
            {
                clickTutorial.SetActive(true);
                PlayerPrefs.SetInt("FirstTime", 1);
                //TODO instead of player prefs, use save system
            }
        }

        private void OnEnable()
        {
            Wheel.OnSpin += DestroyClickTutorial;
        }

        private void OnDisable()
        {
            Wheel.OnSpin -= DestroyClickTutorial;
        }

        public void OpenClickTutorial()
        {
            clickTutorial.SetActive(true);
        }

        public void DestroyClickTutorial()
        {
            Destroy(clickTutorial);
        }
    }
}