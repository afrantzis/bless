SRC= $(wildcard *.cs)

EXTRA_DIST=$(SRC) BlessTests.Tools.Export.bi

DLL= BlessTests.Tools.Export.dll

CLEANFILES=$(DLL) TestResult.xml

check:
	MONO_PATH=$$MONO_PATH:$(top_srcdir)/bin nunit-console /nologo $(DLL)
