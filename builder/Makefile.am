SRC= $(wildcard *.cs) ConfigureDefines.cs

EXTRA_DIST= $(SRC)
EXE= bless-builder.exe

CLEANFILES=$(EXE) ConfigureDefines.cs

builder: $(EXE)

$(EXE): $(SRC)
	$(MCS) -target:exe -out:$(EXE) $(SRC)
