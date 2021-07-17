#include "../inc/sup_line.h"
#include "../../conf-anls.h"

//calculation of linear linearity
static void GetBeelineLinearity(LINE* const org, const int st, const int end, LSQ_INFO *lsq, float *linear);

//line corner detection
static char FM_edgeCheck(const int cenx, const int ceny);
static char FM_preBcurW(const int cenx, const int ceny, const int i);

//!
#include <string.h>  //memset
#include "../inc/common.h"

void LineInit(LINE* org, const char loc)
{
	memset(org->line, 0, LINE_BITS);
	org->endpos = 0;
	org->local = loc;
}

char checkpoint(POINT p)
{
	if (p.x == INIT_VALUE && p.y == INIT_VALUE) { return 0; }
	return 1;
}

void LSQ_algorithm(LINE* const org, int st, int end, LSQ_INFO *LSQ)
{
	int t, count = 0;
	long int sum_x = 0, sum_y = 0, sum_xy = 0, sum_yy = 0;

	LSQ->k = 0.0f;
	LSQ->c = org->line[0].x*1.0f;
	LSQ->pass = org->line[0];

	if (st < 0 || end > org->endpos) return;
	for (t = _imax(st, end); t >= _imin(st, end); t--)
	{
		if (POINT_CHK(org->line[t]) == itrue)
		{
			sum_x += org->line[t].x;
			sum_y += org->line[t].y;
			sum_xy += org->line[t].x*org->line[t].y;
			sum_yy += org->line[t].y*org->line[t].y;
			count++;
		}
	}
	if (count <= 3) return;
	float passx = sum_x / (count*1.0f);
	float passy = sum_y / (count*1.0f);
	float a1 = sum_xy - sum_x*sum_y / (count*1.0f);
	float b1 = sum_yy - sum_y*sum_y / (count*1.0f);

	LSQ->k = a1 / b1;
	LSQ->c = passx - (LSQ->k)*(passy);
	LSQ->pass.x = (int)(passx + 0.5);
	LSQ->pass.y = (int)(passy + 0.5);

	if (LSQ->k > 32.0) LSQ->k = 32.0f;
	else if (LSQ->k < -32.0) LSQ->k = -32.0f;
}

void GetFilterLine(LSQ_INFO* const lsq, const char loc, LINE *out, const char stpkey)
{
	int line = 0, column = 0;
	char firstFlg = itrue;

	LineInit(out, loc);

	for (line = IMAGE_BOTTOM - 1; line > ERROR_LINE; line--)
	{
		column = (int)(lsq->k*line + lsq->c);
		if (stpkey == itrue)
		{
			if (!POINT_WHITE(column, line)) break;
		}
		if (_iinc(column, IMAGE_LEFT, IMAGE_RIGHT))
		{
			if (firstFlg == itrue)
			{
				out->line[0].x = column;
				out->line[0].y = line;
				firstFlg = ifalse;
			}
			else{
				out->endpos++;
				out->line[out->endpos].x = column;
				out->line[out->endpos].y = line;
			}
		} 
	}
}

void NomalFilter(const char ColumnDiffer1, const char ColumnDiffer2, const char loc, LINE *out)
{
	int t = 0;
	POINT Tail = { INIT_VALUE,INIT_VALUE };
	LINE in = *out;

	LineInit(out, loc);

	for (t = 0; t < in.endpos; t++)
	{
		if (POINT_CHK(in.line[t]) == itrue)
		{
			if (POINT_CHK(in.line[t + 1]) == itrue)
			{
				if (_iiabs((in.line[t].x - in.line[t + 1].x)) < ColumnDiffer1
					&& _iiabs((in.line[t].y - in.line[t + 1].y)) < 2) {
					break;
				}
			}
		}
	}
	out->line[out->endpos] = in.line[t];
	for (; t < in.endpos; t++)
	{
		if (POINT_CHK(in.line[t + 1]) == itrue)
		{
			if (POINT_CHK(Tail) == itrue)
			{
				if (_iidif(Tail.x, in.line[t + 1].x) > ColumnDiffer2
					&& _iidif(Tail.y, in.line[t + 1].y) < 3) {
					break;
				}
			}
			Tail = in.line[t + 1];
			out->endpos++;
			out->line[out->endpos] = in.line[t+1];
		}
	}
}

void MedianFilter(const int startpos, const int endpos, const char loc,LINE *org)
{
	LINE in = *org;
	int t = 0; 
	int SplitCount = 0;

	if (endpos - startpos < 9) return;
	LineInit(org, loc);

	SplitCount = (int)((endpos - startpos) / 3.0 - 0.5);

	for (t = 0; t <= SplitCount; t++)
	{
		org->line[t].x = _imid(in.line[t * 3].x, in.line[t * 3 + 1].x, in.line[t * 3 + 2].x);
		org->line[t].y = _imid(in.line[t * 3].y, in.line[t * 3 + 1].y, in.line[t * 3 + 2].y);
	}
	org->endpos = SplitCount;
}

void AverageFilter(const int startpos, const int endpos, const int step, const char loc, LINE *org)
{
	LINE in = *org;
	int t = 0, tt = 0;
	int SplitCount = (int)((endpos - startpos + 1) / step);
	int sum_x = 0, sum_y = 0, end = 0;

	LineInit(org, loc);

	if ((endpos - startpos + 1) % step != 0) SplitCount++;
	for (t = 0; t < SplitCount; t++)
	{
		sum_x = 0;
		sum_y = 0;
		end = (endpos - startpos + 1) - step*t;
		if (end >step) end = step;
		for (tt = 0; tt < end; tt++)
		{
			sum_x += in.line[t*step + tt].x;
			sum_y += in.line[t*step + tt].y;
		}
		org->line[t].x = (int)(sum_x / (float)end + 0.5);
		org->line[t].y = (int)(sum_y / (float)end + 0.5);
	}
	org->endpos = SplitCount;
}

void GetSegment(int x0, int y0, const int x1, const int y1, const char loc, LINE *out)
{
	int dx = _iiabs(x1 - x0), sx = x0 < x1 ? 1 : -1;
	int dy = _iiabs(y1 - y0), sy = y0 < y1 ? 1 : -1;
	int err = (dx > dy ? dx : -dy) >> 1, err2;

	LineInit(out, loc);//initialize

	out->line[0].x = x0;
	out->line[0].y = y0;
	while (x0 != x1 || y0 != y1)//
	{
		err2 = err;
		if (err2 > -dx) { err -= dy; x0 += sx; }
		if (err2 <  dy) { err += dx; y0 += sy; }
		if (x0<IMAGE_LEFT || x0>IMAGE_RIGHT || y0<IMAGE_TOP || y0>IMAGE_BOTTOM) break;

		out->endpos++;
		out->line[out->endpos].x = x0;
		out->line[out->endpos].y = y0;
	}
}

static void GetBeelineLinearity(LINE* const org, const int st, const int end, LSQ_INFO *lsq, float *linear)
{
	int t = 0;
	float maxerr = 0.0f, tempcerr = 0.0f;
	int min = _imin(st, end);
	int max = _imax(st, end);
	LINE origin = *org;

	LSQ_algorithm(&origin, min, max, lsq); //column=k*line+c

	for (t = 0; t <= origin.endpos; t++)
	{
		if (POINT_CHK(origin.line[t]) == itrue)
		{
			tempcerr = origin.line[t].x - (lsq->k*origin.line[t].y + lsq->c);
			if (maxerr < _ifabs(tempcerr))
			{
				maxerr = _ifabs(tempcerr);
			}
		}
	}
	*linear = maxerr;
}

char LineLinear(LINE* org, const int st, const int end, LSQ_INFO *lsq, float *linear)
{
	GetBeelineLinearity(org, st, end, lsq, linear);

	if (end <= 1) return CURVE;//非线段 返回曲线
	if ((*linear) <= BEELINE_VALUE)
	{
		return LINEAR;//直线
	}
	return CURVE;//曲线
}

void LineMerge(LINE* const l2,const int pos1,const int pos2,const char loc, LINE *l1)
{
	int t;
	int max = _imax(pos1, pos2);
	int min = _imin(pos1, pos2);

	l1->local = loc;
	if (l2->endpos < max) max = l2->endpos;
	if (min < 0) min = 0;

	for (t = min; t <= max; t++)
	{
		l1->line[l1->endpos] = l2->line[t];
		l1->endpos++;
	}
	if (min <= max) l1->endpos--;
}

void LineCutting(int startpos, int endpos, const char loc, LINE *org)
{
	int t = 0;
	LINE tmp = *org;

	LineInit(org, loc);

	if (endpos > tmp.endpos) endpos = tmp.endpos;
	if (startpos < 0) startpos = 0;

	for (t = 0; t <= tmp.endpos; t++)
	{
		if (t >= startpos && t<endpos)
		{
			org->line[org->endpos] = tmp.line[t];
			org->endpos++;
		}
		else if (t == endpos)
		{
			org->line[org->endpos] = tmp.line[t];
			break;
		}
	}
}

void LineTrans(const int dis, const float k, const char loc, LINE *org, const char stpkey)
{
	int t = 0, trans_x = 0;
	float tmp_k = k;
	int endpos = org->endpos;
	char firstFlg = itrue;

	if (k < 1.0f && k > -1.0f) tmp_k = 1.0f;
	else tmp_k = _ifabs(k);

	for (t = 0; t < endpos; t++)
	{
		if (POINT_CHK(org->line[t]) == itrue)
		{
			trans_x = (int)(tmp_k*dis) + org->line[t].x;
			if (_iinc(trans_x, IMAGE_LEFT, IMAGE_RIGHT))
			{
				if (stpkey == itrue)
				{
					if (!POINT_WHITE(trans_x, org->line[t].y)) { break; }
				}
				if (firstFlg == itrue)
				{
					firstFlg = ifalse;
					org->endpos = 0;
					org->line[0].x = trans_x;
					org->line[0].y = org->line[t].y;
				}
				else {
					org->endpos++;
					org->line[org->endpos].x = trans_x;
					org->line[org->endpos].y = org->line[t].y;
				}
			}
			if (org->line[t + 1].y >= org->line[t].y) break;
		}
	}
	org->local = loc;
}

void LineSlope(const LINE org, float *slope)
{
	LINE origin = org;
	LSQ_INFO lsq;

	MedianFilter(0, origin.endpos, LOC_UNKNOW, &origin);
	LSQ_algorithm(&origin, 0, origin.endpos, &lsq);
	*slope = -lsq.k;//(-)(/); (+)(/)
}

char LN_allwhite(const int startx, const int starty, const int endx, const int endy, POINT* END)
{
	LINE t_line;
	int t = 0;

	GetSegment(startx, starty, endx, endy, LOC_UNKNOW, &t_line);//两点求线段

	for (t = 0; t <= t_line.endpos; t++)
	{
		if (!POINT_WHITE(t_line.line[t].x, t_line.line[t].y))
		{
			(*END) = t_line.line[t - 1];//record
			return ifalse;
		}
	}
	END->x = endx;
	END->y = endy;
	return itrue;
}

char vLNBumpCheck(LINE* const org, const int pos, const char bump)
{
	int t = 0, err_count = 0;
	char check_Y = ifalse, up_dir = 0;

	if (pos - 2 <= 0 || pos + 2 >= org->endpos) { return ifalse; }
	if (org->line[pos].x == org->line[pos - 1].x)   //正Y型检测
	{
		if (org->line[pos].y - org->line[pos - 1].y == -1)
		{
			if ((org->line[pos].x != org->line[pos + 1].x)
				&& _iiabs(org->line[pos].x - org->line[pos + 1].x) <= 3)//1 ?
			{
				if (org->line[pos].y - org->line[pos + 1].y == 1)
				{
					check_Y = itrue;
					char dir = 0;
					for (t = pos + 1; t <= org->endpos; t++)
					{
						if (org->line[t].x > org->line[t - 1].x)
						{
							dir++;
						}
						else if (org->line[t].x < org->line[t - 1].x)
						{
							dir--;
						}
						if (t - pos >= 5) break;
					}
					if (dir > 0) up_dir = 1;//right
					else if (dir < 0) up_dir = -1;//left
				}
			}
		}
	}
	if (check_Y == ifalse) { return ifalse; }//非正Y型

	for (t = pos + 2; t < org->endpos; t++)
	{
		if (up_dir == 1)
		{
			if (org->line[t].x < org->line[t - 1].x)
			{
				if (org->line[t - 1].x - org->line[pos].x > bump)
				{
					up_dir = 2;//确认 上方 右向
					break;
				}
				if (org->line[t].x - org->line[pos].x < -1)//-1
				{
					err_count++;
				}
			}
			else {
				if (t >= org->endpos - 2)
				{
					if (org->line[t].x - org->line[pos].x > bump)
					{
						up_dir = 2;//确认 上方 右向
						break;
					}
				}
			}
		}
		else if (up_dir == -1)
		{
			if (org->line[t].x > org->line[t - 1].x)
			{
				if (org->line[t - 1].x - org->line[pos].x < -bump)
				{
					up_dir = -2;//确认 上方 左向
					break;
				}
				if (org->line[t].x - org->line[pos].x > 1)//1
				{
					err_count++;
				}
			}
			else {
				if (t >= org->endpos - 2)
				{
					if (org->line[t].x - org->line[pos].x < -bump)
					{
						up_dir = -2;//确认 上方 左向
						break;
					}
				}
			}
		}
		if (err_count > BUMP_ERRTOR) break;
	}
	err_count = 0;
	char bot_dir = 0;
	for (t = pos - 1; t >= 2; t--)
	{
		if (org->line[t - 1].x != org->line[t].x)
		{
			if (org->line[t - 1].x > org->line[t].x) bot_dir = 1;
			else bot_dir = -1;
			break;
		}
	}
	for (t = t - 1; t >= 2; t--)
	{
		if (bot_dir == 1)
		{
			if (org->line[t - 1].x < org->line[t].x)
			{
				if (org->line[t].x - org->line[pos].x > bump)
				{
					bot_dir = 2;//确认 下方 右向
					break;
				}
				if (org->line[t - 1].x - org->line[pos].x < -1)//-1
				{
					err_count++;
				}
			}
			else{
				if (t <= 3)
				{
					if (org->line[t].x - org->line[pos].x > bump)
					{
						bot_dir = 2;//确认 下方 右向
						break;
					}
				}
			}
		}
		else  if (bot_dir == -1)
		{
			if (org->line[t - 1].x > org->line[t].x)
			{
				if (org->line[t].x - org->line[pos].x < -bump)
				{
					bot_dir = -2;//确认 下方 左向
					break;
				}
				if (org->line[t - 1].x - org->line[pos].x > 1)//2
				{
					err_count++;
				}
			}
			else {
				if (t <= 3)
				{
					if (org->line[t].x - org->line[pos].x < -bump)
					{
						bot_dir = -2;//确认 下方 左向
						break;
					}
				}
			}
		}
		if (err_count > BUMP_ERRTOR) break;
	}
	if (up_dir == bot_dir && (up_dir == 2 || up_dir == -2))//相同方向 确认凹凸点
	{
		return itrue;
	}
	return ifalse;
}
