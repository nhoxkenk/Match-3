using System;
using UnityEngine;

namespace CandyCrush
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Gem : MonoBehaviour
    {
        public GemType type;

        public void SetGemType(GemType type)
        {
            this.type = type;
            GetComponent<SpriteRenderer>().sprite = type.Sprite;
        }

        public GemType GetGemType() => type;

        internal void DestroyGem() => Destroy(gameObject);
    }
}
