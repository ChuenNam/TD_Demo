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
        // 补充：开启Alpha混合的正确设置
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off // 可选：关闭背面剔除，双面显示网格
        Lighting Off // 关闭光照，避免影响透明效果
        
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
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // 计算世界空间中的网格位置
                float2 gridPos = i.worldPos.xz / _GridSize;
                gridPos -= _Offset;
                
                // 计算到最近网格线的距离（生成线条遮罩）
                float2 grid = frac(gridPos);
                float2 l = smoothstep(_LineWidth, 0.0, grid) + 
                            smoothstep(1.0 - _LineWidth, 1.0, grid);
                float lineMask = max(l.x, l.y);
                
                // 核心修复：混合颜色时保留线条的原始透明度
                // 方式：线条颜色 * 线条遮罩 + 背景颜色 * (1 - 线条遮罩)
                float4 finalColor = _LineColor * lineMask + _MainColor * (1 - lineMask);
                
                // 关键：不再强行覆盖alpha，而是保留混合后的alpha值
                // 最终alpha = 线条alpha * 线条遮罩 + 背景alpha * (1 - 线条遮罩)
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit" // 可选：添加回退Shader，提升兼容性
}
