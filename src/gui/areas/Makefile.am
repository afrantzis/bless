
SRC= $(wildcard *.cs)

EXTRA_DIST=$(SRC) Bless.Gui.Areas.bi

DLL= Bless.Gui.Areas.dll

CLEANFILES= $(DLL)

REFS=../../util/Bless.Util.dll,../drawers/Bless.Gui.Drawers.dll,../../ByteBuffer/Bless.Buffers.dll,../../tools/find/Bless.Tools.Find.dll

PKGS= gtk-sharp-2.0

dll: | Bless.Util.dll Bless.Gui.Drawers.dll Bless.Buffers.dll Bless.Tools.Find.dll $(DLL)

$(DLL): $(SRC) ../../util/Bless.Util.dll ../drawers/Bless.Gui.Drawers.dll ../../ByteBuffer/Bless.Buffers.dll ../../tools/find/Bless.Tools.Find.dll
	$(MCS) $(MCS_FLAGS) -target:library -r:$(REFS) -pkg:$(PKGS) -out:$(DLL) $(SRC)
	cp $(DLL) ../../../bin

Bless.Util.dll:
	cd ../../util && make dll

Bless.Gui.Drawers.dll:
	cd ../drawers && make dll

Bless.Buffers.dll:
	cd ../../ByteBuffer && make dll

Bless.Tools.Find.dll:
	cd ../../tools/find && make dll 