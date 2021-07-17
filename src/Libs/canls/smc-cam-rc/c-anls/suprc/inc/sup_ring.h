// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sup_ring.h
// brief      数据环
// author     赛北智能车 Rays
// version    v3.0
// date       2018-07-18

#ifndef _SUP_LINK_H
#define _SUP_LINK_H

//<! 时间数据环->记录某一状态及其持续时间 > 

#define EFFECT_DURA     (2)//有效时间点阈值 (<=该阈值 该时间点记录无效)
typedef struct _tm_node
{
	int place;  //place infomatin
	int dura;   //state duration
	struct _tm_node* last;
	struct _tm_node* next;
}_TMNODE;

typedef struct _tm_loop_
{
	_TMNODE* New; //operating position
	int      spc; //maximum storage space
	int      org; //initial value
}_TMLOOP;

// func create a time record ring
// para loop(_TMLOOP*) time record ring
// para _org(const int) initial place
extern void TimeLoopBuild(_TMLOOP* loop, const int _org);

// func time propelling node
// para loop(_TMLOOP*) time record ring
// para plce(const int) place infomation
extern void TimePusher(_TMLOOP* loop, const int plce);

// func refresh time storage ring
// para loop(_TMLOOP*) time record ring
extern void TimeLoopCls(_TMLOOP* loop);

// func delete time storage ring
// para loop(_TMLOOP*) time record ring
extern void TimeLoopFree(_TMLOOP* loop);

// func timer of time record ring
// para loop(_TMLOOP*) time record ring
extern void TimeLoopTimer(_TMLOOP* loop);

// func get time node state (position / duration)
// para loop(_TMLOOP*) time record ring
// para T(const int) time node
extern int  GetLoopStat(const _TMLOOP loop, int num);
extern int  GetLoopDura(const _TMLOOP loop, int num);


//<! 数据连续环-> 抑制数据突变 >
typedef struct _dt_node
{
	float data;
	struct _dt_node* next;
}_DTNODE;

typedef struct _dt_loop
{
	_DTNODE* New; //operating position
	int  maxspc;  //maximum storage space
	char ONCECLS; //first initialization mark
}_DTLOOP;

// func data continuity
// para loop(_DTLOOP*) data storage ring
// para io(float*) data input and output
extern void AveDataLoop(_DTLOOP* loop, float *io);

// func delete the data storage ring
// para loop(_DTLOOP*) data storage ring
extern void DataLoopFree(_DTLOOP* loop);


#endif
