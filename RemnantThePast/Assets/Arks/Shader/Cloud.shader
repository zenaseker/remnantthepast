// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Cloud"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_MoveSpeed("MoveSpeed", Vector) = (0,0,0,0)
		_MianTex2("MianTex2", 2D) = "white" {}
		_MoveSpeed2("MoveSpeed2", Vector) = (0,0,0,0)
		_ColorPower("ColorPower", Float) = 0
		_AlpthPower("AlpthPower", Float) = 0

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _MainTex;
			uniform float2 _MoveSpeed;
			uniform float4 _MainTex_ST;
			uniform sampler2D _MianTex2;
			uniform float2 _MoveSpeed2;
			uniform float4 _MianTex2_ST;
			uniform float _AlpthPower;
			uniform float _ColorPower;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float2 uv_MainTex = v.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 panner55 = ( 1.0 * _Time.y * _MoveSpeed + uv_MainTex);
				float2 uv_MianTex2 = v.ase_texcoord.xy * _MianTex2_ST.xy + _MianTex2_ST.zw;
				float2 panner62 = ( 1.0 * _Time.y * _MoveSpeed2 + uv_MianTex2);
				float4 temp_output_65_0 = ( tex2Dlod( _MainTex, float4( panner55, 0, 0.0) ) * tex2Dlod( _MianTex2, float4( panner62, 0, 0.0) ) );
				
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( temp_output_65_0 * _AlpthPower ).rgb;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float4 color61 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float2 uv_MainTex = i.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 panner55 = ( 1.0 * _Time.y * _MoveSpeed + uv_MainTex);
				float2 uv_MianTex2 = i.ase_texcoord1.xy * _MianTex2_ST.xy + _MianTex2_ST.zw;
				float2 panner62 = ( 1.0 * _Time.y * _MoveSpeed2 + uv_MianTex2);
				float4 temp_output_65_0 = ( tex2D( _MainTex, panner55 ) * tex2D( _MianTex2, panner62 ) );
				
				
				finalColor = ( color61 * _ColorPower * temp_output_65_0 );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18912
118;569;1407;624;1703.024;119.6551;1;True;True
Node;AmplifyShaderEditor.Vector2Node;64;-1078.497,274.6299;Inherit;False;Property;_MoveSpeed2;MoveSpeed2;3;0;Create;True;0;0;0;False;0;False;0,0;0.012,0.001;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;54;-1117.129,-96.43063;Inherit;False;0;53;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;56;-1076.13,30.56937;Inherit;False;Property;_MoveSpeed;MoveSpeed;1;0;Create;True;0;0;0;False;0;False;0,0;0.01,0.003;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;-1110.024,150.3449;Inherit;False;0;63;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;55;-845.1296,-96.43063;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;62;-854.4971,134.6298;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;53;-631.1303,-123.4306;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;20f42fae723442c4b916d39a4e1ff262;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;63;-640.4977,94.82987;Inherit;True;Property;_MianTex2;MianTex2;2;0;Create;True;0;0;0;False;0;False;-1;None;985e11f1a44231b4a9e10266f0458795;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-222.6312,-13.30353;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;61;-7.530701,-370.2307;Inherit;False;Constant;_Color0;Color 0;4;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;57;35.4693,-182.2307;Inherit;False;Property;_ColorPower;ColorPower;4;0;Create;True;0;0;0;False;0;False;0;1.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;53.14958,110.2523;Inherit;False;Property;_AlpthPower;AlpthPower;5;0;Create;True;0;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;350.4693,-172.2307;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;342.5281,30.09806;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;79;818.2116,-131.966;Float;False;True;-1;2;ASEMaterialInspector;100;1;Custom/Cloud;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Transparent=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;55;0;54;0
WireConnection;55;2;56;0
WireConnection;62;0;80;0
WireConnection;62;2;64;0
WireConnection;53;1;55;0
WireConnection;63;1;62;0
WireConnection;65;0;53;0
WireConnection;65;1;63;0
WireConnection;59;0;61;0
WireConnection;59;1;57;0
WireConnection;59;2;65;0
WireConnection;60;0;65;0
WireConnection;60;1;58;0
WireConnection;79;0;59;0
WireConnection;79;1;60;0
ASEEND*/
//CHKSM=1C08687D332714A2BB82643331D7C62FDDEB7F52