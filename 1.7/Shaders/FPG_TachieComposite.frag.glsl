#version 450

layout(location = 1) in vec2 vUv;
layout(location = 0) out vec4 outColor;

// 与原版 GLSL 的材质参数布局一致。
layout(set = 2, binding = 0) uniform MaterialUniforms
{
    // xy：差分左上角位置 / 底图尺寸
    // zw：差分尺寸 / 底图尺寸
    vec4 _OverlayRect; // @default = vec4(0.0, 0.0, 1.0, 1.0)
} uMaterial;

layout(set = 2, binding = 1) uniform sampler2D _MainTex;
layout(set = 2, binding = 2) uniform sampler2D _OverlayTex;

void main()
{
    vec4 bottom = texture(_MainTex, vUv);

    vec2 topUv =
        (vUv - uMaterial._OverlayRect.xy)
        / uMaterial._OverlayRect.zw;

    // 先采样，随后将差分范围以外的部分设为透明。
    vec4 top = texture(_OverlayTex, clamp(topUv, 0.0, 1.0));

    bool inside =
        all(greaterThanEqual(topUv, vec2(0.0))) &&
        all(lessThanEqual(topUv, vec2(1.0)));

    if (!inside)
        top = vec4(0.0);

    // Straight alpha 的 source-over 合成：
    // 即使底图也是半透明，结果仍然正确。
    float alpha = top.a + bottom.a * (1.0 - top.a);

    vec3 premultiplied =
        top.rgb * top.a +
        bottom.rgb * bottom.a * (1.0 - top.a);

    vec3 rgb = alpha > 0.000001
        ? premultiplied / alpha
        : vec3(0.0);

    outColor = vec4(rgb, alpha);
}