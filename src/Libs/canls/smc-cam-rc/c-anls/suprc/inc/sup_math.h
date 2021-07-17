// Copyright 赛北智能车
// All rights reserved.
// 除注明出处外，以下所有内容版权均中国计量大学赛北智能车所有，未经允许，不得用于商业用途，
// 修改内容时必须保留赛北智能车的版权声明。

// file       sup_math.h
// brief      数学运算支持(高效算法)
// author     赛北智能车 Rays(整理修改)
// version    v2.0
// date       2018-08-11

#ifndef _SUP_MATH_H_
#define _SUP_MATH_H_

#define _imax( x, y )   ( ((x) > (y)) ? (x) : (y) )    //maximum value
#define _imin( x, y )   ( ((x) < (y)) ? (x) : (y) )    //minimum value
#define _imid( a, b, c) ((a-b) * (b-c) >= 0?b:((b-a) * (a-c) > 0?a:c))//intermediate value

#define _iinc( i, a, b) (i <= _imax(a,b) && i >= _imin(a,b)) //判断 i 在a,b之间
#define _iexc( i, a, b) (i > _imax(a,b) || i < _imin(a,b))   //判断 i 不在a,b之间
#define _ninc( i, a, b) (i >= a && i <= b)

#define _iiabs(x) (((int)(x) > 0) ? (int)(x) : (-(int)(x)))  //求绝对值
#define _iidif(x,y) _iiabs((int)(x - y))              //求差

#define EPSINON        (0.000001f) //浮点数精度值
#define PII            (3.14159265358979323846)
#define FLOAT_ZERO(x)  (x < EPSINON && x > -EPSINON)

//<!-- dichotomy accelerates the opening number -->
extern double _i2sqrt(int number);

//<!-- quick opening numbe -->
extern float  _isqrt(float number);
extern double _isin(double rad);
extern double _icos(double rad);

//<!-- supports two primitive floating point types: float and double -->
extern float _ifabs(float number);

//<!-- inverse tangent (x, y corresponding to two dimensional plane's vertical and horizontal coordinates)-->
//<!-- radian - range:0 ~ PI and -PI ~ 0 -->
extern double _iatan_r(float x, float y);
//<!-- degree - range:0 ~ 180 and -180 ~ 0 -->
extern double _iatan_d(float x, float y);

//<!-- get the greatest common factor by rolling method-->
extern int _gcd(int x, int y);

#endif
