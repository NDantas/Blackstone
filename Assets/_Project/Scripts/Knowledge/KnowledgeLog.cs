using System;
using System.Collections.Generic;
using Blackstone.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackstone.Knowledge
{
    /// <summary>
    /// Caderno do protagonista: guarda tudo o que ele descobriu.
    /// Tab abre/fecha. ESC também fecha. Enquanto aberto, o jogador fica parado.
    /// O conteúdo permanece mesmo se o jogador for pego (o conhecimento fica com ele).
    /// </summary>
    public class KnowledgeLog : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public string id;
            public string title;
            [TextArea(2, 5)] public string text;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();
        [Tooltip("Tempo (s) do aviso de nova anotação.")]
        [SerializeField] private float newEntryNoticeDuration = 4f;

        public IReadOnlyList<Entry> Entries => entries;
        public bool IsOpen { get; private set; }
        public float NewEntryTimeLeft { get; private set; }

        public event Action<Entry> EntryAdded;

        public bool Has(string id) => entries.Exists(e => e.id == id);

        /// <summary>Adiciona uma anotação. Retorna false se ela já existia.</summary>
        public bool Add(string id, string title, string text)
        {
            if (string.IsNullOrEmpty(id) || Has(id)) return false;

            var entry = new Entry { id = id, title = title, text = text };
            entries.Add(entry);
            NewEntryTimeLeft = newEntryNoticeDuration;
            EntryAdded?.Invoke(entry);
            return true;
        }

        private void Update()
        {
            if (NewEntryTimeLeft > 0f)
            {
                NewEntryTimeLeft -= Time.deltaTime;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
            {
                SetOpen(!IsOpen);
            }
        }

        // ESC é tratado no LateUpdate para o GameSession (Update) ver o caderno
        // ainda aberto e não fechar o jogo no mesmo quadro.
        private void LateUpdate()
        {
            var keyboard = Keyboard.current;
            if (IsOpen && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetOpen(false);
            }
        }

        private void OnDisable() => SetOpen(false);

        public void SetOpen(bool open)
        {
            if (open == IsOpen) return;
            IsOpen = open;
            if (open)
            {
                InputLock.Push();
                NewEntryTimeLeft = 0f;
            }
            else
            {
                InputLock.Pop();
            }
        }
    }
}
