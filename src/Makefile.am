#SUBDIRS= buffers gui tools util

DIST_SUBDIRS = buffers gui tools util plugins

EXTRA_DIST= bless.mi

CLEANFILES= bless

MCS_FLAGS= -nowarn:0169

if ENABLE_DEBUG
    MCS_FLAGS+=-debug
endif

if ENABLE_UNIX_SPECIFIC
    MCS_FLAGS+=-d:ENABLE_UNIX_SPECIFIC
endif

noinst_SCRIPTS = bless.exe

bless.exe: bless util/ConfigureDefines.cs
	$(MONO_PROGRAM) ../builder/bless-builder.exe Bless $(MCS_FLAGS)

util/ConfigureDefines.cs: util/ConfigureDefines.cs.tmpl
	sed -e 's,@blessdatadir\@,$(datadir),g' util/ConfigureDefines.cs.tmpl > util/ConfigureDefines.cs

bless: bless-script
	sed -e 's,@blesslibdir\@,$(libdir),g' bless-script > bless

install-exec-local: bless
	$(INSTALL) -d $(DESTDIR)$(bindir)
	$(INSTALL_SCRIPT) bless $(DESTDIR)$(bindir)

uninstall-local:
	rm -rf $(DESTDIR)$(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/bin
	rm -f $(DESTDIR)$(bindir)/bless
