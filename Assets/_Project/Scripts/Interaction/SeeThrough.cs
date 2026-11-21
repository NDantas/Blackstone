using UnityEngine;

namespace Blackstone.Interaction
{
    /// <summary>
    /// Marca colliders "vazados", como barras de grade: a mira enxerga através deles.
    /// Se não houver nada interativo atrás, o próprio objeto (ex.: a grade) continua mirável.
    /// </summary>
    [DisallowMultipleComponent]
    public class SeeThrough : MonoBehaviour
    {
    }
}
