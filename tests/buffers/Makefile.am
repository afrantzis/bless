SRC= ByteBufferTests.cs FileBufferTests.cs ChangeBufferTests.cs SegmentTests.cs SegmentCollectionTests.cs

EXTRA_DIST=$(SRC) test1.bin

DLL= BlessTests.Buffer.dll

CLEANFILES=$(DLL) Bless.Util.dll Bless.Buffers.dll Bless.Tools.dll TestResult.xml test2.bin

BLESS_BUFFERS_PATH=../../src/ByteBuffer

BLESS_UTIL_PATH=../../src/util

BLESS_TOOLS_PATH=../../src/tools

REFS= -r:Bless.Buffers.dll \
      -r:Bless.Util.dll \
      -r:Bless.Tools.dll \
      -r:nunit.framework.dll

check: Bless.Util.dll Bless.Tools.dll Bless.Buffers.dll $(SRC)
	$(MCS) -target:library $(REFS) -out:$(DLL) $(SRC)    
	nunit-console $(DLL)

Bless.Util.dll:
	cd $(BLESS_UTIL_PATH) && make dll
	mv $(BLESS_UTIL_PATH)/Bless.Util.dll .

Bless.Tools.dll:
	cd $(BLESS_TOOLS_PATH) && make dll
	mv $(BLESS_TOOLS_PATH)/Bless.Tools.dll .

Bless.Buffers.dll:
	cd $(BLESS_BUFFERS_PATH) && make dll
	mv $(BLESS_BUFFERS_PATH)/Bless.Buffers.dll .

