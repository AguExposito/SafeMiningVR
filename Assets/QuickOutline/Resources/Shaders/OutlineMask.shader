//
//  OutlineMask.shader — URP-compatible Quick Outline stencil mask
//

Shader "Custom/Outline Mask"
{
  Properties
  {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
  }

  SubShader
  {
    Tags
    {
      "RenderPipeline" = "UniversalPipeline"
      "Queue" = "Transparent+100"
      "RenderType" = "Transparent"
    }

    Pass
    {
      Name "Mask"
      Tags { "LightMode" = "UniversalForward" }

      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      ColorMask 0

      Stencil
      {
        Ref 1
        Pass Replace
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
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      Varyings vert(Attributes input)
      {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        return output;
      }

      half4 frag(Varyings input) : SV_Target
      {
        return 0;
      }
      ENDHLSL
    }
  }

  FallBack Off
}
