// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sta_island.h
// brief      环岛状态机
// author     赛北智能车 Rays
// version    v5.0
// date       2018-07-18

#ifndef _ANLS_ROTARY_ISLAND_H
#define _ANLS_ROTARY_ISLAND_H
#include "../../suprc/inc/sup_line.h"

//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
//此处添加宏定义标志符

//#define _ISLAND_LOOP_TEST    //环岛小环参数测试标志符
//#define _PRE_WLN_TEST_       //入环全白行测试

#define __SMALL_BIG_     //大小环
//#define __ONLY_SMALL_  //只有小圆环
//#define __ONLY_BIG_    //只有大圆环
//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

#define BLACKSPOT_TOP          (12)  //黑斑顶边标定行  IMAGE_TOP +15    //15
//<!黑块搜索顶行 由赛道最大环决定>

#define INTO_FULL_WLN          (43) //stdw_LN >= 此值 开启黑白搜索  //42？
#define BLACKSPOT_LEFT         (1)   //黑斑左边标定列 IMAGE_LEFT +1
#define BLACKSPOT_RIGHT        (118) //黑斑右边标定列 IMAGE_RIGHT-1
#define EXIST_FCT_FAR          (7)  //预测阈值(最远视野)  最远视野行 >= EXIST_FCT_FAR,开启预测

typedef enum _ISLAND_EXSIGN_ //70~89
{
	ISLAND_UNKNOW = 70,//未知未知

	ISLAND_PRE_L,     //环岛前 左
	ISLAND_LANE_L,    //环岛小巷 左
	ISLAND_LOOP_L,    //环岛小环 左

	ISLAND_PRE_R,     //环岛前 右
	ISLAND_LANE_R,    //环岛小巷 右
	ISLAND_LOOP_R,    //环岛小环 右
}ISLAND_EXSIGN;

typedef struct _ISLAND_VAR_
{
	int toloop_pos;  //小巷入环点 越小进小环越早
	int otloop_pos;  //出小环参数 越小越早出小环
	int toloop_sup;  //环岛入小环中线补充偏差
	int otloop_sup;  //环岛出小环中线补充偏差
	int otlane_nol;  //出小巷边沿平移量 nomal
	int otlane_spc;  //出小巷边沿平移量 special
}ISLANE_VAR;

typedef struct _ISLAND_DATA_
{
	int spot_size; //环岛黑斑大小(用于代替环岛大小)
	int use_varset;//环岛使用参数集(0 - 小环; 1 - 大环)

	int lastwhite_y;  //最后一个白点行值
	int spot_top_y;   //黑斑顶边行值
	int spot_bottom_y;//黑斑底边行值
	int spot_advpos_y;//黑斑顶边和底边平均行值
	int outloop_k;
	int _std_wline; //入环全白行测试 需要先定义_PRE_WLN_TEST_

	struct {
		ISLANE_VAR var[2];//参数
		int lane_trans;  //小巷边沿默认平移量
		int part_size;   //大小环区分参数
	}set;
}ISLAND_DATA;
//!
#define SETI_LANETRANS         (17) //小巷边沿默认平移量
#define SETI_PARTSIZE          (38) //大小环区分参数 ---------- 查看比较参数 - spot_size(i_spot_size)

#define SETI_SMALL_TOLOOPPOS   (37) //入小环位置参数
#define SETI_SMALL_OTLOOPPOS   (22) //出小环位置参数 ---------- 查看比较参数 - outloop_k(i_outloop_k)
#define SETI_SMALL_TOLOOPSUP   (6)  //入小环投射中线增益
#define SETI_SMALL_OTLOOPSUP   (7)  //出小环投射中线增益
#define SETI_SMALL_OTLANENOL   (16) //出小环小巷边沿平移量 nomal
#define SETI_SMALL_OTLANESPC   (13) //出小环小巷边沿平移量 预测

#define SETI_BIG_TOLOOPPOS     (32) //入大环位置参数
#define SETI_BIG_OTLOOPPOS     (18) //出大环位置参数 ---------- 查看比较参数 - outloop_k(i_outloop_k)
#define SETI_BIG_TOLOOPSUP     (-5) //入大环投射中线增益
#define SETI_BIG_OTLOOPSUP     (3)  //出大环投射中线增益
#define SETI_BIG_OTLANENOL     (15) //出大环小巷边沿平移量 nomal
#define SETI_BIG_OTLANESPC     (13) //出大环小巷边沿平移量 预测
//!

extern ISLAND_DATA*  GetIslandAddr(void); //返回 环岛信息库地址
extern unsigned char GetIslandPlace(void);//获取环岛中具体位置

extern void IFSM_Init(const char init); //环岛状态机初始化
extern void Island_FSM(const LINE edgel, const LINE edger, LINE* mid);//环岛状态机
extern char Get_IFSMState(void); //返回环岛状态机运行状态
extern char Goto_IFSM_Err(void); //判断是否错误识别环岛


#endif
