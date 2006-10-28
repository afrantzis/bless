SRC= $(wildcard *.cs)

EXTRA_DIST= $(SRC) Bless.Plugins.bi
DLL= Bless.Plugins.dll

CLEANFILES=$(DLL)

dll: $(DLL)

$(DLL): $(SRC)
	$(MCS) $(MCS_FLAGS) -pkg:gtk-sharp-2.0 -target:library -out:$(DLL) $(SRC)
	cp $(DLL) ../../bin