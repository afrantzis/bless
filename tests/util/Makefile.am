SRC= BaseConverterTests.cs ByteArrayTests.cs DequeTests.cs ListTests.cs RangeTests.cs

EXTRA_DIST=$(SRC)

DLL= BlessTests.Util.dll

CLEANFILES=$(DLL) Bless.Util.dll TestResult.xml

BLESS_UTIL_PATH=../../src/util

REFS= -r:Bless.Util.dll \
      -r:nunit.framework.dll

check: Bless.Util.dll $(SRC)
	$(MCS) -target:library $(REFS) -out:$(DLL) $(SRC)
	nunit-console $(DLL)

Bless.Util.dll: 
	cd $(BLESS_UTIL_PATH) && make dll
	mv $(BLESS_UTIL_PATH)/Bless.Util.dll .
