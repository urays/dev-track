#include "../inc/sup_draw.h"
#include "../../conf-anls.h"

#define white      (IMG_WHITE)
#define black      (IMG_BLACK)

#define DRAW_ROWS      (IMG_HEIGHT)      //image screen height 
#define DRAW_COLS      (IMG_WIDTH)       //image screen width
#define DRAW_LEFT      (IMAGE_LEFT)      //the leftmost end of the screen
#define DRAW_RIGHT     (IMAGE_RIGHT)     //the rightmost end of the screen
#define DRAW_TOP       (IMAGE_TOP)       //top edge of screen
#define DRAW_BOTTOM    (IMAGE_BOTTOM)    //bottom edge of screen
#define DRAW_CENTER    (IMAGE_CENTER)    //screen middle column 

//(var)IMG_ANLS:分析结果
static unsigned char IMG_ANLS[IMG_HEIGHT][IMG_WIDTH] = { 0 };

unsigned char* imageInfo(int* rows, int* cols)
{
	unsigned char* p = &IMG_ANLS[0][0];
	*rows = DRAW_ROWS;
	*cols = DRAW_COLS;
	return p;
}


#include <string.h>   //memset

#include "../inc/common.h"

void anlsImgCLS(void)
{
	if (IMG_DRAW_EN)
	{
		memset(IMG_ANLS, 0, DRAW_ROWS*DRAW_COLS);
	}
}

void PreviewLineBase(void)
{
	if (IMG_DRAW_EN)
	{
		char line, count = 0;
		for (line = DRAW_ROWS - 20; line < DRAW_ROWS; line++)//show base vertical line
		{
			if (count < 5)
			{
				IMG_ANLS[line][DRAW_CENTER] = white;
			}
			count++;
			if (count > 10)
			{
				count = 0;
			}
		}
	}
}

void PreviewLine(const LINE org, const int startpos, const int endpos)
{
	if (IMG_DRAW_EN)
	{
		int t;
		for (t = startpos; t <= endpos; t++)
		{
			if (POINT_CHK(org.line[t]))
			{
				IMG_ANLS[org.line[t].y][org.line[t].x] = white;
			}
		}
	}
}

void PreviewPointX(const POINT point, const char size)
{
	if (IMG_DRAW_EN)
	{
		char t = 0;
		for (t = 1; t <= size; t++)
		{
			if (point.x + t > DRAW_RIGHT || point.x - t < DRAW_LEFT
				|| point.y + t > DRAW_BOTTOM || point.y - t < DRAW_TOP)
			{
				return;
			}
			IMG_ANLS[point.y + t][point.x - t] = white;
			IMG_ANLS[point.y + t][point.x + t] = white;
			IMG_ANLS[point.y - t][point.x + t] = white;
			IMG_ANLS[point.y - t][point.x - t] = white;
		}
		IMG_ANLS[point.y][point.x] = white;
	}
}

void PreviewPoint(const POINT point, const char size)
{
	if (IMG_DRAW_EN)
	{
		char t = 0;
		for (t = 1; t <= size; t++)
		{
			if (point.x + t > DRAW_RIGHT || point.x - t<DRAW_LEFT
				|| point.y + t > DRAW_BOTTOM || point.y - t < DRAW_TOP)
			{
				return;
			}
			IMG_ANLS[point.y + t][point.x] = white;
			IMG_ANLS[point.y][point.x + t] = white;
			IMG_ANLS[point.y - t][point.x] = white;
			IMG_ANLS[point.y][point.x - t] = white;
		}
		IMG_ANLS[point.y][point.x] = white;
	}
}

void PreviewPointRect(const POINT point, const char len)
{
	if (IMG_DRAW_EN)
	{
		int coolen = (int)((len - 1) >> 1);
		int i = 0, cot = (len - 1) * 4;
		int s_x = coolen, s_y = coolen;
		char turn = 0;
		for (i = 0; i < cot; i++)
		{
			if (point.y + s_y <= DRAW_BOTTOM && point.y + s_y >= DRAW_TOP)
			{
				if (point.x + s_x >= DRAW_LEFT && point.x + s_x <= DRAW_RIGHT)
				{
					IMG_ANLS[point.y + s_y][point.x + s_x] = white;
				}
			}
			if (turn == 0)
			{
				if (s_y > -coolen)
				{
					s_y--;
					if (s_y == -coolen) turn = 1;
				}
			}
			else if (turn == 1)
			{
				if (s_x > -coolen)
				{
					s_x--;
					if (s_x == -coolen) turn = 2;
				}
			}
			else if (turn == 2)
			{
				if (s_y < coolen)
				{
					s_y++;
					if (s_y == coolen) turn = 3;
				}
			}
			else if (turn == 3)
			{
				if (s_x < coolen)
				{
					s_x++;
					if (s_x == coolen) break;
				}
			}
		}
	}
}

void PreviewCircle(int x0, int y0, int r)
{
	int a = 0, b = r, di = 3 - r * 2;
	while (a <= b)
	{
		IMG_ANLS[x0 + a][y0 + b] = white;
		IMG_ANLS[x0 + a][y0 - b] = white;
		IMG_ANLS[x0 + b][y0 + a] = white;
		IMG_ANLS[x0 + b][y0 - a] = white;
		IMG_ANLS[x0 - a][y0 - b] = white;
		IMG_ANLS[x0 - a][y0 + b] = white;
		IMG_ANLS[x0 - b][y0 + a] = white;
		IMG_ANLS[x0 - b][y0 - a] = white;
		a++;
		if (di < 0) di += 4 * a + 6;
		else {
			di += 10 + 4 * (a - b);
			b--;
		}
	}
}

void PreviewLabLine(const char x,const char y,const char len)
{
	if (IMG_DRAW_EN)
	{
		char t = 0;
		for (t = 0; t < len; t++)
		{
			if (x + t <= DRAW_RIGHT && x + t >= DRAW_LEFT)
			{
				IMG_ANLS[y][x + t] = white;
			}
			if (x - t <= DRAW_RIGHT && x - t >= DRAW_LEFT)
			{
				IMG_ANLS[y][x - t] = white;
			}
		}
	}
}

void PreviewLabColumn(const char x, const char y, const char len)
{
	if (IMG_DRAW_EN)
	{
		char t = 0;
		for (t = 0; t < len; t++)
		{
			if (y + t <= DRAW_BOTTOM && y + t >= DRAW_TOP)
			{
				IMG_ANLS[y + t][x] = white;
			}
			if (y - t <= DRAW_BOTTOM && y - t >= DRAW_TOP)
			{
				IMG_ANLS[y - t][x] = white;
			}
		}
	}
}

void PreviewScaleMark(void)
{
	if (IMG_SCALE_EN && IMG_DRAW_EN)
	{
		PreviewLabLine(DRAW_CENTER - 30, 10, 6);//left
		PreviewLabLine(DRAW_CENTER - 30, 20, 4);
		PreviewLabLine(DRAW_CENTER - 30, 30, 6);
		PreviewLabLine(DRAW_CENTER - 30, 40, 4);
		PreviewLabLine(DRAW_CENTER - 30, 50, 6);

		PreviewLabLine(DRAW_CENTER + 30, 10, 6);//right
		PreviewLabLine(DRAW_CENTER + 30, 20, 4);
		PreviewLabLine(DRAW_CENTER + 30, 30, 6);
		PreviewLabLine(DRAW_CENTER + 30, 40, 4);
		PreviewLabLine(DRAW_CENTER + 30, 50, 6);

		PreviewLabColumn(DRAW_CENTER - 30, DRAW_CENTER / 2, 4);//middle
		PreviewLabColumn(DRAW_CENTER - 20, DRAW_CENTER / 2, 6);
		PreviewLabColumn(DRAW_CENTER - 10, DRAW_CENTER / 2, 4);
		PreviewLabColumn(DRAW_CENTER, DRAW_CENTER / 2, 6);
		PreviewLabColumn(DRAW_CENTER + 10, DRAW_CENTER / 2, 4);
		PreviewLabColumn(DRAW_CENTER + 20, DRAW_CENTER / 2, 6);
		PreviewLabColumn(DRAW_CENTER + 30, DRAW_CENTER / 2, 4);
	}
}
