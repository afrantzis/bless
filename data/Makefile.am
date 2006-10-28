dist_noinst_DATA=bless-about.png bless-48x48.png bless.desktop \
		bless-default.layout bless-16-bytes-per-row.layout \
		bless-all-number-bases.layout help_script.tmpl.in \
		default-preferences.xml bless.glade

noinst_SCRIPTS=help_script.sh

CLEANFILES=help_script.sh.install help_script.sh

PWD=$(shell pwd)

help_script.sh: help_script.tmpl
	sed -e 's,@blessdocdir\@,$(PWD)/../doc,g' help_script.tmpl > help_script.sh
	chmod +x help_script.sh
	sed -e 's,@blessdocdir\@,$(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION),g' help_script.tmpl > help_script.sh.install

install-data-local:
	$(INSTALL) -d $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/data
	$(INSTALL_DATA) bless-about.png bless-48x48.png bless-default.layout \
	    bless-16-bytes-per-row.layout bless-all-number-bases.layout \
	    default-preferences.xml bless.glade \
	    $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/data
	$(INSTALL) -d $(datadir)/applications
	$(INSTALL_DATA) bless.desktop $(datadir)/applications
	$(INSTALL) -d $(datadir)/pixmaps
	$(INSTALL_DATA) bless-48x48.png $(datadir)/pixmaps
	$(INSTALL_SCRIPT) help_script.sh.install $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/data/help_script.sh

uninstall-local:
	rm -rf $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/data
	rm -f $(datadir)/applications/bless.desktop
	rm -f $(datadir)/pixmaps/bless-48x48.png
