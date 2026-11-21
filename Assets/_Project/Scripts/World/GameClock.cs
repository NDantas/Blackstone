using UnityEngine;

namespace Blackstone.World
{
    /// <summary>
    /// Relógio do jogo. O tempo corre mais rápido que o real
    /// (padrão: 1 segundo real = 0,5 minuto de jogo).
    /// Outros sistemas (ronda, contagem) consultam o horário por aqui.
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        public static GameClock Instance { get; private set; }

        [SerializeField] private int startHour = 5;
        [SerializeField] private int startMinute = 50;
        [Tooltip("Minutos de jogo que passam a cada segundo real.")]
        [SerializeField] private float gameMinutesPerSecond = 0.5f;

        /// <summary>Minutos de jogo desde 00:00 do primeiro dia.</summary>
        public float TotalMinutes { get; private set; }

        public int Hour => Mathf.FloorToInt(TotalMinutes / 60f) % 24;
        public int Minute => Mathf.FloorToInt(TotalMinutes) % 60;
        public string TimeText => $"{Hour:00}:{Minute:00}";

        private void Awake()
        {
            Instance = this;
            TotalMinutes = startHour * 60 + startMinute;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            TotalMinutes += gameMinutesPerSecond * Time.deltaTime;
        }

        /// <summary>Converte minutos de jogo em segundos reais.</summary>
        public float GameMinutesToSeconds(float minutes) =>
            gameMinutesPerSecond > 0f ? minutes / gameMinutesPerSecond : 0f;

        public static string Format(float totalMinutes)
        {
            int h = Mathf.FloorToInt(totalMinutes / 60f) % 24;
            int m = Mathf.FloorToInt(totalMinutes) % 60;
            return $"{h:00}:{m:00}";
        }
    }
}
