//适用于原始图像赛道区域占屏比 1/2 ~ 2/3
#ifndef _ANLS_EXTRO_H
#define _ANLS_EXTRO_H

#include "./status/ex_mark.h" //mark sign
#include "suprc/inc/sup_line.h"

typedef struct _ANLS_INFO_
{
	int*   bac_1LC;        //基础纵向左边沿点数
	int*   bac_1RC;        //基础纵向右边沿点数
	int*   bac_2LC;        //基础横向左边沿点数
	int*   bac_2RC;        //基础横向右边沿点数
	float* bac_slope;      //车身偏角

	float* m_sumerr;       //中线最终偏差
	float* m_horzerr;      //中线水平偏移量(加权处理)
	float* m_curve;        //中线曲率
	float* m_slope;        //中线斜率
	int*   m_boterr;       //最底部偏移量   正-偏离右边;负-偏离左边
	float* m_horz_util;    //水平偏差 利用率
	float* m_linear;       //中线线性度

	int* i_lastw_y;        //环岛投射最远白点行值
	int* i_spot_pos;       //环岛黑斑位置
	int* i_outloop_k;      //环岛出环查看参数
	int* i_spot_size;      //环岛尺寸大小
	int* i_size_num;       //环岛尺寸编号 0-small; 1-large
	int* b_ave_pos;        //障碍位置
	LINE* edgel, * edger, * mid;

	struct {
		int*   bac_en2_1c;       //max(bac_mdrc,bac_mdlc) <= 此值 开启纵向寻线
		float* bac_en2_slp;      //shortslp <= 此值 开启纵向寻线

		float* m_horz_k;         //中线水平偏差放大系数
		float* m_sup_k;          //中线补充偏差放大系数
		int* m_horz_ctl;         //水平偏差权重分配控制量

		int* i_part_size_var;    //大小环区分判断值
		int* i_lane_trans;       //小巷默认边沿平移量
		int* i_otlane_nol[2];    //出小巷默认边沿平移量 nomal
		int* i_otlane_spc[2];    //出小巷中线平移量 special
		int* i_tolp_sup_var[2];  //入环岛小环中线补充偏差
		int* i_outlp_sup_var[2]; //出环岛小环中线补充偏差
		int* i_gotolp_var[2];    //小巷入小环参数(越小越早进环)
		int* i_outlp_var[2];     //出小环参数(越大出小环越迟)

		float* b_near_k;         //靠近障碍 偏离距离比
		int* b_safe_dis;         //安全边沿平移量
		int* b_exist_dis;        //出障碍边沿平移量
	}set;
}ANLS_INFO;
typedef struct _TM_NODE_ { int place, time; }TimSta;
typedef struct __ARRAY { unsigned char* hdp; int rows, cols; }ImgAns;

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// IMG-AMLS external variables and functions              
// (func)anls_init: anls initial configuration            
// (func)anls_anls: path analysis                         
// (func)anls_timer:time-meter of anls                    
// (func)anls_fresh:anls reset boot                       
// (func)anls_state:Get a time point                      
//                  of vehicle status information         
// (func)anls_image:get-IMG_ANLS[IMG_HEIGHT][IMG_WIDTH]   
// (var)pAnls:infomation of track                         
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++
extern void anls_init(void);     //初始化(匹配数据地址,刷新启动机制)
extern void anls_anls(void);     //分析(主要路径分析函数)
extern void anls_fresh(void);    //刷新(只刷新状态机功能,不对调试中的参数进行处理)
extern void anls_timer(int du);  //时间计数器 exp: 3ms中断 du = 33;时间间隔 0.1s
extern TimSta anls_state(int T); //T-时间节点 0-now  1-last 2-...
extern ImgAns anls_image(void);  //返回路径数组首地址(*hdp),rows*cols

extern ANLS_INFO pAnls; //分析获得的赛道信息


#endif 
