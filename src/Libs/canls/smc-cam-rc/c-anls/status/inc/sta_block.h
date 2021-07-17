// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sta_block.h
// brief      赛道障碍处理(假定障碍只在直道出现)
// author     赛北智能车 Rays
// version    v5.0
// date       2018-07-18

#ifndef _ANLS_BLOCK_H
#define _ANLS_BLOCK_H
#include "../../suprc/inc/sup_line.h"

//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
//此处添加宏定义标志符

//#define _BLOCK_CROSS_PRE  //开启此标志  障碍只在十字前 会比较安全
//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

#define BLOCK_MAX_HGT	       (30) 
//BRICK_MAX_HIGHT < IMAGE_BOTTOM - BREAK_ENDLINE //sta_basic.h
#define BLOCK_MIN_HGT	       (3)
#define BLOCK_SHOTRAG		   (3)  //-3 ~ +3  入射点搜索范围
#define BLOCK_SCAN_END		   (6)  //障碍扫描最远限制
#define UNSAFE_TRANS		   (23) //有干扰情况下 中线固定平移量 

typedef enum _BLOCK_EXSIGN_//120~129
{
	BLOCK_UNKNOW = 120, //未知位置

	BLOCK_PRE_L,   //左障碍 前
	BLOCK_SIDE_L,  //左障碍 旁
	BLOCK_PRE_R,   //右障碍 前
	BLOCK_SIDE_R,  //右障碍 旁
}BLOCK_EXSIGN;

typedef struct _BLOCK_DATA_
{
	int top_line;  //障碍顶行
	int bot_line;  //障碍底行
	int ave_pos;   //障碍近似位置

	struct {
		int   exist_dis;  //出障碍边沿平移距离
		int   safe_trans; //安全边沿平移量
		float near_disk;  //靠近障碍 偏离距离比(默认 0.5f)
	}set;
}BLOCK_DATA;
//!
#define SETI_EXISTDIS	        (12)   //出障碍边沿平移距离
#define SETI_SAFETRANS          (10)   //安全边沿平移量
#define SETF_NEARDISK           (0.42f)//靠近障碍 偏离距离比(默认 0.5f)
//!

extern BLOCK_DATA*   GetBlockAddr(void); //返回 障碍信息库地址
extern unsigned char GetBlockPlace(void);//获取障碍中的具体位置

extern void BFSM_Init(const char init); //初始化障碍状态机
extern void Block_FSM(const LINE edgel, const LINE edger, LINE *mid);//障碍独立状态机
extern char Get_BFSMState(void);//获取障碍状态机运行状态
extern char Goto_BFSM_Err(void);//判断是否错误识别障碍


#endif
