DOCS=bless.xmi

dist-hook: 
	cp --parents $(DOCS) $(distdir)

install-data-local: 
	$(INSTALL) -d $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)/developer
	$(INSTALL_DATA) $(DOCS) $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)/developer

uninstall-local:
	rm -rf $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)/developer
