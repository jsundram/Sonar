try:
	import pygtk
	pygtk.require('2.0')
	import gtk
	import gtk.glade
except:
	print 'Install pygtk,libgtk2.0 and libglade2.0'
	import os
	os.exit(1)

class editBox:
	def __init__(self, lbl_msg = 'Message here', dlg_title = '', text = ''):
		self.wTree = gtk.glade.XML('editbox.glade', 'dialog1')
		self.dlg = self.wTree.get_widget('dialog1')
		self.lbl = self.wTree.get_widget('label1')
		self.edt = self.wTree.get_widget('entry1')
		self.dlg.set_title(dlg_title) 
		self.lbl.set_text(lbl_msg)
		self.edt.set_text(text)
		self.newtext = ''
		handlers = { 'on_okbutton1_clicked':self.done }
		self.wTree.signal_autoconnect( handlers )
		out = self.dlg.run()

	def done(self,w):
		self.newtext = self.edt.get_text()
		self.dlg.destroy()

