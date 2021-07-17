#include "../inc/sup_exfunc.h"
#include "../../conf-anls.h"


#include "../inc/common.h"

//宽度调整  如果摄像头角度和高度不变,可通过 修改此值快速 修改赛道宽度数组
#define GAIN_WIDTH        (-3)

//road width
//static int WIDTH_INFO[] = {
//
//	92, 91, 90, 88, 88, 87, 86, 85, 84, 82,
//	81, 80, 78, 76, 76, 75, 74, 72, 70, 70,
//	68, 68, 66, 64, 63, 62, 60, 59, 58, 56,
//	55, 53, 52, 50, 49, 47, 46, 44, 42, 40,
//	39, 37, 35, 34, 32, 30, 28, 26, 25, 22,
//	21 ,20, 18, 16, 15,  0,  0,  0,  0,  0,
//};

static int WIDTH_INFO[] = {
120, 118,116,114,112,110,108,106,104,102,100,
98,96,94,92,90,88,86,84,82,80,
78,76,74,72,70,68,66,64,62,60,
58,56,54,52,50,48,46,44,42,40,
38,36,34,32,30,28,26,24,22,20,
18,16,14,12,10,8,6,4,2,
};

#ifdef _TEST_CFG_WIDTH
int straight_width[IMG_HEIGHT + 5] = { 0 };//watch array
void Cfg_RoadWidth(void)
{
	int i = 0;
	for (i = 0; i < IMG_HEIGHT; i++)
	{
		straight_width[i] = WhiteCount_LR(EXF_BOTH, IMAGE_CENTER, IMG_HEIGHT - 1 - i);
	}
}
#endif

int Get_Road_Width(const int line)
{
	int finl = line < 0 ? 0 : (line > IMAGE_BOTTOM ? IMAGE_BOTTOM : line);
	if (WIDTH_INFO[IMAGE_BOTTOM - finl] + GAIN_WIDTH < 0)
	{
		return 0;
	}
	return WIDTH_INFO[IMAGE_BOTTOM - finl] + GAIN_WIDTH;
}

int WhiteCount_LR(const int dir, const int st_x, const int st_y)
{
	char D = 0, P = 0;
	char column_left, column_right;
	char stop_L = ifalse, stop_R = ifalse;
	int white_count = 0;

	if (dir == EXF_LEFT) { stop_L = ifalse; stop_R = itrue; }
	else if (dir == EXF_RIGHT) { stop_L = itrue; stop_R = ifalse; }
	else if (dir == EXF_BOTH) { stop_L = ifalse; stop_R = ifalse; }
	else { return 0; }

	for (D = 0; D < IMG_WIDTH;)
	{
		P = !P;
		if (P && stop_L == ifalse)
		{
			column_left = st_x - D;
			if (column_left > IMAGE_LEFT && POINT_WHITE(column_left, st_y))
			{
				white_count++;
				D++;
			}
			else stop_L = itrue;
		}
		else {
			column_right = st_x + D;
			if (column_right < IMAGE_RIGHT && POINT_WHITE(column_right, st_y))
			{
				white_count++;
			}
			else stop_R = itrue;
		}
		if (stop_L == itrue && stop_R == itrue) break;
		else if (stop_L == itrue && stop_R == ifalse) P = 1, D++;
		else if (stop_L == ifalse && stop_R == itrue) P = 0;
	}
	return white_count;
}

void FullLine_ShotUpF(const int shotx, const int shoty, const int count, POINT *farW)
{
	char P = 0, S = 0, D = 0, T = 0;
	char stopL = ifalse, stopR = ifalse;
	int end_line = shoty - 1, shot_enter = shotx;
	int l_save = shoty - 1, r_save = shoty - 1;
	char err_flg = ifalse;

	(*farW).x = shotx;
	(*farW).y = shoty - 1;

	for (D = 0; D <= count;)
	{
		S = P = !P;
		if (P && stopL == ifalse) {
			shot_enter = shotx - D;
			if (shot_enter <= IMAGE_LEFT)
			{
				shot_enter = IMAGE_LEFT + 1;
				stopL = itrue;
			}
			else D++;
		}
		else {
			shot_enter = shotx + D;
			if (shot_enter >= IMAGE_RIGHT)
			{
				stopR = itrue;
				shot_enter = IMAGE_RIGHT - 1;
			}
		}
		if (stopL == itrue && stopR == itrue) break;
		else if (stopL == itrue && stopR == ifalse) P = 1, D++;
		else if (stopL == ifalse && stopR == itrue) P = 0;

		for (T = IMAGE_BOTTOM - 1; T > IMAGE_TOP; T--)
		{
			if (!POINT_WHITE(shot_enter, T))
			{
				if ((S && T - l_save>10) || (!S && T - r_save>10))
				{
					err_flg = itrue;
				}
				else if (end_line > T)
				{
					end_line = T;
					(*farW).x = shot_enter;
					(*farW).y = end_line;
				}
				if (S) l_save = T;
				else r_save = T;
				break;
			}
		}
		if ((*farW).y <= IMAGE_TOP || err_flg == itrue) break;
	}
}

void GetLastWhiteXY(const int startx, const int starty, const char dir, POINT* LAST)
{
	int column = 0, line = 0;

	LAST->x = startx;
	LAST->y = starty;
	if (dir == EXF_LEFT) {
		for (column = startx; column >= IMAGE_LEFT; column--) {
			if (POINT_WHITE(column, starty)) {
				LAST->x = column;
				LAST->y = starty;
			}
			else {
				break;
			}
		}
	}
	else if (dir == EXF_RIGHT) {
		for (column = startx; column <= IMAGE_RIGHT; column++) {
			if (POINT_WHITE(column, starty)) {
				LAST->x = column;
				LAST->y = starty;
			}
			else {
				break;
			}
		}
	}
	else if (dir == EXF_UP) {
		for (line = starty; line >= IMAGE_TOP; line--) {
			if (POINT_WHITE(startx, line)) {
				LAST->x = startx;
				LAST->y = line;
			}
			else {
				break;
			}
		}
	}
	else if (dir == EXF_DOWN) {
		for (line = starty; line <= IMAGE_BOTTOM; line++) {
			if (POINT_WHITE(startx, line)) {
				LAST->x = startx;
				LAST->y = line;
			}
			else {
				break;
			}
		}
	}
}

void GetLastWhiteKK(const int startx, const int starty, const float k, POINT* LAST)
{
	int line, column;
	float c = startx - k*starty;

	LAST->x = startx;
	LAST->y = starty;
	for (line = starty - 1; line > IMAGE_TOP; line--)
	{
		column = (int)(k*line + c);
		if (!POINT_WHITE(column, line)
			|| _iexc(column, IMAGE_LEFT + 1, IMAGE_RIGHT - 1))
		{
			LAST->x = column;
			LAST->y = line;
			break;
		}
	}
}

int GetLNrangeMaxWC(const int white_x, const char st_line,int usep)
{
	int line = 0;;
	int white_count = 0;
	int max_white = 0;

	if (st_line - usep < ERROR_LINE)
	{
		usep = st_line - ERROR_LINE;
	}
	for (line = st_line; line > st_line - usep; line--)
	{
		white_count = WhiteCount_LR(EXF_BOTH, white_x, line);
		if (white_count - max_white >= MutationThreshold && max_white != 0) break;
		if (max_white < white_count)
		{
			max_white = white_count;
		}
	}
	return max_white;
}
