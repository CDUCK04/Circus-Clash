using System.Collections;
using UnityEngine;
using CircusClash.Troops.Combat;

namespace CircusClash.Troops.FX
{
    [RequireComponent(typeof(UnitHealth))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlash2D : MonoBehaviour
    {
        [Tooltip("Flash color on damage.")]
        public Color flashColor = Color.white;

        [Min(0f), Tooltip("Seconds the flash lasts.")]
        public float flashDuration = 0.08f;

        private UnitHealth health;
        private SpriteRenderer sr;
        private Color baseColor;
        private Coroutine running;

        void Awake()
        {
            health = GetComponent<UnitHealth>();
            sr = GetComponent<SpriteRenderer>();
            baseColor = sr.color;
        }

        void OnEnable() { health.onDamaged.AddListener(OnDamaged); }
        void OnDisable() { health.onDamaged.RemoveListener(OnDamaged); }

        void OnDamaged(int _)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Flash());
        }

        IEnumerator Flash()
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            sr.color = baseColor;
            running = null;
        }
    }
}
