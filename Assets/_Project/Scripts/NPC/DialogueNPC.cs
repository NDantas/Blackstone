using System;
using System.Collections.Generic;
using Blackstone.Interaction;
using Blackstone.Knowledge;
using UnityEngine;

namespace Blackstone.NPC
{
    /// <summary>
    /// NPC com quem o jogador conversa. A conversa é dividida em "assuntos":
    /// cada assunto só aparece quando o jogador já sabe o que ele exige
    /// (anotações no caderno). Cada E mostra a próxima fala.
    /// Ao terminar um assunto, o protagonista pode anotar algo no caderno.
    /// </summary>
    public class DialogueNPC : MonoBehaviour, IInteractable, IRangedInteractable
    {
        [Serializable]
        public class Topic
        {
            public string id;
            [Tooltip("Anotações que o jogador precisa ter para este assunto aparecer.")]
            public string[] requires = Array.Empty<string>();
            [TextArea(2, 4)] public string[] lines = Array.Empty<string>();
            public string noteId;
            public string noteTitle;
            [TextArea(3, 6)] public string noteText;
        }

        [Header("Nome")]
        [SerializeField] private string unknownName = "Prisioneiro";
        [SerializeField] private string knownName = "Bento";
        [Tooltip("Anotação que revela o nome do NPC.")]
        [SerializeField] private string nameRevealedBy = "vizinho_bento";

        [Header("Conversa")]
        [Tooltip("Distância máxima (m) para conversar.")]
        [SerializeField] private float talkDistance = 6f;
        [SerializeField] private float lineDuration = 7f;
        [SerializeField] private List<Topic> topics = new List<Topic>();
        [TextArea(2, 3)]
        [SerializeField] private string[] idleLines = { "— Já falei demais por hoje." };

        private readonly HashSet<string> done = new HashSet<string>();
        private Topic active;
        private int lineIndex;
        private int idleIndex;
        private KnowledgeLog cachedLog;

        public float LastTalkTime { get; private set; } = -999f;
        public bool InConversation => active != null;

        public string DisplayName
        {
            get
            {
                var log = Log;
                bool known = log != null && !string.IsNullOrEmpty(nameRevealedBy) && log.Has(nameRevealedBy);
                return known ? knownName : unknownName;
            }
        }

        public string Prompt => InConversation ? "Continuar" : "Conversar";
        public float MaxDistance => talkDistance;

        private KnowledgeLog Log
        {
            get
            {
                if (cachedLog == null) cachedLog = FindFirstObjectByType<KnowledgeLog>();
                return cachedLog;
            }
        }

        public bool CanInteract(Interactor interactor) => true;

        public void Interact(Interactor interactor)
        {
            LastTalkTime = Time.time;
            var log = interactor.GetComponent<KnowledgeLog>();

            if (active == null)
            {
                active = NextTopic(log);
                lineIndex = 0;

                if (active == null)
                {
                    if (idleLines != null && idleLines.Length > 0)
                    {
                        interactor.ShowMessage(idleLines[idleIndex % idleLines.Length], lineDuration);
                        idleIndex++;
                    }
                    return;
                }
            }

            interactor.ShowMessage(active.lines[lineIndex], lineDuration);
            lineIndex++;

            if (lineIndex >= active.lines.Length)
            {
                done.Add(active.id);
                if (log != null && !string.IsNullOrEmpty(active.noteId))
                {
                    log.Add(active.noteId, active.noteTitle, active.noteText);
                }
                active = null;
            }
        }

        private Topic NextTopic(KnowledgeLog log)
        {
            foreach (var topic in topics)
            {
                if (done.Contains(topic.id) || topic.lines == null || topic.lines.Length == 0) continue;

                bool available = true;
                if (topic.requires != null)
                {
                    foreach (var req in topic.requires)
                    {
                        if (string.IsNullOrEmpty(req)) continue;
                        if (log == null || !log.Has(req))
                        {
                            available = false;
                            break;
                        }
                    }
                }

                if (available) return topic;
            }
            return null;
        }

        /// <summary>Usado pelas ferramentas de Editor.</summary>
        public void Configure(string unknown, string known, string revealedBy, List<Topic> newTopics, string[] newIdleLines)
        {
            unknownName = unknown;
            knownName = known;
            nameRevealedBy = revealedBy;
            topics = newTopics;
            idleLines = newIdleLines;
        }
    }
}
