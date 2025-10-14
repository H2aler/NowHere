Shader "NowHere/AdvancedPBR"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _MetallicMap ("Metallic Map", 2D) = "black" {}
        _AOMap ("Ambient Occlusion", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "black" {}
        _EmissionMap ("Emission Map", 2D) = "black" {}
        
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Roughness ("Roughness", Range(0,1)) = 0.5
        _NormalStrength ("Normal Strength", Range(0,2)) = 1.0
        _HeightScale ("Height Scale", Range(0,0.1)) = 0.02
        _EmissionStrength ("Emission Strength", Range(0,5)) = 0.0
        
        // Advanced Features
        _SubsurfaceColor ("Subsurface Color", Color) = (1,0.5,0.5,1)
        _SubsurfaceStrength ("Subsurface Strength", Range(0,1)) = 0.0
        _Anisotropy ("Anisotropy", Range(-1,1)) = 0.0
        _Clearcoat ("Clearcoat", Range(0,1)) = 0.0
        _ClearcoatRoughness ("Clearcoat Roughness", Range(0,1)) = 0.0
        
        // AR Features
        _ARReflectionStrength ("AR Reflection Strength", Range(0,2)) = 1.0
        _ARLightingBoost ("AR Lighting Boost", Range(0,3)) = 1.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 400
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0
        
        #include "UnityCG.cginc"
        #include "UnityPBSLighting.cginc"
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _RoughnessMap;
        sampler2D _MetallicMap;
        sampler2D _AOMap;
        sampler2D _HeightMap;
        sampler2D _EmissionMap;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
            float4 screenPos;
        };
        
        fixed4 _Color;
        half _Metallic;
        half _Roughness;
        half _NormalStrength;
        half _HeightScale;
        half _EmissionStrength;
        fixed4 _SubsurfaceColor;
        half _SubsurfaceStrength;
        half _Anisotropy;
        half _Clearcoat;
        half _ClearcoatRoughness;
        half _ARReflectionStrength;
        half _ARLightingBoost;
        
        void vert (inout appdata_full v)
        {
            // Parallax mapping
            float2 offset = ParallaxOffset(tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0)).r, _HeightScale, v.texcoord.xy);
            v.texcoord.xy += offset;
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample textures
            fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed3 normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
            normal.xy *= _NormalStrength;
            
            half roughness = tex2D(_RoughnessMap, IN.uv_MainTex).r * _Roughness;
            half metallic = tex2D(_MetallicMap, IN.uv_MainTex).r * _Metallic;
            half ao = tex2D(_AOMap, IN.uv_MainTex).r;
            fixed3 emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * _EmissionStrength;
            
            // Apply AO
            albedo.rgb *= ao;
            
            // Subsurface scattering
            half3 subsurface = _SubsurfaceColor.rgb * _SubsurfaceStrength * saturate(dot(-IN.viewDir, IN.worldNormal));
            albedo.rgb += subsurface;
            
            // AR lighting enhancement
            albedo.rgb *= _ARLightingBoost;
            emission *= _ARReflectionStrength;
            
            o.Albedo = albedo.rgb;
            o.Normal = normal;
            o.Metallic = metallic;
            o.Smoothness = 1.0 - roughness;
            o.Occlusion = ao;
            o.Emission = emission;
            o.Alpha = albedo.a;
        }
        ENDCG
    }
    
    FallBack "Standard"
}
