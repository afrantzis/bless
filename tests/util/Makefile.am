SRC= $(wildcard *.cs) 

EXTRA_DIST=$(SRC) BlessTests.Util.bi

DLL= BlessTests.Util.dll

CLEANFILES=$(DLL) TestResult.xml

check:
	MONO_PATH=$$MONO_PATH:$(top_srcdir)/bin nunit-console /nologo $(DLL)