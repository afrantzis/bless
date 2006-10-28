
DOCS=legal.xml bless-manual.xml
OMF=bless-manual.omf
FIGURES=$(wildcard figures/*)

CLEANFILES=$(OMF)

noinst_SCRIPTS=$(OMF)


$(OMF): $(OMF).in
	if [ -x `which scrollkeeper-preinstall` ]; then \
	    scrollkeeper-preinstall file:$(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/user/bless-manual.xml $(OMF).in $(OMF); \
	else \
	    cp $(OMF).in $(OMF); \
	fi

doc:
	xsltproc /usr/share/xml/docbook/xsl-stylesheets-current/html/chunk.xsl bless-manual.xml

dist-hook: doc
	cp --parents $(DOCS) $(OMF).in $(wildcard *.html) $(FIGURES) $(distdir)

install-data-local:
	$(INSTALL) -d $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/user
	$(INSTALL) -d $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/user/figures
	$(INSTALL_DATA) $(DOCS) $(wildcard *.html)  $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/user
	$(INSTALL_DATA) $(FIGURES) $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/user/figures
	$(INSTALL) -d $(datadir)/omf/$(PACKAGE_NAME)
	$(INSTALL_DATA) $(OMF) $(datadir)/omf/$(PACKAGE_NAME)
if WITH_SCROLLKEEPER
	scrollkeeper-update -o $(datadir)/omf/$(PACKAGE_NAME)
endif

uninstall-local:
	rm -rf $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)/user
	rm $(datadir)/omf/$(PACKAGE_NAME)/$(OMF)
if WITH_SCROLLKEEPER
	scrollkeeper-update -o $(datadir)/omf/$(PACKAGE_NAME)
endif
	rm -rf $(datadir)/omf/$(PACKAGE_NAME)

