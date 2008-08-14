EXTRA_DIST = AUTHORS COPYING NEWS README README.packaging \
             bless.mdp bless.mds bless.spec autogen.sh bless.mi

SUBDIRS = po builder src bin data doc . tests

DIST_SUBDIRS= po builder src bin doc data tests

CLEANFILES = bless

PWD= $(shell pwd)

MCS_FLAGS= -nowarn:0169

if ENABLE_DEBUG
    MCS_FLAGS+=-debug
endif
    
if ENABLE_UNIX_SPECIFIC
    MCS_FLAGS+=-d:ENABLE_UNIX_SPECIFIC
endif


all: bless-builder
	$(MONO_PROGRAM) builder/bless-builder.exe Bless $(MCS_FLAGS)
	sed -e 's,@blesssrcdir\@,$(PWD),g' bless-script > bless
	chmod +x bless

check-am: bless-builder
	$(MONO_PROGRAM) builder/bless-builder.exe BlessTests $(MCS_FLAGS)

bless-builder:
	cd builder && $(MAKE) builder

dist-hook:
	bzr log > $(distdir)/ChangeLog

install-data-local:
	$(INSTALL) -d $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)
	$(INSTALL_DATA) AUTHORS COPYING INSTALL ChangeLog NEWS \
			README README.packaging bless.spec     \
			$(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)

uninstall-local:
	rm -rf $(DESTDIR)$(libdir)/$(PACKAGE_NAME)
