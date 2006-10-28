SRC= $(wildcard *.cs)

EXTRA_DIST= $(SRC)
EXE= bless-builder.exe

CLEANFILES=$(EXE)

all: $(EXE)

$(EXE): $(SRC)
	$(MCS) -target:exe -out:$(EXE) $(SRC)