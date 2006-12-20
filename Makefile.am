EXTRA_DIST = AUTHORS COPYING NEWS README README.packaging bless.mdp bless.mds bless.spec

SUBDIRS = po builder src po bin data doc tests

DIST_SUBDIRS= po builder src po bin doc data tests

CLEANFILES = bless

PWD= $(shell pwd)

all:
	sed -e 's,@blesssrcdir\@,$(PWD),g' bless-script > bless
	chmod +x bless

dist-hook:
	bzr log > $(distdir)/ChangeLog

install-data-local:
	$(INSTALL) -d $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)
	$(INSTALL_DATA) AUTHORS COPYING INSTALL ChangeLog NEWS README README.packaging bless.spec $(datadir)/doc/$(PACKAGE_NAME)-$(PACKAGE_VERSION)

uninstall-local:
	rm -rf $(libdir)/$(PACKAGE_NAME)-$(PACKAGE_VERSION)
