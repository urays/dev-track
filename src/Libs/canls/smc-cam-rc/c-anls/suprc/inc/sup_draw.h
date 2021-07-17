// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sup_draw.h
// brief      track image rendering
// author     赛北智能车 Rays
// version    v3.0
// date       2018-07-18

#ifndef _SUP_DRAW_H
#define _SUP_DRAW_H

#include "../inc/sup_line.h"

extern unsigned char* imageInfo(int* rows, int* cols);

//initialize analysis image
extern void anlsImgCLS(void);

// func pre display line
// para org(const LINE) original line
// para startpos(const int) start position of aim line
// para endpos(const int) end position of aim line
extern void PreviewLine(const LINE org, const int startpos, const int endpos);

// func pre display a point (cross type)
// para point(POINT) aim point
// para size(const char) size (greater than 1)
extern void PreviewPoint(const POINT point, const char size);

// func pre display a point (fork type)
// para point(POINT) aim point
// para size(const char) size (greater than 1)
extern void PreviewPointX(const POINT point, const char size);

// func pre display a point (rectangular type)
// para point(POINT) aim point
// para len(const char) length (len = 2//n+1 n>=1) integer
extern void PreviewPointRect(const POINT point, const char len);

//<!-- pre display datum line -->
extern void PreviewLineBase(void);

// func pre display cicle on the screen
// plus: for single test!
extern void PreviewCircle(int x0, int y0, int r);

// func pre display line level line
// para x(const char) abscissa
// para y(const char) ordinate
// para len(const char) length
extern void PreviewLabLine(const char x, const char y, const char len);

// func pre display vertical line column
// para x(const char) abscissa
// para y(const char) ordinate
// para len(const char) length
extern void PreviewLabColumn(const char x, const char y, const char len);

//<!-- preview scale mark -->
extern void PreviewScaleMark(void);


#endif
