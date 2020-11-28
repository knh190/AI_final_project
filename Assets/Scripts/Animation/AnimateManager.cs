using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animation
{
    public class AnimateManager : MonoBehaviour
    {
        public static AnimateManager instance;

        private readonly HashSet<IEnumerator> coros = new HashSet<IEnumerator>();

        private void Start()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(this);
                return;
            }
            instance = this;
        }

        private void FixedUpdate()
        {
            while (coros.Contains(null))
                coros.Remove(null);

            foreach (IEnumerator coro in coros)
                StartCoroutine(coro);
        }

        public void Shake(GameObject go)
        {
            coros.Add(Animate.Shake(go));
        }
    }

    public static class Animate
    {
        static readonly float shakeIntensity = .4f;
        static readonly float shakeDecay = .0005f;

        // shake by seconds
        public static IEnumerator Shake(GameObject go)
        {
            if (!go) yield return null;

            Vector3 originPosition = go.transform.position;
            Quaternion originRotation = go.transform.rotation;

            float intensity = shakeIntensity;
            while (intensity > .0001f)
            {
                go.transform.position = originPosition + Random.insideUnitSphere * intensity;
                go.transform.rotation = new Quaternion(
                originRotation.x + Random.Range(-intensity, intensity) * .2f,
                originRotation.y + Random.Range(-intensity, intensity) * .2f,
                originRotation.z + Random.Range(-intensity, intensity) * .2f,
                originRotation.w + Random.Range(-intensity, intensity) * .2f);
                intensity -= shakeDecay;

                yield return new WaitForEndOfFrame();
            }

            go.transform.position = originPosition;
            go.transform.rotation = originRotation;

            yield return null;
        }
    }
}