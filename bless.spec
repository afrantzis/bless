Name:      bless
Summary:   gtk#-based hex-editor written in C# (with mono)
Requires:  mono-core >= 1.1.14 gtk-sharp2 >= 2.8
BuildRequires:  mono-core >= 1.1.14 gtk-sharp2 >= 2.8
Version:   0.5.1
Release:   1
License:   GPL
Vendor:    Alexandros Frantzis <alf82 at freemail dot gr>
Packager:  Mirco Mueller <macslow at bangang dot de>
Group:     Applications/Editors
Source:    http://download.gna.org/bless/%{name}-%{version}.tar.gz
URL:       http://home.gna.org/bless 
BuildRoot: %{_tmppath}/%{name}-root

%description
Bless is a high quality, full featured hex editor. It is written in
mono/Gtk# and its primary platform is GNU/Linux. However it should
be able to run without problems on every platform that mono and
Gtk# run.

%prep
rm -rf $RPM_BUILD_DIR/%{name}-%{version}
tar zxvf $RPM_SOURCE_DIR/%{name}-%{version}.tar.gz
cd $RPM_BUILD_DIR/%{name}-%{version}
./configure --prefix=%{_prefix} --without-scrollkeeper

%build
cd $RPM_BUILD_DIR/%{name}-%{version}
make

%install
cd $RPM_BUILD_DIR/%{name}-%{version}
make prefix=%{buildroot}%{_prefix} install

%clean
rm -rf $RPM_BUILD_ROOT

%post
if which scrollkeeper-update>/dev/null 2>&1; then scrollkeeper-update; fi

%postun
if which scrollkeeper-update>/dev/null 2>&1; then scrollkeeper-update; fi

%files
%defattr(-,root,root)
%{_bindir}/bless
%dir %{_libdir}/%{name}-%{version}
%{_libdir}/%{name}-%{version}/*
%{_datadir}/applications/bless.desktop
%{_datadir}/doc/%{name}-%{version}/
%{_datadir}/omf/bless/*
%{_datadir}/pixmaps/*

%changelog
* Sun Oct  9 2005 Alexandros Frantzis <alf82 at freemail dot gr> 0.4.0
- Updated .spec file for bless-0.4.0
- All documentation now gets installed in %{_datadir}/doc/%{name}-%{version}/
  instead of %{_docdir}/%{name}-%{version}/
- Added post and postun actions for scrollkeeper

* Mon Sep 12 2005 Mirco Mueller <macslow at bangang dot de> 0.4.0-rc1
- initial .spec file written for bless-0.4.0-rc1.tar.gz

