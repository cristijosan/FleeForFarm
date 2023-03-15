using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace AnimalsManager
{
    [Serializable]
    public enum WhatAnimal
    {
        Chick,
        Hen,
        Duck,
        Rooster,
        Sheep,
        Donkey,
        Pig,
        Cow,
        Buffalo,
        None,
    }
    
    public class AnimalsManager : MonoBehaviour
    {
        public Transform player;
        public List<Vector3> animalSpawnPos = new List<Vector3>();

        private Vector3 getRandomSpawn;

        public void SpawnAnimal(Animals animal)
        {
            MathUtilities.Random( ref getRandomSpawn, animalSpawnPos[0], animalSpawnPos[1]);
            var anim = Instantiate(animal, getRandomSpawn, Quaternion.identity);
            anim.Player = this.player;
        }
    }
}
