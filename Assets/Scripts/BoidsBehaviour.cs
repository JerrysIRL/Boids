using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class BoidsBehaviour : MonoBehaviour
{
    private class Boid : MonoBehaviour
    {
        private BoidsBehaviour _owner;
        private RectTransform _rectTransform;

        private const float Margin = 32f;

        public Vector2 Forward => _rectTransform.up;
        public Vector2 Position => _rectTransform.anchoredPosition;


        private void Update()
        {
            List<Boid> neighbors = _owner._boids.FindAll(b => b != this && Vector2.Distance(Position, b.Position) < _owner.Radius);

            Vector2 alignment = Vector2.zero;
            Vector2 cohesion = Vector2.zero;
            Vector2 separation = Vector2.zero;
            Vector2 center = Vector2.zero;

            foreach (Boid neighbor in neighbors)
            {
                Vector2 dirToNeighbor = neighbor.Position - Position;
                float amount = 1f - (dirToNeighbor.magnitude / _owner.Radius);

                //separation
                separation -= dirToNeighbor * amount * _owner.separationStrength;
                //alignment
                alignment += neighbor.Forward * amount * _owner.alignmentStrength;
                
                center += neighbor.Position;
            }

            if (neighbors.Count > 0)
            {
                center = center / neighbors.Count;
                cohesion = (center - this.Position).normalized * _owner.cohesionStrength;
            }

            Vector2 force = Forward + alignment + cohesion + separation;
            force = force.normalized;

            //move
            Vector2 newPos = _rectTransform.anchoredPosition + force * Time.deltaTime * _owner.speed;
            _rectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(-force.x, force.y) * Mathf.Rad2Deg);

            Vector2 size = _owner.Size;
            if (newPos.x < -Margin) newPos.x += size.x + Margin;
            if (newPos.y < -Margin) newPos.y += size.y + Margin;
            if (newPos.x > size.x + Margin) newPos.x -= (size.x + Margin);
            if (newPos.y > size.y + Margin) newPos.y -= (size.y + Margin);

            _rectTransform.anchoredPosition = newPos;
        }

        public static Boid CreateBoid(BoidsBehaviour behaviour, GameObject prefab)
        {
            GameObject boidGo = Instantiate(prefab, prefab.transform.parent);
            boidGo.name = "Boid #" + (behaviour._boids.Count + 1);
            boidGo.SetActive(true);

            Vector2 size = behaviour.Size;
            Boid boid = boidGo.AddComponent<Boid>();
            boid._owner = behaviour;
            boid._rectTransform = boidGo.GetComponent<RectTransform>();
            boid._rectTransform.anchoredPosition = new Vector2(Random.Range(0, size.x), Random.Range(0, size.y));
            boid._rectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(-180f, 180f));

            return boid;
        }
    }


    [SerializeField] public int boidsAmount = 5;
    [SerializeField, Range(0f, 100f)] public float speed = 5;
    [SerializeField, Range(0.0f, 500.0f)] public float Radius = 92.0f;
    [SerializeField, Range(0.0f, 1.0f)] public float separationStrength = 0.1f;
    [SerializeField, Range(0.0f, 1.0f)] public float alignmentStrength = 0.5f;
    [SerializeField, Range(0.0f, 1.0f)] public float cohesionStrength = 0.25f;
    private List<Boid> _boids = new List<Boid>();
    private RectTransform _rt;
    private Vector2 Size => _rt.sizeDelta;


    void Start()
    {
        _rt = GetComponent<RectTransform>();
        GameObject boidTemplate = transform.Find("BoidTemplate").gameObject;
        for (int i = 0; i < boidsAmount; i++)
        {
            _boids.Add(Boid.CreateBoid(this, boidTemplate));
        }
    }
}