// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       anls_mid.h
// brief      赛道中线分析
// author     赛北智能车 Rays
// version    v5.0
// date       2018-07-14

#ifndef _ANLS_MID_DATA_H
#define _ANLS_MID_DATA_H
#include "../../suprc/inc/sup_line.h"

//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
//此处添加宏定义标志符

//#define _MAX_VAL_TEST  //测试最大值 标识符
//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

#define LINE_AVL_NUM      (10) //可利用线段最少点数(抖动抑制)

#define HORZERR_MAX                (+45.0f) //-45~45 horizontal error limit
#define SUMERR_MAX                 (+45.0f) //-45~45 final error limit
#define CURVE_MAX                  (+3.5f)  //-3.5~3.5 curvature limit
#define SLOPE_MAX                  (+3.8f)  //-3.8~3.8 slope limit
#define BOTERR_MAX                 (15)  //-15~15 max value of boterr

typedef struct _MID_DATA_
{
	float sumerr;  //中心线最终偏移量
	float horzerr; //中线水平偏移量(加权处理)
	float horz_util;//水平偏差 直线利用率
	float slope;   //中线斜率
	float curve;   //中线曲率 
	float linear;  //中线线性度
	int   boterr;  //最底部偏移量   正-偏离右边;负-偏离左边

	struct {
		float horz_k; //水平偏差放大系数 
		float sup_k;  //补充偏差放大系数 slope
		int   horz_ctl; //水平偏差权重分配 控制量
	}set;
}MID_DATA;
//!
#define SETF_HORZK             (0.72f) //水平偏差系数 //0.65
#define SETI_HORZCTL           (0)    //偏差系数     //-8
#define SETF_SUPK              (3.5f) //水平偏差权重分配控制量 //6.0f
//!

extern MID_DATA* GetMidDataAddr(void);//返回 中线信息库地址
extern void __anls(LINE* const mid);//中线分析


#endif
