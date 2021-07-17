// freeman.c -v0.1 author:urays data:2018-07-18 email:zhl.rays@outlook.com 
#include "../inc/freeman.h"

#define FM_TRUE           (04)
#define FM_FALSE          (27)

// (-1,-1) (0,-1) (1,-1)     0 2 0
// (-1, 0)        (1, 0)     4   1
// (-1, 1) (0, 1) (1, 1)     0 7 0
static int DIR[8][2] = { { 1,0 },{ 1,-1 },{ 0,-1 },{ -1,-1 },{ -1,0 },{ -1,1 },{ 0,1 },{ 1,1 } };
static int LOCAL[8] = { 1,0,2,0,4,0,7,0 };

static int FM_edgeCheck(const int cenx, const int ceny)
{
	if (_color_write(cenx, ceny))//中心点要求为白色
	{
		if (_color_black(cenx + DIR[0 * 2][0], ceny + DIR[0 * 2][1])) { return FM_TRUE; }
		if (_color_black(cenx + DIR[1 * 2][0], ceny + DIR[1 * 2][1])) { return FM_TRUE; }
		if (_color_black(cenx + DIR[2 * 2][0], ceny + DIR[2 * 2][1])) { return FM_TRUE; }
		if (_color_black(cenx + DIR[3 * 2][0], ceny + DIR[3 * 2][1])) { return FM_TRUE; }
	}
	return FM_FALSE;
}

static int FM_preBcurW(const int cenx, const int ceny, const int i)
{
	int prei = 0; //当前为白色 之前为黑色

	if (i == 7) { prei = 0; }
	else { prei = i + 1; }
	if (_color_black(cenx + DIR[prei][0], ceny + DIR[prei][1]))
	{
		if (_color_write(cenx + DIR[i][0], ceny + DIR[i][1]))
		{
			return FM_TRUE;
		}
	}
	return FM_FALSE;
}

_fmmsg Freeman(const int x, const int y)
{
	int i = 0, res = 0;
	int u_count = 0, noise_c = 0;
	int s_white = -1, e_white = -1;
	int fine = FM_FALSE, turn = FM_FALSE;
	int blackw = 0, blackc = 0;
	_fmmsg freetmp = {
		.grads = 0,
		.dir = FM_NOT,
	};

	if (FM_edgeCheck(x, y) == FM_FALSE)//中心点边缘检测
	{
		return freetmp;
	} //要求:中心点为白色点,XY坐标下存在黑色点
	for (i = 0; i < 8; i++)
	{
		u_count++;
		if (FM_preBcurW(x, y, i) == FM_TRUE)
		{
			s_white = i; //当前为白  前面为黑
			break;
		}
	}
	int aimi = s_white;      // 3 2 1
	if (s_white != -1)       // 4   0
	{                        // 5 6 7
		for (i = 0; i < 7; ++i)
		{
			++aimi;
			if (aimi > 7) aimi = 0;
			if (_color_black(x + DIR[aimi][0], y + DIR[aimi][1]))
			{
				blackw += LOCAL[aimi];
				blackc++;
				if (turn == FM_TRUE)
				{
					turn = FM_FALSE;
					noise_c++; //噪点检测
				}
			}
			else {
				turn = FM_TRUE;
				if (fine == FM_FALSE)
				{
					e_white = aimi;
					fine = FM_TRUE;
				}
			}
		}
	}
	if (noise_c == 0 && blackc <= 4 && blackc > 0)//去除噪点/凹点
	{                    //   0 2 0
		switch (blackw)  //   4   1
		{                //   0 7 0
		case(0): freetmp.dir = FM_NOT; break;
		case(1): freetmp.dir = FM_RIGHT; break;
		case(3): freetmp.dir = FM_RUP; break;
		case(2): freetmp.dir = FM_UP; break;
		case(6): freetmp.dir = FM_LUP; break;
		case(4): freetmp.dir = FM_LEFT; break;
		case(11):freetmp.dir = FM_LDOWN; break;
		case(7): freetmp.dir = FM_DOWN; break;
		case(8): freetmp.dir = FM_RDOWN; break;
		default: freetmp.dir = FM_NOT; break;
		}
		res = s_white - e_white;
		res = res > 0 ? res : (-1 * res);
		if (res > 4) { res = 8 - res; }
		//if (res == FM_SUSP_COR && (blackw == 1 || blackw == 4)) res = FM_SURE_COR;
		freetmp.grads = res;
		return freetmp;
	} //dir = FM_NOT  表明一定不是角点
	return freetmp;
}
