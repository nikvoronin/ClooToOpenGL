// double precision

float2 clrand2(uint id, __global uint4* clrng)
{
	uint s1 = clrng[id].x;
	uint s2 = clrng[id].y;
	uint s3 = clrng[id].z;
	uint b;

	b = (((s1 << 13) ^ s1) >> 19);
	s1 = (((s1 & 4294967294) << 12) ^ b);
	b = (((s2 << 2) ^ s2) >> 25);
	s2 = (((s2 & 4294967288) << 4) ^ b);
	b = (((s3 << 3) ^ s3) >> 11);
	s3 = (((s3 & 4294967280) << 17) ^ b);

	float2 result;
	result.x = (float)((s1 ^ s2 ^ s3) * 2.3283064365e-10);

	b = (((s1 << 13) ^ s1) >> 19);
	s1 = (((s1 & 4294967294) << 12) ^ b);
	b = (((s2 << 2) ^ s2) >> 25);
	s2 = (((s2 & 4294967288) << 4) ^ b);
	b = (((s3 << 3) ^ s3) >> 11);
	s3 = (((s3 & 4294967280) << 17) ^ b);

	result.y = (float)((s1 ^ s2 ^ s3) * 2.3283064365e-10);

	clrng[id] = (uint4)(s1, s2, s3, b);

	return result;
}

__kernel void Mandelbrot(
	const uint  width,
	const uint  height,
	const float reMin,
	const float reMax,
	const float imMin,
	const float imMax,
	const uint maxIter,
	__global uint4* clrng,
	__global uchar4* clout)
{
	const double escapeOrbit = 4.0;

	float2 rand = clrand2(get_global_id(0), clrng);
	double2 c = (double2)(mix(reMin, reMax, rand.x), mix(imMin, imMax, rand.y));

	double2 z = 0.0;
	int iter = 0;

	if (!(((c.x - 0.25)*(c.x - 0.25) + (c.y * c.y))*(((c.x - 0.25)*(c.x - 0.25) + (c.y * c.y)) + (c.x - 0.25)) < 0.25 * c.y * c.y))  //main cardioid
	{
		if (!((c.x + 1.0) * (c.x + 1.0) + (c.y * c.y) < 0.0625))            //2nd order period bulb
		{
			if (!((((c.x + 1.309)*(c.x + 1.309)) + c.y*c.y) < 0.00345))    //smaller bulb left of the period-2 bulb
			{
				if (!((((c.x + 0.125)*(c.x + 0.125)) + (c.y - 0.744)*(c.y - 0.744)) < 0.0088))      // smaller bulb bottom of the main cardioid
				{
					if (!((((c.x + 0.125)*(c.x + 0.125)) + (c.y + 0.744)*(c.y + 0.744)) < 0.0088))  //smaller bulb top of the main cardioid
					{
						while ((iter < maxIter) && ((z.x * z.x + z.y * z.y) < escapeOrbit))
						{
							z = (double2)(z.x * z.x - z.y * z.y, (z.x * z.y * 2.0)) + c;
							iter++;
						}
					}
				}
			}
		}
	}

	int x = (c.x - reMin) / (reMax - reMin) * width;
	int y = height - (c.y - imMin) / (imMax - imMin) * height;

	if ((x >= 0) && (y >= 0) && (x < width) && (y < height))
	{
		int i = x + y * width;
		double clr = 0.0;

		if (iter < maxIter)
		{
			// b&w
			//clr = 255.0f;

			// filled
			//clr = length(z) / escapeOrbit * 255.0f;

			// smoothed
			//double k = 1.0f / half_log(escapeOrbit);
			//clr = 5.0f + iter - half_log(0.5f) * k - half_log(half_log(sqrt(z.x * z.x + z.y * z.y))) * k;

			/*	// just precalc for the fixed escape orbit
			escapeOrbit = 4.0f;
			half_log(escapeOrbit) ≈ 0.60205999;
			k = 1.0f / half_log(escapeOrbit) ≈ 1.66096405;
			half_log(0.5f) ≈ -0.30103;
			half_log(0.5f) * k ≈ -0.5f;

			clr = 5.5f + iter + half_log(half_log(dot(z, z))) * 1.66096405f;
			*/
			clr = 5.0 + iter - log(log(dot(z, z))) * log(1.0 / (reMax - reMin));
		}

		clr = clamp(clr, 0.0, 255.0);
		clr = (clr + clout[i].x) * 0.5;

		clout[i].x = (uchar)clr;
		clout[i].y = (uchar)clr;
		clout[i].z = (uchar)clr;
		clout[i].w = 255;
	} // if iter x y width height
} // __kernel