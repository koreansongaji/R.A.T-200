using UnityEngine;

namespace VolFx
{
    [ExecuteAlways]
    public class AmbientLight : MonoBehaviour
    {
        public float _intensity;
        public Color _color;
        
        // =======================================================================
        private void Update()
        {
            RenderSettings.ambientLight     = _color;
            RenderSettings.ambientIntensity = _intensity;
        }
    }
}