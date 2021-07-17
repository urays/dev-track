#include "../../suprc/inc/common.h"
#include "../../suprc/inc/sup_draw.h"
#include "../../suprc/inc/sup_tlock.h"
#include "../../suprc/inc/sup_exfunc.h"

#include "../../conf-anls.h"
#include "../inc/sta_island.h"
#include "../inc/sta_basic.h"

typedef enum _IIN_SIGN_ 
{
	I_CATCH_SPOT = 3,
	I_CATCH_LOSE,

	I_DIR_LOSE, //方向
	I_DIR_LEFT,
	I_DIR_RIGHT,

	I_PASS_NULL,
	I_PASS_ONCE,
	I_PASS_TWICE,

	I_ISLAND_PRE, //位置
	I_ISLAND_LANE_IN,
	I_ISLAND_LOOP_IN,
}IIN_SIGN;//内部标识

//环岛预处理
static char Island_Identify(LINE edgel, LINE edger, const int bktype, const int lastw_y);//环岛全面识别
static char Island_PreToLaneIn(const LINE edgel, const LINE edger, const LINE mid);//状态跳跃PRE->LANE_IN

//环岛小巷
static void Lane_GetMidline(LINE edgel, LINE edger, LINE *mid);//小巷求中线
static char Island_LaneToOut(const LINE edgel, const LINE edger);//小巷出环岛判断
static char Island_LaneToLoop(void); //小巷入小环判断
static void Island_existFCT(const LINE edgel, const LINE edger, const POINT lastW);//环岛出口预测

//环岛小环
static void Sign_LoopIn(LINE edgel, LINE edger);    //小环标记
static char Island_LoopToLane(const POINT lastwhite, LINE edgel, LINE edger);//小环进小巷
static void LoopMid_ShotUp(const LINE edgel, const LINE edger, const POINT farwhite, LINE *mid);//投射求中线
static void Loop_GetMidline(const LINE edgel, const LINE edger, const POINT farwhite, LINE *mid);//小环求中线

//其它
static void RunBlackSpotCatch(const int leftc, const int rightc);//判断是否开启黑斑捕捉
static void CatchBlackSpot_UD(const int shotcolumn, int *cir_topline, int *cir_bottomline);//捕捉黑斑顶边,底边和侧边
static void BlackSpot_DIV(const int cir_top, const int cir_bott);//黑斑大小区分 确认使用参数集
static void Show_BlackSpot(const int top, const int bott);//显示捕捉到的环岛黑斑信息


static char RunIsland = I_ISLAND_PRE;
static char Island_type = I_DIR_LOSE;//环岛类型
static char RunSpotFlg = I_CATCH_LOSE;//捕捉黑斑标志
static char IntoLose_SIGN = ifalse;//入环丢线标志
static char PassLane_SIGN= I_PASS_NULL; //小巷标记
static char PassLoop_SIGN = I_PASS_NULL;//小环标记
static char Out_LoopSIGN = ifalse;//出小环标记
static char Out_laneSIGN = ifalse;//出环岛 出小巷标记
static char runFCT_SIGN = itrue;  //开启出环岛 预测标记
static char exist_FCT = I_DIR_LOSE;//环岛出口道路预测

static char I_WorkState = WAITING; //环岛状态机状态
static char Err_to_Island = ifalse;//错误进入环岛标志

static TRUST_LOCK I_IStrust = //环岛信任锁
{
	.set_trust = 5,
	.err_tolerant = 5,
	.lock = TL_LOCK_CLOSE,
	.trust_count = 0,
	.error_count = 0,
	.para_pointed = TL_POINT_NULL
};//参数由函数初始化决定

static char break_T = BAS_BK_NOT; //本地储存拐点类型
static POINT farLastW = { IMAGE_CENTER,IMAGE_BOTTOM };//投射最后一个白点
static POINT LoopbotP = { IMAGE_CENTER,IMAGE_BOTTOM };
static int spot_top = IMAGE_BOTTOM;   //黑斑顶边
static int spot_bottom = IMAGE_BOTTOM;//黑斑底边
static int spot_maxsize = 0;//黑斑最大尺寸

//------------------------ ISLAND FSM INFO RECORD -------------------------//

static int  USE_VAR = 0;//使用参数编号
static ISLAND_DATA IslandData =
{
	.set.var[0].toloop_pos = SETI_SMALL_TOLOOPPOS, //入小环位置参数    >>>>>小环默认参数
	.set.var[0].otloop_pos = SETI_SMALL_OTLOOPPOS, //出小环位置参数
	.set.var[0].toloop_sup = SETI_SMALL_TOLOOPSUP, //入小环投射中线增益
	.set.var[0].otloop_sup = SETI_SMALL_OTLOOPSUP, //出小环投射中线增益
	.set.var[0].otlane_spc = SETI_SMALL_OTLANESPC, //出小环小巷边沿平移量
	.set.var[0].otlane_nol = SETI_SMALL_OTLANENOL, //出小环小巷边沿平移量 nomal

	.set.var[1].toloop_pos = SETI_BIG_TOLOOPPOS, //入大环位置参数   >>>>>大环默认参数
	.set.var[1].otloop_pos = SETI_BIG_OTLOOPPOS, //出大环位置参数
	.set.var[1].toloop_sup = SETI_BIG_TOLOOPSUP, //入大环投射中线增益
	.set.var[1].otloop_sup = SETI_BIG_OTLOOPSUP, //出大环投射中线增益
	.set.var[1].otlane_spc = SETI_BIG_OTLANESPC, //出大环小巷边沿平移量
	.set.var[1].otlane_nol = SETI_BIG_OTLANENOL, //出大环小巷边沿平移量 nomal

	.set.part_size = SETI_PARTSIZE,  //大小环区分参数
	.set.lane_trans = SETI_LANETRANS,//小巷边沿默认平移量

	.outloop_k = 0,  //小巷最长线性边沿拟合斜率值
	.use_varset = 0, //默认使用大环参数集
	.spot_size = 0,  //黑斑尺寸
	.lastwhite_y = IMAGE_BOTTOM,
	.spot_top_y = IMAGE_BOTTOM,
	.spot_bottom_y = IMAGE_BOTTOM,
	.spot_advpos_y = IMAGE_BOTTOM,
	._std_wline = 0,//入环全白行测试
};

ISLAND_DATA* GetIslandAddr(void){return (&IslandData);}
static void Island_Data_Record(void)//环岛状态机信息记录
{
	IslandData.lastwhite_y = farLastW.y;
	IslandData.spot_bottom_y = spot_bottom;
	IslandData.spot_top_y = spot_top;
	IslandData.spot_advpos_y = (int)((spot_top + spot_bottom) >> 1);
	IslandData.spot_size = spot_maxsize;//spot_bottom - spot_top;
	IslandData.use_varset = USE_VAR;
}

//------------------------------ ISLAND FSM -------------------------------//

// func 环岛独立状态机
// para edgel(const LINE) 基础左边沿
// para edger(const LINE) 基础右边沿
// get mid(LINE*) 偏差中线
void Island_FSM(const LINE edgel, const LINE edger, LINE* mid)
{
	farLastW.x = IMAGE_CENTER;//初始化
	farLastW.y = IMAGE_BOTTOM;
	spot_top = IMAGE_BOTTOM;
	spot_bottom = IMAGE_BOTTOM;
	Err_to_Island = ifalse;

	if (I_WorkState == WAITING)
	{
		enTrustLock(5, 5, &I_IStrust);//启用并初始化 环岛信任锁
		break_T = GetBasicBreak();//初次进入状态机 角点状态获取
	}
	//动态投射列
	int shot_center = (int)((edgel.line[0].x + edger.line[0].x) >> 1), shot_count;
	if (shot_center > IMAGE_CENTER) shot_count = (int)(-(shot_center << 2) / 5.) + 96;
	else if (shot_center <= IMAGE_CENTER) shot_count = (int)((shot_center << 2) / 5.);
	FullLine_ShotUpF(shot_center, IMAGE_BOTTOM, shot_count, &farLastW);//投射求最远白点
	PreviewPoint(farLastW, 2);
	//
	switch (RunIsland)
	{
	case(I_ISLAND_PRE): {//---------------------------------环岛前预处理
		I_WorkState = RUNNING;
		char be_island = Island_Identify(edgel, edger, break_T, farLastW.y);//环岛全面识别
		TrustLock(ifalse, itrue, &I_IStrust, &be_island);  //环岛识别信任

		if (be_island == ifalse)//判断是否误入环岛
		{
			IFSM_Init(WAITING);//退出初始化状态机
			Err_to_Island = itrue;
			break;
		}
		Lane_GetMidline(edgel, edger, mid);//求小巷中线
		if (RunSpotFlg == I_CATCH_LOSE)
		{
			RunBlackSpotCatch(edgel.endpos, edger.endpos);//判断是否开启捕捉黑斑
		}
		if (Island_PreToLaneIn(edgel, edger, *mid) == itrue 
			&& RunSpotFlg == I_CATCH_SPOT)//Pre->Lane
		{
			RunIsland = I_ISLAND_LANE_IN;
			IntoLose_SIGN = ifalse; //初始化丢线标志
			PassLane_SIGN = I_PASS_ONCE;//标记第一次经过小巷
			PassLoop_SIGN = I_PASS_NULL;//清除小环标记
		}
	}break;
	case(I_ISLAND_LANE_IN): {//----------------------------------小巷处理
		I_WorkState = RUNNING;
		Lane_GetMidline(edgel, edger, mid);//求小巷中线

		if (PassLane_SIGN == I_PASS_ONCE)
		{
			CatchBlackSpot_UD(farLastW.x, &spot_top, &spot_bottom);//捕捉环岛黑斑
			BlackSpot_DIV(spot_top, spot_bottom); //区分黑斑尺寸 确认使用参数集
			Show_BlackSpot(spot_top, spot_bottom);//显示捕捉到的环岛黑斑信息

			if (Island_LaneToLoop() == itrue)//Lane->Loop
			{
				RunIsland = I_ISLAND_LOOP_IN;
				Out_LoopSIGN = ifalse;
				runFCT_SIGN = itrue;
			}
		}
		else if (PassLane_SIGN == I_PASS_TWICE)
		{
			if (Island_LaneToOut(edgel, edger) == itrue) //Lane->Out
			{
				IFSM_Init(WAITING);//退出初始化状态机
			}
		}
	}break;
	case(I_ISLAND_LOOP_IN): {//--------------------------------------小环处理
		I_WorkState = RUNNING;
		Loop_GetMidline(edgel, edger, farLastW, mid);//求小环中线
		Sign_LoopIn(edgel, edger);      //标记小环
		Island_existFCT(edgel, edger, farLastW);//环岛出口路段预测

#ifndef _ISLAND_LOOP_TEST//小环参数测试
		if (PassLoop_SIGN == I_PASS_ONCE)//状态切换
		{
			if (Island_LoopToLane(farLastW, edgel, edger) == itrue)//Loop->Lane
			{
				RunIsland = I_ISLAND_LANE_IN;
				PassLane_SIGN = I_PASS_TWICE;//标记第二次经过小巷
				Out_laneSIGN = ifalse;
			}
		}
#endif
	}break;
	}
	Island_Data_Record();//信息记录
}

// func 状态机初始化
// para init(const char) 初始化状态
// init：
//    WAITING - 退出初始化
//    RUNNING - 不退出初始化
void IFSM_Init(const char init)
{
	I_WorkState = init;

	RunIsland = I_ISLAND_PRE;
	PassLane_SIGN = I_PASS_NULL;
	PassLoop_SIGN = I_PASS_NULL;
	IntoLose_SIGN = ifalse;
	Island_type = I_DIR_LOSE;
	RunSpotFlg = I_CATCH_LOSE;
	exist_FCT = I_DIR_LOSE;
	LoopbotP.x = IMAGE_CENTER;
	LoopbotP.y = IMAGE_BOTTOM;
	Out_LoopSIGN = ifalse;
	Out_laneSIGN = ifalse;
	runFCT_SIGN = itrue;
	spot_maxsize = 0;//黑斑最大尺寸
	USE_VAR = 0;  //使用参数集编号
	break_T = BAS_BK_NOT;
}

// func 返回环岛状态机状态
char Get_IFSMState(void)
{
	return I_WorkState;
}

// func 判断是否错误识别环岛
char Goto_IFSM_Err(void)
{
	return Err_to_Island;
}

// func 获取环岛中具体位置
unsigned char GetIslandPlace(void)
{
	unsigned char ret = ISLAND_UNKNOW;
	if (Island_type != I_DIR_LOSE)
	{
		switch (RunIsland) {
		case(I_ISLAND_PRE):ret = (Island_type == I_DIR_LEFT) ? ISLAND_PRE_L : ISLAND_PRE_R; break;
		case(I_ISLAND_LANE_IN):ret = (Island_type == I_DIR_LEFT) ? ISLAND_LANE_L : ISLAND_LANE_R; break;
		case(I_ISLAND_LOOP_IN):ret = (Island_type == I_DIR_LEFT) ? ISLAND_LOOP_L : ISLAND_LOOP_R; break;
		}
	}
	return ret;
}

//------------------------------ ISLAND PRE -------------------------------//

// func 环岛完全识别
// para edgel(const LINE edgel) 左边沿
// para edger(const LINE edger) 右边沿
// para breaktype(const int) 拐点方向
// return value
// itrue-is island
// ifalse-is not island
static char Island_Identify(LINE edgel, LINE edger, const int bktype, const int lastw_y)
{
	float beem = 0.0f;
	LSQ_INFO lsq;
	int lrx_di = 0, lry_di = 0;
	int t;

	//最远白点检查   用于区分弯道
	if (lastw_y >= 12) { return ifalse; }
	//底部边沿行值检查   至少有一边要靠近底边
	if (edgel.line[0].y < IMAGE_BOTTOM - 1 && edger.line[0].y < IMAGE_BOTTOM - 1) { return ifalse; }

	if (bktype == BAS_BK_LEFT)//--------------------------------------------左环岛
	{
		Island_type = I_DIR_LEFT;

		//右边沿直道检查
		if (CURVE == LineLinear(&edger, 0, edger.endpos - 3, &lsq, &beem)) { return ifalse; }
		if (RunSpotFlg == I_CATCH_LOSE)//
		{
			for (t = edger.endpos - 2; t > 0; t--)
			{
				if (_iexc(edger.line[t].x, IMAGE_LEFT + 1, IMAGE_RIGHT - 1)) { break; }
				if (_iiabs(edger.line[t].x - edger.line[edger.endpos].x) <= 1) { return ifalse; }
			}
		}
		//左右边沿行差应大于某一值
		lry_di = edgel.line[edgel.endpos].y - edger.line[edger.endpos].y;
		//较长边点数应大于某一值
		if (edger.endpos < 25) {return ifalse; }
	}
	else if (bktype == BAS_BK_RIGHT)//--------------------------------------右环岛
	{
		Island_type = I_DIR_RIGHT;
		//右边沿直道检查
		if (CURVE == LineLinear(&edgel, 0, edgel.endpos - 3, &lsq, &beem)) { return ifalse; }
		if (RunSpotFlg == I_CATCH_LOSE)
		{
			for (t = edgel.endpos - 2; t > 0; t--)
			{
				if (_iexc(edgel.line[t].x, IMAGE_LEFT + 1, IMAGE_RIGHT - 1)) { break; }
				if (_iiabs(edgel.line[t].x - edgel.line[edgel.endpos].x) <= 1) { return ifalse; }
			}
		}
		//左右边沿行差应大于某一值
		lry_di = edger.line[edger.endpos].y - edgel.line[edgel.endpos].y;
		//较长边点数应大于某一值
		if (edgel.endpos < 25) { return ifalse; }
	}
	lrx_di = edger.line[edger.endpos].x - edgel.line[edgel.endpos].x;

	if (bktype == BAS_BK_BOTH //出现双拐点
		|| bktype == BAS_BK_NOT  //拐点容易出现闪烁
		|| lrx_di < 5  //左右边沿列差需大于某一值
		|| lry_di < 20)//左右边沿行差应大于某一值
	{
		return ifalse;
	}
	return itrue;
}

#define CHECK_WHITEC   (5)//底部白行检测行数

// func 状态切换判断 PRE->LANE_IN  扫描底下全白行
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para mid(const LINE) 中线信息
// returned value
// ifalse- cannot go to lane in
// itrue- go to lane in
static char Island_PreToLaneIn(const LINE edgel,const LINE edger,const LINE mid)
{
	int t = 0;                    //-Local
	POINT start = mid.line[0];//-Local
	POINT end = mid.line[0];  //-Local
	POINT LAST = mid.line[0];

	if (Island_type == I_DIR_LEFT)
	{ 
		end.x = IMAGE_LEFT + 1;
		if (edgel.endpos <= 1 && edgel.line[0].y >= IMAGE_BOTTOM)
		{
			IntoLose_SIGN = itrue;
		}
	}
	else if (Island_type == I_DIR_RIGHT)
	{
		end.x = IMAGE_RIGHT - 1;
		if (edger.endpos <= 1 && edger.line[0].y >= IMAGE_BOTTOM)
		{
			IntoLose_SIGN = itrue;
		}
	}
	if (IntoLose_SIGN == itrue)
	{
		for (t = 0; t < CHECK_WHITEC; t++)//5
		{
			start = mid.line[t];
			end.y = mid.line[t].y;
			if (LN_allwhite(start.x, start.y, end.x, end.y, &LAST) == ifalse)
			{
				return ifalse;
			}
		}
		return itrue;
	}
	return ifalse;
}

//----------------------------- ISLAND LANE IN -----------------------------//

// func 小巷出环岛判断
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// returned value
// ifalse 在小巷
// itrue 出环岛吧
static char Island_LaneToOut(const LINE edgel, const LINE edger)
{
	char beark_T = GetBasicBreak();//-Local

	if (beark_T == BAS_BK_BOTH && edgel.endpos >= 10
		&& edger.endpos >= 10) { //出现了双拐点 前方可能十字
		return itrue;
	}
	if (Island_type == I_DIR_LEFT)
	{
		if (edgel.endpos <= 1) Out_laneSIGN = itrue;//点数较少
		if (Out_laneSIGN == itrue)
		{
			if (edgel.endpos >= 10 && edger.endpos >= 10)//正常搜线 点数较多
			{
				return itrue;
			}
		}
	}
	else if (Island_type == I_DIR_RIGHT)
	{
		if (edger.endpos <= 1) Out_laneSIGN = itrue;
		if (Out_laneSIGN == itrue)
		{
			if (edgel.endpos >= 10 && edger.endpos >= 10)
			{
				return itrue;
			}
		}
	}
	return ifalse;
}

// func 小巷入小环判断
// returned value
// ifalse 在小巷
// itrue 进入环岛小环
static char Island_LaneToLoop(void)
{
	int spot_advpos = IMAGE_BOTTOM;//-Local

	spot_advpos = (int)((spot_bottom + spot_top) >> 1);
	if (spot_top > BLACKSPOT_TOP
		&&spot_bottom - spot_top >= 5 && spot_advpos < IMAGE_BOTTOM//状态切换
		&& spot_advpos > IslandData.set.var[USE_VAR].toloop_pos)//黑块位置
	{
		return itrue;
	}
	return ifalse;
}

#define LEVEL_MID_RANGE_1    (15)
#define LEVEL_MID_RANGE_2    (-5)

// func 小巷平移边沿获取中线
// para edgel(LINE edgel) 左边沿
// para edger(LINE edger) 右边沿
// para *mid(LINE) 中线信息
static void Lane_GetMidline(LINE edgel, LINE edger, LINE *mid)
{
	LSQ_INFO lsq;     //-Local
	int tmp_trans = 0;

	int i, j;
	for (i = 0, j = 0; i <= edgel.endpos && j <= edger.endpos;)
	{
		if (edgel.line[i].y > edger.line[j].y) i++;
		else if (edgel.line[i].y < edger.line[j].y) j++;
		else { break; }
	}
	int bot_x;
	if (edgel.endpos <= 1 || edger.endpos <= 1)//非常极端的情况
	{
		bot_x = IMAGE_CENTER;
	}
	else {
		bot_x = (int)((edgel.line[i].x + edger.line[j].x) >> 1);
	}

	mid->local = LOC_ISLAND;
	if (Island_type == I_DIR_LEFT)
	{
		if (edger.endpos > 30)
		{
			LineInit(mid, LOC_ISLAND);
			if (edgel.endpos > 5 && PassLane_SIGN == I_PASS_NULL
				&& (bot_x - IMAGE_CENTER <LEVEL_MID_RANGE_1 && bot_x - IMAGE_CENTER> LEVEL_MID_RANGE_2))//点数较多时
			{
				CalcMidBasic_Lv(edgel, edger, LOC_ISLAND, mid);
				LSQ_algorithm(mid, 0, mid->endpos, &lsq);
				GetFilterLine(&lsq, LOC_ISLAND, mid, itrue);
			}
			else {//点数较少时
				*mid = edger;
				LSQ_algorithm(&edger, 0, edger.endpos, &lsq);//获取右边沿斜率
				if (exist_FCT == I_DIR_LEFT && PassLane_SIGN == I_PASS_TWICE)
				{
					tmp_trans = -IslandData.set.var[USE_VAR].otlane_spc;
				}
				else if (PassLoop_SIGN == I_PASS_ONCE)
				{
					tmp_trans = -IslandData.set.var[USE_VAR].otlane_nol;
				}
				else {
					tmp_trans = -IslandData.set.lane_trans;
				}
				LineTrans(tmp_trans, lsq.k, LOC_ISLAND, mid, itrue);//拟合中线
			}
		}
		else {
			GetSegment(IMAGE_CENTER, IMAGE_BOTTOM, farLastW.x, farLastW.y, LOC_ISLAND, mid);
		}
	}
	else if (Island_type == I_DIR_RIGHT)
	{
		if (edgel.endpos > 30)
		{
			LineInit(mid, LOC_ISLAND);
			if (edger.endpos > 5 && PassLane_SIGN == I_PASS_NULL
				&& (IMAGE_CENTER - bot_x <LEVEL_MID_RANGE_1 && IMAGE_CENTER - bot_x> LEVEL_MID_RANGE_2))//点数较多时
			{
				CalcMidBasic_Lv(edgel, edger, LOC_ISLAND, mid);
				LSQ_algorithm(mid, 0, mid->endpos, &lsq);
				GetFilterLine(&lsq, LOC_ISLAND, mid, itrue);
			}
			else {//点数较少时
				*mid = edgel;
				LSQ_algorithm(&edgel, 0, edgel.endpos, &lsq);//获取左边沿斜率
				if (exist_FCT == I_DIR_RIGHT && PassLane_SIGN == I_PASS_TWICE)
				{
					tmp_trans = IslandData.set.var[USE_VAR].otlane_spc;
				}
				else if (PassLoop_SIGN == I_PASS_ONCE)
				{
					tmp_trans = IslandData.set.var[USE_VAR].otlane_nol;
				}
				else {
					tmp_trans = IslandData.set.lane_trans;
				}
				LineTrans(tmp_trans, lsq.k, LOC_ISLAND, mid, itrue);//拟合中线
			}
		}
		else {
			GetSegment(IMAGE_CENTER, IMAGE_BOTTOM, farLastW.x, farLastW.y, LOC_ISLAND, mid);
		}
	}
}

// func 环岛出口道路预测
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para lastW(const POINT) 最后一个白点
static void Island_existFCT(const LINE edgel, const LINE edger, const POINT lastW)
{
	if (exist_FCT == I_DIR_LOSE && lastW.y >= EXIST_FCT_FAR)
	{
		if ((Island_type == I_DIR_LEFT && edger.endpos <= 1) //边沿消失
			|| (Island_type == I_DIR_RIGHT && edgel.endpos <= 1))
		{
			runFCT_SIGN = ifalse;
		}
		if (runFCT_SIGN == itrue && PassLoop_SIGN == I_PASS_NULL)
		{
			int line = 0;
			POINT R_p, L_p;
			int saveL = IMAGE_RIGHT;
			int saveR = IMAGE_LEFT;
			for (line = lastW.y + 1; line <= IMAGE_BOTTOM - 20; line++)
			{
				GetLastWhiteXY(lastW.x, line, EXF_LEFT, &L_p);
				GetLastWhiteXY(lastW.x, line, EXF_RIGHT, &R_p);
				if (L_p.x <= IMAGE_LEFT + 1 || L_p.x - saveL > 10)
				{
					if (L_p.x - saveL > 15)//相差很大说明是大弯
					{
						exist_FCT = I_DIR_LEFT;
					}
					break;
				}
				if (R_p.x >= IMAGE_RIGHT - 1 || saveR - R_p.x > 10)
				{
					if (saveR - R_p.x > 15)//相差很大说明是大弯
					{
						exist_FCT = I_DIR_RIGHT;
					}
					break;
				}
				if (saveL > L_p.x) saveL = L_p.x;
				if (saveR < R_p.x) saveR = R_p.x;
			}
		}
	}
}

//---------------------------- ISLAND LOOP IN -----------------------------//

// func 标记小环中状态
// para edgel(LINE*) 左边沿
// para edger(LINE*) 右边沿
static void Sign_LoopIn(LINE edgel, LINE edger)
{
	char line_T = LINEAR;
	float linear = 0.0f;
	LSQ_INFO lsq;    

	static char loopCon_1 = ifalse; //-Local
	static char loopCon_2 = ifalse; //-Local

	if (PassLoop_SIGN == I_PASS_NULL)
	{
		if (Island_type == I_DIR_LEFT)
		{
			line_T = LineLinear(&edger, 0, edger.endpos, &lsq, &linear);
			if (edger.line[edger.endpos].y < IMAGE_TOP + 13)
			{
				line_T = LINEAR;
			}
			if (loopCon_1 == itrue  && edger.endpos > 35  //先行条件
				&& edger.line[edger.endpos].x < IMAGE_CENTER - 10)
			{
				loopCon_2 = itrue;
			}
			if (edger.endpos <= 1) loopCon_1 = itrue;//右边沿点数较少
		}
		else if (Island_type == I_DIR_RIGHT)
		{
			line_T = LineLinear(&edgel, 0, edgel.endpos, &lsq, &linear);
			if (edgel.line[edgel.endpos].y < IMAGE_TOP + 13)
			{
				line_T = LINEAR;
			}
			if (loopCon_1 == itrue && edgel.endpos > 35//先行条件
				&& edgel.line[edgel.endpos].x > IMAGE_CENTER + 10)
			{
				loopCon_2 = itrue;
			}
			if (edgel.endpos <= 1) loopCon_1 = itrue;//左边沿点数较少
		}
	}

	if (line_T == CURVE && loopCon_2 == itrue)
	{
		PassLoop_SIGN = I_PASS_ONCE;
		loopCon_1 = ifalse;
		loopCon_2 = ifalse;
	}
}

#define LASTWHITE_STD_Y   (10)   //最后一个白点最近标准 <=LASTWHITE_STDLINE LOOP to LANE

// func 小环进小巷判断
// para farwhite(const POINT) 最后一个白点
// para edgel(const LINE edgel) 左边沿
// para edger(const LINE edger) 右边沿
static char Island_LoopToLane(const POINT farwhite, LINE edgel, LINE edger)
{
	char line_T = CURVE;//-Local
	LSQ_INFO lsq;       //-Local
	float linear = 0.0f;//-Local

	if (Island_type == I_DIR_LEFT)
	{
		line_T = LineLinear(&edger, 0, edger.endpos - 3, &lsq, &linear);
		IslandData.outloop_k = _iiabs((int)(lsq.k * 50.0f));
		if (IslandData.outloop_k == 0) { Out_LoopSIGN = itrue; }
		if (Out_LoopSIGN == itrue)
		{
			if (farwhite.y <= LASTWHITE_STD_Y) return itrue;
			if (edger.endpos >= 30 && line_T == LINEAR //右边沿点数较多
				&& _iinc(IslandData.outloop_k, 3, IslandData.set.var[USE_VAR].otloop_pos))
			{
				return itrue;
			}
		}
	}
	else if (Island_type == I_DIR_RIGHT)
	{
		line_T = LineLinear(&edgel, 0, edgel.endpos - 3, &lsq, &linear);
		IslandData.outloop_k = _iiabs((int)(lsq.k * 50.0f));
		if (IslandData.outloop_k == 0) { Out_LoopSIGN = itrue; }
		if (Out_LoopSIGN == itrue)
		{
			if (farwhite.y <= LASTWHITE_STD_Y) return itrue;
			if (edgel.endpos >= 30 && line_T == LINEAR //左边沿点数较多
				&& _iinc(IslandData.outloop_k, 3, IslandData.set.var[USE_VAR].otloop_pos))
			{
				return itrue;
			}
		}
	}
	return ifalse;
}

// func 环岛小环投射获取中线
// para edgel(const LINE) 基础左边沿
// para edger(const LINE*) 基础右边沿
// para farwhite(const POINT) 投射最远白点
// get *mid(LINE) 中线信息
static void Loop_GetMidline(const LINE edgel, const LINE edger, const POINT farwhite, LINE *mid)
{
	if (((Island_type == I_DIR_LEFT && edger.line[edger.endpos].x < IMAGE_CENTER)
		|| (Island_type == I_DIR_RIGHT && edgel.line[edgel.endpos].x > IMAGE_CENTER))
		&& PassLoop_SIGN == I_PASS_ONCE)
	{
		CalcMidBasic_Pp(edgel, edger, 0.52f, LOC_ISLAND, mid);//比例计算中线
		if (mid->endpos <= 1)
		{
			LoopbotP.x = IMAGE_CENTER;
			LoopbotP.y = IMAGE_BOTTOM;
		}
		else {
			LoopbotP = mid->line[0];//record bottom point
		}
	}
	else if (((Island_type == I_DIR_LEFT && edgel.endpos > 30)//大环补线求中线
		|| (Island_type == I_DIR_RIGHT && edger.endpos > 30)) && USE_VAR == 1)
	{
		if (Island_type == I_DIR_LEFT)
		{
			Mid_Repair_ByL(edgel, 0, LOC_ISLAND, mid);
		}
		else if (Island_type == I_DIR_RIGHT)
		{
			Mid_Repair_ByR(edger, 0, LOC_ISLAND, mid);
		}
	}
	else
	{
		LoopMid_ShotUp(edgel, edger, farwhite, mid);//小巷入小环 投射中线
	}
}

// func 小巷到小环 投射中线
// para edgel(const LINE) 左边沿
// para edger(const LINE) 右边沿
// para farwhite(const POINT) 最后一个白点
// get mid(LINE*) 中线
static void LoopMid_ShotUp(const LINE edgel, const LINE edger, const POINT farwhite, LINE *mid)
{
	int line, last_x;
	int  maxline;
	POINT lastP, saveP = { IMAGE_CENTER,IMAGE_BOTTOM };
	POINT lastW = { IMAGE_CENTER,30 };

	if (Island_type == I_DIR_LEFT)
	{
		saveP.x = IMAGE_LEFT;
		saveP.y = IMAGE_BOTTOM;
		last_x = IMAGE_RIGHT;
		maxline = _imax(edgel.line[edgel.endpos].y, farwhite.y);
		for (line = IMAGE_BOTTOM - 1; line >= maxline; line--)//edgel.line[0].y
		{
			GetLastWhiteXY(farwhite.x, line, EXF_LEFT, &lastP);
			if (last_x != IMAGE_RIGHT && _iiabs(last_x - lastP.x) > 5) break;
			if (lastP.x >= saveP.x)
			{
				saveP = lastP;
			}
			last_x = lastP.x;
		}
		GetLastWhiteXY(saveP.x + 3, saveP.y, EXF_UP, &lastW);
	}
	else if (Island_type == I_DIR_RIGHT)
	{
		saveP.x = IMAGE_RIGHT;
		saveP.y = IMAGE_BOTTOM;
		last_x = IMAGE_LEFT;
		maxline = _imax(edger.line[edger.endpos].y, farwhite.y);
		for (line = IMAGE_BOTTOM - 1; line >= maxline; line--)//edger.line[0].y
		{
			GetLastWhiteXY(farwhite.x, line, EXF_RIGHT, &lastP);
			if (last_x != IMAGE_LEFT && _iiabs(last_x - lastP.x) > 5) break;
			if (lastP.x <= saveP.x)
			{
				saveP = lastP;
			}
			last_x = lastP.x;
		}
		GetLastWhiteXY(saveP.x - 3, saveP.y, EXF_UP, &lastW);
	}
	POINT stopP = lastW;
	if (PassLoop_SIGN == I_PASS_NULL)
	{
		if (lastW.x > IMAGE_CENTER)
		{
			stopP.x = lastW.x + IslandData.set.var[USE_VAR].toloop_sup;
			stopP.y = lastW.y + IslandData.set.var[USE_VAR].toloop_sup;
		}
		else if (lastW.x < IMAGE_CENTER)
		{
			stopP.x = lastW.x - IslandData.set.var[USE_VAR].toloop_sup;
			stopP.y = lastW.y + IslandData.set.var[USE_VAR].toloop_sup;
		}
	}
	else if (PassLoop_SIGN == I_PASS_ONCE)
	{
		if (lastW.x > IMAGE_CENTER)
		{
			stopP.x = lastW.x + IslandData.set.var[USE_VAR].otloop_sup;
			stopP.y = lastW.y + IslandData.set.var[USE_VAR].otloop_sup;
		}
		else if (lastW.x < IMAGE_CENTER)
		{
			stopP.x = lastW.x - IslandData.set.var[USE_VAR].otloop_sup;
			stopP.y = lastW.y + IslandData.set.var[USE_VAR].otloop_sup;
		}
	}
	GetSegment(LoopbotP.x, LoopbotP.y, stopP.x, stopP.y, LOC_ISLAND, mid);//计算中线
	//PreviewPoint(saveP, 3);
	//PreviewPointX(lastW, 3);
}

//--------------------------- CATCH BLACK SPOT ----------------------------//

// func 判断是否开启黑斑捕捉
// para leftc(const int) 基础左边沿点数
// para rightc(const int) 基础右边沿点数
static void RunBlackSpotCatch(const int leftc, const int rightc)
{
	int line, c_dir = EXF_BOTH;
	int save_white_c = 0, tmp_white_c = 0;
	int stdw_LN = IMAGE_TOP;
	int rise_count = 0, riseflg = 0;

	if (Island_type == I_DIR_LEFT) c_dir = EXF_LEFT;
	else if (Island_type == I_DIR_RIGHT) c_dir = EXF_RIGHT;

	for (line = IMAGE_BOTTOM; line > ERROR_LINE; line--)
	{
		tmp_white_c = WhiteCount_LR(c_dir, IMAGE_CENTER, line);

		if (save_white_c > tmp_white_c && _iiabs(save_white_c - tmp_white_c) <= 2)
		{
			rise_count++;
		}
		if (rise_count >= 3) riseflg = 1;

		if ((riseflg == 1 && tmp_white_c - save_white_c > 2 && save_white_c != 0)
			|| (c_dir == EXF_LEFT && leftc <= 1) || (c_dir == EXF_RIGHT && rightc <= 1))
		{
			stdw_LN = line;
			break;
		}
		if (save_white_c>tmp_white_c || save_white_c == 0) save_white_c = tmp_white_c;
	}
	if (c_dir == EXF_LEFT) {
		PreviewLabLine(IMAGE_LEFT, stdw_LN, 30);
	}
	else if (c_dir == EXF_RIGHT) {
		PreviewLabLine(IMAGE_RIGHT, stdw_LN, 30);
	}
	else if (c_dir == EXF_BOTH) {
		PreviewLabLine(IMAGE_CENTER, stdw_LN, 30);
	}

#ifndef _PRE_WLN_TEST_//入环全白行测试
	if (stdw_LN >= INTO_FULL_WLN)//42?
	{
		RunSpotFlg = I_CATCH_SPOT;
	}
#else
	IslandData._std_wline = stdw_LN;
#endif
}

//@func 捕捉黑色圆形板块顶边,底边信息
// para shotcolumn(const int) 起始投射列
// get *cir_topline(int) 黑斑显示的最上一行
// get *cir_bottomline(int) 黑斑显示最下一行
// call it after calling (func)AllLine_ShotUp
static void CatchBlackSpot_UD(const int shotcolumn, int *cir_topline, int *cir_bottomline)
{
	int count_dir = EXF_BOTH;
	int line, column, end_count = 0;
	int now_count = 0, last_count = 0;
	int max_count = 0;
	POINT LAST;

	*cir_topline = IMAGE_BOTTOM;//初始化
	*cir_bottomline = IMAGE_BOTTOM;

	if (Island_type == I_DIR_LEFT) count_dir = EXF_LEFT;
	else if (Island_type == I_DIR_RIGHT) count_dir = EXF_RIGHT;

	for (line = BLACKSPOT_TOP; line <= IMAGE_BOTTOM; line++)
	{
		now_count = WhiteCount_LR(count_dir, shotcolumn, line);
		if (now_count >= max_count)
		{
			max_count = now_count;
			*cir_topline = line;
			end_count = 0;
		}
		else if (max_count - now_count >= 3)
		{
			if (last_count <= now_count)
				end_count++;
		}
		if (end_count >= 2) break;
		last_count = now_count;
	}
	*cir_bottomline = *cir_topline;
	if (Island_type == I_DIR_LEFT)
	{
		for (column = BLACKSPOT_LEFT; column <shotcolumn; column++)
		{
			if (LN_allwhite(column, *cir_topline, column, IMAGE_BOTTOM, &LAST) == itrue
				&& column > IMAGE_CENTER - 30)
			{
				break;
			}
			for (line = *cir_topline; line <= IMAGE_BOTTOM; line++)
			{
				if (!POINT_WHITE(column, line) && POINT_WHITE(column, line + 1)
					|| (!POINT_WHITE(column, line) && line >= IMAGE_BOTTOM - 1))
				{
					if (*cir_bottomline <= line + 1)
					{
						*cir_bottomline = line + 1;
					}
					break;
				}
			}
			if (*cir_bottomline >= IMAGE_BOTTOM)
			{
				*cir_bottomline = IMAGE_BOTTOM;
				break;
			}
		}
	}
	else if (Island_type == I_DIR_RIGHT)
	{
		for (column = BLACKSPOT_RIGHT; column >shotcolumn; column--)
		{
			if (LN_allwhite(column, *cir_topline, column, IMAGE_BOTTOM, &LAST) == itrue 
				&& column < IMAGE_CENTER + 30)
			{
				break;
			}
			for (line = *cir_topline; line <= IMAGE_BOTTOM; line++)
			{
				if (!POINT_WHITE(column, line) && POINT_WHITE(column, line + 1)
					|| (!POINT_WHITE(column, line) && line >= IMAGE_BOTTOM - 1))
				{
					if (*cir_bottomline <= line + 1)
					{
						*cir_bottomline = line + 1;
					}
					break;
				}
			}
			if (*cir_bottomline >= IMAGE_BOTTOM)
			{
				*cir_bottomline = IMAGE_BOTTOM;
				break;
			}
		}
	}
}

// func 环岛黑斑大小区分  确定使用的参数集
// para cir_top(const int) 黑斑顶边行
// para cir_bott(const int) 黑斑底边行
static void BlackSpot_DIV(const int cir_top, const int cir_bott)
{
	if (spot_maxsize < spot_bottom - spot_top)//获取黑斑最大尺寸
	{
		spot_maxsize = spot_bottom - spot_top;
	}
	if (spot_maxsize > IslandData.set.part_size && USE_VAR == 0)//36
	{
		USE_VAR = 1;
	}
#ifndef __SMALL_BIG_
#ifdef __ONLY_SMALL_
	USE_VAR = 0; //小环岛
#else
#ifdef __ONLY_BIG_
	USE_VAR = 1; //大环岛
#endif
#endif
#endif
}

// func 显示捕捉到的环岛黑斑信息
// para top(const int) 黑斑顶边行
// para bott(const int) 黑斑底边行
static void Show_BlackSpot(const int top, const int bott)
{
	char begin_x = IMAGE_CENTER;//-Local

	if (Island_type == I_DIR_LEFT)
	{
		begin_x = IMAGE_LEFT;
	}
	else if (Island_type == I_DIR_RIGHT)
	{
		begin_x = IMAGE_RIGHT;
	}
	PreviewLabLine(begin_x, top, 60);
	PreviewLabLine(begin_x, bott, 60);
}
