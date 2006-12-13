SRC= ExporterTests.cs

EXTRA_DIST=$(SRC)

DLL= BlessTests.Export.dll

CLEANFILES=$(DLL) TestResult.xml


REFS= -r:Bless.Util.dll \
      -r:Bless.Buffers.dll \
      -r:Bless.Tools.Export.dll \
      -r:nunit.framework.dll

check: Bless.Util.dll Bless.Buffers.dll Bless.Tools.Export.dll $(SRC)
	$(MCS) -target:library $(REFS) -out:$(DLL) $(SRC)
	mono ./nunit-console.exe $(DLL)

Bless.Util.dll: 
	mv ../../../bin/Bless.Util.dll .

Bless.Buffers.dll:
	mv ../../../bin/Bless.Buffers.dll .

Bless.Tools.Export.dll:
	mv ../../../bin/Bless.Tools.Export.dll .
