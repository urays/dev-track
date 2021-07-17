#include "../../suprc/inc/common.h"
#include "../../suprc/inc/sup_draw.h"
#include "../../suprc/inc/sup_tlock.h"
#include "../../suprc/inc/sup_exfunc.h"

#include "../../conf-anls.h"

#include "../inc/sta_basic.h"
#include "../inc/sta_block.h"

typedef enum _BIN_SIGN_ 
{
	B_BLOCK_LOSE = 3,
	B_BLOCK_LEFT,
	B_BLOCK_RIGHT,

	B_BLOCK_PRE,
	B_BLOCK_SIDE,
}BIN_SIGN;//内部标识

static void calc_shot_last(const LINE edgel, const LINE edger, const int rag, int* shotx, POINT* lastw);
static char Block_Identify(const int shotx, const char shottop, const char shotbot);//障碍识别
static char coord_check(const LINE edgel, const LINE edger, const POINT cood); //黑白跳变计数区分停车线
static void CatchBlock_UD(const int shot_x, const int shot_top, const int shot_bot, int* top, int* bottom);//障碍捕捉
static void GetBlockCoord(const int shotx, const int blockln, POINT* coord);//获取障碍位置坐标

static void Block_MidApproach(const LINE edgel, const LINE edger, const float disk, LINE* mid);//接近障碍中线
static void Block_MidTrans(LINE edgel, LINE edger, const int trans, LINE* mid);//出障碍中线计算
static void Block_GetMid(const LINE edgel, const LINE edger, const int blockln, LINE* mid);//障碍边获取中线

static char Block_SideToOut(const LINE edgel, const LINE edger);//出障碍判断

static void scan_check(const LINE edgel, const LINE edger, const int shotx, int* scantop, int* scanbot);//扫描限制行检查获取
static void Block_existFCT(const POINT lastW, const int neary, const char bk_t);

static char  RunBlock = B_BLOCK_PRE;
static char  B_WorkState = WAITING; //状态机状态
static char  err_block_flg = ifalse;//错误进入障碍标志
static char  catch_flg = ifalse;    //捕捉障碍标志
static char  edgeLost_Flg = ifalse;
static POINT b_lastW = { IMAGE_CENTER,IMAGE_BOTTOM };//最后一个白点
static int   b_shot_x = IMAGE_CENTER;

static char  Block_Dir = B_BLOCK_LOSE;//障碍在赛道中位置
static char  exist_FCT = B_BLOCK_LOSE;//出障碍 前方道路方向预测
static int   block_ave = IMAGE_BOTTOM;   //障碍位置
static int   block_top = IMAGE_BOTTOM;   //障碍顶边
static int   block_bottom = IMAGE_BOTTOM;//障碍底边
static POINT block_coord = { IMAGE_CENTER,IMAGE_BOTTOM };////障碍最边缘坐标
static char  IS_Coord_SIGN = ifalse;
static char  b_side_SIGN = ifalse;

//------------------------- BLOCK FSM INFO RECORD -------------------------//

static BLOCK_DATA BlockData =
{
	.set.exist_dis = SETI_EXISTDIS,   //出障碍边沿平移距离
	.set.safe_trans = SETI_SAFETRANS, //安全边沿平移量
	.set.near_disk = SETF_NEARDISK,  //靠近障碍 偏离距离比(默认 0.5f)

	.top_line = IMAGE_BOTTOM,
	.bot_line = IMAGE_BOTTOM,
	.ave_pos = IMAGE_BOTTOM,
};

BLOCK_DATA* GetBlockAddr(void) { return &BlockData; }
static void Block_Data_Record(void)//障碍状态机信息记录
{
	BlockData.top_line = block_top;
	BlockData.bot_line = block_bottom;
	BlockData.ave_pos = block_ave;
}

//------------------------------- BLOCK FSM -------------------------------//

// func 障碍独立状态机
// para edgel(const LINE) 基础左边沿
// para edger(const LINE) 基础右边沿
// get mid(LINE*) 偏差中线
void Block_FSM(const LINE edgel, const LINE edger, LINE *mid)
{
	int scan_top = IMAGE_BOTTOM;
	int scan_bot = IMAGE_BOTTOM;

	IS_Coord_SIGN = ifalse;
	err_block_flg = ifalse;
	b_lastW.x = IMAGE_CENTER;
	b_lastW.y = IMAGE_BOTTOM;
	block_coord.x = IMAGE_CENTER;
	block_coord.y = IMAGE_BOTTOM;
	b_shot_x = IMAGE_CENTER;

	if (B_WorkState == WAITING)
	{
		catch_flg = ifalse;
		edgeLost_Flg = ifalse;

		block_ave = IMAGE_BOTTOM;  //障碍位置
		block_top = IMAGE_BOTTOM;  //障碍顶边
		block_bottom = IMAGE_BOTTOM;//障碍底边
		exist_FCT = B_BLOCK_LOSE;   //前方大陆预测
		b_side_SIGN = ifalse;  //障碍旁 标志
	}
	char basic_bk = GetBasicBreak();

#ifdef _BLOCK_CROSS_PRE  //开启此标志  障碍只在十字 会比较安全
	if (basic_bk == BAS_BK_BOTH)//only cross-pre
	{
		BFSM_Init(WAITING); //初始化状态机
		err_block_flg = itrue;
		return;
	}
#endif

	//动态投射列
	calc_shot_last(edgel, edger, 50, &b_shot_x, &b_lastW);
	//PreviewPointX(b_lastW, 3);//-test

	scan_check(edgel, edger, b_shot_x, &scan_top, &scan_bot);
	PreviewLabLine(IMAGE_CENTER, scan_top, 30);   //显示上边沿
	PreviewLabLine(IMAGE_CENTER, scan_bot, 30);   //显示下边沿

	if (catch_flg == itrue)//获取障碍坐标位置
	{
		CatchBlock_UD(b_shot_x, scan_top, scan_bot, &block_top, &block_bottom);
		PreviewLabLine(IMAGE_CENTER, block_top, 5);   //显示上边沿
		PreviewLabLine(IMAGE_CENTER, block_bottom, 5);//显示下边沿
		block_ave = (int)((block_top + block_bottom) >> 1);
		GetBlockCoord(b_shot_x, block_ave, &block_coord); //获取障碍坐标位置
		IS_Coord_SIGN = coord_check(edgel, edger, block_coord);
		//PreviewPoint(block_coord, 5);//显示障碍最边缘位置
	}
	//
	switch (RunBlock)
	{
	case(B_BLOCK_PRE): {//障碍前处理
		B_WorkState = RUNNING;
		CalcMidBasic(edgel, edger, 0.5f, LOC_BASIC, mid);//障碍在 直道上

		if (catch_flg == ifalse)
		{
			char basic_type = GetBasicPlace();//获取基本赛道信息(直道/弯道)
			int miny = _imin(edgel.line[edgel.endpos].y, edger.line[edger.endpos].y);
			miny = _imin(miny, b_lastW.y);
			if (basic_bk != BAS_BK_NOT || miny > 20
				|| basic_type == BAS_BEND_L || basic_type == BAS_BEND_R)
			{
				BFSM_Init(WAITING); //初始化状态机
				err_block_flg = itrue;
				break;
			}
			if (Block_Identify(b_shot_x, scan_top, scan_bot) == itrue)
			{
				catch_flg = itrue;
			}
		}
		else if (catch_flg == itrue)
		{
			if (_iinc(block_bottom - block_top, BLOCK_MIN_HGT, BLOCK_MAX_HGT)
				&& IS_Coord_SIGN == itrue)//去除停车线的影响
			{
				RunBlock = B_BLOCK_SIDE;
				break;
			}
			else
			{
				BFSM_Init(WAITING); //初始化状态机
				//err_block_flg = itrue;
				break;
			}
		}
	}break;
	case(B_BLOCK_SIDE): {//障碍旁处理
		B_WorkState = RUNNING;
		Block_existFCT(b_lastW, scan_top, basic_bk);//出口前方道路预测
		Block_GetMid(edgel, edger, block_ave, mid);//计算中线

		if (Block_SideToOut(edgel, edger) == itrue)
		{
			BFSM_Init(WAITING); //初始化状态机
			break;
		}
	}break;
	default:break;
	}
	Block_Data_Record();//障碍状态记录
}


// func 初始化障碍状态机
// para init(const char) 初始化状态
// init：
//     WAITING - 退出初始化
//     RUNNING - 不退出初始化
void BFSM_Init(const char init)
{
	B_WorkState = init;

	RunBlock = B_BLOCK_PRE;
	Block_Dir = B_BLOCK_LOSE; //障碍在赛道中位置
	block_ave = IMAGE_BOTTOM;  //障碍位置
	block_top = IMAGE_BOTTOM;  //障碍顶边
	block_bottom = IMAGE_BOTTOM;//障碍底边
	exist_FCT = B_BLOCK_LOSE;
	b_side_SIGN = ifalse;
}

// func 获取障碍状态机运行状态
char Get_BFSMState(void)
{
	return B_WorkState;
}

// func 判断是否错误识别障碍
char Goto_BFSM_Err(void)
{
	return err_block_flg;
}

// func 获取障碍中具体位置
unsigned char GetBlockPlace(void)
{
	unsigned char ret = BLOCK_UNKNOW;
	if (Block_Dir != B_BLOCK_LOSE)
	{
		switch (RunBlock) {
		case(B_BLOCK_PRE):ret = (Block_Dir == B_BLOCK_LEFT) ? BLOCK_PRE_L : BLOCK_PRE_R; break;
		case(B_BLOCK_SIDE):ret = (Block_Dir == B_BLOCK_LEFT) ? BLOCK_SIDE_L : BLOCK_SIDE_R; break;
		}
	}
	return ret;
}

//------------------------------ BLOCK PRE --------------------------------//

// func 计算入射点/最远白点
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para rag(const int) 最远白点搜索范围
// para shotx(int*) 入射列
// para lastw(POINT*) 最远白点
static void calc_shot_last(const LINE edgel, const LINE edger, const int rag, int* shotx, POINT* lastw)
{
	LINE btmp;
	int miny = IMAGE_BOTTOM;
	int minx = (int)((edgel.line[0].x + edger.line[0].x) >> 1);
	CalcMidBasic_Lv(edgel, edger, LOC_BLOCK, &btmp);
	if (btmp.endpos > 5)
	{
		LSQ_INFO lsq;
		POINT endp;
		LSQ_algorithm(&btmp, btmp.endpos, 0, &lsq);//拟合线段
		GetFilterLine(&lsq, LOC_BLOCK, &btmp, ifalse);
		int t, max = _imax(btmp.line[0].x, btmp.line[btmp.endpos].x);
		int min = _imin(btmp.line[0].x, btmp.line[btmp.endpos].x);

		for (t = min - BLOCK_SHOTRAG; t <= max + BLOCK_SHOTRAG; t++)
		{
			GetLastWhiteXY(t, IMAGE_BOTTOM, EXF_UP, &endp);
			if (miny > endp.y)
			{
				miny = endp.y;
				minx = endp.x;
			}
		}
		(*shotx) = minx;
	}
	FullLine_ShotUpF(minx, IMAGE_BOTTOM, rag, lastw);//投射求最远白点
	if (btmp.endpos <= 5)
	{
		(*shotx) = b_lastW.x;
	}
}

// func 障碍位置坐标有效性检测
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para cood(const POINT) 检测目标坐标
static char coord_check(const LINE edgel, const LINE edger, const POINT cood)
{
	int t, count;
	POINT endp;

	int right_i = 0, left_i = 0;
	for (t = 0; t < edgel.endpos; t++) //寻找对应位置
	{
		left_i = t;
		if (_iiabs(edgel.line[t].y - cood.y) < 2) break;
	}
	for (t = 0; t < edger.endpos; t++)
	{
		right_i = t;
		if (_iiabs(edger.line[t].y - cood.y) <= 2) break;
	}

	if (Block_Dir == B_BLOCK_LEFT)
	{
		if (_iiabs(edgel.line[left_i].x - cood.x) < 5) { return ifalse; }
		for (count = -2; count <= 2; count++)
		{
			GetLastWhiteXY(cood.x + 1, cood.y + count, EXF_RIGHT, &endp);
			if (endp.x >= IMAGE_RIGHT)
			{
				if (endp.x - cood.x < 5) { return ifalse; }
			}
			else
			{  //右边沿一定存在
				if (_iiabs(edger.line[right_i].x - endp.x) > 5) { return ifalse; }
			}
		}
	}
	else if (Block_Dir == B_BLOCK_RIGHT)
	{
		if (_iiabs(edger.line[right_i].x - cood.x) < 5) { return ifalse; }
		for (count = -2; count <= 2; count++)
		{
			GetLastWhiteXY(cood.x - 1, cood.y + count, EXF_LEFT, &endp);
			if (endp.x <= IMAGE_LEFT)
			{
				if (cood.x - endp.x < 5) { return ifalse; }
			}
			else
			{  //左边沿一定存在
				if (_iiabs(edgel.line[left_i].x - endp.x) > 5) { return ifalse; }
			}
		}
	}
	else { return ifalse; }
	return itrue;
}

// func 判断是否存在障碍
// para shotx(const int)
// para shottop(const char) 扫描终止行
// para shotbot(const char) 扫描起始行
static char Block_Identify(const int shotx, const char shottop, const char shotbot)
{
	char line, column;//-Local
	int P = 0, D = 1; //-Local
	char B_flg_L = ifalse, B_flg_R = ifalse;//-Local
	char BL_count = 0, BR_count = 0;    //-Local
	char now_col_L = shotx + 5, now_col_R = shotx - 5;//-Local
	char save_col_L = 0, save_col_R = 0;//-Local
	char stop_L = ifalse, stop_R = ifalse;//-Local
	//char bottom_line = IMAGE_TOP;       //-Local

	Block_Dir = B_BLOCK_LOSE;//初始化障碍位置信息
	for (line = shotbot - 1; line >= shottop; line--)
	{
		P = 0;
		B_flg_L = ifalse, B_flg_R = ifalse;
		stop_L = ifalse, stop_R = ifalse;
		for (D = 1; D < IMG_WIDTH;)
		{
			P = !P;
			if (P && stop_L == ifalse) //left
			{
				column = shotx + 5 - D;
				if (column < IMAGE_LEFT) column = IMAGE_LEFT;
				if (column <= IMAGE_LEFT || POINT_FILTER_LEFT(column, line))
				{
					now_col_L = column;
					stop_L = itrue;
				}
			}
			else //right
			{
				column = shotx - 5 + D;
				if (column > IMAGE_RIGHT) column = IMAGE_RIGHT;
				if (column >= IMAGE_RIGHT || POINT_FILTER_RIGHT(column, line))
				{
					now_col_R = column;
					stop_R = itrue;
				}
				else D++;
			}
			if (stop_L == itrue && stop_R == itrue)  break;
			else if (stop_L == itrue && stop_R == ifalse) P = 1;
			else if (stop_L == ifalse && stop_R == itrue) P = 0, D++;
		}
		//bottom_line = line;
		if (now_col_L - save_col_L >= 10 && save_col_L != 0)//10
		{
			BL_count++;//突变计数
			if (BL_count >= 3)
			{
				B_flg_L = itrue;
				break;
			}
		}
		else if (save_col_L - now_col_L >= 7 && save_col_L != 0)//终止搜索
		{
			B_flg_L = ifalse;
			break;
		}
		else {
			B_flg_L = ifalse;
			save_col_L = now_col_L;
			BL_count = 0;
		}
		if (save_col_R - now_col_R >=10 && save_col_R != 0)//10
		{
			BR_count++;//突变计数
			if (BR_count >= 3)
			{
				B_flg_R = itrue;
				break;
			}
		}
		else if (now_col_R - save_col_R >= 7 && save_col_R != 0)//搜索终止
		{
			B_flg_R = ifalse;
			break;
		}
		else {
			B_flg_R = ifalse;
			save_col_R = now_col_R;
			BR_count = 0;
		}
	}
	if (B_flg_L == itrue && B_flg_R == ifalse) Block_Dir = B_BLOCK_LEFT;
	else if (B_flg_R == itrue && B_flg_L == ifalse) Block_Dir = B_BLOCK_RIGHT;

	if (Block_Dir == B_BLOCK_LOSE)  return ifalse;
	return itrue;
}

//------------------------------ BLOCK SIDE -------------------------------//

// func 靠近障碍 中线计算
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para disk(const float) 偏离距离比(默认 0.5f)
// get mid(LINE*) 中线
static void Block_MidApproach(const LINE edgel, const LINE edger, const float disk, LINE* mid)
{
	LINE lostEG, existEG, radioLN;
	int t = 0, spc_x = IMAGE_CENTER;
	char spc_flg = ifalse;

	radioLN.endpos = 0;
	lostEG.endpos = 1;
	lostEG.line[0].x = block_coord.x;
	lostEG.line[0].y = block_coord.y;
	lostEG.line[1].x = block_coord.x;
	lostEG.line[1].y = block_coord.y - 1;

	if (Block_Dir == B_BLOCK_LEFT)
	{
		if (block_coord.y >= edger.line[edger.endpos].y)
		{
			existEG = edger;
			for (t = edger.endpos - 1; t > 10; t--)
			{
				if (edger.line[t].y - edger.line[edger.endpos].y > 5)
				{
					existEG.endpos = t;
					break;
				}
			}
			CalcMidBasic_Pp(lostEG, existEG, disk, LOC_BLOCK, &radioLN);//比例匹配计算中线
		}
		else
		{
			spc_flg = itrue;
			spc_x = (block_coord.x + UNSAFE_TRANS < IMAGE_RIGHT) ? (block_coord.x + UNSAFE_TRANS) : IMAGE_RIGHT;
			GetSegment(IMAGE_CENTER, IMAGE_BOTTOM, spc_x, block_coord.y, LOC_BLOCK, mid);
		}
	}
	else if (Block_Dir == B_BLOCK_RIGHT)
	{
		if (block_coord.y >= edgel.line[edgel.endpos].y)
		{
			existEG = edgel;
			for (t = edgel.endpos - 1; t > 10; t--)
			{
				if (edgel.line[t].y - edgel.line[edgel.endpos].y > 5)
				{
					existEG.endpos = t;
					break;
				}
			}
			CalcMidBasic_Pp(existEG, lostEG, disk, LOC_BLOCK, &radioLN);//比例匹配计算中线
		}
		else
		{
			spc_flg = itrue;
			spc_x = (block_coord.x - UNSAFE_TRANS>IMAGE_LEFT) ? (block_coord.x - UNSAFE_TRANS) : IMAGE_LEFT;
			GetSegment(IMAGE_CENTER, IMAGE_BOTTOM, spc_x, block_coord.y, LOC_BLOCK, mid);
		}
	}
	if (spc_flg == ifalse)
	{
		if (radioLN.endpos > 5)
		{
			GetSegment(radioLN.line[0].x, IMAGE_BOTTOM, radioLN.line[0].x, radioLN.line[0].y, LOC_BLOCK, mid);
			LineMerge(&radioLN, 0, radioLN.endpos, LOC_BLOCK, mid);
		}
		else {
			mid->endpos = 0;
			mid->local = LOC_BLOCK;
		}
	}
	//mid->local = LOC_BLOCK;
}

// func 靠近障碍 平移边沿 中线计算
// para edgel(LINE) 左边沿
// para edger(LINE) 右边沿
// para trans(const int) 平移量
// get mid(LINE*) 中线
static void Block_MidTrans(LINE edgel, LINE edger, const int trans, LINE* mid)
{
	LSQ_INFO lsq;  //-Local

	LineInit(mid, LOC_BLOCK);

	if (Block_Dir == B_BLOCK_LEFT)
	{
		*mid = edger;
		LSQ_algorithm(&edger, edger.endpos, 0, &lsq);//获取右边沿斜率
		LineTrans(-trans, lsq.k, LOC_BLOCK, mid, itrue);//拟合中线
	}
	else if (Block_Dir == B_BLOCK_RIGHT)
	{
		*mid = edgel;
		LSQ_algorithm(&edgel, edgel.endpos, 0, &lsq);//获取左边沿斜率
		LineTrans(trans, lsq.k, LOC_BLOCK, mid, itrue);//拟合中线
	}
	//mid->local = LOC_BLOCK;
}

// func 障碍边获取中线
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// get mid(LINE*) 中线
static void Block_GetMid(const LINE edgel, const LINE edger, const int blockln, LINE* mid)
{
	mid->local = LOC_BLOCK;

	if (exist_FCT == B_BLOCK_LOSE //前方路况未知
		|| (exist_FCT == B_BLOCK_LEFT && Block_Dir == B_BLOCK_RIGHT)
		|| (exist_FCT == B_BLOCK_RIGHT && Block_Dir == B_BLOCK_LEFT))
	{
		if (blockln >= IMAGE_BOTTOM || IS_Coord_SIGN == ifalse)
		{
			if (Block_Dir == B_BLOCK_RIGHT && edger.endpos > 5 && edger.line[0].y >= IMAGE_BOTTOM
				&&edger.line[0].x - edgel.line[0].x <= Get_Road_Width(IMAGE_BOTTOM) - 5)
			{
				CalcMidBasic_Lv(edgel, edger, LOC_BLOCK, mid);//障碍边
				b_side_SIGN = itrue;
			}
			else if (Block_Dir == B_BLOCK_LEFT && edgel.endpos > 5 && edgel.line[0].y >= IMAGE_BOTTOM
				&&edger.line[0].x - edgel.line[0].x <= Get_Road_Width(IMAGE_BOTTOM) - 5)
			{
				CalcMidBasic_Lv(edgel, edger, LOC_BLOCK, mid);//障碍边
				b_side_SIGN = itrue;
			}
			else if (b_side_SIGN == ifalse)
			{
				Block_MidTrans(edgel, edger, BlockData.set.safe_trans, mid);
			}
			else
			{
				LineInit(mid, LOC_BLOCK);
				//CalcMidBasic_Lv(edgel, edger, LOC_BLOCK, mid);//障碍边
			}
		}
		else {
			Block_MidApproach(edgel, edger, BlockData.set.near_disk, mid);
		}
	}
	else {
		Block_MidTrans(edgel, edger, BlockData.set.exist_dis, mid);
	}
}

// func 障碍出口道路预测
// para lastW(const POINT) 最后一个白点
// para neary(const int) 最近行
// para bk_t(const char) 拐点类型
static void Block_existFCT(const POINT lastW, const int neary, const char bk_t)
{
	int line = 0;
	int saveL = IMAGE_RIGHT;
	int saveR = IMAGE_LEFT;

	if ((Block_Dir == B_BLOCK_LEFT && bk_t == BAS_BK_LEFT)
		|| (Block_Dir == B_BLOCK_RIGHT && bk_t == BAS_BK_RIGHT))
	{
		if (exist_FCT == B_BLOCK_LOSE)
		{
			POINT R_p, L_p;
			for (line = lastW.y; line <= neary; line++)
			{
				GetLastWhiteXY(lastW.x, line, EXF_LEFT, &L_p);
				GetLastWhiteXY(lastW.x, line, EXF_RIGHT, &R_p);
				if (L_p.x <= IMAGE_LEFT + 1 || L_p.x - saveL > 10)
				{
					if (L_p.x - saveL > 15)//相差很大说明是大弯
					{
						exist_FCT = B_BLOCK_LEFT;
					}
					break;
				}
				if (R_p.x >= IMAGE_RIGHT - 1 || saveR - R_p.x > 10)
				{
					if (saveR - R_p.x > 15)//相差很大说明是大弯
					{
						exist_FCT = B_BLOCK_RIGHT;
					}
					break;
				}
				if (saveL > L_p.x) saveL = L_p.x;
				if (saveR < R_p.x) saveR = R_p.x;
			}
		}
	}
}

//------------------------------- BRICK OUT -------------------------------//

// func 出障碍判断
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
static char Block_SideToOut(const LINE edgel, const LINE edger)
{
	char basic_bk = GetBasicBreak();//-Local

	int i, j;
	for (i = edgel.endpos, j = edger.endpos; i >= 0 && j >= 0;)
	{
		if (edgel.line[i].y > edger.line[j].y) j--;
		else if (edgel.line[i].y < edger.line[j].y) i--;
		else { break; }
	}
	int _widthy = edgel.line[i].y;
	int _width = edger.line[j].x - edgel.line[i].x;

	if (Block_Dir == B_BLOCK_LEFT)
	{
		if (edgeLost_Flg == itrue && edgel.endpos > 10)
		{
			if (_width >= Get_Road_Width(_widthy) - 5)
			{
				if (_iiabs(edgel.line[2].x - edgel.line[1].x) <= 1)
				{
					if (_iiabs(edgel.line[1].x - edgel.line[0].x) <= 1) { return itrue; }
				}
			}
		}
		if (edgel.endpos <= 5 && edgeLost_Flg == ifalse
			&& (basic_bk == BAS_BK_NOT || basic_bk == BAS_BK_RIGHT))//不存在左角点
		{
			edgeLost_Flg = itrue;
		}
	}
	else  if (Block_Dir == B_BLOCK_RIGHT)
	{
		if (edgeLost_Flg == itrue && edger.endpos > 10)//消失后 出现边沿  (可能是障碍边沿)
		{
			if (_width >= Get_Road_Width(_widthy) - 5)
			{
				if (_iiabs(edger.line[2].x - edger.line[1].x) <= 1)
				{
					if (_iiabs(edger.line[1].x - edger.line[0].x) <= 1) { return itrue; }
				}
			}
		}
		if (edger.endpos <= 5 && edgeLost_Flg == ifalse
			&& (basic_bk == BAS_BK_NOT || basic_bk == BAS_BK_LEFT))//不存在右角点
		{
			edgeLost_Flg = itrue;
		}
	}
	return ifalse;
}

//------------------------------ BLOCK CATCH ------------------------------//

// func 障碍扫描限制位置获取
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para shotx(const int) 入射列
// get scantop(const int) 扫描限制
static void scan_check(const LINE edgel, const LINE edger, const int shotx, int* scantop, int* scanbot)
{
	//Range: scantop ~ scanbot
	int line, tmp_top, tmp_bot;
	int now, min = IMG_WIDTH;
	char c_dir = EXF_BOTH;

	tmp_top = _imin(edgel.line[edgel.endpos].y, edger.line[edger.endpos].y);//边沿最远终点行
	tmp_top = _imax(b_lastW.y, tmp_top);
	tmp_top = _imax(tmp_top, BLOCK_SCAN_END);//最远限制

	int maxs = _imax(edgel.line[edgel.endpos].y, edger.line[edger.endpos].y);
	if (catch_flg == itrue)
	{
		int left_x = edgel.line[edgel.endpos].x;
		int left_y = edgel.line[edgel.endpos].y;
		int right_x = edger.line[edger.endpos].x;
		int right_y = edger.line[edger.endpos].y;
		POINT endp;
		if (Block_Dir == B_BLOCK_LEFT)
		{
			c_dir = EXF_LEFT;
			for (line = left_y; line >= tmp_top; line--)
			{
				GetLastWhiteXY(left_x, line, EXF_RIGHT, &endp);
				if (endp.x >= shotx)
				{
					maxs = line;
					break;
				}
			}
		}
		else if (Block_Dir == B_BLOCK_RIGHT)
		{
			c_dir = EXF_RIGHT;
			for (line = right_y; line >= tmp_top; line--)
			{
				GetLastWhiteXY(right_x, line, EXF_LEFT, &endp);
				if (endp.x <= shotx)
				{
					maxs = line;
					break;
				}
			}
		}
	}
	tmp_top += 1;
	for (line = tmp_top; line < maxs; line++)
	{
		now = WhiteCount_LR(c_dir, shotx, line);
		if (min > now)
		{
			min = now;
			tmp_top = line;
		}
	}
	*scantop = tmp_top;

	int i, j;
	for (i = 0, j = 0; i <= edgel.endpos && j <= edger.endpos;)
	{
		if (edgel.line[i].y > edger.line[j].y) i++;
		else if (edgel.line[i].y < edger.line[j].y) j++;
		else { break; }
	}
	tmp_bot = _imin(edgel.line[i].y, edger.line[j].y);

	*scanbot = tmp_bot;
}

// func 障碍捕捉(top and bottom)
// para shot_x(const int) 入射列
// para shot_top(const int) 扫描行上
// para shot_bot(const int) 扫描行下
// para top(int*) 左边沿
// para bottom(int*) 右边沿
static void CatchBlock_UD(const int shot_x, const int shot_top,const int shot_bot, int* top, int* bottom)
{
	int line, column;          //-Loacl
	int count_dir = EXF_BOTH;         //-Loacl
	int now_count = 0, last_count = 0;//-Loacl
	int max_count = 0, end_count = 0; //-Loacl

	(*top) = IMAGE_TOP;
	(*bottom) = IMAGE_BOTTOM;

	if (Block_Dir == B_BLOCK_LEFT) count_dir = EXF_LEFT;
	else if (Block_Dir == B_BLOCK_RIGHT) count_dir = EXF_RIGHT;

	for (line = shot_top; line < shot_bot; line++)
	{
		now_count = WhiteCount_LR(count_dir, shot_x, line);
		if (now_count >= max_count)
		{
			max_count = now_count;
			(*top) = line;
			end_count = 0;
		}
		else if (max_count - now_count >= 5)
		{
			if (last_count <= now_count)
				end_count++;
		}
		if (end_count >= 2) break;
		last_count = now_count;
	}
	(*bottom) = (*top);
	if (Block_Dir == B_BLOCK_LEFT)
	{
		for (column = shot_x; column > IMAGE_LEFT; column--)
		{
			if (POINT_FILTER_LEFT(column, (*top))) break;
			for (line = (*top) + 1; line < shot_bot; line++)
			{
				if (!POINT_WHITE(column, line) && POINT_WHITE(column, line + 1)
					|| (!POINT_WHITE(column, line) && line >= shot_bot - 1))
				{
					if ((*bottom) <= line + 1)
					{
						(*bottom) = line + 1;
					}
					break;
				}
				else if (POINT_WHITE(column, line) && POINT_WHITE(column, line + 1))//
				{
					break;
				}
			}
			if ((*bottom) >= shot_bot)
			{
				(*bottom) = shot_bot;
				break;
			}
		}
	}
	else if (Block_Dir == B_BLOCK_RIGHT)
	{
		for (column = shot_x; column <IMAGE_RIGHT; column++)
		{
			if (POINT_FILTER_RIGHT(column, (*top))) break;
			for (line = (*top) + 1; line < shot_bot; line++)
			{
				if (!POINT_WHITE(column, line) && POINT_WHITE(column, line + 1)
					|| (!POINT_WHITE(column, line) && line >= shot_bot - 1))
				{
					if ((*bottom) <= line + 1)
					{
						(*bottom) = line + 1;
					}
					break;
				}
				else if (POINT_WHITE(column, line) && POINT_WHITE(column, line + 1))
				{
					break;
				}
			}
			if ((*bottom) >= shot_bot)
			{
				(*bottom) = shot_bot;
				break;
			}
		}
	}
}

// func 获取障碍坐标位置
// para shotx(const int)
// para blockln(const int) 障碍位置行
// para coord(POINT*) 坐标点
static void GetBlockCoord(const int shotx, const int blockln, POINT* coord)
{
	int column;

	coord->x = shotx;
	coord->y = blockln;
	if (Block_Dir == B_BLOCK_LEFT)
	{
		for (column = shotx; column > IMAGE_LEFT + 1; column--)
		{
			if (POINT_FILTER_LEFT(column, blockln))
			{
				coord->x = column;
				break;
			}
		}
	}
	else if (Block_Dir == B_BLOCK_RIGHT)
	{
		for (column = shotx; column <IMAGE_RIGHT - 1; column++)
		{
			if (POINT_FILTER_RIGHT(column, blockln))
			{
				coord->x = column;
				break;
			}
		}
	}
}
