DLLS= $(wildcard *.dll)

CLEANFILES= $(DLLS) bless.exe

install-data-local:
	$(INSTALL) -d $(DESTDIR)$(libdir)/$(PACKAGE_NAME)
	$(INSTALL_DATA) $(DLLS) bless.exe \
			$(DESTDIR)$(libdir)/$(PACKAGE_NAME)

uninstall-local:
	rm -rf $(DESTDIR)$(libdir)/$(PACKAGE_NAME)
