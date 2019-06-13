Option Strict On

Imports System
Imports System.Windows.Input

NameSpace BBUtil
	'キーボードの状態を管理するやつ
	Module CursorKey
		Private _left,_up,_right,_down As Boolean
		ReadOnly Property left As Boolean
			Get
				Return _left: End Get: End Property
		ReadOnly Property up As Boolean
			Get
				Return _up: End Get: End Property
		ReadOnly Property right As Boolean
			Get
				Return _right: End Get: End Property
		ReadOnly Property down As Boolean
			Get
				Return _down: End Get: End Property

		Public  keyDown As Action(Of KeyEventArgs)=keyState(true)
		Public  keyUp As Action(Of KeyEventArgs)=keyState(false)
		Function keyState(state As Boolean) As Action(Of KeyEventArgs)
			Return Sub(e)
				Select Case e.Key
					case Key.Up: _up=state
					case Key.Left: _left=state
					case Key.Right: _right=state
					case Key.Down: _down=state
				End Select
			End Sub
		End Function
	End Module
End NameSpace
