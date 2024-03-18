#define TWOPI 6.283185307

float inverse_lerp(float min, float max, float v) {
    return (v - min) / (max - min);
}
