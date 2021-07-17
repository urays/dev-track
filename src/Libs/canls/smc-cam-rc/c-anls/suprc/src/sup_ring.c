#include "../inc/common.h"
#include "../inc/sup_ring.h"

#include    <stdlib.h>           //malloc

void TimeLoopBuild(_TMLOOP* loop, const int _org)
{
	int i = 0;
	_TMNODE* pre = inull;
	_TMNODE* now = inull;
	_TMNODE* head = inull;
	loop->New = inull;
	loop->org = _org;
	head = (_TMNODE*)malloc(sizeof(_TMNODE));
	if (head == inull) { return; }
	head->dura = 0;
	head->last = inull;
	head->next = inull;
	head->place = _org;
	pre = head;
	for (i = 1; i < loop->spc; i++)
	{
		now = (_TMNODE*)malloc(sizeof(_TMNODE));
		now->place = _org;
		now->dura = 0;
		now->last = pre;
		now->next = inull;
		pre->next = now;
		pre = now;
	}
	head->last = pre;
	pre->next = head;
	loop->New = head;
}

void TimePusher(_TMLOOP* loop, const int plce)
{
	if (loop->New == inull) { return; }
	if (plce != loop->New->place)//更新节点
	{
		if (loop->New->dura > EFFECT_DURA)
		{
			loop->New = loop->New->next;
		}
		loop->New->dura = 0;
		loop->New->place = plce;
	}
}

void TimeLoopFree(_TMLOOP* loop)
{
	_TMNODE* nt = loop->New;
	_TMNODE* sv = nt;
	if (nt == inull) return;
	nt->last->next = inull;
	while (nt != inull)
	{
		sv = nt->next;
		free(nt);
		nt = inull;
		nt = sv;
	}
	loop->New = inull;
}

void TimeLoopCls(_TMLOOP* loop)
{
	_TMNODE* pre = loop->New;
	int i = 0;
	if (pre != inull)
	{
		for (i = 0; i < loop->spc; i++)
		{
			pre->dura = 0;
			pre->place = loop->org;
			pre = pre->next;
		}
	}
}

void TimeLoopTimer(_TMLOOP* loop)
{
	if (loop->New != inull)
	{
		if (loop->New->dura < 9999)
		{
			(loop->New->dura)++;
		}
	}
}

int GetLoopStat(const _TMLOOP loop, int T)
{
	if (loop.New == inull) { return loop.org; }
	_TMNODE* pre = loop.New;
	int i = 0;
	if (T<0 || T >= loop.spc) T = 0;

	for (i = 0; i < T; i++)
	{
		pre = pre->last;
	}
	return (pre->place);
}

int GetLoopDura(const _TMLOOP loop, int num)
{
	if (loop.New == inull) { return loop.org; }
	_TMNODE* pre = loop.New;
	int i = 0;
	if (num<0 || num >= loop.spc) num = 0;

	for (i = 0; i < num; i++)
	{
		pre = pre->last;
	}
	return (pre->dura);
}


void AveDataLoop(_DTLOOP* loop, float *io)
{
	int i = 0;
	float sum = 0.0f;
	_DTNODE* pre = inull;
	if (loop->ONCECLS == itrue)
	{
		_DTNODE* head = inull;
		loop->ONCECLS = ifalse;
		head = (_DTNODE*)malloc(sizeof(_DTNODE));
		if (head == inull) { return; }
		pre = head;
		pre->data = (*io);
		for (i = 1; i < loop->maxspc; i++)
		{
			pre->next = (_DTNODE*)malloc(sizeof(_DTNODE));
			pre->next->data = (*io);
			pre = pre->next;
		}
		pre->next = head;
		loop->New = head;
	}
	else
	{
		pre = loop->New;
		loop->New->data = (*io);
		loop->New = loop->New->next;
		for (i = 0; i < loop->maxspc; i++)
		{
			sum += pre->data;
			pre = pre->next;
		}
		(*io) = sum / ((loop->maxspc)*1.0f);
	}
}

void DataLoopFree(_DTLOOP* loop)
{
	_DTNODE* nt = loop->New;
	_DTNODE* sv = loop->New;
	int i = 0;
	if (nt == inull) return;
	for (i = 0; i < loop->maxspc; i++)
	{
		sv = nt->next;
		free(nt);
		nt = inull;
		nt = sv;
	}
	loop->New = inull;
	loop->ONCECLS = itrue;
}
