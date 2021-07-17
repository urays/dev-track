// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       core_fsm.h
// brief      核心状态机
// author     赛北智能车 Rays
// version    v3.0
// date       2018-06-13

#ifndef _ANLS_CORE_FSM_H
#define _ANLS_CORE_FSM_H
#include "../../suprc/inc/sup_line.h"

//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
//此处添加宏定义标志符
//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+


typedef enum _CORE_FSM_SIGN_//90~109
{
	FSM_BASIC = 90,//基础
	FSM_BLOCK,     //障碍
	FSM_CROSS,     //十字
	FSM_ISLAND,    //环岛
}CORE_FSM_SIGN;


extern void fresh_fsm(void);//核心状态机刷新
extern unsigned char __fsm(LINE *mid);//路径分析主状态机


#endif
