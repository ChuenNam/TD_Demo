Shader "Custom/Grid"
{
    Properties
    {
        _MainColor ("Background Color", Color) = (0, 0, 0, 0)
        _LineColor ("Line Color", Color) = (1, 1, 1, 1)
        _LineWidth ("Line Width", Range(0.001, 0.2)) = 0.01
        _GridSize ("Grid Size", Float) = 1.0
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };
            
            float4 _MainColor;
            float4 _LineColor;
            float _LineWidth;
            float _GridSize;
            float2 _Offset;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                // 获取世界坐标
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // 计算世界空间中的网格位置
                float2 gridPos = i.worldPos.xz / _GridSize;
                
                // 减去偏移
                gridPos -= _Offset;
                
                // 计算到最近网格线的距离
                float2 grid = frac(gridPos);
                
                // 计算线条（使用反距离）
                float2 l = smoothstep(_LineWidth, 0.0, grid) + 
                            smoothstep(1.0 - _LineWidth, 1.0, grid);
                
                // 使用最大函数确保水平和垂直线都可见
                float lineMask = max(l.x, l.y);
                
                // 混合颜色
                float4 color = lerp(_MainColor, _LineColor, lineMask);
                
                // 确保背景完全透明
                color.a = max(color.a, lineMask);
                
                return color;
            }
            ENDCG
        }
    }
}