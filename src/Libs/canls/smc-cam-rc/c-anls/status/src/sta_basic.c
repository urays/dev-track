#include "../../suprc/inc/common.h"
#include "../../suprc/inc/sup_exfunc.h"
#include "../../suprc/inc/sup_draw.h"

#include "../../conf-anls.h"
#include "../inc/sta_basic.h"
#include "../../suprc/inc/freeman.h"

//优先判断斑马线 须要保证判断的准确性 
static void check_zebra(const int scan_top, const int scan_bot);//检测斑马线
static void calc_edge_STRG(int* strg_top, int* strg_bot);//计算边沿起始点搜索范围

static void InitEdgeBasic(LINE *edgel, LINE *edger);//初始化edgel;edger
static void MD_GetStart_NL(const int startline, const int endline, LINE *edgel, int* endpos);//寻找左边沿起始点
static void MD_GetStart_NR(const int startline, const int endline, LINE *edger, int* endpos);//寻找右边沿起始点
static void MD_FindEdgeLeft(const int startpos, LINE *edgel); //求纵向左边沿
static void MD_FindEdgeRight(const int startpos, LINE *edger);//求纵向右边沿
static void MD_FindBreakLeft(const LINE edgel, int* bkpos); //求纵向左边沿拐点
static void MD_FindBreakRight(const LINE edger, int* bkpos);//求纵向右边沿拐点
static void TD_FindEdgeLeft(const LINE edgel, const int seekrange, LINE *edger); //求横向右边沿
static void TD_FindEdgeRight(const LINE edger, const int seekrange, LINE *edgel);//求横向左边沿

static char BK_LWinCheck(const LINE edgel, const int ceni, const char Dyn);//左边沿Freeman角点检验
static char BK_RWinCheck(const LINE edger, const int ceni, const char Dyn);//右边沿Freeman角点检验
static char _bag_break_check(const LINE edgel, const LINE edger);//包包路段角点去除检测

static void LastWhiteBasic(POINT *lastwhite);//获取图像中线上最后白点坐标
static char TD_GetDirTab(const POINT TdMidTab, const int diroffset);//标定求横向搜线方向
static char JColorBlack_3(const POINT center); //  '.'
static void TD_PreTreatBasic(LINE *edgem);   //非拐点路段横向搜线预处理(为横向搜线找到合适的起始点)

static char basic_T = BAS_UNKNOW;//基础赛道类型
static char break_T = BAS_BK_NOT;//拐点类型
static int  bottom_Width = 110; //底部行赛道宽度
static int  START_CENTER = IMAGE_CENTER;//边沿搜索起始扩撒列
static int  lbkpos = 0, rbkpos = 0;//左右角点
static char _bag_check = ifalse; //包包检测

struct _ZEBRA_ { int locall; int maxwhite; int whitel; };
static struct _ZEBRA_ Zebra =//
{
	.locall = IMAGE_TOP,   //斑马线位置行
	.whitel = IMAGE_TOP,   //斑马线上全白行
	.maxwhite = ZEBRA_WHITEC_,
};

char GetBasicPlace(void) { return basic_T; }//发送基础路径信息
char GetBasicBreak(void) { return break_T; }//发送拐点类型

static BASIC_DATA BacData = {
	.set.entwo_oneamo = SETI_EN2ONEAMO,   //MD较长边点数 <= 此值 开启纵向寻线
	.set.entwo_slope = SETF_EN2SLOPE, //slope <= 此值 开启纵向寻线

	.one_amount[0] = 0,
	.one_amount[1] = 0,
	.two_amount[0] = 0,
	.two_amount[1] = 0,
	.slope = -1.0f,
};
BASIC_DATA* GetBasicAddr(void) { return &BacData; } //返回 基础信息库地址

//---------------------------- Basic Dealing -------------------------------//

// func 计算边沿起始点搜索范围(+斑马线检查)
// para strg_top(int*) 搜索范围上
// para strg_bot(int*) 搜索范围下
static void calc_edge_STRG(int* strg_top, int* strg_bot)
{
	basic_T = BAS_UNKNOW;      //初始化
	(*strg_top) = START_RG_TOP_;
	(*strg_bot) = START_RG_BOT_;

	check_zebra(ZEBRA_RG_TOP_, ZEBRA_RG_BOT_);//此函数 获取了Zebra.locall   // 45 55
	if (Zebra.locall != IMAGE_TOP)
	{
		basic_T = BAS_ZEBRA;
		int line, ButY = START_RG_BOT_;
		int max_c = 0, now_c = 0;
		//斑马线保持标志清除
		for (line = (Zebra.locall>Zebra.whitel) ? (Zebra.locall) : Zebra.whitel;
			line <= IMAGE_BOTTOM; line++)
		{
			if (WhiteCount_LR(EXF_BOTH, IMAGE_CENTER, line) < Zebra.maxwhite) break;//60
		}
		if (line > IMAGE_BOTTOM) Zebra.locall = IMAGE_TOP;

		//斑马线 重新整定边沿起始点搜索范围
		for (line = (Zebra.locall - 20>ERROR_LINE) ? (Zebra.locall - 20) : ERROR_LINE;
			line <= Zebra.locall; line++) //寻找全白行
		{
			now_c = WhiteCount_LR(EXF_BOTH, IMAGE_CENTER, line);
			if (now_c >= max_c)
			{
				max_c = now_c;
				Zebra.maxwhite = max_c;
				Zebra.whitel = line;
				ButY = line;
			}
		}
		if (ButY < START_RG_BOT_)
		{
			(*strg_bot) = ButY;
			(*strg_top) = (ButY - 10 > ERROR_LINE + 15) ? (ButY - 10) : (ERROR_LINE + 15);
		}
	}
	else {
		Zebra.whitel = IMAGE_TOP;
		Zebra.maxwhite = ZEBRA_WHITEC_;
	}
}

// func 基础前期处理-横向搜线
void BasicDeal_MD(LINE *edgel, LINE *edger)
{
	int spos_L = 1, spos_R = 1;
	int scan_rg1, scan_rg2;

	InitEdgeBasic(edgel, edger); //初始化边沿
	calc_edge_STRG(&scan_rg1, &scan_rg2);//边沿起始点搜索范围计算

	MD_GetStart_NL(scan_rg1, scan_rg2, edgel, &spos_L);//寻找纵向左边沿搜线起始点
	MD_FindEdgeLeft(spos_L, edgel);//搜索左边沿

	MD_GetStart_NR(scan_rg1, scan_rg2, edger, &spos_R);//寻找纵向右边沿搜线起始点
	MD_FindEdgeRight(spos_R, edger);//搜索右边沿 

	//
	if (edger->line[0].x - edgel->line[0].x > VALID_EDGE_DIF)//动态搜线起始列差
	{
		if (edgel->endpos != 0 && edger->endpos != 0)//
		{
			if (edgel->line[0].y == IMAGE_BOTTOM && edger->line[0].y == IMAGE_BOTTOM)
			{
				if (edgel->line[0].x > IMAGE_LEFT + 1 && edger->line[0].x < IMAGE_RIGHT - 1)
				{
					bottom_Width = edger->line[0].x - edgel->line[0].x;//记录 正常状态下赛道宽度
				}
			}
		}
		START_CENTER = (int)((edgel->line[0].x + edger->line[0].x) >> 1);//动态起始扩散列
	}
	else { START_CENTER = IMAGE_CENTER; }

	_bag_check = ifalse;
#ifndef _CLOSE_BAG_CHECK
	_bag_check = _bag_break_check(*edgel, *edger);//包包路段去除角点检测
#endif

	BacData.one_amount[0] = edgel->endpos;
	BacData.one_amount[1] = edger->endpos;
}

// func 基础前期处理-寻找拐点
void BasicDeal_BK(LINE* edgel, LINE* edger)
{
	char Lflg = ifalse, Rflg = ifalse;   //-Local
	int lbky = IMAGE_TOP, rbky = IMAGE_TOP;//-Local
	lbkpos = 0, rbkpos = 0;

	MD_FindBreakLeft(*edgel, &lbkpos); //寻找左边沿拐点
	MD_FindBreakRight(*edger, &rbkpos);//寻找右边沿拐点
	if (lbkpos != 0)
	{
		lbky = edgel->line[lbkpos].y;
		if (_bag_check == ifalse)//不是包包路段
		{
			if (_iinc(lbky, BKTYPE_AREA_UP, BKTYPE_AREA_DOWN))//25 ~ 58
			{
				Lflg = itrue;
				PreviewPointRect(edgel->line[lbkpos], 5);//显示矩形左角点
			}
		}
		//PreviewPointX(edgel->line[lbkpos], 3);//显示左拐点
		LineCutting(0, lbkpos, LOC_BASIC, edgel);//去除拐点后段
	}
	if (rbkpos != 0)
	{
		rbky = edger->line[rbkpos].y;
		if (_bag_check == ifalse)//不是包包路段
		{
			if (_iinc(rbky, BKTYPE_AREA_UP, BKTYPE_AREA_DOWN))//25 ~ 58
			{
				Rflg = itrue;
				PreviewPointRect(edger->line[rbkpos], 5);//显示矩形右角点
			}
		}
		//PreviewPointX(edger->line[rbkpos], 3);//显示右拐点
		LineCutting(0, rbkpos, LOC_BASIC, edger);//去除拐点后段
	}

	if (Lflg == itrue && Rflg == itrue) { break_T = BAS_BK_BOTH; }//两拐点
	else if (Lflg == itrue && Rflg == ifalse) { break_T = BAS_BK_LEFT; }//左拐点
	else if (Lflg == ifalse && Rflg == itrue) { break_T = BAS_BK_RIGHT; }//右拐点
	else { break_T = BAS_BK_NOT; }//无拐点
}

// func 基础前期处理-纵向搜线
void BasicDeal_TD(LINE *edgel, LINE *edger)
{
	char dir = BAS_UNKNOW;//-Local
	POINT lastwhite = { IMAGE_CENTER,IMAGE_BOTTOM };//-Local

	LastWhiteBasic(&lastwhite);//寻找IMAGE_CENTER上最后一个白点（定标点）
	dir = TD_GetDirTab(lastwhite, 0);//定标确定横向搜线方向

	int edge_H = 0, edge_W = 0;
	if (dir == BAS_LEFT && rbkpos == 0)
	{
		if (BacData.one_amount[1] <= BacData.set.entwo_oneamo)
		{
			edge_H = IMAGE_BOTTOM - edgel->line[edgel->endpos].y + 1;
			edge_W = IMAGE_CENTER - edgel->line[edgel->endpos].x;
			BacData.slope = (edge_W <= 0.0f) ? 0.1f : (edge_H / (edge_W*1.0f));
			if (_ninc(BacData.slope, 0.0f, BacData.set.entwo_slope))
			{
				if (edgel->line[edgel->endpos].y > edger->line[edger->endpos].y)
				{
					TD_PreTreatBasic(edger);
					TD_FindEdgeLeft(*edgel, TD_SEARCH_RANGE, edger);
				}
			}
		}
	}
	else if (dir == BAS_RIGHT && lbkpos == 0)
	{
		if (BacData.one_amount[0] <= BacData.set.entwo_oneamo)
		{
			edge_H = IMAGE_BOTTOM - edger->line[edger->endpos].y + 1;
			edge_W = edger->line[edger->endpos].x - IMAGE_CENTER;
			BacData.slope = (edge_W <= 0.0f) ? 0.1f : (edge_H / (edge_W*1.0f));
			if (_ninc(BacData.slope, 0.0f, BacData.set.entwo_slope))
			{
				if (edger->line[edger->endpos].y > edgel->line[edgel->endpos].y)
				{
					TD_PreTreatBasic(edgel);
					TD_FindEdgeRight(*edger, TD_SEARCH_RANGE, edgel);
				}
			}
		}
	}
	else {
		BacData.slope = -1.0f;
	}
	//dir: BOTH 暂不做处理

	BacData.two_amount[0] = edgel->endpos - BacData.one_amount[0];
	BacData.two_amount[1] = edger->endpos - BacData.one_amount[1];
}

// func 基础类型处理
void BasicDeal_BS(LINE* const edgel, LINE* const edger)
{
	if (basic_T == BAS_ZEBRA) { return; }//优先判断已经识别为 斑马线

	char longer_T = CURVE;//initialize is not a bee line
	char left_T = CURVE, right_T = CURVE;
	float linearL = 0, linearR = 0;//直线线性度,越大表明直线线性度越差
	LSQ_INFO lsqL, lsqR;           //观察线段

	left_T = LineLinear(edgel, 0, edgel->endpos, &lsqL, &linearL);
	right_T = LineLinear(edger, 0, edger->endpos, &lsqR, &linearR);

	if (edgel->endpos < edger->endpos 
		&& edger->endpos > DEF_MORE_EDGEC)
	{
		if ((longer_T = right_T) == CURVE)
		{
			basic_T = BAS_BEND_L; //判断为左弯道
		}
	}
	else if (edgel->endpos > edger->endpos
		&& edgel->endpos > DEF_MORE_EDGEC)
	{
		if ((longer_T = left_T) == CURVE)
		{
			basic_T = BAS_BEND_R; //判断为右弯道
		}
	}
	else if (edgel->endpos == edger->endpos
		&& edgel->endpos > DEF_MORE_EDGEC)
	{
		longer_T = right_T;
		longer_T = left_T;
	}
	else if (edgel->endpos <= DEF_MORE_EDGEC
		&& edger->endpos <= DEF_MORE_EDGEC)
	{
		//longer_T = CURVE;
		basic_T = BAS_UNKNOW;//边沿点数太少,无法正确判断道路情况
	}

	if (longer_T == LINEAR) basic_T = BAS_STRAIGHT;
}

//----------------------- ANALYTIC VERTICAL LINE ---------------------------//

// func initalize edgel(LINE),edger(LINE)
static void InitEdgeBasic(LINE *edgel,LINE *edger)
{
	LineInit(edgel, LOC_BASIC);
	edgel->line[0].x = IMAGE_LEFT + 1; //初始化
	edgel->line[0].y = IMAGE_BOTTOM;

	LineInit(edger, LOC_BASIC);
	edger->line[0].x = IMAGE_RIGHT - 1;//初始化
	edger->line[0].y = IMAGE_BOTTOM;
}

// func find first point from edge(Left) NOMAL LEFT
// para startline(const int) edge search startline; nomal set IMAGE_BOTTOM
// para endline(const int) edge search endline; nomal set 40
// get *edgel(LINE) left edge line start point
// get *endpos(int) start point position
// aim For (func)MD_FindEdgeLeft
static void MD_GetStart_NL(const int startline, const int endline, LINE *edgel, int* endpos)
{
	int line, column;//-Local
	int min = _imin(startline, endline);
	int max = _imax(startline, endline);
	*endpos = 0;

	for (line = max; line > min; line--)
	{
		for (column = START_CENTER + 10; column > IMAGE_LEFT; column--)
		{
			if (POINT_FILTER_LEFT(column, line))
			{
				edgel->line[0].x = column;
				edgel->line[0].y = line;
				break;
			}
		}
		if (column <= IMAGE_LEFT || column >= IMAGE_RIGHT)
		{
			edgel->line[0].x = IMAGE_LEFT + 1;
			edgel->line[0].y = IMAGE_BOTTOM;
			continue;
		}
		break;
	}
	int i = 0, j = 0;
	for (line = edgel->line[0].y - 1; line > min; line--)//稳定搜线
	{
		for (column = START_CENTER + 10; column > IMAGE_LEFT; column--)
		{
			if (POINT_FILTER_LEFT(column, line))
			{
				for (i = column -1; i > IMAGE_LEFT; i--)
				{
					if (!POINT_WHITE(i, line) && POINT_WHITE(i - 1, line))
					{
						break;
					}
				}
				if (i > IMAGE_LEFT)
				{
					for (j = i - 1; j > IMAGE_LEFT; j--)
					{
						if (!POINT_WHITE(j, line))
						{
							break;
						}
					}
				}
				if (i - j >= 3) { continue; }
				edgel->endpos++;
				edgel->line[edgel->endpos].x = column;
				edgel->line[edgel->endpos].y = line;
				break;
			}
		}
		if (edgel->line[edgel->endpos].x > IMAGE_LEFT + MD_SEARCH_RANGE) break;
	}
	*endpos = (edgel->endpos);
}

// func find first point from edge(Right)  NOMAL RIGHT
// para startline(const int) edge search startline; nomal set IMAGE_BOTTOM
// para endline(const int) edge search endline; nomal set 40
// get *edger(LINE)  right edge line start point
// get *endpos(int) start point position
// aim For (func)MD_FindEdgeRight
static void MD_GetStart_NR(const int startline, const int endline, LINE *edger, int* endpos)
{
	int line, column;//-Local
	int min = _imin(startline, endline);
	int max = _imax(startline, endline);
	*endpos = 0;

	for (line = max; line > min; line--)
	{
		for (column = START_CENTER - 10; column <IMAGE_RIGHT; column++)
		{
			if (POINT_FILTER_RIGHT(column, line))
			{
				edger->line[0].x = column;
				edger->line[0].y = line;
				break;
			}
		}
		if (column<=IMAGE_LEFT || column >=IMAGE_RIGHT)
		{
			edger->line[0].x = IMAGE_RIGHT - 1;
			edger->line[0].y = IMAGE_BOTTOM;
			continue;
		}
		break;
	}
	int i = 0, j = 0;
	for (line = edger->line[0].y - 1; line > min; line--)//稳定搜线
	{
		for (column = START_CENTER - 10; column <IMAGE_RIGHT; column++)
		{
			if (POINT_FILTER_RIGHT(column, line))
			{
				for (i = column + 1; i < IMAGE_RIGHT; i++)
				{
					if (!POINT_WHITE(i, line) && POINT_WHITE(i + 1, line))
					{
						break;
					}
				}
				if (i < IMAGE_RIGHT)
				{
					for (j = i + 1; j < IMAGE_RIGHT; j++)
					{
						if (!POINT_WHITE(j, line))
						{
							break;
						}
					}
				}
				if (j - i >= 3) { continue; }
				edger->endpos++;
				edger->line[edger->endpos].x = column;
				edger->line[edger->endpos].y = line;
				break;
			}
		}
		if (edger->line[edger->endpos].x < IMAGE_RIGHT - MD_SEARCH_RANGE) break;
	}
	*endpos = (edger->endpos);
}

// func get leftedge array in machine direction
// para startpos(const int) 寻线起始位置
// get *edgel(LINE) leftedge line
// call after calling
// (func)MD_GetStart_NL
// left edge needs to be initialized
static void MD_FindEdgeLeft(const int startpos, LINE *edgel)
{
	int column, count = 0;           //-local
	int Polarity = 0, Dvalue = 0;    //-local
	int startline = edgel->line[startpos].y;//-Local   record startline 

	for (count = startpos + 1; count< startline - ERROR_LINE; count++)
	{
		Polarity = 0;
		for (Dvalue = 0; Dvalue <= MD_SEARCH_RANGE;)
		{
			Polarity = !Polarity;
			if (Polarity)
			{
				column = edgel->line[count - 1].x - Dvalue;
				Dvalue++;
			}
			else
				column = edgel->line[count - 1].x + Dvalue;
			if (column <= IMAGE_LEFT || column >= IMAGE_RIGHT)
			{
				edgel->line[count].x = INIT_VALUE;
				edgel->line[count].y = INIT_VALUE;
				break;
			}
			if (POINT_FILTER_LEFT(column, startline - count + startpos))
			{
				edgel->line[count].x = column;
				edgel->line[count].y = startline - count + startpos;
				edgel->endpos++;//record end pos
				break;
			}
		}
		if (column >= edgel->line[count - 1].x + MD_SEARCH_RANGE
			|| column <= edgel->line[count - 1].x - MD_SEARCH_RANGE
			|| column <= IMAGE_LEFT || column >= IMAGE_RIGHT) break;
	}
}

// func get rightedge array in machine direction
// para startpos(const int) 寻线起始位置
// get edger(LINE) rightedge line
// call after calling
// (func)MD_GetStart_NR
// right edge needs to be initialized
static void MD_FindEdgeRight(const int startpos, LINE *edger)
{
	int column, count = 0;           //-local
	int Polarity = 0, Dvalue = 0;    //-local
	int startline = edger->line[startpos].y;//-Local record startline

	for (count = startpos + 1; count <startline - ERROR_LINE; count++)
	{
		Polarity = 0;
		for (Dvalue = 0; Dvalue <= MD_SEARCH_RANGE;)
		{
			Polarity = !Polarity;
			if (Polarity)
			{
				column = edger->line[count - 1].x + Dvalue;
				Dvalue++;
			}
			else
				column = edger->line[count - 1].x - Dvalue;
			if (column <= IMAGE_LEFT || column >= IMAGE_RIGHT)
			{
				edger->line[count].x = INIT_VALUE;
				edger->line[count].y = INIT_VALUE;
				break;
			}
			if (POINT_FILTER_RIGHT(column, startline - count + startpos))
			{
				edger->line[count].x = column;
				edger->line[count].y = startline - count + startpos;
				edger->endpos++;//record end pos
				break;
			}
		}
		if (column <= edger->line[count - 1].x - MD_SEARCH_RANGE
			|| column >= edger->line[count - 1].x + MD_SEARCH_RANGE
			|| column <= IMAGE_LEFT || column >= IMAGE_RIGHT) break;
	}
}

//------------------------- ANALYTIC LINE BREAK ----------------------------//

// func find left edge break point
// para edgel(cosnt LINE) 左边沿
// para bkpos(int*) 角点位置
// call after calling
// (func)MD_FindEdgeLeft;
static void MD_FindBreakLeft(const LINE edgel, int* bkpos)
{
	int t = 1;
	_fmmsg freecnr = { 0, FM_NOT };
	(*bkpos) = 0;

	if (edgel.line[0].y < IMAGE_BOTTOM - 1 && edgel.line[0].x > IMAGE_LEFT + 1) { return; }//拐点条件
	for (t = 1; t <edgel.endpos; t++)
	{
		if (edgel.line[t].y <= ERROR_LINE) { break; }
		freecnr = Freeman(edgel.line[t].x, edgel.line[t].y);
		if ((freecnr.dir == FM_LEFT || freecnr.dir == FM_LDOWN)
			|| (freecnr.dir == FM_LUP && t >= edgel.endpos - 3//此条件笔直点数少
				&& edgel.endpos < 25))
		{
			if (freecnr.grads == FM_SURE_COR || freecnr.grads == FM_SUSP_COR
				|| (freecnr.grads == FM_SURE_STR && t >= edgel.endpos - 3))//此边沿笔直且点数较少
			{
				if (BK_LWinCheck(edgel, t, itrue) == itrue)//ifalse
				{
					(*bkpos) = t;
					break;
				}
				//PreviewPoint(edgel.line[t], 2);
			}
		}
	}
}

// func find right edge break point
// para edger(const LINE) 右边沿
// para bkpos(int*) 角点位置
// call after calling
// (func)MD_FindEdgeRight;
static void MD_FindBreakRight(const LINE edger, int* bkpos)
{
	int t = 1;
	_fmmsg freecnr = { 0, FM_NOT };
	(*bkpos) = 0;

	if (edger.line[0].y < IMAGE_BOTTOM - 1 && edger.line[0].x < IMAGE_RIGHT - 1) { return; }//拐点条件
	for (t = 1; t <edger.endpos; t++)
	{
		if (edger.line[t].y <= ERROR_LINE) { break; }
		freecnr = Freeman(edger.line[t].x, edger.line[t].y);
		if ((freecnr.dir == FM_RIGHT || freecnr.dir == FM_RDOWN)
			|| (freecnr.dir == FM_RUP && t >= edger.endpos - 3))
		{
			if (freecnr.grads == FM_SURE_COR || freecnr.grads == FM_SUSP_COR
				|| (freecnr.grads == FM_SURE_STR && t >= edger.endpos - 3//此边沿笔直且点数较少
					&& edger.endpos < 25))
			{
				if (BK_RWinCheck(edger, t, itrue) == itrue)//ifalse
				{
					(*bkpos) = t;
					break;
				}
				//PreviewPoint(edger.line[t], 2);
			}
		}
	}
}

static char REC_x[] = { 0,1,2,3,4,5,6,6,6,6,6,6,6,5,4,3,2,1,0,-1,-2,-3,-4,-5,-6,-6,-6,-6,-6,-6,-6,-5,-4,-3,-2,-1 };
static char REC_y[] = { 3,3,3,3,3,3,3,2,1,0,-1,-2,-3,-3,-3,-3,-3,-3,-3,-3,-3,-3,-3,-3,-3,-2,-1,0,1,2,3,3,3,3,3,3 };

// func 左边沿角点检验
// para edgel(const LINE) 左边沿
// para ceni(const int) 目标检验点位置
// para Dyn(const char) 动态开窗开关
// return itrue/ifalse.
static char BK_LWinCheck(const LINE edgel, const int ceni, const char Dyn)
{
	int cenx = edgel.line[ceni].x;
	int ceny = edgel.line[ceni].y;
	int supx = 0, i = 0;
	int winlen = 6, Wcount = 0;
	int startx = -6, search_c = 0;
	if (Dyn == itrue)
	{
		if (cenx >= IMAGE_LEFT + 7 && cenx <= IMAGE_RIGHT - 7) { winlen = 6; }
		else if (cenx <= IMAGE_LEFT + 6 && cenx >= IMAGE_LEFT + 2) { winlen = cenx - IMAGE_LEFT - 1; }
		else if (cenx >= IMAGE_RIGHT - 6 && cenx <= IMAGE_RIGHT - 2) { winlen = IMAGE_RIGHT - cenx - 1; }
		if (cenx <= IMAGE_LEFT + 1 || cenx >= IMAGE_RIGHT - 1) { return ifalse; }
	}
	else {
		if (cenx <= IMAGE_LEFT + 6 || cenx >= IMAGE_RIGHT - 6) { return ifalse; }
	}
	if (ceny + 3 > IMAGE_BOTTOM || ceny - 3 <= ERROR_LINE) { return ifalse; }

	if (ceni >= 3) startx = edgel.line[ceni - 3].x - edgel.line[ceni].x;
	if (startx < -6 || ceni < 3) startx = -6;
	for (search_c = (startx + 36) % 36, i = 0; i<36; i++)
	{
		search_c = search_c % 36;//窗户变小,左下位置检测点权重变大
		supx = _imax(REC_x[search_c], -winlen);
		if (POINT_WHITE(cenx + supx, ceny + REC_y[search_c]))
		{
			Wcount++;
		}
		else { break; }
		search_c++;
	}
	if (Wcount >= CORNER_CHECK_WHITE) { return itrue; }
	return ifalse;
}

// func 右边沿角点检验
// para edger(const LINE) 右边沿
// para ceni(const int) 目标检验点位置
// para Dyn(const char) 动态开窗开关
// return itrue/ifalse.
static char BK_RWinCheck(const LINE edger, const int ceni, const char Dyn)
{
	int cenx = edger.line[ceni].x;
	int ceny = edger.line[ceni].y;
	int supx = 0, i = 0;
	int winlen = 6, Wcount = 0;
	int startx = -6, search_c = 0;
	if (Dyn == itrue)
	{
		if (cenx >= IMAGE_LEFT + 7 && cenx <= IMAGE_RIGHT - 7) { winlen = 6; }
		else if (cenx <= IMAGE_LEFT + 6 && cenx >= IMAGE_LEFT + 2) { winlen = cenx - IMAGE_LEFT - 1; }
		else if (cenx >= IMAGE_RIGHT - 6 && cenx <= IMAGE_RIGHT - 2) { winlen = IMAGE_RIGHT - cenx - 1; }
		if (cenx <= IMAGE_LEFT + 1 || cenx >= IMAGE_RIGHT - 1) { return ifalse; }
	}
	else {
		if (cenx <= IMAGE_LEFT + 6 || cenx >= IMAGE_RIGHT - 6) { return ifalse; }
	}
	if (ceny + 3 > IMAGE_BOTTOM || ceny - 3 <= ERROR_LINE) { return ifalse; }

	if (ceni >= 3) startx =  edger.line[ceni].x - edger.line[ceni - 3].x;
	if (startx < -6 || ceni < 3) startx = -6;
	for (search_c = (startx + 36) % 36, i = 0; i<36; i++)
	{
		search_c = search_c % 36;//窗户变小,左下位置检测点权重变大
		supx = _imin(REC_x[search_c], winlen);
		if (POINT_WHITE(cenx - supx, ceny + REC_y[search_c]))
		{
			Wcount++;
		}
		else { break; }
		search_c++;
	}
	if (Wcount >= CORNER_CHECK_WHITE) { return itrue; }
	return ifalse;
}

// func 包包路段拐点去除  (在坡道上)
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
static char _bag_break_check(const LINE edgel, const LINE edger)
{
	POINT ramp_last = { IMAGE_CENTER,IMAGE_BOTTOM };
	char level_sign = ifalse;
	char apeak_sign = ifalse;
	char hug_sign = ifalse;

	int i, j;
	for (i = 0, j = 0; i <= edgel.endpos && j <= edger.endpos;)
	{
		if (edgel.line[i].y > edger.line[j].y) i++;
		else if (edgel.line[i].y < edger.line[j].y) j++;
		else { break; }
	}
	int cen_x = (int)((edgel.line[i].x + edger.line[j].x) >> 1);
	int cen_y = (int)((edgel.line[i].y + edger.line[j].y) >> 1);

	FullLine_ShotUpF(cen_x, cen_y, 50, &ramp_last);

	//垂直检测
	int miny = _imin(edgel.line[edgel.endpos].y, edger.line[edger.endpos].y);
	int far = _imin(ramp_last.y, miny);
	if (far >= BAG_FAR_LIMIT)
	{
		apeak_sign = itrue;
	}
	//水平检测
	int DIS_LIMIT = Get_Road_Width(BAG_FAR_LIMIT);
	DIS_LIMIT = _imax(BAG_DIS_LIMIT, DIS_LIMIT);
	for (i = edgel.endpos, j = edger.endpos; i >0 && j >0;)
	{
		if (edgel.line[i].y > edger.line[j].y) j--;
		else if (edgel.line[i].y < edger.line[j].y) i--;
		else { break; }
	}
	if (edger.line[j].x - edgel.line[i].x < DIS_LIMIT)
	{
		if (_iiabs(edger.line[edger.endpos].y - edgel.line[edgel.endpos].y) <= BAG_HIG_LIMIT)
		{
			level_sign = itrue;
		}
	}
	//拥抱检测
	int _12_dif_L = edgel.line[edgel.endpos].x - edgel.line[0].x;
	int _12_dif_R = edger.line[0].x - edger.line[edger.endpos].x;
	if (_12_dif_L > 3 && _12_dif_R > 3)
	{
		if (_12_dif_R + _12_dif_L > 10)
		{
			if (edger.line[0].y >= IMAGE_BOTTOM - 3 && edgel.line[0].y >= IMAGE_BOTTOM - 3)
			{
				hug_sign = itrue;//
			}
		}
	}
	if (level_sign == itrue) {
		if (apeak_sign == itrue) {
			if (hug_sign == itrue) {
				return itrue;
			}
		}
	}
	return ifalse;
}

//----------------------- ANALYTIC TRANSVERSE LINE ---------------------------//

// func 沿IMAGE_CENTER(屏幕中线)向上寻找最后的白点
// get *lastwhite(POINT) 拟合直线上最后一个白点
// get TdDirOffset = 0
static void LastWhiteBasic(POINT *lastwhite)
{
	int line;//-Local
	lastwhite->x = IMAGE_CENTER;
	lastwhite->y = IMAGE_BOTTOM - 1;

	for (line = IMAGE_BOTTOM - 2; line>IMAGE_TOP; line--)
	{
		if (!POINT_WHITE(IMAGE_CENTER, line))
		{
			lastwhite->x = IMAGE_CENTER;
			lastwhite->y = line + 1;
			break;
		}
	}
}

// func judge the color by judging the surrounding three points
// para center(const POINT) center point horizontal coordinate
// return itrue-all are black
// return ifalse-At least one of them is white
// 形如'.' determine whether the center is black by three colors around
static char JColorBlack_3(const POINT center) //  '.'
{
	if ((POINT_WHITE(center.x - 1, center.y - 1) && center.x - 1 > IMAGE_LEFT && center.y - 1 > IMAGE_TOP)
		|| (POINT_WHITE(center.x + 1, center.y - 1) && center.x + 1 < IMAGE_RIGHT&&center.y - 1 > IMAGE_TOP)
		|| (POINT_WHITE(center.x, center.y + 1) && center.y + 1 < IMAGE_BOTTOM))  //  '.'
	{
		return ifalse;
	}
	return itrue;
}

// func judge seach state
// para TdMidTab(const POINT) 定标中点
// para diroffset(const int) 横向寻线 两标定点纵向差的1/2
// return search direction
// call after calling (func)LastWhiteBasic
//  BAS_LEFT-towards left direction
//  BAS_RIGHT-towards right direction
//  BAS_BOTH-left and right direction
//  BAS_UNKNOW-towards no direction
static char TD_GetDirTab(const POINT TdMidTab,const int diroffset)
{
	char left_black = itrue; //-Local
	char right_black = itrue;//-Local
	POINT tab_L, tab_R;

	tab_L.x = TdMidTab.x - TAB_LENGTH;
	tab_L.y = TdMidTab.y + TAB_HIGHT + diroffset;
	tab_R.x = TdMidTab.x + TAB_LENGTH;
	tab_R.y = TdMidTab.y + TAB_HIGHT - diroffset;

	left_black = JColorBlack_3(tab_L);
	right_black = JColorBlack_3(tab_R);

	//PreviewPoint(tab_L, 2);//显示标定左
	//PreviewPoint(tab_R, 2);//显示标定右

	if (left_black == ifalse && right_black == ifalse) return BAS_BOTH;     //both direction
	else if (left_black == itrue && right_black == ifalse) return BAS_RIGHT;//right direction
	else if (left_black == ifalse && right_black == itrue) return BAS_LEFT; //left direction

	return BAS_UNKNOW;//unknow direction
}

// func 横向线搜索预处理(优化下一次搜线起始点)
// 不用于 拐点路段
// para edgem(LINE*) 纵向线段
// get 去除散点后的纵向边沿线段 以及 横向搜线起点位置
static void TD_PreTreatBasic(LINE *edgem)
{
	int t, save_pos = -1;//-Local

	for (t = edgem->endpos; t >= 0; t--)
	{
		if (save_pos != -1)
		{
			if (_iiabs(edgem->line[t].x - edgem->line[save_pos].x) <= 2
				&& edgem->line[t].y - edgem->line[save_pos].y < 2) {
				break;
			}
			edgem->line[t + 1].x = INIT_VALUE;
			edgem->line[t + 1].y = INIT_VALUE;
			edgem->endpos = save_pos - 1;
		}
		save_pos = t;
	}
}

// func 向左寻找横向线（LEFT）
// para edgel(const LINE)
// para seekrange(const int) up and down range -5 to 5; nomal set 5
// get *edger(LINE) 横向线段
// call after calling
// (func)TD_GetDirTab
// (func)TD_PreTreatBasic
static void TD_FindEdgeLeft(const LINE edgel,const int seekrange, LINE *edger)
{
	int line;//-Local
	int polarity, d;//-Local
	int count, s_count = 0;//-Local
	int save_pos;//-Local

    save_pos = edger->endpos;
	POINT temp = edger->line[save_pos];

	for (count = save_pos + 1; count<save_pos + edger->line[save_pos].x - IMAGE_LEFT; count++)//+1
	{
		polarity = 0;
		for (d = 0; d < seekrange;)
		{
			polarity = !polarity;
			if (polarity)
			{
				line = edger->line[count - 1].y - d;
				d++;
			}
			else line = edger->line[count - 1].y + d;

			temp.x = edger->line[save_pos].x - count + save_pos;
			s_count = (line >= edger->line[count - 1].y) ? (s_count + 1) : 0;
			if (line >= IMAGE_BOTTOM ||
				(edgel.endpos >= 3 && temp.x <= edgel.line[edgel.endpos - 3].x) ||
				(edgel.endpos < 3 && temp.x <= edgel.line[edgel.endpos].x)
				|| (line < TD_IMPOSSIABLE && s_count >= 3))
			{
				edger->line[count].x = INIT_VALUE;
				edger->line[count].y = INIT_VALUE;
				break;
			}
			temp.y = line;
			if (POINT_FILTER_UP(temp.x, temp.y))
			{
				edger->line[count] = temp;
				edger->endpos++;
				break;
			}
		}
		if ((edger->line[count].x == INIT_VALUE &&edger->line[count].y == INIT_VALUE)
			|| _iiabs((edger->line[count].y - edger->line[count - 1].y)) > 2) break;
	}
}

// func 向右寻找横向线（RIGHT）
// para edger(const LINE)
// para seekrange(const int) up and down range -5 to 5; nomal set 5
// get *edgel(LINE) 横向线段
// call after calling
// (func)TD_GetDirTab
// (func)TD_PreTreatBasic
static void TD_FindEdgeRight(const LINE edger,const int seekrange, LINE *edgel)
{
	int line;//-Local
	int polarity,d;//-Local
	int count, s_count = 0;//-Local
	int save_pos;//-Local

	save_pos = edgel->endpos;
	POINT temp = edgel->line[save_pos];

	for (count = save_pos + 1; count<save_pos + IMAGE_RIGHT - edgel->line[save_pos].x; count++)//+1
	{
		polarity = 0;
		for (d = 0; d < seekrange;)
		{
			polarity = !polarity;
			if (polarity)
			{
				line = edgel->line[count - 1].y - d;
				d++;
			}
			else line = edgel->line[count - 1].y + d;

			temp.x = edgel->line[save_pos].x + count - save_pos;
			s_count = (line >= edgel->line[count - 1].y) ? (s_count + 1) : 0;
			if (line > IMAGE_BOTTOM || 
				(edger.endpos>=3 && temp.x >= edger.line[edger.endpos - 3].x)||
				(edger.endpos<3 && temp.x >= edger.line[edger.endpos].x)
				|| (line < TD_IMPOSSIABLE && s_count >= 3))
			{
				edgel->line[count].x = INIT_VALUE;
				edgel->line[count].y = INIT_VALUE;
				break;
			}
			temp.y = line;
			if (POINT_FILTER_UP(temp.x, temp.y))
			{
				edgel->line[count] = temp;
				edgel->endpos++;
				break;
			}
		}
		if ((edgel->line[count].x == INIT_VALUE &&edgel->line[count].y == INIT_VALUE)
			|| _iiabs((edgel->line[count].y - edgel->line[count - 1].y)) > 2) break;
	}
}

//----------------------- CALCULATE MIDDLE LINE ---------------------------//

// func 计算中线
// para edgel(LINE) 左边沿
// para edger(LINE) 右边沿
// para disk(const float) 偏离距离比(默认 0.5f)
// para loc(const char) 归属地
// get *res(POINT) 中线数组
void CalcMidBasic(LINE edgel, LINE edger, const float disk, const char loc, LINE *res)
{
	if (loc == LOC_BASIC && (basic_T == BAS_STRAIGHT || _bag_check == itrue)
		&& edgel.endpos > 15 && edger.endpos > 15)//左右边沿点数较多
	{
		CalcMidBasic_Lv(edgel, edger, LOC_BASIC, res);
	}
	else if (edger.endpos <= 1 && edgel.endpos <= 1)  //条件极度恶劣
	{
		POINT lastW;
		FullLine_ShotUpF(IMAGE_CENTER, IMAGE_BOTTOM, 32, &lastW);
		GetSegment(IMAGE_CENTER, IMAGE_BOTTOM, lastW.x, lastW.y, loc, res);
	}
	else
	{

		if (edgel.endpos <= 0 && edger.endpos > 15)//右边沿点数较多
		{
			if (edgel.line[0].y == IMAGE_BOTTOM && edgel.line[0].x == IMAGE_LEFT + 1)
			{
				edgel.line[0].x = edger.line[0].x - bottom_Width;
			}
		}
		else if (edger.endpos <= 0 && edgel.endpos > 15)//左边沿点数较多
		{
			if (edger.line[0].y == IMAGE_BOTTOM && edger.line[0].x == IMAGE_RIGHT - 1)
			{
				edger.line[0].x = edgel.line[0].x + bottom_Width;
			}
		}
		CalcMidBasic_Pp(edgel, edger, disk, loc, res);
	}
}

// func 全边计算中线(比例计算) <一般情况>
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para disk(const float) 偏离距离比(默认 0.5f)
// para loc(const char) 归属地
// get *res(POINT) 中线数组
void CalcMidBasic_Pp(const LINE edgel, const LINE edger, const float disk, const char loc, LINE *res)
{
	int t = 0;         //-Local
	float radio = 1.0f; //-Local
	int maxt = _imax(edgel.endpos, edger.endpos);//-Local
	LINE edgesmall, edgelarge;//-Local

	LineInit(res, loc);

	if (edgel.endpos >= edger.endpos)
	{
		radio = (edger.endpos >= 1) ? (edgel.endpos / edger.endpos) : (edgel.endpos*1.0f);
		edgesmall = edger;
		edgelarge = edgel;
	}
	else if (edgel.endpos < edger.endpos)
	{
		radio = (edgel.endpos >= 1) ? (edger.endpos / edgel.endpos) : (edger.endpos*1.0f);
		edgesmall = edgel;
		edgelarge = edger;
	}
	POINT small_last_point = edgesmall.line[0];
	POINT large_last_point = edgelarge.line[0];

	for (t = 0; t < maxt; t++)//不取最后一个点
	{
		if (POINT_CHK(edgelarge.line[t]) == itrue)
		{
			large_last_point = edgelarge.line[t];
		}
		else continue;
		int temp = (int)(t / radio + (0.5f));
		if (POINT_CHK(edgesmall.line[temp]) == itrue)
		{
			small_last_point = edgesmall.line[temp];
		}
		else if (POINT_CHK(edgesmall.line[temp + 1]) == itrue)
		{
			small_last_point = edgesmall.line[temp + 1];
		}
		if (POINT_CHK(small_last_point) == ifalse)
		{
			res->line[t].x = INIT_VALUE;
			res->line[t].y = INIT_VALUE;
		}
		else
		{
			res->line[t].x = small_last_point.x + (int)((large_last_point.x - small_last_point.x) * disk);
			res->line[t].y = small_last_point.y + (int)((large_last_point.y - small_last_point.y) * disk);
			if (res->line[t].x < IMAGE_LEFT || res->line[t].x > IMAGE_RIGHT) break;
			res->endpos++;//record end pos
		}
	}
	NomalFilter(2, 5, loc, res);//中线优化
}

// func 计算中线(水平计算) <一般情况>
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para loc(const char) 归属地
// get *res(POINT) 中线数组
void CalcMidBasic_Lv(const LINE edgel, const LINE edger, const char loc, LINE *res)
{
	LineInit(res, loc);

	int i, j, min = _imax(edgel.line[edgel.endpos].y, edger.line[edger.endpos].y);
	for (i = 0, j = 0; i <= edgel.endpos && j <= edger.endpos;)
	{
		if (edgel.line[i].y > edger.line[j].y) i++;
		else if (edgel.line[i].y < edger.line[j].y) j++;
		else { break; }
	}

	for (; i <= edgel.endpos && j <= edger.endpos; i++, j++)
	{
		res->line[res->endpos].x = (int)((edgel.line[i].x + edger.line[j].x) >> 1);
		res->line[res->endpos].y = _imax(edgel.line[i].y, edger.line[j].y);
		if (res->line[res->endpos].y <= min) break;
		res->endpos++;
	}
	NomalFilter(2, 5, loc, res);//中线优化
}

// func through the left side of the repair mid
// para edgel(const LINE) left edge
// para supx(const int) gain; "-"_towards left; "+"_towards right
// para loc(const loc) place of belonging
// get  res(LINE*) result
void Mid_Repair_ByL(const LINE edgel, const int supx, const char loc, LINE *res)
{
	int t = 0;
	LineInit(res, loc);//initlization

	res->line[0].x = edgel.line[t].x + (int)(Get_Road_Width(edgel.line[t].y) >> 1) + supx;
	res->line[0].y = edgel.line[t].y;
	for (t = 1; t <= edgel.endpos; t++)
	{
		if (edgel.line[t].y >= edgel.line[t - 1].y) break;
		if (edgel.line[t].y > ERROR_LINE)
		{
			if (POINT_CHK(edgel.line[t]))
			{
				res->endpos++;
				res->line[res->endpos].x = edgel.line[t].x + (int)(Get_Road_Width(edgel.line[t].y) >> 1) + supx;
				res->line[res->endpos].y = edgel.line[t].y;
			}
		}
	}
}

// func through the right side of the repair mid
// para edgel(const LINE) right edge
// para supx(const int) gain; "-"_towards left; "+"_towards right
// para loc(const loc) place of belonging
// get  res(LINE*) result
void Mid_Repair_ByR(const LINE edger, const int supx, const char loc, LINE *res)
{
	int t = 0;
	LineInit(res, loc);//initlization

	res->line[0].x = edger.line[t].x - (int)(Get_Road_Width(edger.line[t].y) >> 1) + supx;
	res->line[0].y = edger.line[t].y;
	for (t = 1; t <= edger.endpos; t++)
	{
		if (edger.line[t].y >= edger.line[t - 1].y) break;
		if (edger.line[t].y > ERROR_LINE)
		{
			if (POINT_CHK(edger.line[t]))
			{
				res->endpos++;
				res->line[res->endpos].x = edger.line[t].x - (int)(Get_Road_Width(edger.line[t].y) >> 1) + supx;
				res->line[res->endpos].y = edger.line[t].y;
			}
		}
	}
}

//---------------------------- ZEBRA CHECK --------------------------------//

// func 斑马线识别 （黑白条纹总数16 黑条纹总数8左右）
static void check_zebra(const int scan_top, const int scan_bot)
{
	char line, column;
	int leftStopLineCount = 0;
	int rightStopLineCount = 0;
	int zebraRowCount = 0;

	//Zebra.locall==IMAGE_TOP not zebra
	//else , is zebra
	for (line = scan_top; line < scan_bot; line++)
	{
		leftStopLineCount = 0;
		rightStopLineCount = 0;
		for (column = IMAGE_CENTER; column>IMAGE_LEFT + 5; column--)
		{
			if (POINT_FILTER_LEFT(column, line))
			{
				leftStopLineCount++;
				if (leftStopLineCount>4) break;
			}
		}
		for (column = IMAGE_CENTER; column<IMAGE_RIGHT - 5; column++)
		{
			if (POINT_FILTER_RIGHT(column, line))
			{
				rightStopLineCount++;
				if (rightStopLineCount>4) break;
			}
		}
		if (leftStopLineCount + rightStopLineCount >= 6) zebraRowCount++;
		if (zebraRowCount >= 3)
		{
			Zebra.locall = line; //此行一定在斑马线上
		}
	}
}
