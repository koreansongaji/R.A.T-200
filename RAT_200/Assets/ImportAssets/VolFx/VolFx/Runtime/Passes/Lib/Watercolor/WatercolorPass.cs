using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Watercolor")]
    public class WatercolorPass : VolFx.Pass
    {
        private static readonly int s_MotionTex = Shader.PropertyToID("_MotionTex");
        private static readonly int s_PaperTex  = Shader.PropertyToID("_PaperTex");
        private static readonly int s_FocusTex  = Shader.PropertyToID("_FocusTex");
        private static readonly int s_DataA     = Shader.PropertyToID("_DataA");
        private static readonly int s_DataB     = Shader.PropertyToID("_DataB");
        
        public override string ShaderName => string.Empty;
        
        public  Texture2D _motionTexDefault;
        public  Texture2D _paperTexDefault;
        private Texture2D _focusTex;
        private Texture2D _gradTex;
        private float     _fpsLastFrame;
        private float     _offset;
        private float     _contur;
        private float     _strength;
        private float     _notes;

        // =======================================================================
        public override void Init()
        {
            _fpsLastFrame = 0f;
            _contur = 0f;
            _strength = 0f;
            _notes = 0f;
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<WatercolorVol>();

            if (settings.IsActive() == false)
                return false;
            
            if (settings.m_Fps.overrideState && (Time.time - _fpsLastFrame > (1f / settings.m_Fps.value)))
            {
                _fpsLastFrame = Time.time;
                _offset       = Random.value * 3f;
                _contur       = Random.value * settings.m_ContourDeviation.value;
                _strength     = Random.value * settings.m_StrengthDeviation.value;
                _notes        = (Random.value - .5f) * settings.m_ColorNotesDeviation.value;
            }
            else
            if (settings.m_Fps.overrideState == false || settings.m_Fps.value == 0)
            {
                _offset   = 0f;
                _contur   = 0f;
                _strength = 0f;
                _notes    = 0f;
            }
            
            var sat = settings.m_Saturation.value * -1f;
            
            var dataA = new Vector4(
                Mathf.Lerp(0, 0.0333f, settings.m_Strength.value + _strength),
                _offset,
                settings.m_Density.value,
                Mathf.Lerp(.001f, .333f, settings.m_Blending.value)
            );

            var dataB = new Vector4(
                Mathf.Lerp(0, 0.007f, settings.m_Contour.value + _contur),
                Mathf.Lerp(20f, .537f, settings.m_ContourThickness.value),
                Mathf.Lerp(0f, 1f, (settings.m_ColorNotes.value  + 1f) * .5f  + _notes),
                sat > 0f ? Mathf.LerpUnclamped(0f, 1.1f, sat) : Mathf.LerpUnclamped(0f, -7.7f, sat * -1f)
            );

            var motionTex = settings.m_Splatters.overrideState && settings.m_Splatters.value != null ? settings.m_Splatters.value : _motionTexDefault;
            var paperTex  = settings.m_Paper.overrideState && settings.m_Paper.value != null ? settings.m_Paper.value : _paperTexDefault;
            
            mat.SetTexture(s_MotionTex, motionTex);
            mat.SetTexture(s_PaperTex, paperTex);
            mat.SetTexture(s_FocusTex, settings.m_Focus.value.GetTexture(ref _focusTex));
            
            mat.SetVector(s_DataA, dataA);
            mat.SetVector(s_DataB, dataB);

            return true;
        }
        
        protected override bool _editorValidate => _motionTexDefault == null || _paperTexDefault == null;
        protected override void _editorSetup(string folder, string asset)
        {
#if UNITY_EDITOR
			_motionTexDefault = UnityEditor.AssetDatabase.FindAssets("t:texture", new string[] {$"{folder}\\Data"})
							   .Select(n => UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(n)))
							   .Where(n => n != null)
							   .FirstOrDefault(n => n.name == "Splatter_Squashes");
            
			_paperTexDefault = UnityEditor.AssetDatabase.FindAssets("t:texture", new string[] {$"{folder}\\Data"})
							   .Select(n => UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(n)))
							   .Where(n => n != null)
							   .FirstOrDefault(n => n.name == "Paper_Clean");
#endif
        }
    }
}