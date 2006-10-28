DLLS= $(wildcard *.dll)

CLEANFILES= $(DLLS) bless.exe

install-data-local:
	$(INSTALL) -d $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/bin
	$(INSTALL_DATA) $(DLLS) bless.exe $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/bin

uninstall-local:
	rm -rf $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/bin
