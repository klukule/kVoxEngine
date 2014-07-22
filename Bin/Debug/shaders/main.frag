varying vec3 vertex_light_position;
varying vec3 vertex_normal;
varying vec3 vertex_color;
 
void main(void)
{
  float diffuse_value = max(dot(vertex_normal, vertex_light_position), 0.0);
  vec4 color = vec4(vertex_color, 1) * max(0.7, diffuse_value);
  
  gl_FragColor = color;
}