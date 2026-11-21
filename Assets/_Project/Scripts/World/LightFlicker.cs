using System.Collections;
using UnityEngine;

namespace Blackstone.World
{
    /// <summary>
    /// Faz um grupo de luzes piscar (queda de energia rápida).
    /// Usado como aviso antes da ronda.
    /// </summary>
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField] private Light[] lights;
        [Tooltip("Quantas piscadas.")]
        [SerializeField] private int flickers = 3;
        [Tooltip("Intensidade relativa durante a piscada (0 = apaga).")]
        [Range(0f, 1f)]
        [SerializeField] private float dimFactor = 0.1f;

        private float[] baseIntensities;
        private bool running;

        private void Awake()
        {
            baseIntensities = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null) baseIntensities[i] = lights[i].intensity;
            }
        }

        public void Flicker()
        {
            if (!running && isActiveAndEnabled) StartCoroutine(FlickerRoutine());
        }

        private IEnumerator FlickerRoutine()
        {
            running = true;
            for (int f = 0; f < flickers; f++)
            {
                SetFactor(dimFactor);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                SetFactor(1f);
                yield return new WaitForSeconds(Random.Range(0.08f, 0.35f));
            }
            SetFactor(1f);
            running = false;
        }

        private void SetFactor(float factor)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null) lights[i].intensity = baseIntensities[i] * factor;
            }
        }

        /// <summary>Usado pelas ferramentas de Editor.</summary>
        public void Configure(Light[] newLights) => lights = newLights;
    }
}
