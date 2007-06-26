EXTRA_DIST = AUTHORS COPYING NEWS README README.packaging \
             bless.mdp bless.mds bless.spec autogen.sh

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
	$(INSTALL) -d $(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)
	$(INSTALL_DATA) AUTHORS COPYING INSTALL ChangeLog NEWS \
			README README.packaging bless.spec     \
			$(DESTDIR)$(datadir)/doc/$(PACKAGE_NAME)

uninstall-local:
	rm -rf $(DESTDIR)$(libdir)/$(PACKAGE_NAME)
