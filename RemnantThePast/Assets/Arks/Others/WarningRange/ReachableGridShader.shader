Shader "Custom/ReachableGridShader"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "black" {}
        _RangeColor1 ("Player Move Range Color", Color) = (1, 1, 1, 1)//玩家移动区域
        _RangeColor2 ("Move Warning Range Color", Color) = (1, 1, 1, 1)//玩家移动区域与敌人警戒范围交接处
        _RangeColor3 ("Monster Warning Range Color", Color) = (1, 1, 1, 1)//敌人警戒范围
        _RangeColor4 ("Skill Target Color", Color) = (1, 1, 1, 1)//技能范围
        _RangeMask ("Range Mask", 2D) = "black" {} 
        _GridOrigin ("Grid Origin", Vector) = (-0.05, -0.03, 0, 0)
        _GridSize ("Grid Size", Float) = 10.0 
		_Speed("Speed", Vector) = (0,0,0,0)
    }
    
    SubShader
    {
        Tags { 
            "Queue"="Transparent+100"
            "RenderType"="Transparent" 
            "ForceNoShadowCasting"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };
            
            fixed4 _RangeColor1;
            fixed4 _RangeColor2;
            fixed4 _RangeColor3;
            fixed4 _RangeColor4;
            sampler2D _MainTex;
            sampler2D _RangeMask;
            float4 _GridOrigin;
            float _GridSize;
			float2 _Speed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 gridCoord = (i.worldPos.xy - _GridOrigin.xy) / _GridSize;
                float2 maskUV = gridCoord + 0.5;
                float3 texCood = i.worldPos;
                texCood.x += _Time.y * _Speed.x;
                float4 tex = tex2D(_MainTex, texCood);

                //范围判断
                float4 _Range = tex2D(_RangeMask, maskUV);

                float isB = step(0.5, _Range.b);
                
                float isR = step(0.5, _Range.r);
                
                float isG = step(0.5, _Range.g);

                fixed4 result = 
                    (isR * (1 - isB)) * _RangeColor3 +  // 只有R
                    ((1 - isR) * isB) * _RangeColor1 +  // 只有B
                    (isR * isB) * _RangeColor2;         // R和B都有
                result = isG * _RangeColor4 + (1 - isG) * result;

                fixed4 col = tex * result;

                return col;
            }
            ENDCG
        }
    }
}