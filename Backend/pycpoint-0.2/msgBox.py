try:
	import pygtk
	pygtk.require('2.0')
	import gtk
	import gtk.glade
except:
	print 'Install pygtk,libgtk2.0 and libglade2.0'
	import os
	os.exit(1)

class messageBox:
	def __init__(self, lbl_msg = 'Message here',
			dlg_title = ''):
		self.wTree = gtk.glade.XML('msgbox.glade')
		self.dlg = self.wTree.get_widget('dialog1')
		self.lbl = self.wTree.get_widget('label1')
		self.dlg.set_title(dlg_title) 
		self.lbl.set_text(lbl_msg)
		handlers = { 'on_okbutton1_clicked':self.done }
		self.wTree.signal_autoconnect( handlers )

	def done(self,w):
		self.dlg.destroy()
