#SUBDIRS= buffers gui tools util

DIST_SUBDIRS = buffers gui tools util plugins

EXTRA_DIST= bless.mi

CLEANFILES= bless

MCS_FLAGS= -nowarn:0169

if ENABLE_DEBUG
    MCS_FLAGS+=-g
endif

if ENABLE_UNIX_SPECIFIC
    MCS_FLAGS+=-d:ENABLE_UNIX_SPECIFIC
endif

noinst_SCRIPTS = bless.exe

bless.exe: bless
	$(MONO_PROGRAM) ../builder/bless-builder.exe Bless $(MCS_FLAGS)

bless: bless-script
	sed -e 's,@blesslibdir\@,$(libdir),g' bless-script > bless

install-exec-local: bless
	$(INSTALL) -d $(bindir)
	$(INSTALL_SCRIPT) bless $(bindir)

uninstall-local:
	rm -rf $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/bin
	rm -f $(bindir)/bless
