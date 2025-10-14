Shader "NowHere/ARMagicEffect"
{
    Properties
    {
        _MainTex ("Magic Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Magic Color", Color) = (0,0.5,1,1)
        _Intensity ("Intensity", Range(0,5)) = 1.0
        _Speed ("Animation Speed", Range(0,10)) = 1.0
        _Scale ("Noise Scale", Range(0,10)) = 1.0
        _Distortion ("Distortion", Range(0,1)) = 0.1
        _FresnelPower ("Fresnel Power", Range(0,5)) = 2.0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0,10)) = 3.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 300
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        CGPROGRAM
        #pragma surface surf Unlit alpha:fade vertex:vert
        #pragma target 3.0
        
        #include "UnityCG.cginc"
        
        sampler2D _MainTex;
        sampler2D _NoiseTex;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
            float4 screenPos;
        };
        
        fixed4 _Color;
        half _Intensity;
        half _Speed;
        half _Scale;
        half _Distortion;
        half _FresnelPower;
        fixed4 _RimColor;
        half _RimPower;
        
        void vert (inout appdata_full v)
        {
            // Vertex animation
            float time = _Time.y * _Speed;
            float noise = tex2Dlod(_NoiseTex, float4(v.texcoord.xy * _Scale + time * 0.1, 0, 0)).r;
            v.vertex.xyz += v.normal * noise * _Distortion;
        }
        
        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(s.Albedo, s.Alpha);
        }
        
        void surf (Input IN, inout SurfaceOutput o)
        {
            float time = _Time.y * _Speed;
            
            // Animated UV coordinates
            float2 animatedUV = IN.uv_MainTex + float2(time * 0.1, time * 0.05);
            
            // Sample textures
            fixed4 magicTex = tex2D(_MainTex, animatedUV);
            fixed noise = tex2D(_NoiseTex, IN.uv_MainTex * _Scale + time * 0.2).r;
            
            // Fresnel effect
            half fresnel = 1.0 - saturate(dot(IN.viewDir, IN.worldNormal));
            fresnel = pow(fresnel, _FresnelPower);
            
            // Rim lighting
            half rim = 1.0 - saturate(dot(IN.viewDir, IN.worldNormal));
            rim = pow(rim, _RimPower);
            
            // Combine effects
            fixed3 magicColor = _Color.rgb * magicTex.rgb * _Intensity;
            magicColor += _RimColor.rgb * rim * 0.5;
            magicColor *= noise;
            
            float alpha = magicTex.a * fresnel * _Color.a;
            alpha *= noise;
            
            o.Albedo = magicColor;
            o.Alpha = alpha;
        }
        ENDCG
    }
    
    FallBack "Transparent/Diffuse"
}
