SUBDIRS= areas dialogs drawers plugins

DIALOGS_SRC= $(wildcard dialogs/*.cs)

GUI_SRC= $(wildcard *.cs)

DLL= Bless.Gui.dll

EXTRA_DIST= $(GUI_SRC) MainWindow.cs Bless.Gui.bi Bless.bi

CLEANFILES= $(DLL) bless.exe

REFS=-r:../util/Bless.Util.dll\
-r:./drawers/Bless.Gui.Drawers.dll\
-r:./areas/Bless.Gui.Areas.dll\
-r:../ByteBuffer/Bless.Buffers.dll\
-r:../tools/find/Bless.Tools.Find.dll\
-r:../tools/Bless.Tools.dll
    
PKGS= gtk-sharp-2.0,glade-sharp-2.0

#all: exe

exe: | dll bless.exe
    
bless.exe: Bless.Gui.dll MainWindow.cs
	$(MCS) $(MCS_FLAGS) -target:exe  $(REFS) -r:Bless.Gui.dll -pkg:$(PKGS) -out:bless.exe MainWindow.cs
	cp bless.exe ../../bin

dll: | Bless.Util.dll Bless.Gui.Drawers.dll Bless.Buffers.dll Bless.Tools.Find.dll Bless.Tools.dll Bless.Gui.Areas.dll $(DLL)

$(DLL): $(GUI_SRC) $(DIALOGS_SRC) ../util/Bless.Util.dll drawers/Bless.Gui.Drawers.dll ../ByteBuffer/Bless.Buffers.dll ../tools/find/Bless.Tools.Find.dll ../tools/Bless.Tools.dll areas/Bless.Gui.Areas.dll
	$(MCS) -nowarn:0169 -target:library $(RESOURCES) $(REFS) -pkg:$(PKGS) -out:$(DLL) $(GUI_SRC) $(DIALOGS_SRC)
	cp $(DLL) ../../bin

Bless.Util.dll:
	cd ../util && make dll
	
Bless.Gui.Drawers.dll:
	cd drawers && make dll

Bless.Buffers.dll:
	cd ../ByteBuffer && make dll

Bless.Tools.Find.dll:
	cd ../tools/find && make dll

Bless.Tools.dll:
	cd ../tools && make dll

Bless.Gui.Areas.dll:
	cd areas && make dll