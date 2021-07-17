#include "../conf-anls.h"
#include "../extro-anls.h"

#include "../suprc/inc/common.h"
#include "../suprc/inc/sup_exfunc.h"
#include "../suprc/inc/sup_ring.h"//数据环
#include "../suprc/inc/sup_line.h"
#include "../suprc/inc/sup_draw.h"

#include "../status/inc/core_fsm.h"
#include "../status/inc/sta_basic.h"
#include "../status/inc/sta_cross.h"
#include "../status/inc/sta_island.h"
#include "../status/inc/sta_block.h"
#include "../status/inc/sta_outer.h" //出界识别/复原
#include "../status/inc/anls_mid.h"

static int status_CAL(const unsigned char m_sta);//赛道位置信息标准化

static _TMLOOP carTM = //车辆时间记录环
{
	.New = inull,
	.spc = 10,
	.org = AT_UNKNOW,
};
static int anls_cnt = 0;//时间累加值

static BASIC_DATA* BasicAddr = inull;
static ISLAND_DATA* IslandAddr = inull;
static BLOCK_DATA* BlockAddr = inull;
static MID_DATA* MidDataAddr = inull;

//赛道信息
ANLS_INFO pAnls;

static LINE edgel, edger, mid;

// func 初始化配置
void anls_init(void)
{
	BasicAddr = GetBasicAddr();  //匹配基础信息库
	IslandAddr = GetIslandAddr();//匹配环岛信息库
	BlockAddr = GetBlockAddr();   //匹配障碍信息库
	MidDataAddr = GetMidDataAddr(); //匹配中线信息库

	pAnls.edgel = &edgel;
	pAnls.edger = &edger;
	pAnls.mid = &mid;

	pAnls.bac_slope = &BasicAddr->slope;  //BASIC
	pAnls.bac_1LC = &BasicAddr->one_amount[0];
	pAnls.bac_1RC = &BasicAddr->one_amount[1];
	pAnls.bac_2LC = &BasicAddr->two_amount[0];
	pAnls.bac_2RC = &BasicAddr->two_amount[1];
	pAnls.set.bac_en2_1c = &BasicAddr->set.entwo_oneamo;
	pAnls.set.bac_en2_slp = &BasicAddr->set.entwo_slope;

	pAnls.i_lastw_y = &(IslandAddr->lastwhite_y);//ISLAND
	pAnls.i_spot_pos = &(IslandAddr->spot_advpos_y);
	pAnls.i_outloop_k = &(IslandAddr->outloop_k);
	pAnls.i_spot_size = &(IslandAddr->spot_size);
	pAnls.i_size_num = &(IslandAddr->use_varset);//0-small; 1-large
	pAnls.set.i_part_size_var = &(IslandAddr->set.part_size);
	pAnls.set.i_lane_trans = &(IslandAddr->set.lane_trans);

	pAnls.set.i_otlane_nol[0] = &(IslandAddr->set.var[0].otlane_nol);//nol
	pAnls.set.i_otlane_spc[0] = &(IslandAddr->set.var[0].otlane_spc);//spc
	pAnls.set.i_tolp_sup_var[0] = &(IslandAddr->set.var[0].toloop_sup);
	pAnls.set.i_outlp_sup_var[0] = &(IslandAddr->set.var[0].otloop_sup);
	pAnls.set.i_gotolp_var[0] = &(IslandAddr->set.var[0].toloop_pos);
	pAnls.set.i_outlp_var[0] = &(IslandAddr->set.var[0].otloop_pos);//small

	pAnls.set.i_otlane_nol[1] = &(IslandAddr->set.var[1].otlane_nol);//nol
	pAnls.set.i_otlane_spc[1] = &(IslandAddr->set.var[1].otlane_spc);//spc
	pAnls.set.i_tolp_sup_var[1] = &(IslandAddr->set.var[1].toloop_sup);
	pAnls.set.i_outlp_sup_var[1] = &(IslandAddr->set.var[1].otloop_sup);
	pAnls.set.i_gotolp_var[1] = &(IslandAddr->set.var[1].toloop_pos);
	pAnls.set.i_outlp_var[1] = &(IslandAddr->set.var[1].otloop_pos);//large

	pAnls.b_ave_pos = &(BlockAddr->ave_pos);//BLOCK
	pAnls.set.b_exist_dis = &(BlockAddr->set.exist_dis);
	pAnls.set.b_safe_dis = &(BlockAddr->set.safe_trans);
	pAnls.set.b_near_k = &(BlockAddr->set.near_disk);

	pAnls.m_sumerr = &(MidDataAddr->sumerr);//MIDLINE
	pAnls.m_horzerr = &(MidDataAddr->horzerr);
	pAnls.m_curve = &(MidDataAddr->curve);
	pAnls.m_slope = &(MidDataAddr->slope);
	pAnls.m_boterr = &(MidDataAddr->boterr);//最底部偏移量   正-偏离右边;负-偏离左边
	pAnls.m_horz_util = &(MidDataAddr->horz_util);
	pAnls.m_linear = &(MidDataAddr->linear);
	pAnls.set.m_horz_k = &(MidDataAddr->set.horz_k);
	pAnls.set.m_sup_k = &(MidDataAddr->set.sup_k);
	pAnls.set.m_horz_ctl = &(MidDataAddr->set.horz_ctl);

	TimeLoopBuild(&carTM, AT_UNKNOW);//创建时间记录环
	anls_fresh();
}

static unsigned char fsmSTA = FSM_BASIC;  //状态机状态
static unsigned char carSTA = AT_UNKNOW;//车辆状态

// func 路径分析

void anls_anls(void)
{
	LineInit(&edgel, LOC_UNKNOW);
	LineInit(&edger, LOC_UNKNOW);
	LineInit(&mid, LOC_UNKNOW);

	fsmSTA = __fsm(&edgel, &edger, &mid);//核心状态机
	__anls(&mid);      //中线分析

	//赛道位置信息标准化
	carSTA = (check_outer() == itrue) ? AT_OUTER : status_CAL(fsmSTA);
	TimePusher(&carTM, carSTA); //状态推入时间环

#ifdef _TEST_CFG_WIDTH
	Cfg_RoadWidth();
#endif
	PreviewLine(mid, 0, mid.endpos);//显示中线
	PreviewLineBase(); //显示基准线
	PreviewScaleMark();//绘制刻度盘
}

// func 重置启动
void anls_fresh(void)
{
	fsmSTA = FSM_BASIC;
	carSTA = AT_UNKNOW;
	TimeLoopCls(&carTM);//刷新时间记录环
	anls_cnt = 0;//初始化计数
	fresh_fsm();  //状态机刷新
}

// func 时间递推函数
void anls_timer(int du)
{
	anls_cnt++;
	if (anls_cnt >= du)
	{
		anls_cnt = 0;
		TimeLoopTimer(&carTM);
	}
}

// func 获取一时间点状态(位置及持续时间)
TimSta anls_state(int T)
{
	TimSta status;
	status.place = GetLoopStat(carTM, T);
	status.time = GetLoopDura(carTM, T);
	return status;
}

// 返回路径分析图像数组首地址,rows*cols;

ImgAns anls_image(void)
{
	ImgAns meb = { inull,0,0 };
	meb.hdp = imageInfo(&meb.rows, &meb.cols);
	return meb;
}

// func 位置信息标准化(非出界)
static int status_CAL(const unsigned char m_sta)
{
	unsigned char path_status = BAS_UNKNOW;//-Local

	switch (m_sta) //获取状态机状态
	{
	case(FSM_BASIC):path_status = GetBasicPlace();break;  //基础
	case(FSM_BLOCK):path_status = GetBlockPlace(); break;  //障碍
	case(FSM_ISLAND):path_status = GetIslandPlace(); break;//环岛
	case(FSM_CROSS):path_status = GetCrossPlace(); break;  //十字
	default:break;
	}

	switch (path_status)//标志信息标准化
	{
	case(BAS_STRAIGHT) : carSTA = AT_STRAIGHT; break;//直道
	case(BAS_BEND_L): carSTA = AT_BEND_L; break;    //左弯道
	case(BAS_BEND_R): carSTA = AT_BEND_R; break;    //右弯道
	case(BAS_ZEBRA): carSTA = AT_ZEBRA; break;      //斑马线

	case(BLOCK_UNKNOW): carSTA = AT_STRAIGHT; break;
	case(BLOCK_PRE_L): carSTA = AT_BLOCK_PL; break; //障碍前
	case(BLOCK_PRE_R): carSTA = AT_BLOCK_PR; break;
	case(BLOCK_SIDE_L): carSTA = AT_BLOCK_SL; break;//障碍旁左
	case(BLOCK_SIDE_R): carSTA = AT_BLOCK_SR; break;

	case(ISLAND_PRE_L) : carSTA = AT_ISLAND_PRE_L; break;  //环岛前
	case(ISLAND_PRE_R) : carSTA = AT_ISLAND_PRE_R; break;
	case(ISLAND_LANE_L) : carSTA = AT_ISLAND_LANE_L; break;//环岛小巷中
	case(ISLAND_LANE_R) : carSTA = AT_ISLAND_LANE_R; break;
	case(ISLAND_LOOP_L) : carSTA = AT_ISLAND_LOOP_L; break;//环岛小环中
	case(ISLAND_LOOP_R) : carSTA = AT_ISLAND_LOOP_R; break;

	case(CROSS_PRE) : carSTA = AT_CROSS_PRE; break; //十字前
	case(CROSS_IN) : carSTA = AT_CROSS_IN; break;   //十字中

	case(BAS_UNKNOW) : carSTA = AT_UNKNOW; break;//未知
	case(ISLAND_UNKNOW) : carSTA = AT_UNKNOW; break;
	case(CROSS_UNKNOW) : carSTA = AT_UNKNOW; break;
	default:break;
	}
	return carSTA;
}
