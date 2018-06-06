Shader "Lit/Vertex Color" {
	Properties {}
    SubShader {
        CGPROGRAM
        #pragma surface surf Lambert 
        struct Input {
            float4 color: COLOR; // Vertex color
        };
        void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.color.rgb;
	      	o.Alpha = IN.color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
  }