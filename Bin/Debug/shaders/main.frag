varying vec3 vertex_light_position;
varying vec3 vertex_normal;
varying vec3 vertex_color;
 
void main(void)
{
  float diffuse_value = max(dot(vec3(0,1,0), vertex_light_position), 0.0);
  gl_FragData[0] = vec4(vertex_color, 1) * max(0.5, diffuse_value);
  gl_FragData[1] = vec4(vertex_normal, 1);// * max(0.5, diffuse_value);
}