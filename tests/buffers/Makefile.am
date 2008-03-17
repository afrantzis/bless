SRC= $(wildcard *.cs)

EXTRA_DIST=$(SRC) BlessTests.Buffers.bi test1.bin

DLL= BlessTests.Buffers.dll

CLEANFILES=$(DLL) TestResult.xml test2.bin

check:
	MONO_PATH=$$MONO_PATH:$(top_srcdir)/bin nunit-console /nologo $(DLL)