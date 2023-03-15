using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnimalsManager;
using Player;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game
{
    public static class GameKeys
    {
        public static string Level = "Level"; // Current level
        public static string Coins = "Coins";
        public static string TempCoins = "TempCoins";
    }

    public static class CharacterKeys
    {
        // Start price = 500 -> 500 * 2
        public static string Speed = "Speed"; // {0 -> 1 -> 2 -> .. -> 11}
        public static string Weight = "Weight"; // {0 -> .1 -> .15 -> .2 -> .3 -> .. -> .95f}
        public static string CollectTime = "CollectTime"; // {0 -> .1 -> .15 -> .2 -> .3 -> .. -> .95f}
        public static string SellPercent = "SellPercent"; // {0 -> 1 -> 2 -> .. -> 11}
    }

    [Serializable]
    public class GameProgress
    {
        public int Price; // Price for unlock this level
        public bool NewAnimal;
        public Animals Animal; // Animal tu unlock to this level
    }

    public class GameManager : MonoBehaviour
    {
        public static string FirstDate = "FirstDate";
        
        public static GameManager Instance;
        [Range(1, 30)] public int level;

        public List<GameProgress> GameProgresses = new List<GameProgress>();

        // Sell
        // Spawn
        [Header("ANIMAL CONTROL")]
        [SerializeField] private AnimalsManager.AnimalsManager animalsManager;

        private List<Animals> currentAnimals = new List<Animals>();

        [Header("UI CONTROL")] [Header("Shop")]
        public Animator shop;
        [SerializeField] private GameObject nothingCollectShop, nothingUpgradeFarm;
        [SerializeField] private Button closeButton, collectShop, collectX2Shop, upgradeFarm;
        [SerializeField] private TextMeshProUGUI collectText, collectX2Text, upgradeText;
        
        [Header("UI CONTROL")] [Header("In App Purchasing")]
        public Animator inApp;
        
        [Header("UI CONTROL")] [Header("Notification")]
        public Animator notification;
        [SerializeField] private TextMeshProUGUI notificationText;
        
        [Header("UI CONTROL")] [Header("New Animal")]
        public Animator newAnimal;
        [SerializeField] private Button closeNewAnimal;
        [SerializeField] private GameObject uiVisualizeObject;
        [SerializeField] private TextMeshProUGUI newAnimalName;

        [Header("UI CONTROL")] [Header("Upgrade")]
        [SerializeField] private Button upgradeSpeed;
        [SerializeField] private Button upgradeWeight, upgradeTime, upgradePrice;
        [SerializeField] private TextMeshProUGUI textSpeed, textWeight, textTime, textPrice;
        
        [Header("Coins and In-Apps")]
        [SerializeField] private TextMeshProUGUI coinText;

        [SerializeField] private GameObject tutorial;
        
        
        private void Awake()
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 300;
            
            PlayerPrefs.SetInt(GameKeys.Coins, 100000000);
            Instance = this;
            level = PlayerPrefs.GetInt(GameKeys.Level, 0);
            currentAnimals = RefreshAnimalList();

            if (PlayerPrefs.GetInt(FirstDate, 0) == 0)
                tutorial.SetActive(true);
        }

        public void CompleteTutorial()
        {
            PlayerManager.Instance.ResetWeight();
            PlayerPrefs.SetInt(FirstDate, 1);
        }

        private void Start()
        {
            foreach (var animal in currentAnimals)
            {
                StartSpawn(animal, 10);
            }
            
            RefreshCoins();
        }

        #region Animals

        public void StartSpawn(WhatAnimal animal, int count)
        {
            var anim = currentAnimals.First(x => x.name == animal.ToString());
            for (var i = 0; i < count; i++)
                animalsManager.SpawnAnimal(anim);
        }

        private void StartSpawn(Animals animal, int count)
        {
            for (var i = 0; i < count; i++)
                animalsManager.SpawnAnimal(animal);
        }

        // Refresh
        private List<Animals> RefreshAnimalList()
        {
            var anim = new List<Animals>();
            for (var i = 0; i <= level; i++)
                if (GameProgresses[i].NewAnimal)
                    anim.Add(GameProgresses[i].Animal);

            return anim;
        }

        private void RefreshAnimalSpawn()
        {
            var temp = RefreshAnimalList();
            if (temp.Count <= currentAnimals.Count) return;

            ShowNewAnimal(temp.Last());
            StartSpawn(temp.Last(), 10);
            currentAnimals = temp;
        }

        #endregion

        #region UI

        // SHOP
        public void ShowShop()
        {
            ConfigureButtons();

            shop.SetTrigger("On");
            
            // Close
            ConfigureCloseShop(true);
        }

        private void ConfigureButtons()
        {
            var canCollect = PlayerPrefs.GetInt(GameKeys.TempCoins, 0) > 0;
            var canUpgrade = PlayerPrefs.GetInt(GameKeys.Coins, 0) > GameProgresses[PlayerPrefs.GetInt(GameKeys.Level, 0) + 1].Price;
            var sum = PlayerPrefs.GetInt(GameKeys.TempCoins) * PlayerPrefs.GetInt(CharacterKeys.SellPercent, 1);
            var priceCollect = PlayerPrefs.GetInt(GameKeys.TempCoins) + sum;
            var priceUpgrade = GameProgresses[PlayerPrefs.GetInt(GameKeys.Level, 0) + 1].Price;

            collectShop.gameObject.SetActive(canCollect);
            collectX2Shop.gameObject.SetActive(canCollect);
            nothingCollectShop.gameObject.SetActive(!canCollect);
            collectText.text = priceCollect.ToString();
            collectX2Text.text = (priceCollect * 2).ToString();

            upgradeFarm.gameObject.SetActive(canUpgrade);
            nothingUpgradeFarm.gameObject.SetActive(!canUpgrade);
            upgradeText.text = priceUpgrade.ToString();
            
            collectShop.onClick.RemoveAllListeners();
            collectShop.onClick.AddListener(() =>
            {
                ControlCoins(priceCollect);
                PlayerPrefs.SetInt(GameKeys.TempCoins, 0);
                PlayerPrefs.Save();
                PlayerManager.Instance.ResetWeight();
                ConfigureButtons();
            });
            collectX2Shop.onClick.RemoveAllListeners();
            // INSERT AD {COLLECT 2X SHOP}
            upgradeFarm.onClick.RemoveAllListeners();
            upgradeFarm.onClick.AddListener(() =>
            {
                level = PlayerPrefs.GetInt(GameKeys.Level, 0) + 1;
                PlayerPrefs.SetInt(GameKeys.Level, level);
                PlayerPrefs.Save();
                ControlCoins(-priceUpgrade);
                ConfigureButtons();
                RefreshAnimalSpawn();
                FarmManager.Instance.Refresh();
            });
        }

        public void ShowUpgrade()
        {
            shop.SetTrigger("OnUpgrade");
            ConfigureUpgrade();
            // Close
            ConfigureCloseShop(false);
        }

        private void ConfigureCloseShop(bool isUpgrade)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                shop.SetTrigger(isUpgrade ? "Off" : "OffUpgrade");
                if (!isUpgrade)
                    ConfigureCloseShop(true);
            });
        }
        
        // IN APP
        public void ShowInApp()
        {
            inApp.SetTrigger("On");
        }
        
        public void HideInApp()
        {
            inApp.SetTrigger("Off");
        }
        
        // NOTIFICATION
        public void ShowNotification(string title)
        {
            notificationText.text = title;
            notification.SetTrigger("Show");
        }
        
        // NEW ANIMAL
        public void ShowNewAnimal(Animals anim)
        {
            uiVisualizeObject.SetActive(true);
            newAnimalName.text = anim.name;
            VizualizeObject.Instance.AddElement(anim);
            newAnimal.SetTrigger("On");
            closeNewAnimal.onClick.RemoveAllListeners();
            closeNewAnimal.onClick.AddListener(HideNewAnimal);
        }
        
        public void HideNewAnimal()
        {
            newAnimal.SetTrigger("Off");
            uiVisualizeObject.SetActive(false);
        }
        
        // UPGRADE
        private void ConfigureUpgrade()
        {
            var speedX = PlayerPrefs.GetInt(CharacterKeys.Speed, 1);
            var weightX = PlayerPrefs.GetFloat(CharacterKeys.Weight, .1f);
            var timeX = PlayerPrefs.GetFloat(CharacterKeys.CollectTime, .1f);
            var priceX = PlayerPrefs.GetInt(CharacterKeys.SellPercent, 1);

            textSpeed.text = (1000 * speedX * speedX).ToString();
            textWeight.text = ((int)(1000 * (weightX * 10) * (weightX * 10))).ToString();
            textTime.text = ((int)(1000 * (timeX * 10) * (timeX * 10))).ToString();
            textPrice.text = (1000 * priceX * priceX).ToString();

            UpdateElementInt(speedX, upgradeSpeed, CharacterKeys.Speed, () => Movement.Movement.RefreshSpeed?.Invoke(), textSpeed);
            UpdateElementInt(priceX, upgradePrice, CharacterKeys.SellPercent, null, textPrice);
            
            UpdateElementFloat(weightX, upgradeWeight, CharacterKeys.Weight, null, textWeight);
            UpdateElementFloat(timeX, upgradeTime, CharacterKeys.CollectTime, null, textTime);
        }

        private void UpdateElementInt(int prefs, Button btn, string key, Action what, TMP_Text txt)
        {
            if (prefs < 10)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (PlayerPrefs.GetInt(GameKeys.Coins, 0) < 1000 * prefs * prefs)
                    {
                        ShowNotification("You not have enough money !");
                        return;
                    }
                    
                    PlayerPrefs.SetInt(key, prefs + 1);
                    ControlCoins(-(1000 * prefs * prefs));
                    what?.Invoke();
                    ConfigureUpgrade();
                });
            }
            else
            {
                btn.interactable = false;
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0, 1);
                txt.color = new Color(1, 0, 0, 1);
                txt.text = "MAX";
            }
        }
        
        private void UpdateElementFloat(float prefs, Button btn, string key, Action what, TMP_Text txt)
        {
            if ((prefs * 10) < 10)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (PlayerPrefs.GetInt(GameKeys.Coins, 0) < 1000 * prefs * prefs)
                    {
                        ShowNotification("You not have enough money !");
                        return;
                    }
                    
                    PlayerPrefs.SetFloat(key, prefs + .1f);
                    var x = -(1000 * (prefs * 10) * (prefs * 10));
                    ControlCoins((int)x);
                    what?.Invoke();
                    ConfigureUpgrade();
                });
            }
            else
            {
                btn.interactable = false;
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0, 1);
                txt.color = new Color(1, 0, 0, 1);
                txt.text = "MAX";
            }
            
            Debug.LogError(PlayerPrefs.GetFloat( CharacterKeys.CollectTime, 0.1f));
        }
        
        // Coins Status
        public void RefreshCoins() => coinText.text = PlayerPrefs.GetInt(GameKeys.Coins, 0).ToString();

        public void ControlCoins(int value)
        {
            var stats = PlayerPrefs.GetInt(GameKeys.Coins, 0);
            PlayerPrefs.SetInt(GameKeys.Coins, stats + value);
            PlayerPrefs.Save();
            RefreshCoins();
        }

        #endregion

        #region Design and managment

        

        #endregion
    }

    // Utilities
    public static class MathUtilities
    {
        public static void Random(ref Vector3 vec, Vector3 min, Vector3 max) =>
            vec = new Vector3(
                UnityEngine.Random.Range(min.x, max.x), 
                UnityEngine.Random.Range(min.y, max.y), 
                UnityEngine.Random.Range(min.z, max.z)
            );
    }
}
