SUBDIRS= buffers gui tools util plugins

DIST_SUBDIRS = buffers gui tools util plugins

CLEANFILES= bless

noinst_SCRIPTS = bless.exe

bless.exe: bless util/ConfigureDefines.cs

util/ConfigureDefines.cs: util/ConfigureDefines.cs.tmpl
	sed -e 's,@blessdatadir\@,$(datadir),g' util/ConfigureDefines.cs.tmpl > util/ConfigureDefines.cs

bless: bless-script
	sed -e 's,@blesslibdir\@,$(libdir),g' bless-script > bless

install-exec-local: bless
	$(INSTALL) -d $(DESTDIR)$(bindir)
	$(INSTALL_SCRIPT) bless $(DESTDIR)$(bindir)

uninstall-local:
	rm -rf $(DESTDIR)$(libdir)/$(PACKAGE_NAME)
	rm -f $(DESTDIR)$(bindir)/bless

