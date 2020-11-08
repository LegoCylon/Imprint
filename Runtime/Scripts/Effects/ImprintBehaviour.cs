using System.Collections.Generic;
using UnityEngine;

namespace Imprint.Runtime.Effects
{
    public class ImprintBehaviour : MonoBehaviour
    {
        #region Properties
        public static IEnumerable<ImprintBehaviour> Instances => sBehaviours;
        public Renderer Renderer => _Renderer;
        #endregion

        #region Fields
        private static readonly HashSet<ImprintBehaviour> sBehaviours = new HashSet<ImprintBehaviour>();

        [SerializeField] private Renderer _Renderer;
        #endregion

        protected virtual void OnEnable () => sBehaviours.Add(item: this);

        protected virtual void OnDisable () => sBehaviours.Remove(item: this);
    }
}