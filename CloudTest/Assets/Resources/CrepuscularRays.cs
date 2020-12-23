using System;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{

    public enum QualityMode
    {
        defaultQuality,
        customQuality
    }


    [Serializable]
    public sealed class CrepuscularRaysParameter : ParameterOverride<QualityMode> { }

    [Serializable]
    [PostProcess(typeof(CrepuscularRaysRender), PostProcessEvent.AfterStack, "Custom/CrepuscularRays")]
    public sealed class CrepuscularRays : PostProcessEffectSettings
    {

        public CrepuscularRaysParameter QualitySetting = new CrepuscularRaysParameter { value = QualityMode.defaultQuality };

        public ColorParameter LightColour = new ColorParameter { value = new Color(1.33f, 0.98f, 0.69f, 1) };
        [Range(0, 2)]
        public FloatParameter RayRange = new FloatParameter { value = 0.94f };
        public FloatParameter RayIntensity = new FloatParameter { value = 2 };
        [Range(1, 3)]
        public FloatParameter RayPower = new FloatParameter { value = 1.25f };
        [Range(0, 1)]
        public FloatParameter LightThreshold = new FloatParameter { value = 0.29f };


        [Range(2, 64)]
        public FloatParameter qualityStep = new FloatParameter { value = 32 };
        [Range(0, 0.1f)]
        public FloatParameter OffsetUV = new FloatParameter { value = 0.027f };
        [Range(0, 0.01f)]
        public FloatParameter BoxBlur = new FloatParameter { value = 0.00126f };
        [Range(1, 16)]
        public IntParameter Downsample = new IntParameter { value = 4 };
    }
}

public sealed class CrepuscularRaysRender : PostProcessEffectRenderer<CrepuscularRays>
{

    private GameObject LightObject = null;
    private Light componetLight = null;

    public override void Init()
    {
        LightObject = GameObject.Find("Directional Light");
        componetLight = LightObject.GetComponent<Light>();
    }
    public override void Release()
    {
        if (componetLight)
        {
            LightObject = null;
            componetLight = null;
        }
    }

    public override void Render(PostProcessRenderContext context)
    {
        var cmd = context.command;
        cmd.BeginSample("CrepuscularRays");
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/CrepuscularRays"));

        Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false);
        sheet.properties.SetMatrix(Shader.PropertyToID("_InverseProjectionMatrix"), projectionMatrix.inverse);
        sheet.properties.SetMatrix(Shader.PropertyToID("_InverseViewMatrix"), context.camera.worldToCameraMatrix.inverse);

        sheet.properties.SetFloat(Shader.PropertyToID("_RayRange"), settings.RayRange);
        sheet.properties.SetFloat(Shader.PropertyToID("_RayIntensity"), settings.RayIntensity);
        sheet.properties.SetFloat(Shader.PropertyToID("_RayPower"), settings.RayPower);
        sheet.properties.SetFloat(Shader.PropertyToID("_LightThreshold"), settings.LightThreshold);
        sheet.properties.SetFloat(Shader.PropertyToID("qualityStep"), settings.qualityStep);
        sheet.properties.SetFloat(Shader.PropertyToID("OffsetUV"), settings.OffsetUV);
        sheet.properties.SetFloat(Shader.PropertyToID("_BoxBlur"), settings.BoxBlur.value);
        sheet.properties.SetColor(Shader.PropertyToID("LightColor"),settings.LightColour);
        if (LightObject)
        {
            //取摄像机位置，只取灯光旋转方向
            Vector3 LightPos = context.camera.WorldToViewportPoint(context.camera.transform.position + LightObject.transform.forward * context.camera.farClipPlane);
            Vector4 viewPortLightPos = new Vector4(LightPos.x, LightPos.y, 0, 0);
            sheet.properties.SetVector(Shader.PropertyToID("_LightViewPos"), viewPortLightPos);
            sheet.properties.SetVector(Shader.PropertyToID("_LightDir"), LightObject.transform.forward);
            sheet.properties.SetVector(Shader.PropertyToID("_CamDir"), context.camera.transform.forward);
        }

        var DownsampleTempID = Shader.PropertyToID("_DownsampleTemp");

        context.GetScreenSpaceTemporaryRT(cmd, DownsampleTempID, 0,context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, context.screenWidth/ settings.Downsample.value, context.screenHeight/ settings.Downsample.value);
        cmd.BlitFullscreenTriangle(context.source, DownsampleTempID, sheet,1);
        
        cmd.SetGlobalTexture(Shader.PropertyToID("_LowResTexture"), DownsampleTempID);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

        cmd.ReleaseTemporaryRT(DownsampleTempID);
        cmd.EndSample("CrepuscularRays");
    }

}