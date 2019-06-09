;カラーコード
#module @cc2rgb
	#deffunc local cc2rgb array RGB,str _cc
		cc=_cc
		ccLen=strlen(cc)
		cci=int("$"+strmid(cc,1,ccLen-1))
		if ccLen=7 {
			R=cci>>16&$FF
			G=cci>>8 &$FF
			B=cci    &$FF
		}
		else:if ccLen=4 {
			R=cci>>8&$F: R|=R<<4
			G=cci>>4&$F: G|=G<<4
			B=cci   &$F: B|=B<<4
		}
		else: return 1
		RGB=R,G,B
	return 0
	#define global cc2rgb(%1,%2) dim %1,3: cc2rgb@@cc2rgb %1,%2
	#define global ctype apply3(%1) %1(0),%1(1),%1(2)
	#define global ccolor(%1) %tccolor \
		cc2rgb %i,%1 :\
		color apply3(%o)
#global

;モジュール型変数用コンテナ
#module MList list,count
	#modcfunc mlCount
		return count
	#define new(%1,%2) dimtype %1,5: newmod %1,MList,%2
	#modinit array ary
		count=length(ary)
		dimtype list,5,count
		foreach ary: list.cnt=ary.cnt: loop
	return
	#modfunc local mlRef int index,var item
		item=list.index: return
	#define global mlRef(%1,%2,%3) dimtype %3,5: mlRef@MList %1,%2,%3
	#modfunc mlRemove int index
		count--
		repeat count-index,index
			list(cnt)=list(cnt+1)
		loop
	return
#global

;ベクトル計算マクロ
;X,Y
#enum global X=0
#enum global Y
;ベクトル加算
#define global ctype vcAdd(%1,%2) \
	%1(X)+%2(X),%1(Y)+%2(Y)
#define global vcAddSet(%1,%2) \
	%1(X),%1(Y)=%1(X)+%2(X),%1(Y)+%2(Y)
;ベクトル減算
#define global ctype vcSub(%1,%2) \
	%1(X)-%2(X),%1(Y)-%2(Y)
#define global vcSubSet(%1,%2) \
	%1(X),%1(Y)=%1(X)-%2(X),%1(Y)-%2(Y)
;ないせき
#define global ctype vcDot(%1,%2) \
	%1(X)*%2(X)+%1(Y)*%2(Y)
;ベクトルの正規化
#define global vcNormalize(%1) %tvecNormalize \
	%i=sqrt(powf(%1(X),2)+powf(%1(Y),2)) :\
	%1(X)/=%p :\
	%1(Y)/=%o
;ベクトルのコピー
#define global vcClone(%1,%2) \
	%1=%2(X),%2(Y)
;ベクトルの文字列化
#define global ctype vcStr(%1) strf("Vector(%%.1f, %%.1f)",%1(X),%1(Y))

;キーボードの状態を管理するやつ
dim isKeyDown@KursorKey
#module KursorKey
	#const keyUp 38
	#const keyLeft 37
	#const keyRight 39
	#const keyBottom 40

	#define up keyState@KursorKey(keyUp@KursorKey)
	#define left keyState@KursorKey(keyLeft@KursorKey)
	#define right keyState@KursorKey(keyRight@KursorKey)
	#define bottom keyState@KursorKey(keyBottom@KursorKey)

	#defcfunc local keyState int keyCode
		getkey isKeyDown,keyCode
		return isKeyDown
#global