using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGame.Mono
{
    public class Marketplace : MonoBehaviour
    {
        CardContainerMono[] containers = new CardContainerMono[4];

        void Awake()
        {
            int i = 0;

            foreach (Transform t in transform.GetChild(0)) {
                containers[i++] = t.gameObject.GetComponent<CardContainerMono>();
            }
        }

        void Start()
        {}

        void Update()
        {}
    }
}
