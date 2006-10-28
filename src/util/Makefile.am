SRC= $(wildcard *.cs)

EXTRA_DIST= $(SRC) Bless.Util.bi
DLL= Bless.Util.dll

CLEANFILES=$(DLL)

dll: $(DLL)

$(DLL): $(SRC)
	$(MCS) $(MCS_FLAGS) -pkg:gtk-sharp-2.0 -target:library -out:$(DLL) $(SRC)
	cp $(DLL) ../../bin