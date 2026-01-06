Shader "Custom/BuildPreviewUltraSimple"
{
    Properties
    {
        // 只需要两个参数
        _MainColor ("Main Color", Color) = (0, 0.5, 1, 0.3)
        _EdgeColor ("Edge Color", Color) = (0, 1, 0, 1)
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float rim : TEXCOORD0;
            };
            
            fixed4 _MainColor;
            fixed4 _EdgeColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // 计算边缘因子
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                float3 normal = normalize(v.normal);
                o.rim = 1.0 - abs(dot(viewDir, normal));
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 基础颜色
                fixed4 col = _MainColor;
                
                // 添加边缘高亮
                float edge = smoothstep(0.4, 0.6, i.rim);
                col.rgb = lerp(col.rgb, _EdgeColor.rgb, edge * _EdgeColor.a);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}