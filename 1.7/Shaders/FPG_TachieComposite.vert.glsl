#version 450

layout(location = 0) in vec3 inPosition; // @vertex Position required

layout(location = 1) out vec2 vUv;

void main()
{
    // 当前 UnityStubs.Graphics.Blit 使用 XZ 平面的四边形，
    // x、z 的范围都是 -0.5 到 0.5。
    vec2 p = inPosition.xz;

    gl_Position = vec4(p * 2.0, 0.0, 1.0);

    // 本实现采用左上角为原点的纹理坐标约定。
    vUv = p + vec2(0.5);
}