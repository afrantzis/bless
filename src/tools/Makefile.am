DIST_SUBDIRS= find

SRC= $(wildcard *.cs)

EXTRA_DIST= $(SRC) Bless.Tools.bi

DLL= Bless.Tools.dll

CLEANFILES=$(DLL)

REFS=

dll: $(DLL)

$(DLL): $(SRC)
	$(MCS) $(MCS_FLAGS) -target:library $(REFS) -out:$(DLL) $(SRC)
	cp $(DLL) ../../bin