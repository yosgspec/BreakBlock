import math

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

#キーボードの状態を管理するやつ
class CursorKey:
	up=False
	left=False
	right=False
	bottom=False
	def keyDown(e):return CursorKey.keyState(True)(e)
	def keyUp(e):return CursorKey.keyState(False)(e)
	def keyState(state):
		def keyState(e):
			key=e.keysym
			if key=="Up": CursorKey.up=state
			elif key=="Left": CursorKey.left=state
			elif key=="Right": CursorKey.right=state
			elif key=="Down": CursorKey.down=state
		return  keyState
