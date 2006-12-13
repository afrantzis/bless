#                                               -*- Autoconf -*-
AC_PREREQ(2.59)
AC_INIT([bless], [0.4.9], [alf82@freemail.gr])
AC_CONFIG_SRCDIR([src/buffers/ByteBuffer.cs])
AM_INIT_AUTOMAKE

# Checks for programs.
AC_PATH_PROG(PKG_CONFIG,pkg-config)
AC_PATH_PROG(MCS,gmcs)
AC_PATH_PROG(MONO_PROGRAM, mono)


# Checks for libraries.
PKG_CHECK_MODULES(MONO, mono >= 1.1.4)
PKG_CHECK_MODULES(GTK_SHARP_2_0, gtk-sharp-2.0 >= 2.4)
PKG_CHECK_MODULES(GLADE_SHARP_2_0, glade-sharp-2.0 >= 2.4)

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

AC_CONFIG_FILES([Makefile
		 builder/Makefile
		 bin/Makefile
                 data/Makefile
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
                 src/util/Makefile
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
