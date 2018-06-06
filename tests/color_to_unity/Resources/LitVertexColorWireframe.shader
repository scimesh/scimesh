Shader "Lit/Vertex Color Wireframe"
{
	Properties{
		//[NoScaleOffset] _MainTex("Texture", 2D) = "white" {} // Changed to vertex color
	}
	SubShader{
		Pass{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			// shadow helper functions and macros
			#include "AutoLight.cginc"
			struct appdata {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				// SUP
				float3 normal : NORMAL;
				fixed4 color : COLOR;
			};
			struct v2f{
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1) // put shadows data into TEXCOORD1
				fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
				float4 pos : SV_POSITION;
				// SUP
				fixed4 color : COLOR2;
				float4 posWorld : TEXCOORD3;
				float3 normalDir : TEXCOORD4;
			};
			v2f vert(appdata v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				//half nl = max(0, dot(o.normalDir, _WorldSpaceLightPos0.xyz));
				//o.diff = nl * _LightColor0.rgb;
				//o.ambient = ShadeSH9(half4(o.normalDir,1));
				// compute shadows data
				TRANSFER_SHADOW(o)
				// SUP
				o.color = v.color;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				//o.normalDir = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			sampler2D _MainTex;
			fixed4 frag(v2f i) : SV_Target{
				// SUP
				float3 dpdx = ddx(i.posWorld);
				float3 dpdy = ddy(i.posWorld);
				i.normalDir = normalize(cross(dpdy, dpdx));
				// MAIN
				half nl = max(0, dot(i.normalDir, _WorldSpaceLightPos0.xyz));
				i.diff = nl * _LightColor0.rgb;
				i.ambient = ShadeSH9(half4(i.normalDir, 1));
				//fixed4 col = tex2D(_MainTex, i.uv); // Changed to vertex color
				fixed4 col = i.color;
				// compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
				fixed shadow = SHADOW_ATTENUATION(i);
				// darken light's illumination with shadow, keep ambient intact
				fixed3 lighting = i.diff * shadow + i.ambient;
				col.rgb *= lighting;
				return col;
				}
			ENDCG
		}
		// shadow casting support
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}

// TRYING TO IMPLEMENT WIREFRAME SHADER ...
// CONCLUSION: it can be do with geometry shader:
// (#pragma geometry name - compile function name as DX10 geometry shader.
// Having this option automatically turns on #pragma target 4.0, described below.) TODO
//
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
//Shader "Lit/Vertex Color Wireframe" {
//	Properties{
//		//_MainTex("Base (RGB)", 2D) = "white" {}
//		//_Gain("Gain", Float) = 1.5
//		//_WireColor("WireColor", Color) = (1,0,0,1)
//		//_Color("Color", Color) = (1,1,1,1)
//		_WireThickness("Wire Thickness", RANGE(0, 800)) = 100
//		_WireSmoothness("Wire Smoothness", RANGE(0, 20)) = 3
//		_WireColor("Wire Color", Color) = (0.0, 1.0, 0.0, 1.0)
//		_BaseColor("Base Color", Color) = (0.0, 0.0, 0.0, 1.0)
//		_MaxTriSize("Max Tri Size", RANGE(0, 200)) = 25
//	}
//	//SubShader{
//	//	CGPROGRAM
//	//	#pragma surface surf Lambert
//	//	struct Input {
//	//	float4 color: COLOR; // Vertex color
//	//	};
//	//	void surf(Input IN, inout SurfaceOutput o) {
//	//		o.Albedo = IN.color.rgb;
//	//		o.Alpha = IN.color.a;
//	//	}
//	//	ENDCG
//	//}
//	/*SubShader{
//		Tags{ "RenderType" = "Opaque" "LightMode" = "Vertex" }
//		LOD 200
//		Pass{
//			CGPROGRAM
//			#pragma target 3.0
//			#pragma glsl
//			#pragma vertex vert
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//			sampler2D _MainTex;
//			float _Gain;
//			struct appdata {
//				float4 vertex : POSITION;
//				float4 tangent : TANGENT;
//				float3 normal : NORMAL;
//				float2 uv : TEXCOORD0;
//			};
//			struct vs2ps {
//				float4 vertex : POSITION;
//				float2 uv : TEXCOORD0;
//				float3 bary;
//			};
//			vs2ps vert(appdata IN) {
//				vs2ps o;
//				o.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
//				o.bary = IN.tangent.xyz;
//				o.uv = IN.uv;
//				return o;
//			}
//			float4 frag(vs2ps IN) : COLOR{
//				float3 d = fwidth(IN.bary);
//				float3 a3 = smoothstep(float3(0.0), _Gain * d, IN.bary);
//				float t = min(min(a3.x, a3.y), a3.z);
//				float4 c = tex2D(_MainTex, IN.uv);
//				return t * c;
//			}
//			ENDCG
//		}*/
//	//SubShader{
//	//	Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
//	//	Blend SrcAlpha OneMinusSrcAlpha
//	//	Pass{
//	//		CGPROGRAM
//	//		#include "UnityCG.cginc"
//	//		#pragma target 4.0
//	//		#pragma vertex vert
//	//		#pragma geometry geom
//	//		#pragma fragment frag
//	//		half4 _WireColor, _Color;
//	//		struct v2g
//	//		{
//	//			float4  pos : SV_POSITION;
//	//			float2  uv : TEXCOORD0;
//	//		};
//	//		struct g2f
//	//		{
//	//			float4  pos : SV_POSITION;
//	//			float2  uv : TEXCOORD0;
//	//			float3 dist : TEXCOORD1;
//	//		};
//	//		v2g vert(appdata_base v)
//	//		{
//	//			v2g OUT;
//	//			OUT.pos = UnityObjectToClipPos(v.vertex);
//	//			OUT.uv = v.texcoord;
//	//			return OUT;
//	//		}
//	//		[maxvertexcount(3)]
//	//		void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
//	//		{
//	//			float2 WIN_SCALE = float2(_ScreenParams.x / 2.0, _ScreenParams.y / 2.0);
//	//			//frag position
//	//			float2 p0 = WIN_SCALE * IN[0].pos.xy / IN[0].pos.w;
//	//			float2 p1 = WIN_SCALE * IN[1].pos.xy / IN[1].pos.w;
//	//			float2 p2 = WIN_SCALE * IN[2].pos.xy / IN[2].pos.w;
//	//			//barycentric position
//	//			float2 v0 = p2 - p1;
//	//			float2 v1 = p2 - p0;
//	//			float2 v2 = p1 - p0;
//	//			//triangles area
//	//			float area = abs(v1.x*v2.y - v1.y * v2.x);
//	//			g2f OUT;
//	//			OUT.pos = IN[0].pos;
//	//			OUT.uv = IN[0].uv;
//	//			OUT.dist = float3(area / length(v0),0,0);
//	//			triStream.Append(OUT);
//
//	//			OUT.pos = IN[1].pos;
//	//			OUT.uv = IN[1].uv;
//	//			OUT.dist = float3(0,area / length(v1),0);
//	//			triStream.Append(OUT);
//
//	//			OUT.pos = IN[2].pos;
//	//			OUT.uv = IN[2].uv;
//	//			OUT.dist = float3(0,0,area / length(v2));
//	//			triStream.Append(OUT);
//	//		}
//	//		half4 frag(g2f IN) : COLOR
//	//		{
//	//			//distance of frag from triangles center
//	//			float d = min(IN.dist.x, min(IN.dist.y, IN.dist.z));
//	//			//fade based on dist from center
//	//			float I = exp2(-4.0*d*d);
//	//			return lerp(_Color, _WireColor, I);
//	//		}
//	//		ENDCG
//	//	}
//	//}
//	SubShader
//	{
//		Tags{"RenderType" = "Opaque"}
//		Pass{
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma geometry geom
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//			#include "Wireframe.cginc"
//			ENDCG
//		}
//	}
//	FallBack "Diffuse"
//}
//Shader "BarycentricWireframeUv1" {
//	Properties{
//		_LineColor("Line Color", Color) = (1,1,1,1)
//		_GridColor("Grid Color", Color) = (0,0,0,0)
//		_LineWidth("Line Width", float) = 0.1
//	}
//	SubShader{
//		Pass{
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//			uniform float4 _LineColor;
//			uniform float4 _GridColor;
//			uniform float _LineWidth;
//			// vertex input: position, uv1, uv2
//			struct appdata {
//				float4 vertex : POSITION;
//				float4 texcoord1 : TEXCOORD1;
//				float4 color : COLOR;
//			};
//			struct v2f {
//				float4 pos : POSITION;
//				float4 texcoord1 : TEXCOORD1;
//				float4 color : COLOR;
//			};
//			v2f vert(appdata v) {
//				v2f o;
//				o.pos = UnityObjectToClipPos(v.vertex);
//				o.texcoord1 = v.texcoord1;
//				o.color = v.color;
//				return o;
//			}
//			float4 frag(v2f i) : COLOR
//			{
//				if (i.texcoord1.x < _LineWidth || i.texcoord1.y < _LineWidth)
//				{
//					return _LineColor;
//				}
//				if ((i.texcoord1.x - i.texcoord1.y) < _LineWidth &&
//					(i.texcoord1.y - i.texcoord1.x) < _LineWidth)
//				{
//					return _LineColor;
//				}
//				else
//				{
//					return _GridColor;
//				}
//			}
//			ENDCG
//		}
//	}
//	Fallback "Diffuse"
//}
//struct VertexInput {
//	float4 vertex : POSITION;       //local vertex position
//	float3 normal : NORMAL;         //normal direction
//	float4 tangent : TANGENT;       //tangent direction    
//	float2 texcoord0 : TEXCOORD0;   //uv coordinates
//	float2 texcoord1 : TEXCOORD1;   //lightmap uv coordinates
//};
//struct VertexOutput {
//	float4 pos : SV_POSITION;              //screen clip space position and depth
//	float2 uv0 : TEXCOORD0;                //uv coordinates
//	float2 uv1 : TEXCOORD1;                //lightmap uv coordinates
//										   //below we create our own variables with the texcoord semantic. 
//	float4 posWorld : TEXCOORD3;           //world position of the vertex
//	float3 normalDir : TEXCOORD4;          //normal direction   
//	float3 tangentDir : TEXCOORD5;         //tangent direction 
//	float3 bitangentDir : TEXCOORD6;       //bitangent direction 
//	LIGHTING_COORDS(7, 8)                   //this initializes the unity lighting and shadow
//		UNITY_FOG_COORDS(9)                    //this initializes the unity fog
//};
//VertexOutput vert(VertexInput v) {
//	VertexOutput o = (VertexOutput)0;
//	o.uv0 = v.texcoord0;
//	o.uv1 = v.texcoord1;
//	o.normalDir = UnityObjectToWorldNormal(v.normal);
//	o.tangentDir = normalize(mul(_Object2World, half4(v.tangent.xyz, 0.0)).xyz);
//	o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
//	o.posWorld = mul(_Object2World, v.vertex);
//	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//	UNITY_TRANSFER_FOG(o, o.pos);
//	TRANSFER_VERTEX_TO_FRAGMENT(o)
//		return o;
//}
//float4 frag(VertexOutput i) : COLOR{
//	//normal direction calculations
//	i.normalDir = normalize(i.normalDir);
//float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
//float3 normalMap = UnpackNormal(tex2D(_NormalMap,TRANSFORM_TEX(i.uv0, _NormalMap)));
//float3 normalDirection = normalize(mul(normalMap.rgb, tangentTransform));
////diffuse color calculations
//float3 mainTexColor = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
//float3 diffuseColor = _Color.rgb * mainTexColor.rgb;
////light calculations
//float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
//float attenuation = LIGHT_ATTENUATION(i);
//float3 attenColor = attenuation * _LightColor0.rgb;
//float lightingModel = max(0, dot(normalDirection, lightDirection));
//
//float4 finalDiffuse = float4(lightingModel * diffuseColor * attenColor,1);
//UNITY_APPLY_FOG(i.fogCoord, finalDiffuse);
//return finalDiffuse;
//}
//Shader "Debug/Vertex color" {
//	SubShader{
//		Pass{
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//			// vertex input: position, color
//			struct appdata {
//				float4 vertex : POSITION;
//				fixed4 color : COLOR;
//				float3 normal : NORMAL;
//			};
//			struct v2f {
//				float4 pos : SV_POSITION;
//				fixed4 color : COLOR;
//				float4 posWorld : TEXCOORD3;
//				float3 normalDir : TEXCOORD4;
//			};
//			v2f vert(appdata v) {
//				v2f o;
//				o.pos = UnityObjectToClipPos(v.vertex);
//				o.color = v.color;
//				o.posWorld = mul(_Object2World, v.vertex);
//				o.normalDir = UnityObjectToWorldNormal(v.normal);
//				return o;
//			}
//			fixed4 frag(v2f i) : SV_Target{ 
//				float3 dpdx = ddx(i.posWorld);
//				float3 dpdy = ddy(i.posWorld);
//				i.normalDir = normalize(cross(dpdy, dpdx));
//				return i.color;
//			}
//			ENDCG
//		}
//	}
//}
