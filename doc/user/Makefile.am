
DOCS=legal.xml bless-manual.xml
OMF=bless-manual.omf
FIGURES=$(wildcard figures/*)

CLEANFILES=$(OMF)

noinst_SCRIPTS=$(OMF)


$(OMF): $(OMF).in
	if [ -x `which scrollkeeper-preinstall` ]; then \
	    scrollkeeper-preinstall file:$(datadir)/doc/$(PACKAGE_NAME)/user/bless-manual.xml $(OMF).in $(OMF); \
	else \
	    cp $(OMF).in $(OMF); \
	fi

doc:
	xsltproc /usr/share/xml/docbook/stylesheet/nwalsh/html/chunk.xsl bless-manual.xml

dist-hook: doc
	cp --parents $(DOCS) $(OMF).in $(wildcard *.html) \
		     $(FIGURES) $(distdir)

install-data-local:
	$(INSTALL) -d $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)/user
	$(INSTALL) -d $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)/user/figures
	$(INSTALL_DATA) $(DOCS) $(wildcard *.html) \
			$(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)/user
	$(INSTALL_DATA) $(FIGURES) $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)/user/figures
	$(INSTALL) -d $(DESTDIR)$(datadir)/omf/$(PACKAGE_NAME)
	$(INSTALL_DATA) $(OMF) $(DESTDIR)$(datadir)/omf/$(PACKAGE_NAME)
if WITH_SCROLLKEEPER
	scrollkeeper-update -o $(DESTDIR)$(datadir)/omf/$(PACKAGE_NAME)
endif

uninstall-local:
	rm -rf $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)/user
	rm $(DESTDIR)$(datadir)/omf/$(PACKAGE_NAME)/$(OMF)
if WITH_SCROLLKEEPER
	scrollkeeper-update -o $(DESTDIR)$(datadir)/omf/$(PACKAGE_NAME)
endif
	rm -rf $(DESTDIR)$(datadir)/omf/$(PACKAGE_NAME)

