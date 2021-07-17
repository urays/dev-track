#include "../inc/sup_math.h"
#include "../inc/sup_tlock.h"


void enTrustLock(const int set_trust, const int err_tol, TRUST_LOCK* clock)
{
	clock->lock = TL_LOCK_OPEN;
	clock->trust_count = 0;
	clock->error_count = 0;
	clock->para_pointed = TL_POINT_NULL;
	clock->set_trust = set_trust; //5
	clock->err_tolerant = err_tol;//5
}

void TrustLock(const char comp_para1, const char comp_para2, TRUST_LOCK *clock, char *ios)
{
	if (clock->lock == TL_LOCK_OPEN)
	{
		if (_iiabs(clock->trust_count) < T_LEVEL_MAX)//置信度达到满值 置信锁关闭
		{
			if ((*ios) == comp_para1) clock->trust_count--;
			else if ((*ios) == comp_para2) clock->trust_count++;
			else clock->error_count++;

			if (clock->error_count > clock->err_tolerant) {
				(*ios) = TL_POINT_ERROR;//信任错误,出现了第三者
				return;
			}
		}
		if (_iiabs(clock->trust_count) > clock->set_trust)
		{
			if (clock->trust_count < 0) {
				clock->para_pointed = comp_para1;//信任第一个参数
			}
			else {
				clock->para_pointed = comp_para2;//信任第二个参数
			}
			clock->lock = TL_LOCK_CLOSE;
		}
	}
	if (clock->para_pointed != TL_POINT_NULL){
		*ios = clock->para_pointed;
	}
}
