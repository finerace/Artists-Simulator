Shader "Custom/SimpleLayersShader"
{
    Properties
    {
        [Header(Layer 1)]
        [Space(10)]
        [Toggle] _Tex1Enabled ("Enable Layer 1", Float) = 0
        _Tex1 ("Layer 1 Texture", 2D) = "white" {}
        _Tex1Color ("Layer 1 Color", Color) = (1,1,1,1)
        _Tex1Offset ("Layer 1 Offset", Vector) = (0,0,0,0)
        
        [Header(Layer 2)]
        [Space(10)]
        [Toggle] _Tex2Enabled ("Enable Layer 2", Float) = 0
        _Tex2 ("Layer 2 Texture", 2D) = "white" {}
        _Tex2Color ("Layer 2 Color", Color) = (1,1,1,1)
        _Tex2Offset ("Layer 2 Offset", Vector) = (0,0,0,0)
        
        [Header(Layer 3)]
        [Space(10)]
        [Toggle] _Tex3Enabled ("Enable Layer 3", Float) = 0
        _Tex3 ("Layer 3 Texture", 2D) = "white" {}
        _Tex3Color ("Layer 3 Color", Color) = (1,1,1,1)
        _Tex3Offset ("Layer 3 Offset", Vector) = (0,0,0,0)

        [Header(Alpha Settings)]
        [Space(10)]
        [Toggle] _AlphaClip ("Alpha Clipping", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        [Header(Stencil Settings)]
        [Space(10)]
        [Toggle] _UseStencil ("Use Stencil", Float) = 0
        _StencilRef ("Stencil Reference", Int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass", Int) = 0

        // Blending state
        _Surface("__surface", Float) = 0.0
        _Blend("__blend", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0.0

        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0
        _QueueOffset("Queue offset", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "SimpleLit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
            Pass [_StencilPass]
        }

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite[_ZWrite]
            Cull[_Cull]
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple

            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ALPHACLIP_ON
            #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_Tex1);
            SAMPLER(sampler_Tex1);
            TEXTURE2D(_Tex2);
            SAMPLER(sampler_Tex2);
            TEXTURE2D(_Tex3);
            SAMPLER(sampler_Tex3);

            CBUFFER_START(UnityPerMaterial)
                half _Cutoff;
                
                float _Tex1Enabled;
                half4 _Tex1Color;
                float4 _Tex1Offset;
                
                float _Tex2Enabled;
                half4 _Tex2Color;
                float4 _Tex2Offset;
                
                float _Tex3Enabled;
                half4 _Tex3Color;
                float4 _Tex3Offset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float4 fogFactorAndVertexLight : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings LitPassVertexSimple(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.uv = input.texcoord;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;

                half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

                output.shadowCoord = GetShadowCoord(vertexInput);

                return output;
            }

            half4 LitPassFragmentSimple(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 albedo = half4(0, 0, 0, 1);

                if (_Tex1Enabled > 0.5)
                {
                    float2 uv1 = input.uv + _Tex1Offset.xy;
                    half4 layer1 = SAMPLE_TEXTURE2D(_Tex1, sampler_Tex1, uv1) * _Tex1Color;
                    albedo = layer1;
                }
                
                if (_Tex2Enabled > 0.5)
                {
                    float2 uv2 = input.uv + _Tex2Offset.xy;
                    half4 layer2 = SAMPLE_TEXTURE2D(_Tex2, sampler_Tex2, uv2) * _Tex2Color;
                    albedo.rgb = lerp(albedo.rgb, layer2.rgb, layer2.a);
                    albedo.a = saturate(albedo.a + layer2.a);
                }
                
                if (_Tex3Enabled > 0.5)
                {
                    float2 uv3 = input.uv + _Tex3Offset.xy;
                    half4 layer3 = SAMPLE_TEXTURE2D(_Tex3, sampler_Tex3, uv3) * _Tex3Color;
                    albedo.rgb = lerp(albedo.rgb, layer3.rgb, layer3.a);
                    albedo.a = saturate(albedo.a + layer3.a);
                }

            #ifdef _ALPHACLIP_ON
                clip(albedo.a - _Cutoff);
            #endif

                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = SafeNormalize(input.viewDirWS);

                half4 shadowCoord = input.shadowCoord;
                Light mainLight = GetMainLight(shadowCoord);

            #if defined(_RECEIVE_SHADOWS_OFF)
                mainLight.shadowAttenuation = 1.0h;
            #endif

                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 color = albedo.rgb * mainLight.color * NdotL * mainLight.shadowAttenuation;

            #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    half NdotL2 = saturate(dot(normalWS, light.direction));
                    color += albedo.rgb * light.color * NdotL2 * light.distanceAttenuation * light.shadowAttenuation;
                }
            #endif

            #ifdef _ADDITIONAL_LIGHTS_VERTEX
                color += input.fogFactorAndVertexLight.yzw * albedo.rgb;
            #endif

                color += unity_AmbientSky.rgb * albedo.rgb;

                color = MixFog(color, input.fogFactorAndVertexLight.x);

                return half4(color, albedo.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half _Cutoff;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.uv = input.texcoord;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

                         half4 DepthOnlyFragment(Varyings input) : SV_TARGET
             {
                 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                 return 0;
             }
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            Cull Off

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaSimple

            #pragma shader_feature EDITOR_VISUALIZATION

                                     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings UniversalVertexMeta(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv0;
                return output;
            }

            half4 UniversalFragmentMetaSimple(Varyings input) : SV_Target
            {
                return half4(1.0, 1.0, 1.0, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
} 