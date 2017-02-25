#ifndef LOS_INCLUDED
#define LOS_INCLUDED

#include "UnityCG.cginc"

float LinearStep(float minValue, float maxValue, float v)
{
  return clamp((v - minValue) / (maxValue - minValue), 0, 1);
}

float ReduceLightBleeding(float p_max, float Amount)
{
	//Remove the [0, Amount] tail and linearly rescale (Amount, 1].
	return LinearStep(Amount, 1, p_max);
}

float ChebyshevUpperBound(float2 moments, float t, float minVariance)
{
	float p = (t <= moments.x);
	float variance = moments.y - (moments.x * moments.x);
	variance = max(variance, minVariance);

	float d = t - moments.x;
	float p_max = variance / (variance + d*d);
	p_max = ReduceLightBleeding(p_max, 0.5);

	return max(p, p_max);
}

float CalculateFade(float value, float fadeAmount)
{
	float fadedValue = value;
	fadedValue -= 1 - fadeAmount;
	fadedValue /= max(fadeAmount, 0.00000001);

	return 1 - saturate(fadedValue);
}

#endif