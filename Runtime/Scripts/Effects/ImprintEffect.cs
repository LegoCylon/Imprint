using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Imprint.Runtime.Effects
{
    [Serializable]
    [PostProcess(renderer: typeof(ImprintRenderer),
        eventType: PostProcessEvent.AfterStack,
        menuItem: "Imprint")]
    public class ImprintEffect : PostProcessEffectSettings
    {
        #region Fields
        [Range(min: 0f, max: 1f), Tooltip(tooltip: "Filter intensity.")]
        public FloatParameter Blend = new FloatParameter { value = 1.0f };
        #endregion
    }
}