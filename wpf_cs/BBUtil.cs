using System;
using System.Windows.Input;

namespace BBUtil{
	//キーボードの状態を管理するやつ
	static class CursorKey{
		public static bool up;
		public static bool left;
		public static bool right;
		public static bool down;

		public static Action<KeyEventArgs> keyDown=keyState(true);
		public static Action<KeyEventArgs> keyUp=keyState(false);
		static Action<KeyEventArgs> keyState(bool state){
			return new Action<KeyEventArgs>(e=>{
				switch(e.Key){
					case Key.Up: up=state; break;
					case Key.Left: left=state; break;
					case Key.Right: right=state; break;
					case Key.Down: down=state; break;
				}
			});
		}
	}
}
