Shader "Spine/CustomShader" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass {
			Name "Normal"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			float _BaselineZ;

			struct VertexInput {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};
			float4 GetSpineZ(float4 vertex) {
				float4 viewVertex = mul(UNITY_MATRIX_MV, vertex);
				float4 viewOrigin = mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1));
				viewVertex.z += 2.0 * (viewVertex.y - viewOrigin.y);
				float4 clipPos = mul(UNITY_MATRIX_P, viewVertex);	
				return clipPos;
			}

			VertexOutput vert (VertexInput v) {
				VertexOutput o = (VertexOutput)0;
				o.uv = v.uv;
				o.vertexColor = v.vertexColor;
				o.pos = UnityObjectToClipPos(v.vertex);
				float4 p = GetSpineZ(v.vertex);
				o.pos.z = p.z / p.w * o.pos.w;
				return o;
			}

			float4 frag (VertexOutput i) : SV_Target {
				float4 texColor = tex2D(_MainTex, i.uv);
				texColor.rgb *= texColor.a;
				return (texColor * i.vertexColor);
			}
			ENDCG
		}
		}
}
