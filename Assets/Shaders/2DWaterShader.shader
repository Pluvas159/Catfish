Shader "Custom/2DWaterShader" {
    Properties {
        _MainTex ("Water Texture", 2D) = "white" {}
        _Speed ("Speed", Range(0, 5)) = 1
        _Amplitude ("Amplitude", Range(0, 1)) = 0.1
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert
        #pragma vertex vert

        sampler2D _MainTex;
        float _Speed;
        float _Amplitude;

        struct Input {
            float2 uv_MainTex;
        };

        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_MainTex = v.texcoord + _Amplitude * sin(_Time.y * _Speed * 2 * UNITY_PI);
        }

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
