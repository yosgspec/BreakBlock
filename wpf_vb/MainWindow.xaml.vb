'/*******************************************************************************
'	ブロック崩し
'	参考: http:'nn-hokuson.hatenablog.com/entry/2017/08/17/200918
'
'	.NetCore/WPFで制作し、各クラス化とか速度の分解と角度の追加とか。
'	ゲームとか殆ど作ったことがない人が作ったガラクタ。
'	もう当たり判定全部短形でいいんじゃないかなとか。
'	なんか斜めからボールが入ると当たり判定がガタガタになる。
'
'	のVB.Net版。VBはWPFが.NetCore対応してないなんてそんな…
'*******************************************************************************/
'
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.ColorConverter
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.Windows.Threading
Imports BBUtil
'
NameSpace BreakBlock
	'配置オブジェクトのインターフェース
	Interface IDraw
		Property shp As Shape
		ReadOnly Property left As Single
		ReadOnly Property top As Single
		ReadOnly Property right As Single
		ReadOnly Property bottom As Single
		Sub update()
		Function vsCircle(center As Vector,radius As Single) As Boolean
		Function hit(rad As Single,speed As Single) As (Single,Single)
	End InterFace

	'ボールクラス
	Class Ball: Implements IDraw
		Dim shps As List(Of IDraw)
		Dim _ball As Ellipse
		Property shp As Shape Implements IDraw.shp
		Public radius As Single
		Public pos As Vector
		Dim pos0 As Vector
		Dim deg As Single
		Public rad As Single
		Public speed As Single
		Dim speed0 As Single
		Dim isStart As Task
		Dim color As Brush
		Dim isCleared As Boolean

		Public ReadOnly Property left As Single Implements IDraw.left
			Get
				Return CSng(pos.X-radius): End Get: End Property
		Public ReadOnly Property top As Single Implements IDraw.top
			Get
				Return CSng(pos.Y-radius): End Get: End Property
		Public ReadOnly Property right As Single Implements IDraw.right
			Get
				Return CSng(pos.X+radius): End Get: End Property
		Public ReadOnly Property bottom As Single Implements IDraw.bottom
			Get
				Return CSng(pos.Y+radius): End Get: End Property

		Sub New(shps As List(Of IDraw),radius As Single,pos As Vector,deg As Single,speed As Single,color As String)
			Me.shps=shps
			Me.radius=radius
			pos0=pos
			Me.deg=deg
			speed0=speed
			Me.color=New SolidColorBrush(CType(ConvertFromString(color),Color))
			ready()
		End Sub

		Private Sub ready()
			pos=pos0
			rad=CSng(deg/180*Math.PI)
			speed=speed0

			_ball=New Ellipse()
			Win.Field.Children.Add(_ball)
			_ball.Width=radius*2
			_ball.Height=radius*2
			_ball.Fill=color
			shp=CType(_ball,Shape)
			draw()
			isStart=Task.Delay(1500)
		End Sub

		Sub update() Implements IDraw.update
			'クリア判定
			If shps.Count<=1 Then
				clear()
				Exit Sub
			End If

			'動くのは用意してからちょっと待つ
			If Not isStart.IsCompleted Then Exit Sub

			'ボール
			pos+=New Vector(
				Math.Cos(rad)*speed,
				Math.Sin(rad)*speed
			)

			'壁
			If left<0 Then
				pos.X=radius
				rad=CSng(Math.PI-rad)
			ElseIf Win.Field.Width<right Then
				pos.X=Win.Field.Width-radius
				rad=CSng(Math.PI-rad)
			End If
			If top<0 Then
				pos.Y=radius
				rad=CSng(2*Math.PI-rad)
'			ElseIf Win.Field.Height<bottom Then
'				pos.Y=Win.Field.Height-radius
'				rad=CSng(2*Math.PI-rad)
			End If

			'ボールロスト
			If Win.Field.Height<top Then
				Win.Field.Children.Remove(_ball)
				ready()
				Exit Sub
			End If

			'パドルとブロックの当たり判定
			shps.RemoveAll(Function(v)
				If v.vsCircle(pos,radius) Then
					Dim t=v.hit(rad,speed)
					rad=t.Item1: speed=t.Item2
					If TypeName(v)<>"Block" Then Return False
					Win.Field.Children.Remove(v.shp)
					Return True
				End If
				Return False
			End Function)
			rad=CSng(rad Mod 2*Math.PI)
			draw()
		End Sub

		Private Sub draw()
			Canvas.SetLeft(_ball,left)
			Canvas.SetTop(_ball,top)
		End Sub

		'クリア処理
		Private Sub clear()
			If isCleared Then Exit Sub
			Win.Field.Children.Remove(_ball)
			Dim tb As New TextBlock()
			Win.Field.Children.Add(tb)
			tb.Text="Clear!"
			tb.FontSize=72
			tb.Foreground=New SolidColorBrush(CType(ConvertFromString("#AAAAFF"),Color))
			Canvas.SetLeft(tb,Win.Field.Width/2-100)
			Canvas.SetTop(tb,Win.Field.Height/2-50)
			isCleared=True
		End Sub

		Function vsCircle(center As Vector,radius As Single) As Boolean Implements IDraw.vsCircle
			Return False: End Function
		Function hit(rad As Single,speed As Single) As (Single,Single) Implements IDraw.hit
			Return (rad,speed): End Function
	End Class

	'ブロッククラス
	Class Block: Implements IDraw
		Dim _block As Rectangle
		Public Property shp As Shape Implements IDraw.shp
		Public size As Vector
		Public pos As Vector
		Dim lastHit As Vector()

		ReadOnly Property left As Single Implements IDraw.left
			Get
				Return CSng(pos.X-size.X/2): End Get: End Property
		ReadOnly Property top As Single Implements IDraw.top
			Get
				Return CSng(pos.Y-size.Y/2): End Get: End Property
		ReadOnly Property right As Single Implements IDraw.right
			Get
				Return CSng(pos.X+size.X/2): End Get: End Property
		ReadOnly Property bottom As Single Implements IDraw.bottom
			Get
				Return CSng(pos.Y+size.Y/2): End Get: End Property

		Sub New(size As Vector,pos As Vector,color As String)
			Me.size=size
			Me.pos=pos

			_block=New Rectangle()
			Win.Field.Children.Add(_block)
			_block.Width=size.X
			_block.Height=size.Y
			_block.Fill=New SolidColorBrush(CType(ConvertFromString(color),Color))
			shp=CType(_block,Shape)
		End Sub

		Overridable Sub update() Implements IDraw.update
			draw()
		End Sub

		Protected Sub draw()
			Canvas.SetLeft(_block,left)
			Canvas.SetTop(_block,top)
		End Sub

		'上下左右の線のセット
		Private ReadOnly Property lines() As Vector()()
			Get
				Return {
					New Vector(){New Vector(left ,top),   New Vector(right,top)},
					New Vector(){New Vector(right,top),   New Vector(right,bottom)},
					New Vector(){New Vector(right,bottom),New Vector(left, bottom)},
					New Vector(){New Vector(left ,bottom),New Vector(left, top)}
				}: End Get: End Property

		'当たり判定
		Public Function vsCircle(bPos As Vector,radius As Single) As Boolean Implements IDraw.vsCircle
			For Each v In lines
				If Block.lineVsCircle(v,bPos,radius) Then
					If v(0).X=v(1).X Then
						If v(0).X=left Then: bPos.X=left-radius
						ElseIf v(0).X=right Then: bPos.X=right+radius
						End If
					ElseIf v(0).Y=v(1).Y Then
						If v(0).Y=top Then: bPos.Y=top-radius
						ElseIf v(0).Y=bottom Then: bPos.Y=bottom+radius
						End If
					End If
					lastHit=v
					Return True
				End If
			Next
			Return False
		End Function

		'線と円の当たり判定
		'理解できていないメソッド
		Public Shared Function lineVsCircle(p As Vector(),center As Vector,radius As Single) As Boolean
			Dim lineDir As Vector=p(1)-p(0)           'パドルの方向ベクトル
			Dim n As New Vector(lineDir.Y,-lineDir.X) 'パドルの法線
			n.Normalize()

			Dim dir1 As Vector=center-p(0)
			Dim dir2 As Vector=center-p(1)

			Dim dist As Double=Math.Abs(dir1*n)
			Dim a1 As Double=dir1*lineDir
			Dim a2 As Double=dir2*lineDir
			Return a1*a2<0 And dist<radius
		End Function

		'当たると反射する
		Public Overridable Function  hit(radB As Single,speedB As Single) As (Single,Single) Implements IDraw.hit
			Dim x=lastHit(1).X-lastHit(0).X
			Dim y=lastHit(1).Y-lastHit(0).Y
			Dim line=CSng(1+Math.Cos(Math.Atan2(y,x)))
			radB=CSng(line*Math.PI-radB)
			Return (radB,speedB)
		End Function
	End Class

	'パドルクラス キーボードで動かせる
	Class Paddle: Inherits Block
		Public rad As Single
		Public accel As Single
		Public speed As Single

		Public Sub New(size As Vector,pos As Vector,color As String,accel As Single)
			MyBase.New(size,pos,color)
			Me.rad=0
			Me.accel=accel
			Me.speed=0
		End Sub

		'うごく
		Public Overrides Sub update()
			If CursorKey.left And 0<left Then
				speed=accel
				rad=CSng(Math.PI)
				pos.X-=speed
			ElseIf CursorKey.right And right<Win.Field.Width Then
				speed=accel
				rad=0
				pos.X+=speed
			Else
				speed=0
			End If

			draw()
		End Sub

		'パドルのスピードでボールに変化をもたらす存在
		Public Overrides Function hit(radB As Single,speedB As Single) As (Single,Single)
			Dim t=MyBase.hit(radB,speedB)
			radB=t.Item1:speedB=t.Item2
			Return (CSng(radB+Math.Cos(rad)*speed/180*Math.PI),speedB)
		End Function
	End Class

	'ボールフィールドを静的に配置するための何か
	Module Win
		Public Field As Canvas: End Module

	'メインクラス
	Partial Class MainWindow: Inherits Window
		Sub MainWindow()
			Win.Field=Field
			Dim shps As New List(Of IDraw)
			shps.Add(New Paddle(New Vector(100,5),New Vector(Field.Width/2,Field.Height-50),"#66CCFF",20))
			Dim _blockColors={"#9999FF","#99FF99","#FF9999","#99FFFF","#FFFF99","#FF99FF","#FFFFFF"}
			For i=0i To 7-1
				For n=0i To 5-1
					shps.Add(New Block(New Vector(80,30),New Vector(50+i*90,25+n*40),_blockColors(i)))
				Next
			Next
			Dim _ball As New Ball(shps,10,New Vector(Field.Width/2,300),90,10,"#FF00FF")

			'インターバル
			Dim timer As New DispatcherTimer()
			timer.Interval=TimeSpan.FromMilliseconds(31)'(33)
			AddHandler timer.Tick,Sub(sender,e)
				For Each v In shps
					v.update()
				Next
				_ball.update()
			End	Sub
			timer.Start()
		End Sub
	End Class
End NameSpace
