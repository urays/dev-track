#include "../../suprc/inc/common.h"
#include "../../suprc/inc/sup_tlock.h"
#include "../../suprc/inc/sup_draw.h"

#include "../../suprc/inc/sup_exfunc.h"

#include "../inc/sta_cross.h"
#include "../inc/sta_basic.h"
#include "../../conf-anls.h"

typedef enum _CIN_SIGN_ {
	C_LOSE = 3,  //方向
	C_LEFT,
	C_RIGHT,

	C_CROSS_PRE, //位置
	C_CROSS_IN,
}CIN_SIGN;//内部标识

//十字前
static void ThrowMidline(LINE edgel, LINE edger, const char bkt, LINE *mid);//投射中线

//十字中
static char Judge_CrossOut(const LINE edgel, const LINE edger, const int white_x);//判断是否出十字
static void PushUp_MidPT(const int starty, const POINT pstop, POINT* pmid);
static void CrossIn_GetMid(const int starty, const POINT pstop, const POINT pmid, LINE* mid);

static char RunCross= C_CROSS_PRE;
static char C_WorkState = WAITING;
static POINT botP = { IMAGE_CENTER,IMAGE_BOTTOM };
static char c_edge_loss = ifalse; //判断在十字中时 边沿是否都消失过
static int  C_SHOT_CENTER = IMAGE_CENTER; //最远白点投射动态列

//------------------------------ CROSS FSM --------------------------------//

// func 十字独立状态机
// para edgel(const LINE) 基础左边沿
// para edger(const LINE) 基础右边沿
// get *mid(LINE) 偏差中线
void Cross_FSM(const LINE edgel, const LINE edger, LINE *mid)
{
	POINT midPT = { IMAGE_CENTER,IMAGE_BOTTOM - 1 }; //中线中点
	POINT stopPT = { IMAGE_CENTER,IMAGE_BOTTOM - 2 };//中线终止点

	int topmax = _imin(edgel.line[edgel.endpos].y, edger.line[edger.endpos].y);
	switch (RunCross)/////状态处理
	{
	case(C_CROSS_PRE): {
		C_WorkState = RUNNING;
		char breaktype = GetBasicBreak();//获取角点类型
		if (breaktype == BAS_BK_NOT || topmax >= TURNIN_MAX)//
		{
			RunCross = C_CROSS_IN;
			break;
		}
		ThrowMidline(edgel, edger, breaktype, mid);
	}break;
	case(C_CROSS_IN): {
		C_WorkState = RUNNING;
		//
		if (edgel.endpos <= 1 && edger.endpos <= 1  //判断在十字中时 边沿是否都消失过
			&& edgel.line[0].y == IMAGE_BOTTOM && edger.line[0].y == IMAGE_BOTTOM)
		{
			c_edge_loss = itrue;
		}
		int scan_bot = ((c_edge_loss == itrue) ? IMAGE_BOTTOM : topmax);
		FullLine_ShotUpF(C_SHOT_CENTER, IMAGE_BOTTOM, 10, &stopPT);
		//PreviewPoint(stopPT, 3);

		PushUp_MidPT(scan_bot - 1, stopPT, &midPT);//十字中推中点
		CrossIn_GetMid(scan_bot - 1, stopPT, midPT, mid);

		if (Judge_CrossOut(edgel, edger, stopPT.x) == itrue)
		{
			CFSM_Init(WAITING);
			break;
		}
	}break;
	default:break;
	}
}

// func 初始化十字状态机
// para init(const char) 初始化状态
// init：
//     WAITING - 退出初始化
//     RUNNING - 不退出初始化
void CFSM_Init(const char init)
{
	C_WorkState = init;

	RunCross = C_CROSS_PRE;
	botP.x = IMAGE_CENTER;
	botP.y = IMAGE_BOTTOM;
	c_edge_loss = ifalse;
	C_SHOT_CENTER = IMAGE_CENTER;
}

// func 获取十字状态机状态
char Get_CFSMState(void)
{
	return C_WorkState;
}

// func 获取十字位置
unsigned char GetCrossPlace(void)
{
	unsigned char ret = CROSS_UNKNOW;

	switch (RunCross) {
	case(C_CROSS_PRE):ret = CROSS_PRE; break;
	case(C_CROSS_IN):ret = CROSS_IN; break;
	}
	return ret;
}
//----------------------------- CROSS PRE --------------------------------//

// func 拐点投射中线
// para edgel(LINE) 左边沿
// para edger(LINE) 右边沿
// para bkt(const char) 拐点信息
// get *mid(LINE) 输出中线
static void ThrowMidline(LINE edgel, LINE edger, const char bkt, LINE *mid)
{
	LINE tmpmid; //-Local
	LSQ_INFO lsq;//-Local

	LineInit(mid, LOC_CROSS);

	mid->line[0].x = IMAGE_CENTER;
	mid->line[0].y = IMAGE_BOTTOM;

	if ((edgel.line[edgel.endpos].y >= TURNIN_MAX
		|| edger.line[edger.endpos].y >= TURNIN_MAX)
		&& (bkt == BAS_BK_BOTH || (bkt == BAS_BK_LEFT && edger.endpos < edgel.endpos)
			|| (bkt == BAS_BK_RIGHT && edger.endpos > edgel.endpos)))
	{
		if (edgel.endpos < edger.endpos)
		{
			if (edger.line[edger.endpos].x - edger.line[0].x <= edger.endpos)//排除底部离散点干扰
			{
				LSQ_algorithm(&edger, 0, edger.endpos, &lsq);
				GetFilterLine(&lsq, LOC_CROSS, mid, ifalse);
				GetSegment(botP.x, botP.y,
					mid->line[mid->endpos].x - TRANS_AMOUNT,
					mid->line[mid->endpos].y, LOC_CROSS, mid);
			}
		}
		else if (edgel.endpos >= edger.endpos)
		{
			if (edgel.line[0].x - edgel.line[edgel.endpos].x <= edgel.endpos)
			{
				LSQ_algorithm(&edgel, 0, edgel.endpos, &lsq);
				GetFilterLine(&lsq, LOC_CROSS, mid, ifalse);
				GetSegment(botP.x, botP.y,
					mid->line[mid->endpos].x + TRANS_AMOUNT,
					mid->line[mid->endpos].y, LOC_CROSS, mid);
			}
		}
	}
	else {
		if (bkt == BAS_BK_BOTH &&
			((edgel.endpos > edger.endpos && edgel.line[edgel.endpos].x >= IMAGE_CENTER - 3) ||
			(edgel.endpos < edger.endpos && edger.line[edger.endpos].x <= IMAGE_CENTER + 3))) {
			CalcMidBasic_Pp(edgel, edger, 0.5f, LOC_CROSS, &tmpmid);
			//PreviewLine(tmpmid,0,tmpmid.endpos);
		}
		else {//左右边沿水平计算中线
			CalcMidBasic_Lv(edgel, edger, LOC_CROSS, &tmpmid);
		}
		if (tmpmid.endpos <= 5) { return; }
		LSQ_algorithm(&tmpmid, 0, tmpmid.endpos, &lsq);//拟合线段
		GetFilterLine(&lsq, LOC_CROSS, mid, itrue);
		botP = mid->line[0];//记录十字两拐点前中线起始点
	}
	C_SHOT_CENTER = mid->line[mid->endpos].x;
}

//----------------------------- CROSS IN --------------------------------//

#define MUTATE_VALUE   (5)//突变比较值 
//<放在十字中间位置取左右突变平均值>
#define PUSH_MIDDLE_NUM  (5)
//<十字中 边沿起始点,信任值; 该值越大,预计算中点位置越靠上>

// func 上推获得十字中中点
// para starty(const int) 起始行
// para pstop(const POINT) 终止点
// get *pmid(POINT) 十字中中点
static void PushUp_MidPT(const int starty, const POINT pstop, POINT* pmid)
{
	int t, LFlg = ifalse, RFlg = ifalse; //the sign of effective start point
	char L_1_bk = ifalse, R_1_bk = ifalse;//first mutation
	char L_2_bk = ifalse, R_2_bk = ifalse;//second mutation
	char L_x = IMAGE_CENTER, R_x = IMAGE_CENTER;
	char L_y = IMAGE_BOTTOM, R_y = IMAGE_BOTTOM;
	int L0_count = 0, L0_max = 0, L0_last = 0, L0_true = 0;
	int R0_count = 0, R0_max = 0, R0_last = 0, R0_true = 0;

	POINT pMiddle = { IMAGE_CENTER,IMAGE_BOTTOM - 1 };//中线中点
	for (t = starty; t > pstop.y; t--)//
	{
		L0_count = WhiteCount_LR(EXF_LEFT, pstop.x, t);
		R0_count = WhiteCount_LR(EXF_RIGHT, pstop.x, t);
		if (LFlg == ifalse)
		{
			if (L0_count >= L0_max && L_1_bk == ifalse)
			{
				L0_true = 0;
				L0_max = L0_count;
			}
			else if (L0_max - L0_count >= MUTATE_VALUE)//检测到突变点(白点数突然变少)
			{
				L_1_bk = itrue;//标记第一次突变
				L0_true++;
			}
			if (L0_true >= PUSH_MIDDLE_NUM)//从找到十字中起始点往上推n个点
			{
				L_x = pstop.x - L0_count;//record
				L_y = t;
				L0_last = L0_count;
				LFlg = itrue;
			}
		}
		else if (LFlg == itrue)
		{
			if (L0_count - L0_last >= MUTATE_VALUE) L_2_bk = itrue;//停止左推点
			if (L_2_bk == ifalse)
			{
				L_x = pstop.x - L0_count;
				L_y = t;
			}
		}
		if (RFlg == ifalse)
		{
			if (R0_count >= R0_max && R_1_bk == ifalse)
			{
				R0_true = 0;
				R0_max = R0_count;
			}
			else if (R0_max - R0_count >= MUTATE_VALUE)
			{
				R_1_bk = itrue;//标记第一次突变
				R0_true++;
			}
			if (R0_true >= PUSH_MIDDLE_NUM)
			{
				R_x = pstop.x + R0_count;
				R_y = t;
				R0_last = R0_count;
				RFlg = itrue;
			}
		}
		else if (RFlg == itrue)
		{
			if (R0_count - R0_last >= MUTATE_VALUE) R_2_bk = itrue;//停止右推点
			if (R_2_bk == ifalse)
			{
				R_x = pstop.x + R0_count;
				R_y = t;
			}
		}
		if (LFlg == itrue && RFlg == itrue)//
		{
			if (L_2_bk == ifalse || R_2_bk == ifalse)
			{
				POINT shotP;//投射终止点
				if (L_2_bk == itrue && R_2_bk == ifalse)
				{
					GetLastWhiteXY(L_x + 1, L_y, EXF_RIGHT, &shotP);
					pMiddle.x = (int)((L_x + shotP.x) >> 1);
					pMiddle.y = L_y;
					break;
				}
				else if (L_2_bk == ifalse && R_2_bk == itrue)
				{
					GetLastWhiteXY(R_x - 1, R_y, EXF_LEFT, &shotP);
					pMiddle.x = (int)((R_x + shotP.x) >> 1);
					pMiddle.y = R_y;
					break;
				}
			}
			if (R_y == L_y)//locate on the same line
			{
				pMiddle.x = (int)((L_x + R_x) >> 1);
				pMiddle.y = t;
				break;
			}
		}
	}
	(*pmid) = pMiddle;
}

// func 十字中获取中线
// para starty(const int) 十字中推点起始行
// para pstop(const POINT) 终止点
// para pmid(const POINT) 中点
// para mid(LINE*) 输出中线
static void CrossIn_GetMid(const int starty, const POINT pstop, const POINT pmid, LINE* mid)
{
	int line, fineY;
	POINT L_p, R_p;

	if (pmid.y < starty)
	{
		fineY = _imin(IMAGE_BOTTOM, pmid.y + MIDDLE_SUPLEN);
		GetSegment(pmid.x, fineY, pmid.x, pmid.y, LOC_CROSS, mid);
		for (line = pmid.y - 1; line>pstop.y + 1; line--)
		{
			GetLastWhiteXY(pmid.x, line, EXF_LEFT, &L_p);
			GetLastWhiteXY(pmid.x, line, EXF_RIGHT, &R_p);
			if (L_p.x <= IMAGE_LEFT || R_p.x >= IMAGE_RIGHT) break;
			mid->endpos++;
			mid->line[mid->endpos].x = (int)((L_p.x + R_p.x) >> 1);
			mid->line[mid->endpos].y = line;
		}
		NomalFilter(2, 5, LOC_CROSS,mid);
	}
	//
	if (mid->endpos <= 5 || pmid.y > IMAGE_BOTTOM - 5)//
	{
		GetSegment(botP.x, botP.y, pstop.x, pstop.y, LOC_CROSS, mid);
	}
	C_SHOT_CENTER = mid->line[mid->endpos].x;
}

//----------------------------- CROSS OUT -------------------------------//

// func 判断是否出了十字
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para white_x(const int) 全白列
// returned value
// itrue out of cross
static char Judge_CrossOut(const LINE edgel,const LINE edger, const int white_x)
{
	if (c_edge_loss == itrue)
	{
		if (edgel.endpos >= 20 && edger.endpos >= 20)//正常搜线 点数较多
		{
			return itrue;
		}
	}
	if (GetLNrangeMaxWC(white_x, EXT_CROSS_CHK_LN, 10) < Get_Road_Width(EXT_CROSS_CHK_LN) + 5)
	{
		return itrue;
	}
	return ifalse;
}
