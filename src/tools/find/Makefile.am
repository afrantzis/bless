SRC= $(wildcard *.cs)

EXTRA_DIST=$(SRC) Bless.Tools.Find.bi

DLL= Bless.Tools.Find.dll

CLEANFILES= $(DLL)

REFS= -r:../../ByteBuffer/Bless.Buffers.dll \
      -r:../../util/Bless.Util.dll

dll: | Bless.Buffers.dll Bless.Util.dll $(DLL)

$(DLL): $(SRC) ../../util/Bless.Util.dll ../../ByteBuffer/Bless.Buffers.dll
	$(MCS) $(MCS_FLAGS) -target:library $(REFS) -out:$(DLL) $(SRC)
	cp $(DLL) ../../../bin

Bless.Util.dll:
	cd ../../util && make dll

Bless.Buffers.dll:
	cd ../../ByteBuffer && make dll
