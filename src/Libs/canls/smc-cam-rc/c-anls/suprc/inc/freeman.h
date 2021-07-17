// freeman.c -v0.1 author:rayzco  data:2018-07-18 email:zhl.rays@outlook.com 
#ifndef _CORNER_PARTIAL_DETECT
#define _CORNER_PARTIAL_DETECT

#include "../../conf-anls.h"

#define _color_write(x,y)  (POINT_WHITE(x,y))
#define _color_black(x,y)  (!POINT_WHITE(x,y))

// �ƶ�����Ե�ǵ���:
// author Rays
// blackc:һ��3*3���кڵ���
// grads: 0 - ���в����ںڵ�(���Ǳ�Ե��)
// 		1 - ����
// 		2 - һ��Ϊ�ǵ�(BUG:С͹������)
// 		3 - �п���Ϊ�ǵ�
// 		4 - һ�����ǽǵ�(����λ��ͬһֱ����)
// dir:�ڵ�Ⱥλ�� (FM_NOT ����һ�����ǽǵ�)

// �ڰ�Ⱥλ�ü��:
// 			0 2 0
// 			4   1
// 			0 7 0
// 		1 - right       2 - up
// 		4 - left        7 - down
// 		3 - right_up    6 - left_up
// 		11 - left_down  8 - right_down
// freeman ��Ч�ǵ�����-���ѳ�������ܽǵ�,��Ҫ�����ж�:)
 
enum _FREEMAN_GRAD
{
	FM_SURE_COR = 2, // ȷ�Žǵ��ݶ�ֵ
	FM_SUSP_COR = 3, // ���ɽǵ��ݶ�ֵ
	FM_SURE_STR = 4, // ȷ�ϱ�ֱ�ݶ�
};

enum _FREEMAN_DIR
{
	FM_NOT,   // ���Ͻǵ�
	FM_LEFT,  // ��  
	FM_RIGHT, // ��
	FM_UP,    // ��   
	FM_DOWN,  // ��
	FM_LUP,   // ���� 
	FM_RUP,   // ����
	FM_LDOWN, // ���� 
	FM_RDOWN, // ����
};

typedef struct _FREEMAN_MSG
{
	char grads;//�ݶ�ֵ 
	char dir;//�ڵ�Ⱥ����
}_fmmsg;

// �ǵ�ֲ����
extern _fmmsg Freeman(const int x, const int y);


#endif