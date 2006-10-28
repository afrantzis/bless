DOCS=bless.xmi

dist-hook: 
	cp --parents $(DOCS) $(distdir)
	

install-data-local: 
	$(INSTALL) -d $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/developer
	$(INSTALL_DATA) $(DOCS) $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/developer

uninstall-local:
	rm -rf $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/developer