// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sup_exfunc.h
// brief      全局分析函数
// author     赛北智能车 Rays
// version    v3.0
// date       2018-07-18

#ifndef _SUP_EX_FNC_H
#define _SUP_EX_FNC_H

#include "../inc/sup_line.h"

//!
//#define _TEST_CFG_WIDTH //赛道宽度数组读取 测试标志

#ifdef _TEST_CFG_WIDTH 
//
// func debug to get the track width
// get straight_width[]

extern void Cfg_RoadWidth(void);
#endif
// 获取某行赛道宽度 IMAGE_TOP ~ IMAGE_BOTTOM
extern int  Get_Road_Width(const int line);


typedef enum _EXFUNC_DIR_
{
	EXF_LEFT = 20,
	EXF_RIGHT,
	EXF_BOTH,
	EXF_UP,
	EXF_DOWN,
}EXFUNC_DIR;

// func counting white points from (startcolumn, startline)
// para dir(const int)
// statistical direction of white spot:
//     EXF_LEFT(towards left);
//     EXF_RIGHT(towards right);
//     EXF_BOTH(both direction);else(0)
// para st_x(const int) starting column
// para st_y(const int) starting line
// retruned value white_count(int)
extern int WhiteCount_LR(const int dir, const int st_x, const int st_y);

// func full line projection for the farthest white spot
// para shotx(const int) initial diffusion column
// para shoty(const int) bottom launch starting line
// para count(const int) observation of the harness; range：count//2
// get //farW(POINT) the farthest projection of white spots
extern void FullLine_ShotUpF(const int shotx, const int shoty, const int count, POINT* farW);

// func finding the last white spot in the XY coordinate system
// para startx(int) starting point abscissa
// para starty(int) starting point ordinate
// para dir(const char) search direction
// get  LAST(POINT//) the farthest projection of white spots
extern void GetLastWhiteXY(const int startx, const int starty, const char dir, POINT *LAST);

// func the last white spot for slope projection
// para startx(int) starting point abscissa
// para starty(int) starting point ordinate
// para k(const float) slope
// get  LAST(POINT//) the farthest projection of white spots
extern void GetLastWhiteKK(const int startx, const int starty, const float k, POINT* LAST);

// func the maximum number of white points in the track where white_x is located
// para white_x(const int) full white column
// para st_line(const char) count start line(up)
// para usep(int) nomal:10
#define MutationThreshold   (10) //白点数突变阈值  用于去除十字中的全白行干扰(一般不用改)
extern int GetLNrangeMaxWC(const int white_x, const char st_line, int usep);


#endif
