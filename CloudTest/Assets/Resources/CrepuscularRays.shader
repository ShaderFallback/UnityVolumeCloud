Shader "Hidden/Custom/CrepuscularRays" 
{		  
		HLSLINCLUDE

		#pragma target 3.0
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
		float4 _LightViewPos;
		float3 _LightDir;
		float3 _CamDir;

		float _RayRange;
		float _RayIntensity;
		float _RayPower;
		float _DepthThreshold;
				
		float _LightThreshold;

		float3 ShadowedFogColour;
		float3 LightColor;
				
		float3 TerrainSize;
		float qualityStep;
		float OffsetUV;
		float _BoxBlur;

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_LowResTexture, sampler_LowResTexture);
		float4 _MainTex_TexelSize;

		//盒装模糊
		half4 DownsampleBox4Tap(TEXTURE2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
		{
			float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);

			half4 s;
			s = (SAMPLE_TEXTURE2D(tex, samplerTex, UnityStereoTransformScreenSpaceTex(uv + d.xy)));
			s += (SAMPLE_TEXTURE2D(tex, samplerTex, UnityStereoTransformScreenSpaceTex(uv + d.zy)));
			s += (SAMPLE_TEXTURE2D(tex, samplerTex, UnityStereoTransformScreenSpaceTex(uv + d.xw)));
			s += (SAMPLE_TEXTURE2D(tex, samplerTex, UnityStereoTransformScreenSpaceTex(uv + d.zw)));

			return s * (1.0 / 4.0);
		}

		half4 FragDownsample4(VaryingsDefault i) : SV_Target
		{
			half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, _BoxBlur.xx);
			return color;
		}

		half4 FragUpsample(VaryingsDefault i) : SV_Target
		{
			const float2 lowResTexelSize = 2.0 * _MainTex_TexelSize.xy;
			float2 UV = i.texcoord - 0.5 * lowResTexelSize;
			half4 color = SAMPLE_TEXTURE2D(_LowResTexture, sampler_LowResTexture, UnityStereoTransformScreenSpaceTex(UV));
			return color;
		}


		float2 random(float2 p) {  //给UV 一个噪音
			float a = dot(p, float2(114.5, 141.9));
			float b = dot(p, float2(364.3, 648.8));
			float2 c = sin(float2(a, b)) * 643.1;
			return frac(c);
		}

		float4 Frag(VaryingsDefault i ):SV_Target
		{				
			float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,i.texcoord);

			//背面消隐
			float backDir = saturate(dot(-_LightDir,_CamDir));

			float2 ScreenUv = _LightViewPos.xy -  i.texcoord; //模糊向量
			float lightViewDir = length(ScreenUv);
			float distanceControl = saturate(_RayRange - lightViewDir);

			float3 colorFinal = float3(0,0,0); 
			float2 originalUV = i.texcoord;

			float2 ScrUV = ScreenUv * OffsetUV;
			float2 jitter = random(i.texcoord);

			for (int ray = 0;ray<qualityStep; ray++)
			{
				float3 Addcolor = SAMPLE_TEXTURE2D(_LowResTexture, sampler_LowResTexture,originalUV + jitter * 0.005f).rgb;
				float3 thresholdColor = saturate(Addcolor - _LightThreshold) * distanceControl;
				float luminanceColor = dot(thresholdColor,float3(0.3f,0.59f,0.11f));
				luminanceColor = pow(luminanceColor, _RayPower);
				//luminanceColor *= sign(saturate(lindepth - _DepthThreshold));
				colorFinal += luminanceColor;
				originalUV += ScrUV ;
			}
			colorFinal = (colorFinal/qualityStep) * LightColor.rgb * _RayIntensity;
					
			return float4(colorFinal * backDir,1) + color;
		}
		ENDHLSL
		  


		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment Frag

				ENDHLSL
			}
			Pass
			{
				HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment FragDownsample4

				ENDHLSL
			}
			Pass
			{
				HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment FragUpsample

				ENDHLSL
			}
		 }

}
