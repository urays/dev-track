// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sup_line.h
// brief      图像线段处理
// author     赛北智能车 Rays
// version    v3.0
// date       2018-07-18

#ifndef _SUP_LINE_H
#define _SUP_LINE_H

#include "sup_math.h"

#define MAXL            (200)//segment array space
#define INIT_VALUE      (0)  //initial point value

//LINE TYPE
#define LINEAR          (6)//直线
#define CURVE           (7)//曲线

//LINE LOCAL
typedef enum _LINE_LOCAL_
{
	LOC_UNKNOW = 17, //归属度未知
	LOC_BASIC,       //基础
	LOC_CROSS,       //十字
	LOC_ISLAND,      //环岛
	LOC_BLOCK,       //障碍
}LINE_LOCAL;

typedef struct _POINT_ {
	int x;
	int y;
}POINT;

#define LINE_BITS     (sizeof(POINT)*MAXL)

extern char checkpoint(POINT p);
//whether it is a valid point
#define POINT_CHK(x)    (checkpoint(x) == 1)

typedef struct _LINE_
{
	POINT line[MAXL]; //line[1]-起点;line[0]-初始化点
	int   endpos;     //结束点位置
	char  local;      //线段归属地
}LINE;//线

typedef struct _LSQ_ 
{
	POINT pass; //必过点
	float k;    //系数
	float c;    //常数项
}LSQ_INFO;

//<!-- initialize line -->
extern void LineInit(LINE* org, const char loc);

// func 一元一次方程 最小二乘法  column=k*line+c;(列值差的平方)
// para org(LINE) 点阵数组(优化过后的直线)
// para st org 计算起始位置
// para ed org 计算终止位置
// get LSQ 一元一次方程 参数信息
// plus 使用前需先定义结构体 LSQ_INFO
extern void LSQ_algorithm(LINE* const org, int st, int end, LSQ_INFO*LSQ);

// func 拟合信息求线段
// para lsq(LSQ_INFO*) 拟合信息
// para loc(const char) 归属地
// get *out(LINE) 输出直线
// para stpkey(const char 截断开关) 遇到黑点停止
extern void GetFilterLine(LSQ_INFO*const lsq, const char loc, LINE*out, const char stpkey);

// func 两点求线(线段)
// para x0(int) 开始点 横坐标
// para y0(int) 开始点 纵坐标
// para x1(const int) 结束点 横坐标
// para y1(const int) 结束点 纵坐标
// para loc(const char) 归属地
// get *out(LINE) 输出结果
extern void GetSegment(int x0, int y0, const int x1, const int y1, const char loc, LINE*out);

// func 判断任意两点之间是否全为白点(具有方向性) start_p->end_P
// para startx(const int)
// para starty(const int)
// para endx(const int)
// para endy(const int)
// get  END(POINT*) 最后一个白点
// ifalse-include balck point
// itrue-all white point
extern char LN_allwhite(const int startx, const int starty, const int endx, const int endy, POINT*END);

// func 竖直线某一位置 碰撞检测(凹凸检测)
// author Rays   date 2018-06-01
// para org(const LINE) 源线段
// para pos(const int)  目标检测位置
// para bump(const char) 凹凸点程度限制  = 3?
// return ifalse/itrue
#define BUMP_ERRTOR   (3) //容错度
extern char vLNBumpCheck(LINE* const org, const int pos, const char bump);

// func 一般滤波器 (求线段中部连续段)
// para ColumnDiffer1(const char) 列差 一般 2
// para ColumnDiffer2(const char) 列差 一般 5
// para loc(const char) 归属地
// get *out(LINE) 输出直线
// ColumnDiffer越大优化越低,ColumnDiffer太小存在误处理
extern void NomalFilter(const char ColumnDiffer1, const char ColumnDiffer2, const char loc, LINE*out);

// func 中值滤波器 (获取直线上相距3的特征点线段)
// para startpos(const int) 起始位置
// para endpos(const int) 结束位置
// para loc(const char) 归属地
// get *org(LINE) 源直线
// 觉得 还是3个点比较适合单片机：）
extern void MedianFilter(const int startpos, const int endpos, const char loc, LINE*org);

// func 均值滤波器 (获取直线上相距step的特征点线段)
// para startpos(const int) 起始位置
// para endpos(const int) 结束位置
// para step(const int) 步长
// para loc(const char) 归属地
// get *org(LINE) 源直线  EX: 1 2 3 4 5 step:2
extern void AverageFilter(const int startpos, const int endpos, const int step, const char loc, LINE *org);

// func 合并线段（将l2合并到l1）  l1(all)+l2(pos1~pos2)
// para l2( LINE) 合并源
// para loc(const char) 归属地
// get *line1(LINE) 合并line2后的线段
extern void LineMerge(LINE* const l2, const int pos1, const int pos2, const char loc, LINE *l1);

// func 截取线段
// para startpos(int) 截取首
// para endpos(int) 截取尾
// para loc(const char) 归属地
// get *org(LINE) 截取后的线段
extern void LineCutting(int startpos, int endpos, const char loc, LINE *org);

// func 线段平移(斜率) sinx 近似为 tanx
// para dis(const int) 设定两直线的距离 （负数-向左平移；正数-向右平移）
// para k(const float) 源直线斜率 k=(column-c)/line;
// para loc(const char) 归属度
// get *org(LINE) 平移后线段
// para stpkey(const char) 截断开关 遇到黑点停止
extern void LineTrans(const int dis, const float k, const char loc, LINE *org, const char stpkey);


// func 线性判断
// para org(const LINE) 源直线
// para startpos(const int) 起始位置
// para endpos(const int) 结束位置
// get *lsq(LSQ_INFO) 拟合信息
// get *linear(float) 判断参数 最大列偏差
// returned value
// LINEAR-is straight line
// CURVE-is bend line
#define BEELINE_VALUE   (6.0f)//给值越小 直线线性度判断标准越高
extern char LineLinear(LINE* org, const int st, const int end, LSQ_INFO *lsq, float *linear);

// func 求连续线段斜率k(LSQ)   k=-(column-c)/line
// para org(const LINE) 源连续线段
// get *slope(float) 斜率
// (-)(/); (+)(/)
extern void LineSlope(const LINE org, float *slope);


#endif
