using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(RayMarchingCloudRenderer), PostProcessEvent.BeforeStack, "Custom/RayMarchingCloud")]
public sealed class RayMarchingCloud : PostProcessEffectSettings
{
    //Texture
    public TextureParameter noise3D = new TextureParameter { value = null };
    public TextureParameter noiseDetail3D = new TextureParameter { value = null };

    public FloatParameter shapeTiling = new FloatParameter { value = 0.01f };
    public FloatParameter detailTiling = new FloatParameter { value = 0.1f };

    public TextureParameter weatherMap = new TextureParameter { value = null };
    public TextureParameter maskNoise = new TextureParameter { value = null };
    public TextureParameter blueNoise = new TextureParameter { value = null };

    //light
    //public FloatParameter numStepsLight = new FloatParameter { value = 6 };
    public ColorParameter colA = new ColorParameter { value = Color.white };
    public ColorParameter colB = new ColorParameter { value = Color.white };
    public FloatParameter colorOffset1 = new FloatParameter { value = 0.59f };
    public FloatParameter colorOffset2 = new FloatParameter { value = 1.02f };
    public FloatParameter lightAbsorptionTowardSun = new FloatParameter { value = 0.1f };
    public FloatParameter lightAbsorptionThroughCloud = new FloatParameter { value = 1 };
    public Vector4Parameter phaseParams = new Vector4Parameter { value = new Vector4(0.72f, 1, 0.5f, 1.58f) };

    //density
    public FloatParameter densityOffset = new FloatParameter { value = 4.02f };
    public FloatParameter densityMultiplier = new FloatParameter { value = 2.31f };
    public FloatParameter rayStep = new FloatParameter { value = 1.2f };
    public FloatParameter rayOffsetStrength = new FloatParameter { value = 1.5f };

    public Vector4Parameter shapeNoiseWeights = new Vector4Parameter { value = new Vector4(-0.17f, 27.17f, -3.65f, -0.08f) };
    public FloatParameter detailWeights = new FloatParameter { value = -3.76f };
    public FloatParameter detailNoiseWeight = new FloatParameter { value = 0.12f };

    public Vector4Parameter xy_Speed_zw_Warp = new Vector4Parameter { value = new Vector4(0.05f, 1, 1, 10) };
}



public sealed class RayMarchingCloudRenderer : PostProcessEffectRenderer<RayMarchingCloud>
{
    GameObject findCloudBox;
    Transform cloudTransform;
    Vector3 boundsMin;
    Vector3 boundsMax;

    public override DepthTextureMode GetCameraFlags()
    {
        return DepthTextureMode.Depth; 
    }

    public override void Init()
    {
        findCloudBox = GameObject.Find("CloudBox");

        if (findCloudBox != null)
        {
            cloudTransform = findCloudBox.GetComponent<Transform>();
            
        }
    }
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/RayMarchingCloud"));
        //sheet.properties.SetColor(Shader.PropertyToID("_color"), settings.color);
         
        Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false);
        sheet.properties.SetMatrix(Shader.PropertyToID("_InverseProjectionMatrix"), projectionMatrix.inverse);
        sheet.properties.SetMatrix(Shader.PropertyToID("_InverseViewMatrix"), context.camera.cameraToWorldMatrix);
        sheet.properties.SetVector(Shader.PropertyToID("_CameraDir"), context.camera.transform.forward);
        //sheet.properties.SetVector(Shader.PropertyToID("_WorldSpaceLightPos0"), LightDir.transform.forward);

        if (cloudTransform != null){
            boundsMin = cloudTransform.position - cloudTransform.localScale / 2;
            boundsMax = cloudTransform.position + cloudTransform.localScale / 2;

            sheet.properties.SetVector(Shader.PropertyToID("_boundsMin"), boundsMin);
            sheet.properties.SetVector(Shader.PropertyToID("_boundsMax"), boundsMax);
        }

        if (settings.noise3D.value != null)
        {
            sheet.properties.SetTexture(Shader.PropertyToID("_noiseTex"), settings.noise3D.value);
        }
        if (settings.noiseDetail3D.value != null)
        {
            sheet.properties.SetTexture(Shader.PropertyToID("_noiseDetail3D"), settings.noiseDetail3D.value);
        }
        if (settings.weatherMap.value != null)
        {
            sheet.properties.SetTexture(Shader.PropertyToID("_weatherMap"), settings.weatherMap.value);
        }
        if (settings.maskNoise.value != null)
        {
            sheet.properties.SetTexture(Shader.PropertyToID("_maskNoise"), settings.maskNoise.value);
        }


        if (settings.blueNoise.value != null)
        {
            Vector4 screenUv = new Vector4(
            (float)context.screenWidth / (float)settings.blueNoise.value.width,
            (float)context.screenHeight / (float)settings.blueNoise.value.height,0,0);
            sheet.properties.SetVector(Shader.PropertyToID("_BlueNoiseCoords"), screenUv);
            sheet.properties.SetTexture(Shader.PropertyToID("_BlueNoise"), settings.blueNoise.value);
        }
        
        sheet.properties.SetFloat(Shader.PropertyToID("_shapeTiling"), settings.shapeTiling.value);
        sheet.properties.SetFloat(Shader.PropertyToID("_detailTiling"), settings.detailTiling.value);

        sheet.properties.SetFloat(Shader.PropertyToID("_rayStep"), settings.rayStep.value);

        //sheet.properties.SetFloat(Shader.PropertyToID("_dstTravelled"),settings.dstTravelled.value);
        sheet.properties.SetFloat(Shader.PropertyToID("_densityOffset"), settings.densityOffset.value);
        sheet.properties.SetFloat(Shader.PropertyToID("_densityMultiplier"), settings.densityMultiplier.value);

        
        //sheet.properties.SetInt(Shader.PropertyToID("_numStepsLight"), (int)settings.numStepsLight.value);

        sheet.properties.SetColor(Shader.PropertyToID("_colA"), settings.colA.value);
        sheet.properties.SetColor(Shader.PropertyToID("_colB"), settings.colB.value);
        sheet.properties.SetFloat(Shader.PropertyToID("_colorOffset1"), settings.colorOffset1.value);
        sheet.properties.SetFloat(Shader.PropertyToID("_colorOffset2"), settings.colorOffset2.value);
        sheet.properties.SetFloat(Shader.PropertyToID("_lightAbsorptionTowardSun"), settings.lightAbsorptionTowardSun.value);
        sheet.properties.SetFloat(Shader.PropertyToID("_lightAbsorptionThroughCloud"), settings.lightAbsorptionThroughCloud.value);

        
        sheet.properties.SetFloat(Shader.PropertyToID("_rayOffsetStrength"), settings.rayOffsetStrength.value);
        sheet.properties.SetVector(Shader.PropertyToID("_phaseParams"), settings.phaseParams.value);
        sheet.properties.SetVector(Shader.PropertyToID("_xy_Speed_zw_Warp"), settings.xy_Speed_zw_Warp.value);
        
        sheet.properties.SetVector(Shader.PropertyToID("_shapeNoiseWeights"), settings.shapeNoiseWeights.value);

        sheet.properties.SetFloat(Shader.PropertyToID("_detailWeights"), settings.detailWeights.value);
        sheet.properties.SetFloat(Shader.PropertyToID("_detailNoiseWeight"), settings.detailNoiseWeight.value);

        

        Quaternion rotation = Quaternion.Euler(cloudTransform.eulerAngles);
        Vector3 scaleMatrix = cloudTransform.localScale * 0.1f;
        scaleMatrix = new Vector3(1 / scaleMatrix.x, 1 / scaleMatrix.y, 1 / scaleMatrix.z);
        Matrix4x4 TRSMatrix = Matrix4x4.TRS(cloudTransform.position, rotation, scaleMatrix);
        sheet.properties.SetMatrix(Shader.PropertyToID("_TRSMatrix"), TRSMatrix);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

    }
}
