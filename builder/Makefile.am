SRC= $(wildcard *.cs)

EXTRA_DIST= $(SRC)
EXE= bless-builder.exe

CLEANFILES=$(EXE)

builder: $(EXE)

$(EXE): $(SRC)
	$(MCS) -target:exe -out:$(EXE) $(SRC)