import math
from copy import deepcopy

#ベクトル計算クラス
class Vector:
	def __init__(self,X,Y):
		self.X=X
		self.Y=Y
	#ベクトル加算
	def __add__(self,vec):
		return Vector(self.X+vec.X,self.Y+vec.Y)
	#ベクトル減算
	def __sub__(self,vec):
		return Vector(self.X-vec.X,self.Y-vec.Y)
	#ないせき
	def __mul__(self,vec):
		return self.X*vec.X+self.Y*vec.Y
	#ベクトルの正規化
	def Normalize(self):
		np=math.sqrt(self.X**2+self.Y**2)
		self.X/=np
		self.Y/=np
	#ベクトルのコピー
	def clone(self):
		return deepcopy(self)
	#ベクトルの文字列化
	def __str__(self):
		return f"Vector({self.X}, {self.Y})"

#キーボードの状態を管理するやつ
class CursorKey:
	__up=False
	__left=False
	__right=False
	__bottom=False
	@property
	def up():return CursorKey.__up
	def left(): return CursorKey.__left
	def right():return CursorKey.__right
	def bottom():return CursorKey.__bottom
	def keyDown(e):return CursorKey.__keyState(True)(e)
	def keyUp(e):return CursorKey.__keyState(False)(e)
	def __keyState(state):
		def keyState(e):
			key=e.keysym
			if key=="Up": CursorKey.__up=state
			elif key=="Left": CursorKey.__left=state
			elif key=="Right": CursorKey.__right=state
			elif key=="Down": CursorKey.__down=state
		return  keyState
