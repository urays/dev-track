// freeman.c -v0.1 author:rayzco  data:2018-07-18 email:zhl.rays@outlook.com 
#ifndef _CORNER_PARTIAL_DETECT
#define _CORNER_PARTIAL_DETECT

#include "../../conf-anls.h"

#define _color_write(x,y)  (POINT_WHITE(x,y))
#define _color_black(x,y)  (!POINT_WHITE(x,y))

// 菲尔曼边缘角点检测:
// author Rays
// blackc:一个3*3表中黑点数
// grads: 0 - 表中不存在黑点(不是边缘点)
// 		1 - 凹点
// 		2 - 一定为角点(BUG:小凸起误判)
// 		3 - 有可能为角点
// 		4 - 一定不是角点(三点位于同一直线上)
// dir:黑点群位置 (FM_NOT 表明一定不是角点)

// 黑斑群位置检测:
// 			0 2 0
// 			4   1
// 			0 7 0
// 		1 - right       2 - up
// 		4 - left        7 - down
// 		3 - right_up    6 - left_up
// 		11 - left_down  8 - right_down
// freeman 有效角点搜索-会搜出多个可能角点,需要后期判断:)
 
enum _FREEMAN_GRAD
{
	FM_SURE_COR = 2, // 确信角点梯度值
	FM_SUSP_COR = 3, // 可疑角点梯度值
	FM_SURE_STR = 4, // 确认笔直梯度
};

enum _FREEMAN_DIR
{
	FM_NOT,   // 否认角点
	FM_LEFT,  // 左  
	FM_RIGHT, // 右
	FM_UP,    // 上   
	FM_DOWN,  // 下
	FM_LUP,   // 左上 
	FM_RUP,   // 右上
	FM_LDOWN, // 左下 
	FM_RDOWN, // 右下
};

typedef struct _FREEMAN_MSG
{
	char grads;//梯度值 
	char dir;//黑点群朝向
}_fmmsg;

// 角点局部检测
extern _fmmsg Freeman(const int x, const int y);


#endif