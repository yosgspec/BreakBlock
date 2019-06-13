import tkinter as tk
from BreakBlock import MainWindow
from BBUtil import CursorKey

root=tk.Tk()
root.title("BreakBlock")
root.geometry("640x480")
root.resizable(0,0)

Field=tk.Canvas(root,width=640,height=480,bg="#000099",highlightbackground="#000099")
Field.pack(side="left")

#キーイベント
root.bind("<Key>",CursorKey.keyDown)
root.bind("<KeyRelease>",CursorKey.keyUp)

MainWindow.MainWindow(root,Field)

root.mainloop()
