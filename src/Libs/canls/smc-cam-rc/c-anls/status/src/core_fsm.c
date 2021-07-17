#include "../../suprc/inc/common.h"
#include "../../suprc/inc/sup_draw.h"

#include "../../conf-anls.h"
#include "../inc/core_fsm.h"

#include "../inc/sta_basic.h"
#include "../inc/sta_block.h"
#include "../inc/sta_cross.h"
#include "../inc/sta_island.h"

static void ShiftState(void);             //状态切换
static void HandleState(LINE* edgel, LINE* edger, LINE* mid);   //状态处理
static void to_state(unsigned char state);//切换状态函数

static unsigned char RunSta = FSM_BASIC;

// func 核心状态机
// para mid(LINE*) 输出中线
unsigned char __fsm(LINE* edgel, LINE* edger, LINE* mid)
{
	anlsImgCLS();//初始化分析图像

	BasicDeal_MD(edgel, edger);//基础-横向
	BasicDeal_BK(edgel, edger);//基础-拐点
	BasicDeal_TD(edgel, edger);//基础-纵向
	BasicDeal_BS(edgel, edger);  //基础-弯直

	ShiftState();         //状态切换 
	HandleState(edgel, edger, mid); //处理状态

	PreviewLine(*edgel, 0, (*edgel).endpos);    //显示左边沿
	PreviewLine(*edger, 0, (*edger).endpos);    //显示右边沿

	return RunSta;//返回状态机状态
}

// func 刷新核心状态机
void fresh_fsm(void)
{
	RunSta = FSM_BASIC; //初始化核心状态

	CFSM_Init(WAITING); //十字状态机初始化
	IFSM_Init(WAITING); //环岛状态机初始化
	BFSM_Init(WAITING); //障碍状态机初始化
}

// 切换
static void ShiftState(void)/////状态切换
{
	switch (RunSta)  //一次识别 主状态机外部识别
	{
	case(FSM_BASIC): {
		char basic_bk = GetBasicBreak();//basic break point type
		char basic_pa = GetBasicPlace();//basic path type

		if (basic_bk == BAS_BK_BOTH) to_state(FSM_CROSS);  //十字
		else if (basic_bk == BAS_BK_LEFT) to_state(FSM_ISLAND);//环岛
		else if (basic_bk == BAS_BK_RIGHT) to_state(FSM_ISLAND);//环岛
		else if (basic_bk == BAS_BK_NOT &&
			(basic_pa == BAS_STRAIGHT || basic_pa == BAS_ZEBRA)
			//&& edgel.line[0].y >= IMAGE_BOTTOM
			//&&edger.line[0].y >= IMAGE_BOTTOM
			) to_state(FSM_BLOCK);//障碍
		else {
			to_state(FSM_BASIC);//基础
		}
	}break;
	case(FSM_CROSS) : {
		if (Get_CFSMState() == WAITING) to_state(FSM_BASIC);
	}break;
	case(FSM_ISLAND) : {
		if (Get_IFSMState() == WAITING) to_state(FSM_BASIC);
	}break;
	case(FSM_BLOCK) : {
		if (Get_BFSMState() == WAITING) to_state(FSM_BASIC);
	}break;
	default:break;
	}
  }

// 处理
static void HandleState(LINE* edgel, LINE* edger, LINE *mid)
{
	char ErrSta_Flg = ifalse;//state correction identifier

	switch (RunSta)/////状态处理
	{
	case(FSM_BASIC): {
		CalcMidBasic(*edgel, *edger, 0.5f, LOC_BASIC, mid);//基础处理
	}break;
	case(FSM_CROSS): {
		Cross_FSM(*edgel, *edger, mid);//十字处理
	}break;
	case(FSM_ISLAND): {
		Island_FSM(*edgel, *edger, mid);//环岛处理
		ErrSta_Flg = Goto_IFSM_Err();
	}break;
	case(FSM_BLOCK) : {
		Block_FSM(*edgel, *edger, mid);//障碍处理
		ErrSta_Flg = Goto_BFSM_Err();
	}break;
	default:break;
	}

	if (ErrSta_Flg == itrue)//二次识别 独立状态机内部识别
	{
		CalcMidBasic(*edgel, *edger, 0.5f, LOC_BASIC, mid);//基础处理
		to_state(FSM_BASIC);
	}
}

// 状态转换
static void to_state(unsigned char state)
{
	if (state != RunSta)
	{
		RunSta = state;
	}
}
