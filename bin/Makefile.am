DLLS= $(wildcard *.dll)

CLEANFILES= $(DLLS) bless.exe

install-data-local:
	$(INSTALL) -d $(DESTDIR)$(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/bin
	$(INSTALL_DATA) $(DLLS) bless.exe $(DESTDIR)$(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/bin

uninstall-local:
	rm -rf $(DESTDIR)$(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/bin
