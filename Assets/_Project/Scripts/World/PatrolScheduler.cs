using Blackstone.NPC;
using UnityEngine;

namespace Blackstone.World
{
    /// <summary>
    /// Rotina da ronda: a cada X minutos de jogo (em horários "redondos", ex.: 06:00, 06:30),
    /// as luzes piscam alguns minutos antes e depois o guarda começa a ronda.
    /// </summary>
    public class PatrolScheduler : MonoBehaviour
    {
        [SerializeField] private GuardPatrol guard;
        [SerializeField] private LightFlicker warningLights;
        [Tooltip("Intervalo entre rondas (minutos de jogo).")]
        [SerializeField] private int intervalMinutes = 30;
        [Tooltip("Quantos minutos de jogo antes da ronda as luzes piscam.")]
        [SerializeField] private float warningMinutes = 5f;

        private int lastWarnedSlot = int.MinValue;
        private int lastPatrolSlot = int.MinValue;
        private bool initialized;

        private void Update()
        {
            var clock = GameClock.Instance;
            if (clock == null || guard == null || intervalMinutes <= 0) return;

            float now = clock.TotalMinutes;

            if (!initialized)
            {
                // Não dispara uma ronda "atrasada" logo ao iniciar o jogo.
                lastPatrolSlot = Mathf.FloorToInt(now / intervalMinutes);
                initialized = true;
            }

            // Aviso: luzes piscam antes do próximo horário.
            int nextSlot = Mathf.FloorToInt(now / intervalMinutes) + 1;
            if (nextSlot != lastWarnedSlot && now >= nextSlot * intervalMinutes - warningMinutes)
            {
                lastWarnedSlot = nextSlot;
                if (warningLights != null) warningLights.Flicker();
            }

            // Ronda: chegou um novo horário.
            int currentSlot = Mathf.FloorToInt(now / intervalMinutes);
            if (currentSlot > lastPatrolSlot)
            {
                lastPatrolSlot = currentSlot;
                guard.BeginPatrol();
            }
        }

        /// <summary>Usado pelas ferramentas de Editor.</summary>
        public void Configure(GuardPatrol newGuard, LightFlicker lights)
        {
            guard = newGuard;
            warningLights = lights;
        }
    }
}
