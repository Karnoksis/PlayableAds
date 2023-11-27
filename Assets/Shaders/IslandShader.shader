Shader "PlayableAds/IslandShader" {
  Properties{

  _Color("Color", Color) = (1,1,1,1)
  //_MainTex("Texture", 2D) = "white" {}
  [HideInInspector] _BlendTex ("BlendTex", 2D) = "white" {}
  [NoScaleOffset] _1Tex ("1Tex", 2D) = "white" {}
          _1Tiling ("1Tiling", Range(1,2000)) = 75
  [NoScaleOffset] _2Tex ("2Tex", 2D) = "white" {}
          _2Tiling ("2Tiling", Range(1,2000)) = 75
  [NoScaleOffset] _3Tex ("3Tex", 2D) = "white" {}
          _3Tiling ("3Tiling", Range(1,2000)) = 75
  [NoScaleOffset] _4Tex ("4Tex", 2D) = "white" {}
          _4Tiling ("4Tiling", Range(1,2000)) = 75
  
  }
    SubShader{
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {

          //float2 uv_MainTex;
        float2 uv_BlendTex;  

      };
      //sampler2D _MainTex;
      fixed4 _Color;
      sampler2D _BlendTex;
      sampler2D _1Tex;
      sampler2D _2Tex;
      sampler2D _3Tex;
      sampler2D _4Tex;
      float _1Tiling;
      float _2Tiling;
      float _3Tiling;
      float _4Tiling;

      void surf(Input IN, inout SurfaceOutput o) {

        //o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;

        fixed4 c0 = tex2D(_BlendTex, IN.uv_BlendTex);
        fixed4 c1 = tex2D(_1Tex, IN.uv_BlendTex * _1Tiling);
        fixed4 c2 = tex2D(_2Tex, IN.uv_BlendTex * _2Tiling);
        fixed4 c3 = tex2D(_3Tex, IN.uv_BlendTex * _3Tiling);
        fixed4 c4 = tex2D(_4Tex, IN.uv_BlendTex * _4Tiling);

      
        fixed3 c = (((1.0 - (c0.r + c0.g + c0.b)) * c1.rgb) + (c0.r * c2.rgb) + (c0.g * c3.rgb) + (c0.b * c4.rgb)) * _Color.rgb;
        o.Albedo = c.rgb;
        o.Alpha = 0;
      }
      ENDCG
  }
    Fallback "Diffuse"
}