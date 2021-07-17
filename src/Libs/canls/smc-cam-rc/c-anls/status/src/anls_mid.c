#include "../../suprc/inc/common.h"
#include "../../suprc/inc/sup_draw.h"
#include "../../suprc/inc/sup_ring.h"

#include "../../conf-anls.h"
#include "../inc/anls_mid.h"


static void MID_FarNearPos(LINE* const mid);//寻找中线最近最远点位置
static void MID_MaxWPos(LINE* const mid);   //寻找权重最大行(小S)

static void TGraphOffset(const int ctl, LINE* const mid, float* horzerr);//动态全图加权
static void Calc_Curve(LINE* const mid, float* curve); //获取中线弯曲度
static void Calc_Slope(LINE* const mid, float* slope); //获取中线斜率

static char mid_T = CURVE;//中线类型

static MID_DATA Mid = { //初始化中线信息
	.set.horz_k = SETF_HORZK, //水平偏差系数 //0.40
	.set.sup_k = SETF_SUPK,   //偏差系数     //8.0
	.set.horz_ctl = SETI_HORZCTL,  //水平偏差权重分配控制量

	.horz_util = 0.0f,//水平偏差直线利用率
	.horzerr = 0.0f, //中线水平偏移量(加权处理)
	.sumerr = 0.0f,  //中心线最终偏移量
	.linear = 0.0f,  //中线线性度

	.curve = 0.0f, //中心线弯曲度
	.slope = 0.0f, //中线斜率
	.boterr = 0,   //中线最底部偏移量
};

MID_DATA* GetMidDataAddr(void) { return (&Mid); }

static int farpos = 0; //中线最远位置
static int nearpos = 0;//中线最近位置

// func 寻找中线中最远,最近点位置
// para mid(LINE* const) 中线
static void MID_FarNearPos(LINE* const mid)
{
	int t = 0, pos = 0;

	nearpos = 0;//initialize
	farpos = mid->endpos;
	for (t = 1; t <= mid->endpos; t++)
	{
		if (POINT_CHK(mid->line[t]) == itrue)
		{
			if (POINT_CHK(mid->line[t - 1]) == itrue)
			{
				if (mid->line[pos].y > mid->line[t].y) pos = t;
			}
		}
	}
	farpos = pos; //record farthest position
	for (t = farpos - 1; t >= 0; t--)//保证 farpos 和 nearppos 不在同一个位置
	{
		if (POINT_CHK(mid->line[t]) == itrue)
		{
			if (mid->line[pos].y < mid->line[t].y)//  <
			{
				pos = t;
			}
		}
	}
	nearpos = pos;//record nearest position
}

static int maxpos = 0;//最大权重位置

// func 寻找中线中最大权重位置
// para mid(LINE* const) 中线
static void MID_MaxWPos(LINE* const mid)
{
	int t, dir = 0;
	int max_di = 0, cur_di = 0;
	int check_t = farpos, effc = 0;
	char stopflg = ifalse;

	maxpos = farpos;
	if (mid->local == LOC_ISLAND  //环岛不需要
		|| mid->local == LOC_BLOCK  //障碍不需要
		//|| mid->local == LOC_CROSS //十字不需要
		|| LINEAR == mid_T) { //直道需要
		return;
	}
	for (t = farpos; t > 0; t--) {
		cur_di = mid->line[t].x - mid->line[check_t].x;
		if (_iiabs(cur_di) > 1)
		{
			if (cur_di > 0) dir = 2;//right
			else if (cur_di < 0) dir = 1;//left
			break;
		}
	}
	for (; t > nearpos; t--)
	{
		if (POINT_CHK(mid->line[t]) == itrue)
		{
			if (stopflg == ifalse)
			{
				cur_di = mid->line[t].x - mid->line[check_t].x;
				if ((dir == 2 && cur_di < 0) || (dir == 1 && cur_di > 0))
				{
					check_t = t;
					stopflg = itrue;
				}
			}
			if (stopflg == itrue)
			{
				cur_di = mid->line[t].x - mid->line[check_t].x;
				if (_iiabs(cur_di) > 1)
				{
					if (cur_di > 0 && dir == 1) dir = 2;
					else if (cur_di < 0 && dir == 2) dir = 1;//LEFT
					else { break; }
					stopflg = ifalse;
				}
			}
			else {//竖线凹凸检测
				if (vLNBumpCheck(mid, t, 5) == itrue)
				{
					cur_di = _iiabs(mid->line[t].x - mid->line[check_t].x);
					if (effc <= 0) {
						maxpos = t;
						max_di = cur_di;
						effc++;
					}
					else if (max_di < cur_di) {
						max_di = cur_di;
						maxpos = t;
						effc++;//可以凹凸点检测 数量记录
					}
				}
			}
		}
		if (effc >= 5) break; //凹凸点检测数量上线 5
	}
	//
	if (mid->line[mid->endpos].y <= 15)
	{
		if (_iiabs(mid->line[farpos].x - mid->line[maxpos].x) - 5 >
			_iiabs(mid->line[0].x - mid->line[maxpos].x))
		{
			if (_iiabs(mid->line[0].x - mid->line[maxpos].x)<15)
			{
				maxpos = farpos;
			}
		}
	}
}


static float save_horzerr = 0.0f;

// func 全图动态加权 计算中心水平偏移量  [+-]
// para ctl(const int) 控制权重 (0)
// para mid(LINE* const) 中线
// get horzerr(float*) 中心水平偏移量
static void TGraphOffset(const int ctl, LINE* const mid, float* horzerr)
{
	int t = 0, W_i = 0, U_i = 0;
	int acuity = 0; //趋势平缓度控制 越大权重差距越不明显, 越小越明显(+)
	int control = 0;//权重分配控制   越小最远处权重越大(+-)
	float W = 0.0f, W_sum = 0.0f, X_sum = 0.0f;
	float tmp_err = 0.0f;

	acuity = farpos - nearpos + 1;
	W_i = maxpos - farpos;

	int MIT = (int)(acuity*0.02f*(acuity - 1));//最大控制量
	control = _imid(ctl, MIT, (-MIT));//控制限定  可调范围为 -MIT 到 MIT

	for (t = farpos; t >= nearpos; t--)
	{
		if (POINT_CHK(mid->line[t]) == itrue)
		{
			if (_iiabs(W_i) <= acuity)
			{
				W = 0.02f*(acuity - _iiabs(W_i))*(acuity - _iiabs(W_i));
				W = W + control*1.0f;
				if (W > 0.0f)
				{
					W_sum += W;
					X_sum += mid->line[t].x*W;
					U_i++;//计算利用率
				}
			}
		}
		W_i++;
	}
	tmp_err = X_sum / W_sum - (IMAGE_CENTER*1.0f);
	Mid.horz_util = (W_i >= 1) ? (U_i / (W_i*1.0f)) : 0.0f;//计算利用率
#ifndef _MAX_VAL_TEST
	tmp_err = _imid(tmp_err, (-HORZERR_MAX), HORZERR_MAX);
#endif
	if (mid->endpos < LINE_AVL_NUM) (*horzerr) = save_horzerr;
	else {
		(*horzerr) = tmp_err;
		save_horzerr = tmp_err;
	}
}

static float save_curve = 1.5f;
static float tmpcur[3] = { 1.5f ,1.5f ,1.5f };

// func 中线弯曲度 (+)
// para mid(LINE* const) 中线
// get curve(float*) 中线曲率
static void Calc_Curve(LINE* const mid, float* curve)
{
	float tmpCur = 1.5f;
	int near = nearpos;
	int far = farpos - 3;

	int t, ac_len = 0, bc_len = 0;
	if (far - nearpos > 3)
	{
		for (t = 0; t < 3; t++)
		{
			if (far > farpos) far = farpos;
			if (near > farpos - 3) near = farpos - 3;
			ac_len = mid->line[near].y - mid->line[far].y;
			bc_len = mid->line[far].x - mid->line[near].x;
			far++, near++;
			if (bc_len != 0 || ac_len != 0)
			{
				tmpcur[t] = 200.0f*bc_len / (1.0f*(bc_len*bc_len + ac_len * ac_len));
			}
		}
		tmpCur = _imid(tmpcur[0], tmpcur[1], tmpcur[2]);
	}
	//
	////弓型 中线 曲率特殊处理 需要进一步测试
	////......
	
#ifndef _MAX_VAL_TEST
	tmpCur = _imid(tmpCur, (-CURVE_MAX), CURVE_MAX);
#endif
	if (mid->endpos < LINE_AVL_NUM)
	{
		if (save_curve > 0.5f || save_curve < -0.5f) (*curve) = save_curve;
		else (*curve) = 1.0f;
	}
	else {
		(*curve) = tmpCur;
		save_curve = tmpCur;
	}
}

// func 获取中线斜率
// para mid(LINE* const) 中线
// get *slope(float) 中线斜率
static void Calc_Slope(LINE* const mid, float* slope)
{
	float tslope = 0.0f;
	int di_y = 1, di_x = 0, t = 0;

	for (t = 0; t < maxpos - nearpos; t++)
	{
		di_y = mid->line[nearpos].y - mid->line[maxpos - t].y;
		if (di_y != 0) break;
		else di_y = 1;
	}
	if (maxpos > 0)
	{
		di_x = mid->line[maxpos].x - IMAGE_CENTER;
	}
	tslope = di_x / (di_y*1.0f);

#ifndef _MAX_VAL_TEST
	tslope = _imid(tslope, (-SLOPE_MAX), SLOPE_MAX);
#endif
	(*slope) = tslope;
}

// func 获取路径信息
void __anls(LINE* const mid)
{
	float SUM_err = 0.0f;
	LSQ_INFO lsq;

	Mid.boterr = IMAGE_CENTER - mid->line[0].x;//计算最底部偏移量
	Mid.boterr = _imid(Mid.boterr, (-BOTERR_MAX), BOTERR_MAX);

	MID_FarNearPos(mid);//获取中线最远/最近位置
	//mid_T = LineLinear(mid, nearpos, farpos, &lsq, &Mid.linear);
	//printf("horz: %d\n", mid_T);

	MID_MaxWPos(mid);   //获取最大偏差行
	//PreviewPointX(mid->line[maxpos], 2);//显示最大偏位置
	//PreviewPointX(mid->line[nearpos], 2);//显示最近位置
	//PreviewPointX(mid->line[farpos], 2); //显示最远位置

	TGraphOffset(Mid.set.horz_ctl, mid, &(Mid.horzerr));//计算中线水平偏移量(+-)
	Calc_Slope(mid, &(Mid.slope));  //获取斜率(+-)
	Calc_Curve(mid, &(Mid.curve));  //获取曲率(+)

	SUM_err = Mid.set.horz_k*Mid.horzerr + Mid.set.sup_k*Mid.slope;

#ifndef _MAX_VAL_TEST
	SUM_err = _imid(SUM_err, (-SUMERR_MAX), SUMERR_MAX);
#endif
	Mid.sumerr = SUM_err;
}
