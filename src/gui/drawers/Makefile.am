SRC= $(wildcard *.cs)

EXTRA_DIST=$(SRC) Bless.Gui.Drawers.bi

DLL= Bless.Gui.Drawers.dll

CLEANFILES= $(DLL)

PKGS= gtk-sharp-2.0

dll: $(DLL)

$(DLL): $(SRC)
	$(MCS) $(MCS_FLAGS) -target:library -pkg:$(PKGS) -out:$(DLL) $(SRC)
	cp $(DLL) ../../../bin