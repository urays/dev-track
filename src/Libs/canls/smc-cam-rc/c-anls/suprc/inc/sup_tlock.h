// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sup_tlock.h
// brief      信任锁(确信稳定输入)
// author     赛北智能车 Rays
// version    v1.0
// date       2018-07-18

#ifndef _SUP_TRUST_LK_H
#define _SUP_TRUST_LK_H

#define T_LEVEL_MAX     (50)   //最大置信等级  最小等级0

//置信锁状态 打开
//置信锁状态 关闭
//待定
//错误数超过容错度
typedef enum _TRUST_LK_
{
	TL_LOCK_OPEN,
	TL_LOCK_CLOSE,
	TL_POINT_NULL,
	TL_POINT_ERROR,
};

typedef struct _TRUST_LOCK_
{
	int set_trust;    //置信度 给值越大,信任条件越苛刻
	int err_tolerant;
	int lock;         //置信锁  一旦一方置信度达到最高 置信锁关闭

	int trust_count;  //信任累积值
	int error_count;  //错误累积值

	int para_pointed; //被信任参数
}TRUST_LOCK;

// func enable and initialize the trust lock
// para set_trust(const int) 置信度 给值越大,信任条件越苛刻; nomal set 5
// para err_tol(const int) 容错度 给值越大容忍度越高; nomal set 5
extern void enTrustLock(const int set_trust, const int err_tol, TRUST_LOCK* clock);


// func trust lock (trust stable input) 
// para comp_para1(const char) 第一个参数
// para comp_para2(const char) 第二个参数
// para *ios(char) 输入数据
extern void TrustLock(const char comp_para1, const char comp_para2, TRUST_LOCK *clock, char *ios);



#endif
