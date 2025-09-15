Shader "Custom/SimpleShader"
{
    // Parameter names for code access:
    // "_MainTex", "_Color", "_Offset", "_Cutoff", "_SrcBlend", "_DstBlend", "_ZWrite", "_ZTest", "_AlphaClip", "_StencilRef", "_StencilComp", "_StencilPass"
    
    Properties
    {
        [Header(Basic Rendering)]
        [Space(10)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Int) = 0
        [Enum(Off,0,On,1)] _ZWrite ("Z Write", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Int) = 4
        [Toggle] _AlphaClip ("Alpha Clipping", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        
        [Header(Shader Settings)]
        [Space(10)]
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Offset ("Offset", Vector) = (0,0,0,0)
        
        [Header(Shadow Settings)]
        [Space(10)]
        [Toggle] _ReceiveShadows ("Receive Shadows", Float) = 1
        [Toggle] _CastShadows ("Cast Shadows", Float) = 1
        
        [Header(Advanced Rendering)]
        [Space(10)]
        [Toggle] _UseStencil ("Use Stencil", Float) = 0
        _StencilRef ("Stencil Reference", Int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass", Int) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }
        
        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
            Pass [_StencilPass]
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma shader_feature_local _ALPHACLIP_ON
            #pragma shader_feature_local _RECEIVESHADOWS_ON
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _Offset;
                half _Cutoff;
                float _ReceiveShadows;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = input.uv + _Offset.xy;
                output.shadowCoord = GetShadowCoord(positionInputs);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 result = texColor * _Color;
                
                #ifdef _ALPHACLIP_ON
                    clip(result.a - _Cutoff);
                #endif
                
                Light mainLight = GetMainLight(input.shadowCoord);
                
                half shadow = mainLight.shadowAttenuation;
                #ifdef _RECEIVESHADOWS_ON
                    if (_ReceiveShadows < 0.5)
                    {
                        shadow = 1.0;
                    }
                #endif
                
                half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                half3 color = result.rgb * mainLight.color * shadow * NdotL;
                
                return half4(color, result.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #pragma shader_feature_local _ALPHACLIP_ON
            
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            float3 _LightDirection;
            float3 _LightPosition;
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Cutoff;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                
                float normalBias = 0.01;
                float3 biasedPositionWS = positionWS + normalWS * normalBias;
                
                output.positionCS = TransformWorldToHClip(biasedPositionWS);
                output.uv = input.uv;
                
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                #ifdef _ALPHACLIP_ON
                    half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                    clip(albedo.a - _Cutoff);
                #endif
                
                return 0;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma shader_feature_local _ALPHACLIP_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Cutoff;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.uv = input.uv;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                #ifdef _ALPHACLIP_ON
                    half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                    clip(albedo.a - _Cutoff);
                #endif
                
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
} 