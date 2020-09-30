using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System;

namespace UnityEditor.Rendering.PostProcessing
{
    
    [PostProcessEditor(typeof(CrepuscularRays))]
    internal sealed class CrepuscularRaysEditor : PostProcessEffectEditor<CrepuscularRays>
    {
        SerializedParameterOverride QualitySetting;
        SerializedParameterOverride LightColour;
        SerializedParameterOverride RayRange;
        SerializedParameterOverride RayIntensity;
        SerializedParameterOverride RayPower;
        SerializedParameterOverride LightThreshold;

        SerializedParameterOverride qualityStep;
        SerializedParameterOverride OffsetUV;
        SerializedParameterOverride BoxBlur;
        SerializedParameterOverride Downsample;
        public override void OnEnable()
        {
            QualitySetting = FindParameterOverride(x => x.QualitySetting);
            LightColour = FindParameterOverride(x => x.LightColour);
            RayRange = FindParameterOverride(x => x.RayRange);
            RayIntensity = FindParameterOverride(x => x.RayIntensity);
            RayPower = FindParameterOverride(x => x.RayPower);
            LightThreshold = FindParameterOverride(x => x.LightThreshold);

            qualityStep = FindParameterOverride(x => x.qualityStep);
            OffsetUV = FindParameterOverride(x => x.OffsetUV);
            BoxBlur = FindParameterOverride(x => x.BoxBlur);
            Downsample = FindParameterOverride(x => x.Downsample);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(QualitySetting);
            EditorGUILayout.Space();
            if (QualitySetting.value.intValue == (int)QualityMode.defaultQuality)
            {
                PropertyField(LightColour);
                PropertyField(RayRange);
                PropertyField(RayIntensity);
                PropertyField(RayPower);
                PropertyField(LightThreshold);
            }
            else
            {
                

                PropertyField(LightColour);
                PropertyField(RayRange);
                PropertyField(RayIntensity);
                PropertyField(RayPower);
                PropertyField(LightThreshold);
                EditorGUILayout.Space();
                EditorUtilities.DrawHeaderLabel("CustomQuality");
                PropertyField(qualityStep);
                PropertyField(OffsetUV);
                PropertyField(BoxBlur);
                PropertyField(Downsample);
            }
        }

    }
}
