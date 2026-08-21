//
//  OutlineFill.shader — URP-compatible Quick Outline fill pass
//

Shader "Custom/Outline Fill"
{
  Properties
  {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
    _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
    _OutlineWidth("Outline Width", Range(0, 10)) = 2
  }

  SubShader
  {
    Tags
    {
      "RenderPipeline" = "UniversalPipeline"
      "Queue" = "Transparent+110"
      "RenderType" = "Transparent"
      "DisableBatching" = "True"
    }

    Pass
    {
      Name "Fill"
      Tags { "LightMode" = "UniversalForward" }

      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask RGB

      Stencil
      {
        Ref 1
        Comp NotEqual
      }

      HLSLPROGRAM
      #pragma target 2.0
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_instancing

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      struct Attributes
      {
        float4 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        half4 color : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      CBUFFER_START(UnityPerMaterial)
        half4 _OutlineColor;
        float _OutlineWidth;
      CBUFFER_END

      Varyings vert(Attributes input)
      {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        float3 normalOS = any(input.smoothNormal) ? input.smoothNormal : input.normalOS;
        float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
        float3 normalWS = TransformObjectToWorldNormal(normalOS);
        float3 viewDirWS = GetWorldSpaceViewDir(positionWS);
        float viewDistance = length(viewDirWS);
        positionWS += normalize(normalWS) * (viewDistance * _OutlineWidth / 1000.0);

        output.positionCS = TransformWorldToHClip(positionWS);
        output.color = _OutlineColor;
        return output;
      }

      half4 frag(Varyings input) : SV_Target
      {
        return input.color;
      }
      ENDHLSL
    }
  }

  FallBack Off
}
