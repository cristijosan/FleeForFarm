using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FarmManager : MonoBehaviour
    {
        public static FarmManager Instance;
        private List<GameObject> farms = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
            
            foreach (Transform child in transform)
            {
                farms.Add(child.gameObject);
            }

            Refresh();
        }

        public void Refresh()
        {
            var lvl = PlayerPrefs.GetInt(GameKeys.Level, 0);
            for (var i = 0; i < lvl; i++)
            {
                farms[i].SetActive(true);
            }
        }
    }
}
