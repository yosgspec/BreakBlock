import tkinter as tk

root=tk.Tk()
root.title("BreakBlock")
root.geometry("640x480")
root.resizable(0,0)

Field=tk.Canvas(root,width=640,height=480,bg="#000099",highlightbackground="#000099")
Field.pack(side="left")

import BreakBlock
BreakBlock.MainWindow.MainWindow(root,Field)

root.mainloop()
