# -*- Autoconf -*-
AC_PREREQ(2.59)
AC_INIT([bless], [0.5.0rc1], [alf82@freemail.gr])
AC_CONFIG_SRCDIR([src/buffers/ByteBuffer.cs])
AM_INIT_AUTOMAKE

#
# I18N
#
AM_GNU_GETTEXT([external])
AM_GNU_GETTEXT_VERSION(0.14)

GETTEXT_PACKAGE=bless
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE, "$GETTEXT_PACKAGE", [Gettext Package])

#
# Check for programs.
#
AC_PATH_PROG(PKG_CONFIG,pkg-config, no)

if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR([You need to install pkg-config])
fi
	
# C# compiler
AC_PATH_PROG(MCS, gmcs, no)

CS="gmcs C#"
if test "x$MCS" = "xno"  ; then
	AC_MSG_ERROR([No $CS compiler found])
fi

# Mono runtime
AC_PATH_PROG(MONO_PROGRAM, mono, no)
	
if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([No mono runtime found])
fi

#
# Checks for libraries.
#
PKG_CHECK_MODULES(MONO, mono >= 1.1.14)
PKG_CHECK_MODULES(GTK_SHARP_2_0, gtk-sharp-2.0 >= 2.8)
PKG_CHECK_MODULES(GLADE_SHARP_2_0, glade-sharp-2.0 >= 2.8)

#
# Check for GNOME
#
#AC_ARG_WITH(gnome, 
			 #		AS_HELP_STRING([--with-gnome],[use gnome services (default: yes)]),
			 #		[USEGNOME="$withval"], [USEGNOME="yes"])

#if test "$USEGNOME" = "yes" ; then
#    PKG_CHECK_MODULES(GNOME, [libgnome-2.0, gnome-sharp >= 1.0.8], [USEGNOME="yes"], [USEGNOME="no"]) 
#fi

#AM_CONDITIONAL(WITH_GNOME, [test "$USEGNOME" = "yes"])

#
# Check for unix
#
AC_ARG_ENABLE(unix-specific, 
			  AS_HELP_STRING([--enable-unix-specific],[ Enable unix specific features (default: yes)]),
			  [UNIX_SPECIFIC="$enableval"], [UNIX_SPECIFIC="yes"])

AM_CONDITIONAL(ENABLE_UNIX_SPECIFIC, [test "$UNIX_SPECIFIC" = "yes"])

#
# Check scrollkeeper
#

AC_ARG_WITH(scrollkeeper, 
			AS_HELP_STRING([--with-scrollkeeper],[register documentation with scrollkeeper (autodetect)]),
			[SCROLLKEEPER="$withval"], [SCROLLKEEPER="yes"])

if test "$SCROLLKEEPER" = "yes" ; then
	SCROLLKEEPER=
	AC_CHECK_PROG(SCROLLKEEPER, scrollkeeper-config, yes, no)
fi

AM_CONDITIONAL(WITH_SCROLLKEEPER, [ test "$SCROLLKEEPER" = "yes" ])

#
# Check debug option
#
DEBUG=no
AC_ARG_ENABLE(debug, 
			  AS_HELP_STRING([--enable-debug=yes|no],[compile with debug information (default: no)]),
			  [DEBUG="$enableval"])

AM_CONDITIONAL(ENABLE_DEBUG, [test "$DEBUG" = "yes"])

AC_CONFIG_FILES([Makefile po/Makefile.in
				 builder/Makefile
				 bin/Makefile
				 data/Makefile
				 data/bless.desktop
				 data/help_script.tmpl
				 doc/Makefile
				 doc/developer/Makefile
				 doc/user/Makefile
				 src/buffers/Makefile
				 src/Makefile
				 src/gui/Makefile
				 src/gui/areas/Makefile
				 src/gui/dialogs/Makefile
				 src/gui/drawers/Makefile
				 src/gui/plugins/Makefile
				 src/tools/Makefile
				 src/tools/find/Makefile
 				 src/tools/export/Makefile
				 src/tools/export/plugins/Makefile
				 src/util/Makefile
				 src/util/ConfigureDefines.cs
				 src/plugins/Makefile
				 tests/Makefile
				 tests/buffers/Makefile
				 tests/util/Makefile
				 tests/tools/Makefile
				 tests/tools/export/Makefile
				 src/bless-script
				 bless-script])
AC_OUTPUT

echo "
	Enabling Unix specific features: $UNIX_SPECIFIC
	Registering documentation with Scrollkeeper: $SCROLLKEEPER
	Compiling with debug information: $DEBUG
	Installing to: $prefix
	"
