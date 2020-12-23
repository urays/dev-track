//camera.c -v0.2 author:urays data:2020-08-06 email:urays@foxmail.com
#include <string.h>

#include "camera.h"

static unsigned char* ssrc = ((void*)0);

static unsigned char dp[FRAME_WIDTH * FRAME_HEIGHT / 8] = { 0x00 };
static unsigned char usrc[FRAME_WIDTH * FRAME_HEIGHT] = { 0x00 };

int _w(int x, int y)
{
	unsigned long long p1 = y * FRAME_WIDTH + x;
	unsigned long long p2 = p1 >> 3;

	if (dp[p2] == 0x00)
	{
		unsigned char ch = *(ssrc + p2);
		unsigned char* tp = usrc + (p2 << 3);

		*(tp + 7) = (((ch) & 0x01)); ch >>= 1;
		*(tp + 6) = (((ch) & 0x01)); ch >>= 1;
		*(tp + 5) = (((ch) & 0x01)); ch >>= 1;
		*(tp + 4) = (((ch) & 0x01)); ch >>= 1;
		*(tp + 3) = (((ch) & 0x01)); ch >>= 1;
		*(tp + 2) = (((ch) & 0x01)); ch >>= 1;
		*(tp + 1) = (((ch) & 0x01)); ch >>= 1;
		*(tp) = (((ch) & 0x01));

		dp[p2] = 0x01;
	}
	return (*(usrc + p1) == WRITE_PIXEL);
}

void _camerafunc(unsigned char* src)
{
	ssrc = src;
	memset(dp, 0x00, sizeof(dp));
}