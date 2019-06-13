using System;
using System.Windows.Input;

namespace BBUtil{
	//キーボードの状態を管理するやつ
	static class CursorKey{
		static bool _left,_up,_right,_down;
		public static bool left{get{return _left;}}
		public static bool up{get{return _up;}}
		public static bool right{get{return _right;}}
		public static bool down{get{return _down;}}

		public static Action<KeyEventArgs> keyDown=keyState(true);
		public static Action<KeyEventArgs> keyUp=keyState(false);
		static Action<KeyEventArgs> keyState(bool state){
			return new Action<KeyEventArgs>(e=>{
				switch(e.Key){
					case Key.Left: _left=state; break;
					case Key.Up: _up=state; break;
					case Key.Right: _right=state; break;
					case Key.Down: _down=state; break;
				}
			});
		}
	}
}
