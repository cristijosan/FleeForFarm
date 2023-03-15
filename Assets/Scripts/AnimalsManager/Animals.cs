using System;
using System.Collections;
using DG.Tweening;
using Game;
using Player;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace AnimalsManager
{
    public class Animals : MonoBehaviour
    {
        [NonSerialized] public Transform Player;
        private NavMeshAgent navMeshAgent;
        public WhatAnimal whatAnimal;
        [Header("Settings: ")]
        public float speed;
        public float acceleration;
        public float enemyDistanceRun;
        public Animator animalControl;
        [Header("Animal data: ")]
        public int height;
        public int selfPrice;
        public float timeForCollect = 1.1f;

        private Action run;
        private readonly int speedId = Animator.StringToHash("Speed");
        private readonly int eatId = Animator.StringToHash("Eat");
        
        // Optimize
        [Header("Optimize: ")]
        private new Camera camera;
        private Plane[] cameraFrustum;
        private new Collider collider;
        
        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            navMeshAgent.speed = speed;
            navMeshAgent.acceleration = acceleration;

            waitAndCollect = WaitAndCollect();
            camera = Camera.main;
            collider = GetComponent<Collider>();
            run = RunAnimal;
        }
        
        // Optimize
        private void Update()
        {
            var bounds = collider.bounds;
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(camera);
            animalControl.gameObject.SetActive(GeometryUtility.TestPlanesAABB(cameraFrustum, bounds));
        }

        // Move away
        private void FixedUpdate()
        {
            run?.Invoke();
        }
        
        // Move from Player
        private void RunAnimal()
        {
            var distance = Vector3.Distance(transform.position, Player.position);
            
            // Run away from player
            if (!(distance < enemyDistanceRun))
            {
                animalControl.SetFloat(speedId, 0);
                eat ??= StartCoroutine(Eat());
                return;
            }

            if (eat != null)
                StopCoroutine(eat);
            eat = null;
            
            var position = transform.position;
            var dirToPlayer = position - Player.position;
            var newPos = position + dirToPlayer;
            animalControl.SetFloat(speedId, Mathf.Abs(navMeshAgent.velocity.magnitude));
            navMeshAgent.SetDestination(newPos);
        }
        
        // Random eat animation
        private Coroutine eat;

        private IEnumerator Eat()
        {
            yield return new WaitForSeconds(1.5f);
            while (true)
            {
                animalControl.SetTrigger(eatId);
                yield return new WaitForSeconds(Random.Range(.5f, 2f));
            }
        }

        // Collect me !
        private IEnumerator waitAndCollect;
        private void OnTriggerEnter(Collider col)
        {
            if (!col.CompareTag("Collector"))
                return;

            navMeshAgent.speed = speed / 2;
            StartCoroutine(waitAndCollect);
        }

        private void OnTriggerExit(Collider col)
        {
            if (!col.CompareTag("Collector"))
                return;
            
            navMeshAgent.speed = speed;
            StopCoroutine(waitAndCollect);
        }

        private float t;
        private IEnumerator WaitAndCollect()
        {
            var exactTime = timeForCollect - PlayerPrefs.GetFloat(CharacterKeys.CollectTime, .1f);
            while (t < exactTime)
            {
                t += 0.1f;
                yield return new WaitForSeconds(.1f);
            }

            Collect();
        }

        private void Collect()
        {
            run = null;
            navMeshAgent.enabled = false;
            
            transform.DOScale(new Vector3(.4f, .4f, .4f), 1f).OnComplete(() =>
            {
                GameManager.Instance.StartSpawn(whatAnimal, 1);
                Destroy(gameObject);
            });
            var tw = transform.DOMove(PlayerManager.Instance.collectPoint.position, .2f);
            tw.OnUpdate(() => tw.ChangeEndValue(PlayerManager.Instance.collectPoint.position, true));
            
            var minus = (height - .1f) * PlayerPrefs.GetFloat(CharacterKeys.Weight, .1f);
            PlayerManager.Instance.IncreaseWeight(height - minus, selfPrice);
        }
    }
}
