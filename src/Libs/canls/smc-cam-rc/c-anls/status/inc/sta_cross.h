// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sta_cross.h
// brief      十字赛道处理
// author     赛北智能车 Rays
// version    v5.0
// date       2018-07-08

#ifndef _ANLS_CROSS_H
#define _ANLS_CROSS_H
#include "../../suprc/inc/sup_line.h"

//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
//此处添加宏定义标志符
//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+


#define TURNIN_MAX         (50)  //判断进入十字中的最小行值   IMAGE_BOTTOM-10
//左右边沿中最高行值 大于 TURNIN_MAX 行

#define TRANS_AMOUNT       (16)//十字前最长边沿平移量
#define MIDDLE_SUPLEN      (12)//十字中中线补充增长量
#define EXT_CROSS_CHK_LN   (50) //出十字白点数 检测行


typedef enum _CROSS_EXSIGN_//80~99
{
	CROSS_UNKNOW = 50,
	CROSS_PRE, //十字前
	CROSS_IN,  //十字中
}CROSS_EXSIGN;

extern unsigned char GetCrossPlace(void);//获取十字中具体位置

extern void CFSM_Init(const char init); //初始化状态机
extern void Cross_FSM(const LINE edgel, const LINE edger, LINE *mid);//十字独立状态机
extern char Get_CFSMState(void);//返回十字状态机状态


#endif
