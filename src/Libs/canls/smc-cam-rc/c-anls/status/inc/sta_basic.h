// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sta_basic.h
// brief      赛道基本路况处理(全边搜索/拐点/计算中线)
// 			识别优先级:zebra->bend,straight
// author     赛北智能车 Rays
// version    v5.0
// date       2018-07-14

#ifndef _ANLS_BASIC_H
#define _ANLS_BASIC_H
#include "../../suprc/inc/sup_line.h"

//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
//此处添加宏定义标志符

//#define _CLOSE_BAG_CHECK    //关闭包包检测  如果不存在坡道,就关闭包包检测
//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

//识别优先级: zebra->bend,straight

#define VALID_EDGE_DIF   (5) //有效两边沿起点最小差值

#define ZEBRA_WHITEC_    (60) //斑马线保持标志清除 所在行赛道白点数小于这个值说明还在斑马线上
#define ZEBRA_RG_TOP_    (45) //斑马线搜索范围 上
#define ZEBRA_RG_BOT_    (55) //下
#define START_RG_TOP_    (37) //正常起始点搜索范围 上
#define START_RG_BOT_    (59) //下

#define MD_SEARCH_RANGE  (6) //左右搜线范围
#define TD_SEARCH_RANGE  (5) //上下搜线范围
#define DEF_MORE_EDGEC   (20)//边沿较多点数 判定阈值

#define TD_IMPOSSIABLE   (10) //纵向边沿 一定不会出现在 line<TD_IMPOSSIABLE

#define BKTYPE_AREA_UP      (25) //拐点类型检测范围(上)
#define BKTYPE_AREA_DOWN    (58) //拐点类型检测范围(下)
#define CORNER_CHECK_WHITE  (25) //开窗检测角点白点阈值  
//(越小条件越松 max=36)

#define TAB_HIGHT       (3)     //标定点距纵向起始点行差(向下)
#define TAB_LENGTH      (30)    //标定点距纵向起始点列差

//包包检测 去除无效角点
#define BAG_FAR_LIMIT    (15) //最远白点 行值      包包 需要 大于此值
#define BAG_DIS_LIMIT    (35) //左右边沿水平距离差 包包 需要 小于此值
#define BAG_HIG_LIMIT    (10) //左右边沿高度差     包包 需要 小于此值

typedef enum _BASIC_SIGN_//30~49
{
	BAS_UNKNOW = 30, //待定
	BAS_LEFT,     //左边   DIRECTION
	BAS_RIGHT,    //右边
	BAS_BOTH,     //两边

	BAS_BK_NOT,   //不存在拐点
	BAS_BK_LEFT,  //左拐点
	BAS_BK_RIGHT, //右拐点
	BAS_BK_BOTH,  //存在两个拐点
 
	BAS_ZEBRA,    //斑马线
	BAS_STRAIGHT, //直道
	BAS_BEND,     //弯道
	BAS_BEND_L,   //弯道 左
	BAS_BEND_R,   //弯道 右
}BASIC_SIGN;

typedef struct _BASIC_DATA_
{
	int one_amount[2];//one_amount[0] - left edge   one_amount[1] - right edge
	int two_amount[2];
	float slope;//基础寻线 较短边 末点与屏幕中点斜率值

	struct {
		int entwo_oneamo; //one_amount 较长边点数 <= 此值 开启纵向寻线
		float entwo_slope;//short_slope <= 此值 开启纵向寻线
	}set;
}BASIC_DATA;
//!
#define SETI_EN2ONEAMO        (45)   //<= 此值开启纵向寻线---------- 查看比较参数 - one_amount[2](max(bac_1LC,bac_1RC))
#define SETF_EN2SLOPE         (0.46f)//<= 此值开启纵向寻线---------- 查看比较参数 - slope(bac_slope)
//!

extern BASIC_DATA* GetBasicAddr(void); //返回 基础信息库地址

//基本赛道信息获取
extern void BasicDeal_MD(LINE *edgel, LINE *edger);//基础横向搜线
extern void BasicDeal_BK(LINE *edgel, LINE *edger);//基础搜索横向拐点
extern void BasicDeal_TD(LINE *edgel, LINE *edger);//基础纵向搜线
extern void BasicDeal_BS(LINE* const edgel, LINE* const edger);//基础 (弯/直) 判断

extern void CalcMidBasic(LINE edgel, LINE edger, const float disk, const char loc, LINE *res);//计算中线
extern void CalcMidBasic_Pp(const LINE edgel, const LINE edger, const float disk, const char loc, LINE *res);//比例
extern void CalcMidBasic_Lv(const LINE edgel, const LINE edger, const char loc, LINE *res); //水平
extern void Mid_Repair_ByL(const LINE edgel, const int supx, const char loc, LINE *res); //单边(左依赖)
extern void Mid_Repair_ByR(const LINE edger, const int supx, const char loc, LINE *res); //单边(右依赖)


//基本赛道信息发送
extern char GetBasicPlace(void);//发送基础路径信息
extern char GetBasicBreak(void);//发送拐点类型



#endif
