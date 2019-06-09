"""*****************************************************************************
	ブロック崩し
	参考: http://nn-hokuson.hatenablog.com/entry/2017/08/17/200918

	.NetCore/WPFで制作し、各クラス化とか速度の分解と角度の追加とか。
	ゲームとか殆ど作ったことがない人が作ったガラクタ。
	もう当たり判定全部短形でいいんじゃないかなとか。
	なんか斜めからボールが入ると当たり判定がガタガタになる。

	のTkinter/Python3版。ゲームスピードはWPFより速く、HSPより遅い。
*****************************************************************************"""

import time
import threading
import math
from tkinter import messagebox as msg
from BBUtil import *

#ボールクラス
class Ball:
	@property
	def left(self):return self.pos.X-self.radius
	@property
	def top(self):return self.pos.Y-self.radius
	@property
	def right(self):return self.pos.X+self.radius
	@property
	def bottom(self):return self.pos.Y+self.radius

	def __init__(self,shps,radius,pos,deg,speed,color):
		self.__shps=shps
		self.radius=radius
		self.__pos0=pos
		self.__deg=deg
		self.__speed0=speed
		self.__isCleared=False
		self.__color=color
		self.__ready()

	def __ready(self):
		self.pos=self.__pos0.clone()
		self.__mpos=self.pos
		self.rad=self.__deg/180*math.pi
		self.speed=self.__speed0
		self.__ball=Field.create_oval(
			self.left,self.top,
			self.right,self.bottom,
			outline=self.__color,fill=self.__color
		)
		self.shp=self.__ball
		self.__draw()
		self.__isStart=threading.Thread(target=time.sleep,args=(1.500,))
		self.__isStart.start()

	def update(self):
		#クリア判定
		if len(self.__shps)<=1:
			self.__clear()
			return

		#動くのは用意してからちょっと待つ
		if self.__isStart.isAlive(): return
		#ボール
		self.__mpos=self.pos.clone()
		self.pos+=Vector(
			math.cos(self.rad)*self.speed,
			math.sin(self.rad)*self.speed
		);
		#壁
		if self.left<0 or float(Field.cget("width"))<self.right: self.rad=math.pi-self.rad
		if self.top<0: self.rad=2.0*math.pi-self.rad
		#ボールロスト
		if float(Field.cget("height"))<self.top:
			Field.delete(self.__ball)
			self.__ready()
			return

		#パドルとブロックの当たり判定
		def chkShps(v):
			if v.vsCircle(self.pos,self.radius):
				self.rad,self.speed=v.hit(self.rad,self.speed)
				if type(v) is not Block: return True
				Field.delete(v.shp)
				return False
			return True
		self.__shps=[v for v in self.__shps if chkShps(v)]
		self.rad%=2*math.pi
		self.__draw()

	def __draw(self):
		Field.move(self.__ball,self.pos.X-self.__mpos.X,self.pos.Y-self.__mpos.Y)

	#クリア処理
	def __clear(self):
		if self.__isCleared: return
		Field.delete(self.__ball)
		tb=Field.create_text(
			float(Field.cget("width"))/2,
			float(Field.cget("height"))/2,
			text="Clear!",
			fill="#AAAAFF",
			font=("メイリオ",72)
		)
		self.__isCleared=True

#ブロッククラス
class Block:
	@property
	def left(self):return self.pos.X-self.size.X/2
	@property
	def top(self):return self.pos.Y-self.size.Y/2
	@property
	def right(self):return self.pos.X+self.size.X/2
	@property
	def bottom(self):return self.pos.Y+self.size.Y/2

	def __init__(self,size,pos,color):
		self.size=size
		self.pos=pos

		self._block=Field.create_rectangle(
			self.left,self.top,
			self.right,self.bottom,
			outline=color,fill=color
		)
		self.shp=self._block

	def update(self):
		self._mpos=self.pos.clone()
		self._draw()

	def _draw(self):
		Field.move(self._block,self.pos.X-self._mpos.X,self.pos.Y-self._mpos.Y)

	#上下左右の線のセット
	@property
	def __lines(self): return [
		[Vector(self.left, self.top),   Vector(self.right,self.top)],
		[Vector(self.right,self.top),   Vector(self.right,self.bottom)],
		[Vector(self.right,self.bottom),Vector(self.left, self.bottom)],
		[Vector(self.left, self.bottom),Vector(self.left, self.top)]
	]

	#当たり判定
	def vsCircle(self,bPos,radius):
		for v in self.__lines:
			if Block.__lineVsCircle(v,bPos,radius):
				if v[0].X==v[1].X:
					if v[0].X==self.left: bPos.X=self.left-radius
					elif v[0].X==self.right: bPos.X=self.right+radius
				elif v[0].Y==v[1].Y:
					if v[0].Y==self.top: bPos.Y=self.top-radius
					elif v[0].Y==self.bottom: bPos.Y=self.bottom+radius
				self.__lastHit=v
				return True
		return False

	#線と円の当たり判定
	#理解できていないメソッド
	def __lineVsCircle(p,center,radius):
		lineDir=p[1]-p[0]              #パドルの方向ベクトル
		n=Vector(lineDir.Y,-lineDir.X) #パドルの法線
		n.Normalize()

		dir1=center-p[0]
		dir2=center-p[1]

		dist=abs(dir1*n)
		a1=dir1*lineDir
		a2=dir2*lineDir

		return a1*a2<0 and dist<radius

	#当たると反射する
	def hit(self,radB,speedB):
		x=self.__lastHit[1].X-self.__lastHit[0].X
		y=self.__lastHit[1].Y-self.__lastHit[0].Y
		line=1+math.cos(math.atan2(y,x))
		radB=line*math.pi-radB
		return radB,speedB

#パドルクラス キーボードで動かせる
class Paddle(Block):
	def __init__(self,size,pos,color,accel):
		super().__init__(size,pos,color)
		self.rad=0
		self.accel=accel
		self.speed=0

	#うごく
	def update(self):
		self._mpos=self.pos.clone()
		if CursorKey.left() and 0<self.left:
			self.speed=self.accel
			self.rad=math.pi
			self.pos.X-=self.speed
		elif CursorKey.right() and self.right<float(Field.cget("width")):
			self.speed=self.accel
			self.rad=0
			self.pos.X+=self.speed
		else: self.speed=0
		self._draw()

	#パドルのスピードでボールに変化をもたらす存在
	def hit(self,radB,speedB):
		(radB,speedB)=super().hit(radB,speedB)
		return radB+math.cos(self.rad)*self.speed/180*math.pi,speedB

#メインクラス
class MainWindow:
	def MainWindow(root,field):
		def printKey(event):
			key = event.keysym
			msg.showinfo("",key)
		global Field
		Field=field
		shps=[]
		shps.append(Paddle(Vector(100,5),Vector(float(Field.cget("width"))/2,float(Field.cget("height"))-50),"#66CCFF",20))
		blockColors=["#9999FF","#99FF99","#FF9999","#99FFFF","#FFFF99","#FF99FF","#FFFFFF"]
		shps.extend([Block(Vector(80,30),Vector(50+i*90,25+n*40),blockColors[i]) 
			for i in range(7)
				for n in range(5)])
		ball=Ball(shps,10,Vector(float(Field.cget("width"))/2,300),90,10,"#FF00FF")
		#インターバル
		def update():
			for v in shps:
				v.update()
			ball.update()
			root.after(33,update)
		update()
