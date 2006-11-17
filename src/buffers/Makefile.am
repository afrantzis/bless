SRC= $(wildcard *.cs)

EXTRA_DIST=$(SRC) Bless.Buffers.bi

DLL= Bless.Buffers.dll

CLEANFILES=$(DLL)

REFS= -r:../util/Bless.Util.dll -r:../tools/Bless.Tools.dll -pkg:gtk-sharp-2.0

dll: | Bless.Util.dll Bless.Tools.dll $(DLL)

$(DLL): $(SRC) ../util/Bless.Util.dll ../tools/Bless.Tools.dll
	$(MCS) $(MCS_FLAGS) -target:library $(REFS) -out:$(DLL) $(SRC)
	cp $(DLL) ../../bin

Bless.Util.dll:
	cd ../util && make dll

Bless.Tools.dll:
	cd ../tools && make dll
	