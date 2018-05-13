Shader "Lit/Vertex Color" {
Properties {
//        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader {
//        Tags { "RenderType"="Transparent" }
//        Tags { "Queue"="Transparent" }
       
        CGPROGRAM
        #pragma surface surf Lambert
//        #pragma surface surf Lambert alpha
       
//        sampler2D _MainTex;
 
        struct Input {
//            float2 uv_MainTex;
            float4 color: COLOR; // Vertex color
        };
 
        void surf (Input IN, inout SurfaceOutput o) {
//            o.Albedo = tex2D (_MainTex, IN.uv_MainTex);
//            o.Albedo *= IN.color;
			o.Albedo = IN.color.rgb;
	      	o.Alpha = IN.color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
  }